using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdToDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    AuthUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "HealthcareWorkers",
                columns: table => new
                {
                    HealthcareWorkerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    AuthUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthcareWorkers", x => x.HealthcareWorkerId);
                });

            migrationBuilder.CreateTable(
                name: "AvailableSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HealthcareWorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsBooked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableSlots_HealthcareWorkers_HealthcareWorkerId",
                        column: x => x.HealthcareWorkerId,
                        principalTable: "HealthcareWorkers",
                        principalColumn: "HealthcareWorkerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    HealthcareWorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AvailableSlotId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AvailableSlots_AvailableSlotId",
                        column: x => x.AvailableSlotId,
                        principalTable: "AvailableSlots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_HealthcareWorkers_HealthcareWorkerId",
                        column: x => x.HealthcareWorkerId,
                        principalTable: "HealthcareWorkers",
                        principalColumn: "HealthcareWorkerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentTasks_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    ChangeDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AppointmentId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLogs_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChangeLogs_Appointments_AppointmentId1",
                        column: x => x.AppointmentId1,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailableSlotId",
                table: "Appointments",
                column: "AvailableSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClientId",
                table: "Appointments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_HealthcareWorkerId",
                table: "Appointments",
                column: "HealthcareWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTasks_AppointmentId",
                table: "AppointmentTasks",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_HealthcareWorkerId",
                table: "AvailableSlots",
                column: "HealthcareWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_AppointmentId",
                table: "ChangeLogs",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_AppointmentId1",
                table: "ChangeLogs",
                column: "AppointmentId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentTasks");

            migrationBuilder.DropTable(
                name: "ChangeLogs");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AvailableSlots");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "HealthcareWorkers");
        }
    }
}
