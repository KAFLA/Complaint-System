using System;

namespace ReklamacjeSystem.Models
{
    public class Solution
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // Do kategoryzacji rozwiązań
        public DateTime CreatedAt { get; set; }
    }
}