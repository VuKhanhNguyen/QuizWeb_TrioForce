using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWeb_TrioForce.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCompletedToPQS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ProgressQuestionSets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ProgressQuestionSets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ProgressQuestionSets");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ProgressQuestionSets");
        }
    }
}
