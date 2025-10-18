using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewGenie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    PositiveReviews = table.Column<int>(type: "int", nullable: false),
                    NeutralReviews = table.Column<int>(type: "int", nullable: false),
                    NegativeReviews = table.Column<int>(type: "int", nullable: false),
                    RespondedReviews = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    NewReviews = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewMetrics_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sentiment = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GeneratedResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasResponded = table.Column<bool>(type: "bit", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlatformUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewMetrics_BusinessId_Date",
                table: "ReviewMetrics",
                columns: new[] { "BusinessId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewMetrics_Date",
                table: "ReviewMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BusinessId",
                table: "Reviews",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Platform_ExternalId",
                table: "Reviews",
                columns: new[] { "Platform", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PostedAt",
                table: "Reviews",
                column: "PostedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Sentiment",
                table: "Reviews",
                column: "Sentiment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewMetrics");

            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
