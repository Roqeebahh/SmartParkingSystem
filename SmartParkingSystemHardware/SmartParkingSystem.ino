#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include "secrets.h"

WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
#define API_HOST         "192.168.197.170"  
#define API_PORT         5062
#define API_ENDPOINT     "/api/parking/update"
#define SPACE_NUMBER     "CIT-01"
#define SENSOR_ID        "ESP32-CIT-01"

#define TRIG_PIN               5
#define ECHO_PIN               18
#define LED_GREEN              2
#define LED_RED                4
#define OCCUPANCY_THRESHOLD_CM 30
#define READING_INTERVAL_MS    2500
#define CONFIRM_COUNT          3
#define HTTP_TIMEOUT_MS        8000
#define STATUS_AVAILABLE       0
#define STATUS_OCCUPIED        1

int currentStatus     = -1;
int candidateStatus   = -1;
int confirmationCount = 0;

void setup() {
  Serial.begin(115200);
  Serial.println("=== LASU Smart Parking Sensor ===");
  Serial.printf("Space: %s | Sensor: %s\n", SPACE_NUMBER, SENSOR_ID);

  pinMode(TRIG_PIN,  OUTPUT);
  pinMode(ECHO_PIN,  INPUT);
  pinMode(LED_GREEN, OUTPUT);
  pinMode(LED_RED,   OUTPUT);

  blinkBoth(3, 200);
  connectWiFi();
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("[WiFi] Reconnecting...");
    connectWiFi();
  }

  float distance = measureDistanceCm();
  int   reading  = detectStatus(distance);

  Serial.printf("[Sensor] %.1f cm → %s\n",
    distance, reading == STATUS_OCCUPIED ? "OCCUPIED" : "AVAILABLE");

  if (reading == candidateStatus) {
    confirmationCount++;
  } else {
    candidateStatus   = reading;
    confirmationCount = 1;
  }

  if (confirmationCount >= CONFIRM_COUNT && reading != currentStatus) {
    Serial.printf("[State] Confirmed: → %s\n",
      reading == STATUS_OCCUPIED ? "OCCUPIED" : "AVAILABLE");

    if (reportStatus(reading, distance)) {
      confirmationCount = CONFIRM_COUNT;
    }
    currentStatus = reading;
  }

  updateLEDs(currentStatus == -1 ? STATUS_AVAILABLE : currentStatus);
  delay(READING_INTERVAL_MS);
}

float measureDistanceCm() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);

  long duration = pulseIn(ECHO_PIN, HIGH, 30000UL);
  if (duration == 0) return 999.0f;
  return (float)duration * 0.0343f / 2.0f;
}

int detectStatus(float distance) {
  if (distance < 2.0f || distance > 400.0f) return STATUS_AVAILABLE;
  return (distance < OCCUPANCY_THRESHOLD_CM) ? STATUS_OCCUPIED : STATUS_AVAILABLE;
}

bool reportStatus(int status, float distance) {
  if (WiFi.status() != WL_CONNECTED) return false;

  StaticJsonDocument<200> doc;
  doc["spaceNumber"] = SPACE_NUMBER;
  doc["status"]      = status;
  doc["sensorId"]    = SENSOR_ID;
  doc["distance"]    = (int)distance;

  String body;
  serializeJson(doc, body);

  String url = "http://" + String(API_HOST) + ":" + API_PORT + API_ENDPOINT;
  Serial.printf("[HTTP] POST %s\n", url.c_str());

  WiFiClient client;
  //client.setInsecure();
  HTTPClient http;
  http.begin(client, url);
  http.addHeader("Content-Type", "application/json");
  http.setTimeout(HTTP_TIMEOUT_MS);

  int code = http.POST(body);
  http.end();

  if (code == 200 || code == 201) {
    Serial.printf("[HTTP] OK (%d)\n", code);
    return true;
  }
  Serial.printf("[HTTP] Failed: %d\n", code);
  Serial.println(http.errorToString(code));
  return false;
}

void connectWiFi() {
  WiFi.mode(WIFI_STA);
  WiFi.disconnect(true, true);
  delay(1000);

  Serial.println("Scanning WiFi...");

  int n = WiFi.scanNetworks();

  if (n == 0) {
    Serial.println("No networks found.");
  } else {
    Serial.printf("%d networks found\n", n);

    for (int i = 0; i < n; i++) {
      Serial.printf("%d: %s (%d dBm)\n",
                    i + 1,
                    WiFi.SSID(i).c_str(),
                    WiFi.RSSI(i));
    }
  }

  Serial.printf("\nConnecting to %s\n", WIFI_SSID);

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  int timeout = 30;

  while (WiFi.status() != WL_CONNECTED && timeout--) {
    Serial.print(".");
    delay(1000);
  }

  Serial.println();

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("Connected!");
    Serial.println(WiFi.localIP());
  } else {
    Serial.printf("Failed. Status=%d\n", WiFi.status());
  }
}

void updateLEDs(int status) {
  digitalWrite(LED_GREEN, status == STATUS_AVAILABLE ? HIGH : LOW);
  digitalWrite(LED_RED,   status == STATUS_OCCUPIED  ? HIGH : LOW);
}

void blinkBoth(int times, int delayMs) {
  for (int i = 0; i < times; i++) {
    digitalWrite(LED_GREEN, HIGH); digitalWrite(LED_RED, HIGH); delay(delayMs);
    digitalWrite(LED_GREEN, LOW);  digitalWrite(LED_RED, LOW);  delay(delayMs);
  }
}