using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RockoCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Tenants",
                newName: "SubscriptionEndDate");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "SubscriptionEndDate",
                table: "Tenants",
                newName: "CreatedAt");
        }
    }
}
