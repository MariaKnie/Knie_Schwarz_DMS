namespace ASP_Rest_API.Model
{
    public class MyDoc
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EditedDate { get; set; }
        public string Author { get; set; }
        public string TextField { get; set; }

        // Constructor
        public MyDoc(int id, string title, DateTime createdDate, DateTime editedDate, string author, string textField)
        {
            Id = id;
            Title = title;
            CreatedDate = createdDate;
            EditedDate = editedDate;
            Author = author;
            TextField = textField;
        }

        // Example ToString override for easy display
        public override string ToString()
        {
            return $"Document ID: {Id}, Title: {Title}, Created: {CreatedDate}, Edited: {EditedDate}, Owner: {Author}, Text: {TextField}";
        }
    }
}


