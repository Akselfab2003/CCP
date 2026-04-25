using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ChatService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedTicketanalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketAnalysis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    ProblemSummary = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Component = table.Column<string>(type: "text", nullable: true),
                    Symptoms = table.Column<string[]>(type: "text[]", nullable: false),
                    ErrorCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    ProblemAnalysedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReanalysisCount = table.Column<int>(type: "integer", nullable: false),
                    LastReanalysedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RootCause = table.Column<string>(type: "text", nullable: true),
                    SolutionSummary = table.Column<string>(type: "text", nullable: true),
                    SolutionSteps = table.Column<string[]>(type: "text[]", nullable: true),
                    PreventionTips = table.Column<string[]>(type: "text[]", nullable: true),
                    SolutionAnalysedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAnalysis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketEmbedding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemEmbeddingSource = table.Column<string>(type: "text", nullable: false),
                    ProblemVector = table.Column<Vector>(type: "vector", nullable: false),
                    ProblemEmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SolutionEmbeddingSource = table.Column<string>(type: "text", nullable: true),
                    SolutionVector = table.Column<Vector>(type: "vector", nullable: true),
                    SolutionEmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSemanticSearchable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketEmbedding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketEmbedding_TicketAnalysis_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "TicketAnalysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketEmbedding_AnalysisId",
                table: "TicketEmbedding",
                column: "AnalysisId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketEmbedding");

            migrationBuilder.DropTable(
                name: "TicketAnalysis");
        }
    }
}
