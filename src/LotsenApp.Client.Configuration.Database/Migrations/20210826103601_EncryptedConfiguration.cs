using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LotsenApp.Client.Configuration.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EncryptedConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_configuration");

            migrationBuilder.DropTable(
                name: "user_configuration");

            migrationBuilder.CreateTable(
                name: "global_configuration_encrypted",
                columns: table => new
                {
                    ConfigurationId = table.Column<string>(type: "TEXT", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_configuration_encrypted", x => x.ConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "user_configuration_encrypted",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_configuration_encrypted", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_configuration_encrypted");

            migrationBuilder.DropTable(
                name: "user_configuration_encrypted");

            migrationBuilder.CreateTable(
                name: "global_configuration",
                columns: table => new
                {
                    ConfigurationId = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationMode = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultTheme = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_configuration", x => x.ConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "user_configuration",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    EncryptedDataKey = table.Column<string>(type: "TEXT", nullable: true),
                    EncryptedPrivateKeyByDataPassword = table.Column<string>(type: "TEXT", nullable: true),
                    EncryptedPrivateKeyByRecoveryKey = table.Column<string>(type: "TEXT", nullable: true),
                    FirstSignIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    HashedDataPassword = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Server = table.Column<string>(type: "TEXT", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_configuration", x => x.UserId);
                });
        }
    }
}
