namespace TemplateDocumentGenerator.Models
{
    public class DocxTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public DocxTemplate()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            Path = string.Empty;
        }

    }
}
