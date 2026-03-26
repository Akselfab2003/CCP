using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessagingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInternalNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInternalNote",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInternalNote",
                table: "Messages");
        }
    }
}
