using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewGenie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalAccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalLocationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessIntegrations_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessIntegrations_BusinessId_Platform",
                table: "BusinessIntegrations",
                columns: new[] { "BusinessId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessIntegrations_ExternalAccountId",
                table: "BusinessIntegrations",
                column: "ExternalAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessIntegrations");
        }
    }
}
