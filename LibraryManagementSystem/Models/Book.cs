using System;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int BookTypeID { get; set; }
        public string? BookTypeName { get; set; }
        public DateTime? PublishDate { get; set; }
        public int NumOfPages { get; set; } = 0;
        public int NumCopies { get; set; } = 0;
        public string? Edition { get; set; }
        public string? Publisher { get; set; }
        public string? BookSource { get; set; }
        public string? Remark { get; set; }
    }
}
