using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patsanstha.Modules.Recovery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recovery");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "recovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "recovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recovery_cases",
                schema: "recovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DaysPastDue = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ResolvedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_cases", x => x.Id);
                    table.CheckConstraint("ck_recovery_days_past_due", "\"DaysPastDue\" >= 0");
                    table.CheckConstraint("ck_recovery_outstanding_positive", "\"OutstandingAmount\" > 0");
                    table.CheckConstraint("ck_recovery_status", "\"Status\" IN ('Open','InProgress','Resolved','WrittenOff')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "recovery",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_timestamp",
                schema: "recovery",
                table: "audit_logs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_created_at",
                schema: "recovery",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status",
                schema: "recovery",
                table: "outbox_messages",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recovery_cases_TenantId_IsDeleted",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ix_recovery_tenant_assigned_user",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "AssignedToUserId" });

            migrationBuilder.CreateIndex(
                name: "ix_recovery_tenant_branch_status",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_recovery_tenant_loan_application",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "LoanApplicationId" });

            migrationBuilder.CreateIndex(
                name: "ix_recovery_tenant_member",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "ux_recovery_tenant_case_number",
                schema: "recovery",
                table: "recovery_cases",
                columns: new[] { "TenantId", "CaseNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "recovery");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "recovery");

            migrationBuilder.DropTable(
                name: "recovery_cases",
                schema: "recovery");
        }
    }
}
