using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentIdSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "ChangeLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentIdSnapshot",
                table: "ChangeLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "AppointmentIdSnapshot",
                table: "ChangeLogs");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "ChangeLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
