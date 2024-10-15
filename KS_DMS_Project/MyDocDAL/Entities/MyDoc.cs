namespace MyDocDAL.Entities
{
    public class MyDoc
    {
        public int Id { get; set; } // This should be the primary key
        public string ? Author { get; set; }
        public string ? Titel { get; set; }
        public string ? Textfield { get; set; }
    }
}
