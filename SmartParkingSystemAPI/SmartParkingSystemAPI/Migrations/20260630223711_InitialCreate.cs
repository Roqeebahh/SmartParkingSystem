using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartParkingSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParkingMonitoringEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpaceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasSensor = table.Column<bool>(type: "bit", nullable: false),
                    SensorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FacultyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSpaces_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Description", "Latitude", "Longitude", "Name", "ParkingMonitoringEnabled" },
                values: new object[,]
                {
                    { 1, "Humanities, languages and creative arts studies.", 6.4642150000000003, 3.2019380000000002, "Faculty of Arts", false },
                    { 2, "Teacher training and educational programmes.", 6.4730189999999999, 3.1998769999999999, "Faculty of Education", false },
                    { 3, "Legal studies and jurisprudence.", 6.4675089999999997, 3.2019600000000001, "Faculty of Law", false },
                    { 4, "Business, accounting and management studies.", 6.4760429999999998, 3.199862, "Faculty of Management Sciences", false },
                    { 5, "Pure and applied science programmes.", 6.4662920000000002, 3.2000510000000002, "Faculty of Science", false },
                    { 6, "Economics, sociology and political science.", 6.4753439999999998, 3.1992069999999999, "Faculty of Social Sciences", false },
                    { 7, "Computing, IT and software engineering hub.", 6.4729640000000002, 3.199897, "Faculty of Computing and Information Technology", true },
                    { 8, "Architecture, urban planning and surveying.", 6.4672939999999999, 3.2002510000000002, "Faculty of Environmental Sciences", false },
                    { 9, "Mass communication and media studies.", 6.4682000000000004, 3.1808000000000001, "School of Communication", false },
                    { 10, "Transportation management and logistics.", 6.4744619999999999, 3.1979950000000001, "School of Transport and Logistics", false },
                    { 11, "Library science and archival management.", 6.4649999999999999, 3.2006320000000001, "School of Library, Archival and Information Science", false },
                    { 12, "Creative arts, tourism and cultural studies.", 6.4642999999999997, 3.1855000000000002, "School of Creativity, Culture and Tourism Studies", false },
                    { 13, "Advanced degrees and research programmes.", 6.4685879999999996, 3.2012339999999999, "Postgraduate School", false }
                });

            migrationBuilder.InsertData(
                table: "ParkingSpaces",
                columns: new[] { "Id", "FacultyId", "HasSensor", "LastUpdated", "SensorId", "SpaceNumber", "Status" },
                values: new object[,]
                {
                    { 1, 1, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ART-01", 2 },
                    { 2, 1, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ART-02", 2 },
                    { 3, 1, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ART-03", 2 },
                    { 4, 1, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ART-04", 2 },
                    { 5, 1, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ART-05", 2 },
                    { 6, 2, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "EDU-01", 2 },
                    { 7, 2, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "EDU-02", 2 },
                    { 8, 2, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "EDU-03", 2 },
                    { 9, 2, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "EDU-04", 2 },
                    { 10, 2, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "EDU-05", 2 },
                    { 11, 3, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LAW-01", 2 },
                    { 12, 3, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LAW-02", 2 },
                    { 13, 3, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LAW-03", 2 },
                    { 14, 3, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LAW-04", 2 },
                    { 15, 3, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LAW-05", 2 },
                    { 16, 4, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MGT-01", 2 },
                    { 17, 4, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MGT-02", 2 },
                    { 18, 4, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MGT-03", 2 },
                    { 19, 4, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MGT-04", 2 },
                    { 20, 4, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MGT-05", 2 },
                    { 21, 5, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SCI-01", 2 },
                    { 22, 5, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SCI-02", 2 },
                    { 23, 5, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SCI-03", 2 },
                    { 24, 5, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SCI-04", 2 },
                    { 25, 5, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SCI-05", 2 },
                    { 26, 6, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SOC-01", 2 },
                    { 27, 6, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SOC-02", 2 },
                    { 28, 6, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SOC-03", 2 },
                    { 29, 6, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SOC-04", 2 },
                    { 30, 6, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "SOC-05", 2 },
                    { 31, 7, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESP32-CIT-01", "CIT-01", 0 },
                    { 32, 7, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESP32-CIT-02", "CIT-02", 0 },
                    { 33, 7, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESP32-CIT-03", "CIT-03", 0 },
                    { 34, 7, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESP32-CIT-04", "CIT-04", 0 },
                    { 35, 7, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESP32-CIT-05", "CIT-05", 0 },
                    { 36, 8, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ENV-01", 2 },
                    { 37, 8, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ENV-02", 2 },
                    { 38, 8, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ENV-03", 2 },
                    { 39, 8, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ENV-04", 2 },
                    { 40, 8, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ENV-05", 2 },
                    { 41, 9, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "COM-01", 2 },
                    { 42, 9, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "COM-02", 2 },
                    { 43, 9, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "COM-03", 2 },
                    { 44, 9, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "COM-04", 2 },
                    { 45, 9, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "COM-05", 2 },
                    { 46, 10, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "TRL-01", 2 },
                    { 47, 10, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "TRL-02", 2 },
                    { 48, 10, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "TRL-03", 2 },
                    { 49, 10, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "TRL-04", 2 },
                    { 50, 10, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "TRL-05", 2 },
                    { 51, 11, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LIB-01", 2 },
                    { 52, 11, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LIB-02", 2 },
                    { 53, 11, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LIB-03", 2 },
                    { 54, 11, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LIB-04", 2 },
                    { 55, 11, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LIB-05", 2 },
                    { 56, 12, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "CCT-01", 2 },
                    { 57, 12, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "CCT-02", 2 },
                    { 58, 12, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "CCT-03", 2 },
                    { 59, 12, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "CCT-04", 2 },
                    { 60, 12, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "CCT-05", 2 },
                    { 61, 13, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PGS-01", 2 },
                    { 62, 13, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PGS-02", 2 },
                    { 63, 13, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PGS-03", 2 },
                    { 64, 13, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PGS-04", 2 },
                    { 65, 13, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PGS-05", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpaces_FacultyId",
                table: "ParkingSpaces",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpaces_SpaceNumber",
                table: "ParkingSpaces",
                column: "SpaceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSpaces");

            migrationBuilder.DropTable(
                name: "Faculties");
        }
    }
}
