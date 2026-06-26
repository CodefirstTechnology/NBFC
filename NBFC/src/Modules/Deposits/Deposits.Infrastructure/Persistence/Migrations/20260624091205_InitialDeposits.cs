using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patsanstha.Modules.Deposits.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deposits");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "deposits",
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
                name: "deposit_accounts",
                schema: "deposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TenureMonths = table.Column<int>(type: "integer", nullable: true),
                    InterestPayoutMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AutoRenewal = table.Column<bool>(type: "boolean", nullable: false),
                    OpenedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_deposit_accounts", x => x.Id);
                    table.CheckConstraint("ck_deposits_balance_non_negative", "\"CurrentBalance\" >= 0");
                    table.CheckConstraint("ck_deposits_principal_positive", "\"PrincipalAmount\" > 0");
                    table.CheckConstraint("ck_deposits_status", "\"Status\" IN ('Active','Matured','Closed','PrematureClosed')");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "deposits",
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
                schema: "deposits",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_timestamp",
                schema: "deposits",
                table: "audit_logs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_deposit_accounts_TenantId_IsDeleted",
                schema: "deposits",
                table: "deposit_accounts",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ix_deposits_tenant_branch_status",
                schema: "deposits",
                table: "deposit_accounts",
                columns: new[] { "TenantId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_deposits_tenant_member",
                schema: "deposits",
                table: "deposit_accounts",
                columns: new[] { "TenantId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "ix_deposits_tenant_member_name",
                schema: "deposits",
                table: "deposit_accounts",
                columns: new[] { "TenantId", "MemberName" });

            migrationBuilder.CreateIndex(
                name: "ux_deposits_tenant_account_number",
                schema: "deposits",
                table: "deposit_accounts",
                columns: new[] { "TenantId", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_created_at",
                schema: "deposits",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status",
                schema: "deposits",
                table: "outbox_messages",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "deposits");

            migrationBuilder.DropTable(
                name: "deposit_accounts",
                schema: "deposits");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "deposits");
        }
    }
}
