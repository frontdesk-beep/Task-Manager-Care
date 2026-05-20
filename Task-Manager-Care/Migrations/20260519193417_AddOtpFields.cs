using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetTokenExpiry",
                table: "Users",
                newName: "OtpExpiry");

            migrationBuilder.RenameColumn(
                name: "ResetToken",
                table: "Users",
                newName: "OtpCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OtpExpiry",
                table: "Users",
                newName: "ResetTokenExpiry");

            migrationBuilder.RenameColumn(
                name: "OtpCode",
                table: "Users",
                newName: "ResetToken");
        }
    }
}
