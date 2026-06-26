using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patsanstha.Modules.Members.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "members");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "members",
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
                name: "members",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MobileNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AddressLine1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AadhaarEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PanEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AadhaarHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NomineeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NomineeRelation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    JoinedOn = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_members", x => x.Id);
                    table.CheckConstraint("ck_members_status", "\"Status\" IN ('Pending','Active','Inactive','Suspended','Closed')");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "members",
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
                schema: "members",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_timestamp",
                schema: "members",
                table: "audit_logs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_members_tenant_branch_status",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_members_tenant_full_name",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_members_TenantId_IsDeleted",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ux_members_tenant_aadhaar_hash",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "AadhaarHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_members_tenant_member_number",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "MemberNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_created_at",
                schema: "members",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_status",
                schema: "members",
                table: "outbox_messages",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "members");

            migrationBuilder.DropTable(
                name: "members",
                schema: "members");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "members");
        }
    }
}
