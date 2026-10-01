using System;

namespace LibraryManagementSystem.Models
{
    public class Author
    {
        public int AuthorID { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string? POB { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public byte[]? Photo { get; set; }
    }
}
