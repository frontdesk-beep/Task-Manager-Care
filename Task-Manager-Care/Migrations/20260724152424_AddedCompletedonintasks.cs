using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class AddedCompletedonintasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedOn",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedOn",
                table: "Tasks");
        }
    }
}
