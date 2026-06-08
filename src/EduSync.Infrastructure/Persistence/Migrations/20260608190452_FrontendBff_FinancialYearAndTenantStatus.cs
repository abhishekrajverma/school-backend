using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FrontendBff_FinancialYearAndTenantStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchoolEmail",
                schema: "tenancy",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialYear",
                schema: "students",
                table: "Students",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "2024-25");

            migrationBuilder.AddColumn<string>(
                name: "FinancialYear",
                schema: "attendance",
                table: "Records",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "2024-25");

            migrationBuilder.AddColumn<string>(
                name: "FinancialYear",
                schema: "fees",
                table: "Invoices",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "2024-25");

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_FinancialYear",
                schema: "students",
                table: "Students",
                columns: new[] { "TenantId", "FinancialYear" });

            migrationBuilder.CreateIndex(
                name: "IX_Records_TenantId_FinancialYear",
                schema: "attendance",
                table: "Records",
                columns: new[] { "TenantId", "FinancialYear" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId_FinancialYear",
                schema: "fees",
                table: "Invoices",
                columns: new[] { "TenantId", "FinancialYear" });

            migrationBuilder.Sql("""
                UPDATE tenancy.Tenants
                SET SchoolEmail = 'admin@school.edu'
                WHERE ExternalId = 'demo-school-001' AND SchoolEmail IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_TenantId_FinancialYear",
                schema: "students",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Records_TenantId_FinancialYear",
                schema: "attendance",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TenantId_FinancialYear",
                schema: "fees",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SchoolEmail",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FinancialYear",
                schema: "students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FinancialYear",
                schema: "attendance",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "FinancialYear",
                schema: "fees",
                table: "Invoices");
        }
    }
}
