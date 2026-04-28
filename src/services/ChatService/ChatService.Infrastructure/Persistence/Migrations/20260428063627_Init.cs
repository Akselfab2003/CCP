using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace ChatService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedTicketId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaqEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                });

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
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsFromUser = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

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
                name: "DomainDetails");

            migrationBuilder.DropTable(
                name: "FaqEntries");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "TicketEmbedding");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "TicketAnalysis");
        }
    }
}
