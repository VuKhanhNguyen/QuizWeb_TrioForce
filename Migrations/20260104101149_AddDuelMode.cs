using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWeb_TrioForce.Migrations
{
    /// <inheritdoc />
    public partial class AddDuelMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DuelMatches",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MatchCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QSetId = table.Column<int>(type: "int", nullable: false),
                    Player1UserName = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Player2UserName = table.Column<string>(type: "varchar(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Player1Score = table.Column<int>(type: "int", nullable: false),
                    Player2Score = table.Column<int>(type: "int", nullable: false),
                    WinnerUserName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentQuestionIndex = table.Column<int>(type: "int", nullable: false),
                    Player1DisconnectedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Player2DisconnectedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelMatches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_DuelMatches_AspNetUsers_Player1UserName",
                        column: x => x.Player1UserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuelMatches_AspNetUsers_Player2UserName",
                        column: x => x.Player2UserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuelMatches_QuestionSets_QSetId",
                        column: x => x.QSetId,
                        principalTable: "QuestionSets",
                        principalColumn: "QSetId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuelRankings",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalMatches = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    Draws = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelRankings", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_DuelRankings_AspNetUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuelAnswers",
                columns: table => new
                {
                    DuelAnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswerId = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResponseTimeSeconds = table.Column<double>(type: "double", nullable: false),
                    ScoreEarned = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelAnswers", x => x.DuelAnswerId);
                    table.ForeignKey(
                        name: "FK_DuelAnswers_Answers_SelectedAnswerId",
                        column: x => x.SelectedAnswerId,
                        principalTable: "Answers",
                        principalColumn: "AnswerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuelAnswers_AspNetUsers_UserName",
                        column: x => x.UserName,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuelAnswers_DuelMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "DuelMatches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DuelAnswers_MatchId",
                table: "DuelAnswers",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelAnswers_QuestionId",
                table: "DuelAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelAnswers_SelectedAnswerId",
                table: "DuelAnswers",
                column: "SelectedAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelAnswers_UserName",
                table: "DuelAnswers",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_MatchCode",
                table: "DuelMatches",
                column: "MatchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_Player1UserName",
                table: "DuelMatches",
                column: "Player1UserName");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_Player2UserName",
                table: "DuelMatches",
                column: "Player2UserName");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_QSetId",
                table: "DuelMatches",
                column: "QSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuelAnswers");

            migrationBuilder.DropTable(
                name: "DuelRankings");

            migrationBuilder.DropTable(
                name: "DuelMatches");
        }
    }
}
