using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ErpArchitectureRemediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_TenantId_FinancialYear",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "staff",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "AttendancePercent",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassName",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FeeStatus",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FinancialYear",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentName",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentPhone",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RollNo",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Section",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ChildrenJson",
                schema: "parents",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "parents",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "StudentIdsJson",
                schema: "parents",
                table: "Parents");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "students",
                table: "Students",
                newName: "LifecycleStatus");

            migrationBuilder.RenameIndex(
                name: "IX_Students_TenantId_Status",
                schema: "students",
                table: "Students",
                newName: "IX_Students_TenantId_LifecycleStatus");

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStatus",
                schema: "staff",
                table: "Teachers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AdmissionApplicationId",
                schema: "students",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                schema: "attendance",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "attendance",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStatus",
                schema: "parents",
                table: "Parents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "events",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                schema: "fees",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "fees",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                schema: "admissions",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ApprovedStudentExternalId",
                schema: "admissions",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "admissions",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RegistrationId",
                schema: "admissions",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "admissions",
                table: "Applications",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "tenancy",
                table: "AcademicYears",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateTable(
                name: "AdmissionApprovals",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissionApprovals_Applications_AdmissionApplicationId",
                        column: x => x.AdmissionApplicationId,
                        principalSchema: "admissions",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "tenancy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHeadOffice = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenancy",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchMemberships",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                schema: "students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    RollNo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EnrollmentStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PromotedFromEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionBatches",
                schema: "students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FromAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TotalStudents = table.Column<int>(type: "int", nullable: false),
                    PromotedCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    ExecutedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RolledBackAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSought = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicantFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicantLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicantEmail = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ApplicantPhone = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FormDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentParents",
                schema: "parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentParents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentParents_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAssignments",
                schema: "staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubjectExternalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignmentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalSchema: "staff",
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionBatchItems",
                schema: "students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SkipReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBatchItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionBatchItems_PromotionBatches_PromotionBatchId",
                        column: x => x.PromotionBatchId,
                        principalSchema: "students",
                        principalTable: "PromotionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationDocuments",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    StorageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationDocuments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "admissions",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_RegistrationId",
                schema: "admissions",
                table: "Applications",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TenantId_BranchId_AcademicYearId",
                schema: "admissions",
                table: "Applications",
                columns: new[] { "TenantId", "BranchId", "AcademicYearId" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TenantId_RegistrationId",
                schema: "admissions",
                table: "Applications",
                columns: new[] { "TenantId", "RegistrationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId_Name",
                schema: "tenancy",
                table: "AcademicYears",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionApprovals_AdmissionApplicationId",
                schema: "admissions",
                table: "AdmissionApprovals",
                column: "AdmissionApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_Code",
                schema: "tenancy",
                table: "Branches",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_ExternalId",
                schema: "tenancy",
                table: "Branches",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_IsActive",
                schema: "tenancy",
                table: "Branches",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMemberships_BranchId_UserId",
                schema: "identity",
                table: "BranchMemberships",
                columns: new[] { "BranchId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchMemberships_TenantId_UserId",
                schema: "identity",
                table: "BranchMemberships",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMemberships_UserId",
                schema: "identity",
                table: "BranchMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                schema: "students",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TenantId_AcademicYearId_EnrollmentStatus",
                schema: "students",
                table: "Enrollments",
                columns: new[] { "TenantId", "AcademicYearId", "EnrollmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TenantId_BranchId_AcademicYearId_StudentId",
                schema: "students",
                table: "Enrollments",
                columns: new[] { "TenantId", "BranchId", "AcademicYearId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TenantId_ExternalId",
                schema: "students",
                table: "Enrollments",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBatches_TenantId_BranchId_FromAcademicYearId_ToAcademicYearId",
                schema: "students",
                table: "PromotionBatches",
                columns: new[] { "TenantId", "BranchId", "FromAcademicYearId", "ToAcademicYearId" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBatches_TenantId_ExternalId",
                schema: "students",
                table: "PromotionBatches",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBatchItems_PromotionBatchId_StudentId",
                schema: "students",
                table: "PromotionBatchItems",
                columns: new[] { "PromotionBatchId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDocuments_RegistrationId",
                schema: "admissions",
                table: "RegistrationDocuments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId_ApplicantEmail_ApplicantPhone",
                schema: "admissions",
                table: "Registrations",
                columns: new[] { "TenantId", "ApplicantEmail", "ApplicantPhone" });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId_ExternalId",
                schema: "admissions",
                table: "Registrations",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId_RegistrationNo",
                schema: "admissions",
                table: "Registrations",
                columns: new[] { "TenantId", "RegistrationNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId_Status",
                schema: "admissions",
                table: "Registrations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_ParentId_IsActive",
                schema: "parents",
                table: "StudentParents",
                columns: new[] { "ParentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_StudentId_ParentId_Relationship",
                schema: "parents",
                table: "StudentParents",
                columns: new[] { "StudentId", "ParentId", "Relationship" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_TenantId_ExternalId",
                schema: "parents",
                table: "StudentParents",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TeacherId",
                schema: "staff",
                table: "TeacherAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TenantId_ExternalId",
                schema: "staff",
                table: "TeacherAssignments",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TenantId_TeacherId_AcademicYearId_ClassName_SubjectExternalId",
                schema: "staff",
                table: "TeacherAssignments",
                columns: new[] { "TenantId", "TeacherId", "AcademicYearId", "ClassName", "SubjectExternalId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Registrations_RegistrationId",
                schema: "admissions",
                table: "Applications",
                column: "RegistrationId",
                principalSchema: "admissions",
                principalTable: "Registrations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Registrations_RegistrationId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "AdmissionApprovals",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "tenancy");

            migrationBuilder.DropTable(
                name: "BranchMemberships",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Enrollments",
                schema: "students");

            migrationBuilder.DropTable(
                name: "PromotionBatchItems",
                schema: "students");

            migrationBuilder.DropTable(
                name: "RegistrationDocuments",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "StudentParents",
                schema: "parents");

            migrationBuilder.DropTable(
                name: "TeacherAssignments",
                schema: "staff");

            migrationBuilder.DropTable(
                name: "PromotionBatches",
                schema: "students");

            migrationBuilder.DropTable(
                name: "Registrations",
                schema: "admissions");

            migrationBuilder.DropIndex(
                name: "IX_Applications_RegistrationId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_TenantId_BranchId_AcademicYearId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_TenantId_RegistrationId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId_Name",
                schema: "tenancy",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                schema: "staff",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "AdmissionApplicationId",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                schema: "attendance",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "attendance",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                schema: "parents",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                schema: "fees",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "fees",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ApprovedStudentExternalId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "RegistrationId",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "admissions",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "LifecycleStatus",
                schema: "students",
                table: "Students",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Students_TenantId_LifecycleStatus",
                schema: "students",
                table: "Students",
                newName: "IX_Students_TenantId_Status");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "staff",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AttendancePercent",
                schema: "students",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                schema: "students",
                table: "Students",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeeStatus",
                schema: "students",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialYear",
                schema: "students",
                table: "Students",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                schema: "students",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                schema: "students",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPhone",
                schema: "students",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RollNo",
                schema: "students",
                table: "Students",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                schema: "students",
                table: "Students",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChildrenJson",
                schema: "parents",
                table: "Parents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "parents",
                table: "Parents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentIdsJson",
                schema: "parents",
                table: "Parents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "events",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "tenancy",
                table: "AcademicYears",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_FinancialYear",
                schema: "students",
                table: "Students",
                columns: new[] { "TenantId", "FinancialYear" });
        }
    }
}
