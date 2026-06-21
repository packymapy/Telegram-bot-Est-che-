using System;

namespace AdminApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }
}
