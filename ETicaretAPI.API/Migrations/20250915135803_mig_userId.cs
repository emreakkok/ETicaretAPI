using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretAPI.API.Migrations
{
    /// <inheritdoc />
    public partial class mig_userId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Carts",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Carts",
                newName: "CustomerId");
        }
    }
}
