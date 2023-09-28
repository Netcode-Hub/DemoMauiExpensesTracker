
using DemoMauiExpensesTracker.Models;
using SQLite;

namespace DemoMauiExpensesTracker.Services
{
    public class ServiceImplementation : ServiceInterface
    {
        private SQLiteAsyncConnection connection;
        public ServiceImplementation()
        {
            SetupDatabase();
        }

        private async void SetupDatabase()
        {
            if (connection is null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DemoExpensesdb.db3");
                connection = new SQLiteAsyncConnection(dbPath);
                await connection.CreateTablesAsync<Expenses, Fund>();
            }
        }

        //Expenses service
        public async Task<(bool flag, string message, Expenses newData)> AddOrUpdateExpensesAsync(Expenses model)
        {
            if (model is null)
                return (false, "Bads Request", null);

            int result;
            if (model.Id > 0)
            {
                var (flag, message, newData) = await UpdateExpensesAsync(model);
                return (flag, message, newData);
            }

            result = await connection.InsertAsync(model);
            if (result > 0)
            {
                //Get last inserted row
                var data = await connection.Table<Expenses>().ToListAsync();
                var lastData = data.OrderBy(_ => _.Id).Last();
                return (true, "Saved", lastData);
            }

            return (false, "Internal Server Error", null);
        }

        public async Task<(bool flag, string message)> DeleteExpensesAsync(Expenses model)
        {
            var result = await connection?.DeleteAsync(model);
            if (result > 0)
                return (true, "Deleted");
            return (false, "Internal Server Error");
        }

        public async Task<List<Expenses>> GetAllExpensesAsync() => await connection.Table<Expenses>().ToListAsync();

        private async Task<(bool flag, string message, Expenses? newData)> UpdateExpensesAsync(Expenses model)
        {
            var result = await connection.UpdateAsync(model);
            if (result > 0)
                return (true, "Updated", model);
            return (false, "Internal Server Error", null);
        }


        // Fund service
        public async Task<int> AddFundAsync(Fund fund)
        {
            try
            {
                var alreadyFund = await connection.Table<Fund>().Where(_ => _.Amount >= 0).FirstOrDefaultAsync();
                if (alreadyFund is not null)
                {
                    //check if the amount is not negative
                    decimal currentAmount = alreadyFund.Amount + fund.Amount;
                    if (currentAmount >= 0)
                    {
                        await connection.DeleteAsync(alreadyFund);
                        await connection.InsertAsync(new Fund() { Amount = currentAmount });
                        return 2; // updated
                    }
                    return 3; // fund cannot be negative

                }

                if (fund.Amount < 0)
                    return 4; // fund is negative

                await connection.InsertAsync(fund);
                return 1; // inserted
            }
            catch (Exception)
            {
                return -1; // unknown error
            }
        }

        public async Task<decimal> GetAvailableFund()
        {
            var fund = await connection.Table<Fund>().ToListAsync();
            return fund.Select(_ => _.Amount).Sum();
        }
    }


}
