using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.Data.Migrations
{
    public partial class ForumMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cohort_CohortId",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cohort",
                table: "Cohort");

            migrationBuilder.RenameTable(
                name: "Cohort",
                newName: "Cohorts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cohorts",
                table: "Cohorts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Forums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Create_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CohortId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    Create_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForumId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cohorts_CohortId",
                table: "AspNetUsers",
                column: "CohortId",
                principalTable: "Cohorts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cohorts_CohortId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Forums");

            migrationBuilder.DropTable(
                name: "PostImages");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cohorts",
                table: "Cohorts");

            migrationBuilder.RenameTable(
                name: "Cohorts",
                newName: "Cohort");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cohort",
                table: "Cohort",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cohort_CohortId",
                table: "AspNetUsers",
                column: "CohortId",
                principalTable: "Cohort",
                principalColumn: "Id");
        }
    }
}
