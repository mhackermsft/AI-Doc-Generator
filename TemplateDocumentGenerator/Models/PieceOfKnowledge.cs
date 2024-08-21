namespace TemplateDocumentGenerator.Models
{
    public class PieceOfKnowledge
    {
        public Guid Id { get; set; }
        public string Source { get; set; }

        public PieceOfKnowledge()
        {
            Id = Guid.NewGuid();
            Source = string.Empty;
        }
    }
}
