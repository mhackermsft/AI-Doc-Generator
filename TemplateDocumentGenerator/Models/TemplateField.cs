using DocumentFormat.OpenXml.ExtendedProperties;

namespace TemplateDocumentGenerator.Models
{
    public class TemplateField
    {
        public Guid Id { get; set; }

        public Guid TemplateId { get; set; }
        public string Placeholder { get; set; }
        public string Prompt { get; set; }

        public TemplateField()
        {
            Id = Guid.NewGuid();
            TemplateId = Guid.Empty;
            Placeholder = string.Empty;
            Prompt = string.Empty;
        }
    }
}
