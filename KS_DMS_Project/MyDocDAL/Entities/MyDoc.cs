using System.ComponentModel.DataAnnotations;

namespace MyDocDAL.Entities
{
    public class MyDoc
    {
        public int id { get; set; } // This should be the primary key
        public DateTime ? createddate { get; set; }
        public DateTime ? editeddate { get; set; }
        public string ? author { get; set; }
        public string ? title { get; set; }
        public string ? textfield { get; set; }
    }
}
