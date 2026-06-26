using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patsanstha.Modules.Collections.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "collections");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "collections",
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
                name: "collection_receipts",
                schema: "collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LoanApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CollectedOn = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_collection_receipts", x => x.Id);
                    table.CheckConstraint("ck_collections_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint("ck_collections_status", "\"Status\" IN ('Collected','Reversed')");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "collections",
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
                schema: "collections",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_timestamp",
                schema: "collections",
                table: "audit_logs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_collection_receipts_TenantId_IsDeleted",
                schema: "collections",
                table: "collection_receipts",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ix_collections_tenant_branch_status",
                schema: "collections",
                table: "collection_receipts",
                columns: new[] { "TenantId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_collections_tenant_loan_number",
                schema: "collections",
                table: "collection_receipts",
                columns: new[] { "TenantId", "LoanNumber" });

            migrationBuilder.CreateIndex(
                name: "ix_collections_tenant_member",
                schema: "collections",
                table: "collection_receipts",
                columns: new[] { "TenantId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "ux_collections_tenant_receipt_number",
                schema: "collections",
                table: "collection_receipts",
                columns: new[] { "TenantId", "ReceiptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_created_at",
                schema: "collections",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status",
                schema: "collections",
                table: "outbox_messages",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "collections");

            migrationBuilder.DropTable(
                name: "collection_receipts",
                schema: "collections");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "collections");
        }
    }
}
