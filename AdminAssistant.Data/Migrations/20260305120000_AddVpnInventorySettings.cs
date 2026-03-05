using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminAssistant.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVpnInventorySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VpnInventorySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalLicenses = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnInventorySettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "VpnInventorySettings",
                columns: new[] { "Id", "TotalLicenses" },
                values: new object[] { 1, 150 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VpnInventorySettings");
        }
    }
}
