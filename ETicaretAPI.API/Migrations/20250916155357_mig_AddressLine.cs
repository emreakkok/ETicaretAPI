using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretAPI.API.Migrations
{
    /// <inheritdoc />
    public partial class mig_AddressLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdrressLine",
                table: "Orders",
                newName: "AddressLine");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressLine",
                table: "Orders",
                newName: "AdrressLine");
        }
    }
}
