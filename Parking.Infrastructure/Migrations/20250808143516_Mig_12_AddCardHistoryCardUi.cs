using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_12_AddCardHistoryCardUi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CardUid",
                table: "AddCardItems",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardUid",
                table: "AddCardItems");
        }
    }
}
