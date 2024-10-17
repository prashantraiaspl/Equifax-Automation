using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equifax.Api.Migrations
{
    /// <inheritdoc />
    public partial class editedmasterrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "confirmation_number",
                table: "RequestMaster",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "creditor_name",
                table: "RequestMaster",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "open_date",
                table: "RequestMaster",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmation_number",
                table: "RequestMaster");

            migrationBuilder.DropColumn(
                name: "creditor_name",
                table: "RequestMaster");

            migrationBuilder.DropColumn(
                name: "open_date",
                table: "RequestMaster");
        }
    }
}
