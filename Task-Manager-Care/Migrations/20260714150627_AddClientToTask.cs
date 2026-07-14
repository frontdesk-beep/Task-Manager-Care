using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Manager_Care.Migrations
{
    /// <inheritdoc />
    public partial class AddClientToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ClientId",
                table: "Tasks",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Clients_ClientId",
                table: "Tasks",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Clients_ClientId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ClientId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Tasks");
        }
    }
}
