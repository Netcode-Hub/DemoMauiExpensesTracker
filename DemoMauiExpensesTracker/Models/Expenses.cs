using SQLite;
namespace DemoMauiExpensesTracker.Models
{
    [Table("Expenses")]
    public class Expenses
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? DateAdded { get; set; }
    }
}
