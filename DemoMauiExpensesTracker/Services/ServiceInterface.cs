

using DemoMauiExpensesTracker.Models;

namespace DemoMauiExpensesTracker.Services
{
    public interface ServiceInterface
    {
        // Expense
        Task<(bool flag, string message, Expenses newData)> AddOrUpdateExpensesAsync(Expenses model);
        Task<List<Expenses>> GetAllExpensesAsync();
        Task<(bool flag, string message)> DeleteExpensesAsync(Expenses model);

        //Fund
        Task<int> AddFundAsync(Fund fund);
        Task<decimal> GetAvailableFund();
    }
}
