namespace ExpenseTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = string.Empty;

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}