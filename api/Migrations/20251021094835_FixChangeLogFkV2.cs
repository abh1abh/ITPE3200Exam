using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class FixChangeLogFkV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId1",
                table: "ChangeLogs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_AppointmentId1",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "AppointmentId1",
                table: "ChangeLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId1",
                table: "ChangeLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_AppointmentId1",
                table: "ChangeLogs",
                column: "AppointmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId",
                table: "ChangeLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Appointments_AppointmentId1",
                table: "ChangeLogs",
                column: "AppointmentId1",
                principalTable: "Appointments",
                principalColumn: "Id");
        }
    }
}
