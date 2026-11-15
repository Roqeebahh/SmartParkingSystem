const SignalRClient = (() => {
  'use strict';

  const HUB_URL = 'http://192.168.197.170:5062/parkinghub';
  //const HUB_URL = 'https://localhost:7240/parkinghub';
  let connection = null;
  let reconnectAttempts = 0;
  const MAX_RECONNECT = 10;

  function setStatus(state, label) {
    const dot   = document.getElementById('connDot');
    const lbl   = document.getElementById('connLabel');
    const badge = document.getElementById('connectionBadge');
    if (!dot || !lbl) return;
    dot.className   = `conn-dot ${state}`;
    lbl.textContent = label;
    badge.title     = `SignalR: ${label}`;
  }

  function buildConnection() {
    return new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount < 3) return 2000;
          if (retryContext.previousRetryCount < 6) return 5000;
          return 15000;
        }
      })
      .configureLogging(signalR.LogLevel.Debug)  // ← Debug so we see everything
      .build();
  }

  function registerHandlers(conn) {
    conn.on('ReceiveParkingUpdate', update => {
      console.log('[SignalR] ReceiveParkingUpdate received:', update);

      const statusMap = { 
        'Available': 0, 
        'Occupied':  1, 
        'Inactive':  2 
      };

      const enriched = {
        spaceNumber: update.spaceNumber,
        facultyId:   update.facultyId,
        status:      update.status,
        statusCode:  statusMap[update.status] ?? 2,
        lastUpdated: update.lastUpdated
      };

      console.log('[SignalR] Enriched update:', enriched);

      if (window.App) {
        App.updateSpaceChip(enriched);
        App.showToast(
          `${update.spaceNumber}: ${update.status}`,
          update.status === 'Available' ? 'success' : 'info',
          2500
        );
      }
    });

    conn.on('ReceiveAllSpaces', spaces => {
      console.log('[SignalR] ReceiveAllSpaces received:', spaces.length);
      if (!window.App) return;
      const statusMap = { 'Available': 0, 'Occupied': 1, 'Inactive': 2 };
      spaces.forEach(s => {
        App.updateSpaceChip({
          spaceNumber: s.spaceNumber,
          facultyId:   s.facultyId,
          status:      s.status,
          statusCode:  statusMap[s.status] ?? 2,
          lastUpdated: s.lastUpdated
        });
      });
    });
  }

  async function start() {
    connection = buildConnection();
    registerHandlers(connection);

    connection.onreconnecting(() => {
      setStatus('', 'Reconnecting…');
      if (window.App) App.showToast('Connection lost – reconnecting…', 'error', 4000);
    });

    connection.onreconnected(async () => {
      setStatus('connected', 'Connected');
      reconnectAttempts = 0;
      if (window.App) App.showToast('Reconnected to live updates', 'success', 2500);
      await joinGroup();
    });

    connection.onclose(async () => {
      setStatus('disconnected', 'Disconnected');
      if (reconnectAttempts < MAX_RECONNECT) {
        const delay = Math.min(2000 * Math.pow(1.5, reconnectAttempts), 30000);
        reconnectAttempts++;
        setTimeout(() => startConnection(), delay);
      }
    });

    await startConnection();
  }

  async function startConnection() {
    try {
      setStatus('', 'Connecting…');
      await connection.start();
      setStatus('connected', 'Live');
      reconnectAttempts = 0;
      await joinGroup();
    } catch (err) {
      console.warn('[SignalR] Connection failed:', err.message);
      setStatus('disconnected', 'Offline');
      const delay = Math.min(3000 * Math.pow(1.5, reconnectAttempts), 60000);
      reconnectAttempts++;
      setTimeout(() => startConnection(), delay);
    }
  }

  async function joinGroup() {
    try {
      await connection.invoke('JoinParkingMonitor');
      console.log('[SignalR] Successfully joined ParkingMonitors group.');
    } catch (err) {
      console.error('[SignalR] JoinParkingMonitor failed:', err);
    }
  }

  return { start };
})();

document.addEventListener('DOMContentLoaded', () => {
  if (typeof signalR !== 'undefined') {
    SignalRClient.start();
  } else {
    document.getElementById('connLabel').textContent = 'Offline';
  }
});