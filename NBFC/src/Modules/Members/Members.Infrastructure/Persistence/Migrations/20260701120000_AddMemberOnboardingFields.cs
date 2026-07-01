using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Patsanstha.Modules.Members.Infrastructure.Persistence;

#nullable disable

namespace Patsanstha.Modules.Members.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(MembersDbContext))]
    [Migration("20260701120000_AddMemberOnboardingFields")]
    /// <inheritdoc />
    public partial class AddMemberOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_members_tenant_aadhaar_hash",
                schema: "members",
                table: "members");

            migrationBuilder.AlterColumn<string>(
                name: "PanEncrypted",
                schema: "members",
                table: "members",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "AadhaarHash",
                schema: "members",
                table: "members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "AadhaarEncrypted",
                schema: "members",
                table: "members",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "members",
                table: "members",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "AadhaarVerificationStatus",
                schema: "members",
                table: "members",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                schema: "members",
                table: "members",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerName",
                schema: "members",
                table: "members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                schema: "members",
                table: "members",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NomineeAddressSameAsMember",
                schema: "members",
                table: "members",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeAddressLine1",
                schema: "members",
                table: "members",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeAddressLine2",
                schema: "members",
                table: "members",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeCity",
                schema: "members",
                table: "members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NomineeDateOfBirth",
                schema: "members",
                table: "members",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineePinCode",
                schema: "members",
                table: "members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NomineeSharePercent",
                schema: "members",
                table: "members",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "NomineeState",
                schema: "members",
                table: "members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfShares",
                schema: "members",
                table: "members",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                schema: "members",
                table: "members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingStep",
                schema: "members",
                table: "members",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PanVerificationStatus",
                schema: "members",
                table: "members",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "PanVerifiedName",
                schema: "members",
                table: "members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoStorageKey",
                schema: "members",
                table: "members",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShareFaceValue",
                schema: "members",
                table: "members",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<string>(
                name: "SharePaymentMode",
                schema: "members",
                table: "members",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "member_documents",
                schema: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_member_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_member_documents_members_MemberId",
                        column: x => x.MemberId,
                        principalSchema: "members",
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_member_documents_tenant_member_type",
                schema: "members",
                table: "member_documents",
                columns: new[] { "TenantId", "MemberId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_member_documents_MemberId",
                schema: "members",
                table: "member_documents",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "ux_members_tenant_aadhaar_hash",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "AadhaarHash" },
                unique: true,
                filter: "\"AadhaarHash\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member_documents",
                schema: "members");

            migrationBuilder.DropIndex(
                name: "ux_members_tenant_aadhaar_hash",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "AadhaarVerificationStatus",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "EmployerName",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeAddressSameAsMember",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeAddressLine1",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeAddressLine2",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeCity",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeDateOfBirth",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineePinCode",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeSharePercent",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NomineeState",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "NumberOfShares",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "Occupation",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "OnboardingStep",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PanVerificationStatus",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PanVerifiedName",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PhotoStorageKey",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "ShareFaceValue",
                schema: "members",
                table: "members");

            migrationBuilder.DropColumn(
                name: "SharePaymentMode",
                schema: "members",
                table: "members");

            migrationBuilder.AlterColumn<string>(
                name: "PanEncrypted",
                schema: "members",
                table: "members",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AadhaarHash",
                schema: "members",
                table: "members",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AadhaarEncrypted",
                schema: "members",
                table: "members",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "members",
                table: "members",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_members_tenant_aadhaar_hash",
                schema: "members",
                table: "members",
                columns: new[] { "TenantId", "AadhaarHash" },
                unique: true);
        }
    }
}
