namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Foreign key for ApplicationUser
        public  string? UserId { get; set; }
        public  ApplicationUser? User { get; set; }

       // Prevent cascading deletes
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
