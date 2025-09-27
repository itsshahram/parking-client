using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_13_AddPhoneNumberToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerPhoneNumber",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerPhoneNumber",
                table: "Cards");
        }
    }
}
