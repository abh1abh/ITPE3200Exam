using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClientIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Clients",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "AvailableSlotId",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments",
                column: "AvailableSlotId",
                principalTable: "AvailableSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Clients",
                newName: "ClientId");

            migrationBuilder.AlterColumn<int>(
                name: "AvailableSlotId",
                table: "Appointments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                table: "Appointments",
                column: "AvailableSlotId",
                principalTable: "AvailableSlots",
                principalColumn: "Id");
        }
    }
}
