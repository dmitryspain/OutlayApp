using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OutlayApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecureSessionsUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Dates were stored as Kyiv wall-clock time ("timestamp without time zone"); they become real
            // instants (timestamptz, UTC). Money on cards was in kopiykas; it becomes hryvnias like transactions.
            migrationBuilder.Sql("""
                ALTER TABLE "ClientTransactions" ALTER COLUMN "DateOccured" TYPE timestamp with time zone USING "DateOccured" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "LogoReferences" ALTER COLUMN "LastTimeRetrieved" TYPE timestamp with time zone USING "LastTimeRetrieved" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "InvalidReferences" ALTER COLUMN "LastTimeRetrieved" TYPE timestamp with time zone USING "LastTimeRetrieved" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "ClientCards" ALTER COLUMN "Balance" TYPE numeric(18,2) USING round("Balance" / 100, 2);
                ALTER TABLE "ClientCards" ALTER COLUMN "CreditLimit" TYPE numeric(18,2) USING round("CreditLimit" / 100.0, 2);
                """);
            migrationBuilder.DropIndex(
                name: "IX_ClientTransactions_ClientCardId",
                table: "ClientTransactions");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Cashback",
                table: "ClientTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ClientTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterName",
                table: "ClientTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hold",
                table: "ClientTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PersonalToken",
                table: "Clients",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedToken",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "ClientCards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaskedPan",
                table: "ClientCards",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BackfillJobs",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Start = table.Column<long>(type: "bigint", nullable: false),
                    CursorTo = table.Column<long>(type: "bigint", nullable: false),
                    Floor = table.Column<long>(type: "bigint", nullable: false),
                    Imported = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    NextRunAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackfillJobs", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "ClientSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSessions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientTransactions_ClientCardId_DateOccured",
                table: "ClientTransactions",
                columns: new[] { "ClientCardId", "DateOccured" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TokenHash",
                table: "Clients",
                column: "TokenHash",
                unique: true,
                filter: "\"TokenHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCards_ExternalCardId",
                table: "ClientCards",
                column: "ExternalCardId");

            migrationBuilder.CreateIndex(
                name: "IX_BackfillJobs_State_NextRunAtUtc",
                table: "BackfillJobs",
                columns: new[] { "State", "NextRunAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientSessions_ClientId",
                table: "ClientSessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSessions_TokenHash",
                table: "ClientSessions",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // back to Kyiv wall-clock time and kopiykas; tokens stay encrypted (the plain ones are gone)
            migrationBuilder.Sql("""
                ALTER TABLE "ClientTransactions" ALTER COLUMN "DateOccured" TYPE timestamp without time zone USING "DateOccured" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "LogoReferences" ALTER COLUMN "LastTimeRetrieved" TYPE timestamp without time zone USING "LastTimeRetrieved" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "InvalidReferences" ALTER COLUMN "LastTimeRetrieved" TYPE timestamp without time zone USING "LastTimeRetrieved" AT TIME ZONE 'Europe/Kyiv';
                ALTER TABLE "ClientCards" ALTER COLUMN "Balance" TYPE numeric USING "Balance" * 100;
                ALTER TABLE "ClientCards" ALTER COLUMN "CreditLimit" TYPE integer USING round("CreditLimit" * 100)::integer;
                UPDATE "Clients" SET "PersonalToken" = '' WHERE "PersonalToken" IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "BackfillJobs");

            migrationBuilder.DropTable(
                name: "ClientSessions");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropIndex(
                name: "IX_ClientTransactions_ClientCardId_DateOccured",
                table: "ClientTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TokenHash",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_ClientCards_ExternalCardId",
                table: "ClientCards");

            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Cashback",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "CounterName",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "Hold",
                table: "ClientTransactions");

            migrationBuilder.DropColumn(
                name: "EncryptedToken",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "ClientCards");

            migrationBuilder.DropColumn(
                name: "MaskedPan",
                table: "ClientCards");

            migrationBuilder.AlterColumn<string>(
                name: "PersonalToken",
                table: "Clients",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientTransactions_ClientCardId",
                table: "ClientTransactions",
                column: "ClientCardId");
        }
    }
}
