namespace Knie_Schwarz_project_WS2024_DMS
{
    public class Document
    {
        public int Id { get; set; }
        public DateTime DateUpload { get; set; }
        public DateTime DateEdit { get; set; }
        public string? Author { get; set; }

        public string? Title { get; set; }
        public string? Text { get; set; }
    }
}

