using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class updatedlogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStatusId",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "OldStatusId",
                table: "TaskHistories");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "TaskHistories",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "TaskHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_ChangedById",
                table: "TaskHistories",
                column: "ChangedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_Users_ChangedById",
                table: "TaskHistories",
                column: "ChangedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_Users_ChangedById",
                table: "TaskHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistories_ChangedById",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "TaskHistories");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TaskHistories",
                newName: "Note");

            migrationBuilder.AddColumn<int>(
                name: "NewStatusId",
                table: "TaskHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldStatusId",
                table: "TaskHistories",
                type: "int",
                nullable: true);
        }
    }
}
