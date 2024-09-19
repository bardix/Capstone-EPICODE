using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone_EPICODE.Migrations
{
    /// <inheritdoc />
    public partial class ActiveReservationCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveReservationCount",
                table: "Parkings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveReservationCount",
                table: "Parkings");
        }
    }
}
