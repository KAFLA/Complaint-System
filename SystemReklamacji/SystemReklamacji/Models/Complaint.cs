using System;

namespace ReklamacjeSystem.Models
{
    public enum ComplaintStatus
    {
        New,
        InProgress,
        Resolved,
        Closed
    }

    public enum ComplaintPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public class Complaint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ComplaintStatus Status { get; set; }
        public ComplaintPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; } // Może być null, jeśli nieprzypisana lub użytkownik usunięty
        public User User { get; set; } // Właściwość nawigacyjna
    }
}