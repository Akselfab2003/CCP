using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTenantEmailConfigurationtoincludeInternalEmailandInternalEmailPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "TenantEmailConfigurations",
                newName: "InternalEmailPassword");

            migrationBuilder.AddColumn<string>(
                name: "InternalEmail",
                table: "TenantEmailConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "EmailSent",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailId",
                table: "EmailReceived",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalEmail",
                table: "TenantEmailConfigurations");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "EmailSent");

            migrationBuilder.DropColumn(
                name: "MailId",
                table: "EmailReceived");

            migrationBuilder.RenameColumn(
                name: "InternalEmailPassword",
                table: "TenantEmailConfigurations",
                newName: "Domain");
        }
    }
}
