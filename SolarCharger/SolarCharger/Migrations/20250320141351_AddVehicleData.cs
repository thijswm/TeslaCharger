using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarCharger.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleDataLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleDataLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleDataLog_ChargeSessions_ChargeSessionId",
                        column: x => x.ChargeSessionId,
                        principalTable: "ChargeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDataLog_ChargeSessionId",
                table: "VehicleDataLog",
                column: "ChargeSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleDataLog");
        }
    }
}
