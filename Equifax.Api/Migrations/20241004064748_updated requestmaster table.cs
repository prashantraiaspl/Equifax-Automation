using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equifax.Api.Migrations
{
    /// <inheritdoc />
    public partial class updatedrequestmastertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestStatus",
                table: "RequestMaster",
                newName: "request_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "request_status",
                table: "RequestMaster",
                newName: "RequestStatus");
        }
    }
}
