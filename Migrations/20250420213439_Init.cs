using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebCrawler.Data;

#nullable disable

namespace WebCrawler.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoundPdfs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Link = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SearchHistoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoundPdfs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoundPdfs_SearchHistories_SearchHistoryId",
                        column: x => x.SearchHistoryId,
                        principalTable: "SearchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoundPdfs_SearchHistoryId",
                table: "FoundPdfs",
                column: "SearchHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoundPdfs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SearchHistories");
        }
    }
}
