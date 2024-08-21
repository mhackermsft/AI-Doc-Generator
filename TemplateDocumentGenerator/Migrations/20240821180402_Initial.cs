using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemplateDocumentGenerator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocxTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocxTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Knowledge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knowledge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Placeholder = table.Column<string>(type: "TEXT", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFields", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocxTemplates");

            migrationBuilder.DropTable(
                name: "Knowledge");

            migrationBuilder.DropTable(
                name: "TemplateFields");
        }
    }
}
