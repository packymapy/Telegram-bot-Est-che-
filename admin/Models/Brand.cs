using System;

namespace AdminApp.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
    }
}
