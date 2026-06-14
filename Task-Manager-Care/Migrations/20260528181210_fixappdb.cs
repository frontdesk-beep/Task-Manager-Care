using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class fixappdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tasks_TaskItemId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TaskItemId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TaskId",
                table: "Notifications",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tasks_TaskId",
                table: "Notifications",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tasks_TaskId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TaskId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "TaskItemId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TaskItemId",
                table: "Notifications",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tasks_TaskItemId",
                table: "Notifications",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
