using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TemplateDocumentGenerator.Models;

namespace TemplateDocumentGenerator.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly AppSettings _appSettings;

        public ApplicationDbContext(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to SQLite database
            options.UseSqlite(_appSettings.ConnectionStrings.LocalDB);
        }

        public DbSet<DocxTemplate> DocxTemplates { get; set; }
        public DbSet<TemplateField> TemplateFields { get; set; }
        public DbSet<PieceOfKnowledge> Knowledge { get; set; }

    }
}
