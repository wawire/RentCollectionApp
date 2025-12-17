using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCollection.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRentReminderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReminderSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LandlordId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultChannel = table.Column<int>(type: "int", nullable: false),
                    SevenDaysBeforeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ThreeDaysBeforeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OneDayBeforeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OnDueDateEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OneDayOverdueEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ThreeDaysOverdueEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SevenDaysOverdueEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SevenDaysBeforeTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ThreeDaysBeforeTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OneDayBeforeTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OnDueDateTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OneDayOverdueTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ThreeDaysOverdueTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SevenDaysOverdueTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReminderSettings_Users_LandlordId",
                        column: x => x.LandlordId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LandlordId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ReminderType = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RentDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MessageTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MessageSent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SmsMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastRetryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentReminders_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RentReminders_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RentReminders_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RentReminders_Users_LandlordId",
                        column: x => x.LandlordId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantReminderPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RemindersEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredChannel = table.Column<int>(type: "int", nullable: false),
                    OptOutSevenDaysBefore = table.Column<bool>(type: "bit", nullable: false),
                    OptOutThreeDaysBefore = table.Column<bool>(type: "bit", nullable: false),
                    OptOutOneDayBefore = table.Column<bool>(type: "bit", nullable: false),
                    OptOutOnDueDate = table.Column<bool>(type: "bit", nullable: false),
                    OptOutOverdueReminders = table.Column<bool>(type: "bit", nullable: false),
                    AlternatePhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AlternateEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantReminderPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantReminderPreferences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReminderSettings_LandlordId",
                table: "ReminderSettings",
                column: "LandlordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_LandlordId",
                table: "RentReminders",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_PropertyId",
                table: "RentReminders",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_ScheduledDate",
                table: "RentReminders",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_Status",
                table: "RentReminders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_TenantId",
                table: "RentReminders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_TenantId_RentDueDate",
                table: "RentReminders",
                columns: new[] { "TenantId", "RentDueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RentReminders_UnitId",
                table: "RentReminders",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantReminderPreferences_TenantId",
                table: "TenantReminderPreferences",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderSettings");

            migrationBuilder.DropTable(
                name: "RentReminders");

            migrationBuilder.DropTable(
                name: "TenantReminderPreferences");
        }
    }
}
