namespace MyDocDAL.Entities
{
    public class MyDoc
    {
        public int Id { get; set; } // This should be the primary key
        public string ? Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
