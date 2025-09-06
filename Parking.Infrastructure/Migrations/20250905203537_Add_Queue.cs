using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Queue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddCardItems");

            migrationBuilder.AddColumn<int>(
                name: "TicketDescriptionItemId",
                table: "ParkingTickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketDescriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsQueueEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDescriptionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketDescriptionItemId = table.Column<int>(type: "int", nullable: false),
                    QueueNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParkingTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketQueueItems_ParkingTickets_ParkingTicketId",
                        column: x => x.ParkingTicketId,
                        principalTable: "ParkingTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketQueueItems_TicketDescriptionItems_TicketDescriptionItemId",
                        column: x => x.TicketDescriptionItemId,
                        principalTable: "TicketDescriptionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketQueueResetPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketDescriptionItemId = table.Column<int>(type: "int", nullable: false),
                    ResetIntervalDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketQueueResetPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketQueueResetPolicies_TicketDescriptionItems_TicketDescriptionItemId",
                        column: x => x.TicketDescriptionItemId,
                        principalTable: "TicketDescriptionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_TicketDescriptionItemId",
                table: "ParkingTickets",
                column: "TicketDescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlates_GroupId",
                table: "LicensePlates",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQueueItems_ParkingTicketId",
                table: "TicketQueueItems",
                column: "ParkingTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQueueItems_TicketDescriptionItemId",
                table: "TicketQueueItems",
                column: "TicketDescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketQueueResetPolicies_TicketDescriptionItemId",
                table: "TicketQueueResetPolicies",
                column: "TicketDescriptionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicensePlates_LicensePlateGroups_GroupId",
                table: "LicensePlates",
                column: "GroupId",
                principalTable: "LicensePlateGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingTickets_TicketDescriptionItems_TicketDescriptionItemId",
                table: "ParkingTickets",
                column: "TicketDescriptionItemId",
                principalTable: "TicketDescriptionItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicensePlates_LicensePlateGroups_GroupId",
                table: "LicensePlates");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingTickets_TicketDescriptionItems_TicketDescriptionItemId",
                table: "ParkingTickets");

            migrationBuilder.DropTable(
                name: "TicketQueueItems");

            migrationBuilder.DropTable(
                name: "TicketQueueResetPolicies");

            migrationBuilder.DropTable(
                name: "TicketDescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_TicketDescriptionItemId",
                table: "ParkingTickets");

            migrationBuilder.DropIndex(
                name: "IX_LicensePlates_GroupId",
                table: "LicensePlates");

            migrationBuilder.DropColumn(
                name: "TicketDescriptionItemId",
                table: "ParkingTickets");

            migrationBuilder.CreateTable(
                name: "AddCardItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardUid = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnLicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentDiscount = table.Column<int>(type: "int", nullable: false),
                    VehicleSegmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddCardItems", x => x.Id);
                });
        }
    }
}
