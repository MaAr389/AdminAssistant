using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminAssistant.Data.Migrations
{
    public partial class AddOuPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OuPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Area = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DistinguishedName = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OuPermissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OuPermissions_Area_DistinguishedName",
                table: "OuPermissions",
                columns: new[] { "Area", "DistinguishedName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OuPermissions");
        }
    }
}
