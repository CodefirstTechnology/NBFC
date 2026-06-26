using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patsanstha.Modules.Loans.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "loans");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "loans",
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
                name: "loan_applications",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InterestRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TenureMonths = table.Column<int>(type: "integer", nullable: false),
                    EmiAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OutstandingPrincipal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppliedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ApprovedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    DisbursedOn = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_loan_applications", x => x.Id);
                    table.CheckConstraint("ck_loans_requested_positive", "\"RequestedAmount\" > 0");
                    table.CheckConstraint("ck_loans_status", "\"Status\" IN ('Submitted','UnderReview','Approved','Rejected','Disbursed','Closed','Npa')");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "loans",
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

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "loans",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_timestamp",
                schema: "loans",
                table: "audit_logs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_TenantId_IsDeleted",
                schema: "loans",
                table: "loan_applications",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ix_loans_tenant_branch_status",
                schema: "loans",
                table: "loan_applications",
                columns: new[] { "TenantId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_loans_tenant_member",
                schema: "loans",
                table: "loan_applications",
                columns: new[] { "TenantId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "ux_loans_tenant_loan_number",
                schema: "loans",
                table: "loan_applications",
                columns: new[] { "TenantId", "LoanNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_created_at",
                schema: "loans",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status",
                schema: "loans",
                table: "outbox_messages",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "loan_applications",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "loans");
        }
    }
}
