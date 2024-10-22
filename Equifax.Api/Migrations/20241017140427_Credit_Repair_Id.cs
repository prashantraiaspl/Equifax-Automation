using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equifax.Api.Migrations
{
    /// <inheritdoc />
    public partial class Credit_Repair_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "credit_repair_id",
                table: "RequestMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credit_repair_id",
                table: "RequestMaster");
        }
    }
}
