using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class Addpriorityappdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClientCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.InsertData(
                table: "Priorities",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "Urgent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.InsertData(
                table: "ClientCategories",
                columns: new[] { "Id", "ClientType" },
                values: new object[] { 3, "Not Sure" });
        }
    }
}
