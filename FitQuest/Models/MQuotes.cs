namespace FitQuest.Models
{
    public class MQuote
    {
        public int ID { get; set; }  // The ID column in the MQuotes table
        public string Quote { get; set; }  // The quote text column in the MQuotes table
        public byte Used { get; set; } // The Used flag column to track if the quote has been used
        public DateTime? LastUsedAt { get; set; }
    }
}
