using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutlayApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LiveUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "ClientTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientTransactions_ClientCardId_ExternalId",
                table: "ClientTransactions",
                columns: new[] { "ClientCardId", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientTransactions_ClientCardId_ExternalId",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "Clients");
        }
    }
}
