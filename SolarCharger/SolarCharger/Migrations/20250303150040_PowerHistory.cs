using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarCharger.Migrations
{
    /// <inheritdoc />
    public partial class PowerHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChargePowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    CompensatedPower = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargePowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargePowers_ChargeSessions_ChargeSessionId",
                        column: x => x.ChargeSessionId,
                        principalTable: "ChargeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChargePowers_ChargeSessionId",
                table: "ChargePowers",
                column: "ChargeSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargePowers");
        }
    }
}
