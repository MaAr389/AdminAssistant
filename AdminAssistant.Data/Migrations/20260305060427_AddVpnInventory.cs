using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminAssistant.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVpnInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VpnAccessCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AssignedAdUser = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Pin = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnAccessCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VpnSmartcardReaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AssignedAdUser = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnSmartcardReaders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VpnAccessCards_CardNumber",
                table: "VpnAccessCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VpnSmartcardReaders_SerialNumber",
                table: "VpnSmartcardReaders",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VpnAccessCards");

            migrationBuilder.DropTable(
                name: "VpnSmartcardReaders");
        }
    }
}
