using Microsoft.EntityFrameworkCore.Migrations;

namespace TitanTracker.Data.Migrations
{
    public partial class ProjectStatusPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectPriorities_ProjectPriorityId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectPriorityId",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "ProjectPriority",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectStatus",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectPriority",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectStatus",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectPriorityId",
                table: "Projects",
                column: "ProjectPriorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectPriorities_ProjectPriorityId",
                table: "Projects",
                column: "ProjectPriorityId",
                principalTable: "ProjectPriorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
