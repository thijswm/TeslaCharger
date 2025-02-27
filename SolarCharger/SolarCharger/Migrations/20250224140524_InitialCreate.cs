using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarCharger.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChargeSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BatteryLevelStarted = table.Column<int>(type: "INTEGER", nullable: false),
                    BatteryLevelEnded = table.Column<int>(type: "INTEGER", nullable: true),
                    EnergyAdded = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    HomeWizardAddress = table.Column<string>(type: "TEXT", nullable: false),
                    SolarMovingAverage = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EnoughSolarWatt = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumAmp = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumAmp = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfPhases = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumChargeDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    MinimumCurrentDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TeslaFleetAddress = table.Column<string>(type: "TEXT", nullable: false),
                    TeslaCommandsAddress = table.Column<string>(type: "TEXT", nullable: false),
                    TeslaRefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    TeslaClientId = table.Column<string>(type: "TEXT", nullable: false),
                    Vin = table.Column<string>(type: "TEXT", nullable: false),
                    PollTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    MinimumInitialChargeDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChargeCurrentChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Current = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeCurrentChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeCurrentChanges_ChargeSessions_ChargeSessionId",
                        column: x => x.ChargeSessionId,
                        principalTable: "ChargeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeCurrentChanges_ChargeSessionId",
                table: "ChargeCurrentChanges",
                column: "ChargeSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeCurrentChanges");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "ChargeSessions");
        }
    }
}
