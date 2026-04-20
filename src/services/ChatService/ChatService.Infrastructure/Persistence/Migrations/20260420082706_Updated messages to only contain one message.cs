using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Updatedmessagestoonlycontainonemessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageInput",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "MessageOutput",
                table: "Messages",
                newName: "Message");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Messages",
                newName: "MessageOutput");

            migrationBuilder.AddColumn<string>(
                name: "MessageInput",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
