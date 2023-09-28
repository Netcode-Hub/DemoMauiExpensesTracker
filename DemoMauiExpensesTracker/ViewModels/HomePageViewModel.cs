using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoMauiExpensesTracker.Models;
using DemoMauiExpensesTracker.Services;
using DemoMauiExpensesTracker.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMauiExpensesTracker.ViewModels
{
    public partial class HomePageViewModel : ExpensesBaseViewModel
    {
        private readonly ServiceInterface service;
        public HomePageViewModel(ServiceInterface service)
        {
            Title = "Expenses Tracker Dashboard";
            this.service = service;
            LoadGraphColors();
            ShowPopup = false;
            ThisMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
        }

        [ObservableProperty]
        private string thisMonth;

        // Load default graph color - Bar chart
        public ObservableCollection<Brush> CustomBrushes { get; set; } = new();
        private void LoadGraphColors()
        {
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(77, 208, 225)));
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(38, 198, 218)));
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(0, 188, 212)));
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(0, 172, 193)));
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(0, 151, 167)));
            CustomBrushes.Add(new SolidColorBrush(Color.FromRgb(0, 131, 143)));
        }

        // Expenses methods
        public ObservableCollection<Expenses> AllExpensesData { get; set; } = new();
        public ObservableCollection<Expenses> AllYearsData { get; set; } = new();
        public ObservableCollection<Expenses> LastYearData { get; set; } = new();
        public ObservableCollection<Expenses> ThisMonthData { get; set; } = new();

        public ObservableCollection<ExpensesModelForMonth> MonthsInLastYear { get; set; } = new();
        public ObservableCollection<ExpensesModelForYear> YearsData { get; set; } = new();

        // populate all data when the pag loads
        [RelayCommand]
        private async Task PopulateData()
        {
            AllExpensesData?.Clear();
            AllExpensesData?.Clear();
            AllYearsData?.Clear();
            LastYearData?.Clear();
            ThisMonthData?.Clear();
            MonthsInLastYear?.Clear();
            YearsData?.Clear();

            var results = await service.GetAllExpensesAsync();
            if (results.Count != 0)
            {
                foreach (var expense in results)
                {
                    AllExpensesData.Add(expense);
                }
                await PrepareAllExpensesChart();
            }
            return;
        }

        private async Task PrepareAllExpensesChart()
        {
            var groupedData = AllExpensesData.GroupBy(item => item.DateAdded!.Value.Year);
            foreach (var group in groupedData.OrderBy(_ => _.Key))
            {
                // populate all years data year
                foreach (var item in group)
                {
                    AllYearsData.Add(item);
                }


                //populate last year
                if (group.Key == DateTime.Now.AddYears(-1).Date.Year)
                {
                    foreach (var item in group)
                    {
                        LastYearData.Add(item);
                    }
                }
            }

            //populate this month
            var expenses = AllExpensesData.Where(_ => _.DateAdded!.Value.Year == DateTime.Now.Year && _.DateAdded!.Value.Month == DateTime.Now.Month).ToList();
            foreach (var item in expenses)
            {
                ThisMonthData.Add(item);
            }

            // // Call these methods.
            await GetAvailableFund();
            GetYesterdayExpenses();
            GetLastWeekData();
            GetYesterdayAndLastWeekProgressbar();


            //Group by months with Last Year record and total the Amount
            var result = LastYearData.GroupBy(d => new { d.DateAdded.Value.Month, d.Name }).Select(g => new { Month = g.Key.Month, Name = g.Key.Name, TotalAmount = g.Sum(d => d.Amount) });
            var groupByMonthNumber = result.GroupBy(d => new { d.Month }).Select(g => new { Month = g.Key.Month, Name = g.Key.Month, TotalAmount = g.Sum(d => d.TotalAmount) });

            List<ExpensesModelForMonth> list = new();
            foreach (var i in groupByMonthNumber.OrderBy(_ => _.Month))
            {
                MonthsInLastYear.Add(new ExpensesModelForMonth() { Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i.Month), TotalAmount = i.TotalAmount, MonthNumber = i.Month });
            }




            //Group by Years and total the Amount
            var yearResult = AllYearsData.GroupBy(d => new { d.DateAdded.Value.Year, d.Name }).Select(g => new { Year = g.Key.Year, Name = g.Key.Name, TotalAmount = g.Sum(d => d.Amount) });
            var groupByYearNumber = yearResult.GroupBy(d => new { d.Year }).Select(g => new { Year = g.Key.Year, TotalAmount = g.Sum(d => d.TotalAmount) });
            List<ExpensesModelForYear> yearList = new();
            foreach (var r in groupByYearNumber.OrderBy(_ => _.Year))
            {
                YearsData.Add(new ExpensesModelForYear() { Year = r.Year, TotalAmount = r.TotalAmount });
            }

        }

        //Fund methods
        [ObservableProperty]
        decimal availableFund;

        [ObservableProperty]
        private Fund myFund;

        [ObservableProperty]
        private bool showPopup;

        [RelayCommand]
        private void AddFundPopup()
        {
            MyFund = new Fund();
            ShowPopup = true;
        }
        private void HideAddFunPopup()
        {
            MyFund = new Fund();
            ShowPopup = false;
        }

        [RelayCommand]
        private async Task AddFund()
        {
            if (MyFund.Amount == 0)
            {
                await Shell.Current.DisplayAlert("Please enter amount", "Amount must not be 0", "Ok");
                ShowPopup = true;
                return;
            }

            int result = await service.AddFundAsync(MyFund);
            switch (result)
            {
                case 1:
                    await Shell.Current.DisplayAlert("Funds added", " Process Completed", "Ok");
                    HideAddFunPopup();
                    await GetAvailableFund();
                    break;
                case 2:
                    await Shell.Current.DisplayAlert("Funds updated", " Process Completed", "Ok");
                    HideAddFunPopup();
                    await GetAvailableFund();
                    break;
                case 3 or 4:
                    await Shell.Current.DisplayAlert("Amount cannot be negative", " Process Not Completed", "Ok");
                    ShowPopup = true;
                    break;

                case -1:
                    await Shell.Current.DisplayAlert("Unknow error occurred", " Process Not Successfull", "Ok");
                    ShowPopup = true;
                    break;
            }
        }

        [ObservableProperty]
        decimal yesterdayExpenses;

        [ObservableProperty]
        decimal lastWeekExpenses;

        [ObservableProperty]
        decimal thisMonthExpenses;

        private async Task GetAvailableFund()
        {
            decimal fund = await service.GetAvailableFund();
            if (fund > 0)
                AvailableFund = fund - ThisMonthData.Sum(_ => _.Amount);
            AvailableFund = fund;
        }

        private void GetYesterdayExpenses()
        {
            YesterdayExpenses = ThisMonthData.Where(_ => _.DateAdded!.Value.Date == DateTime.Now.AddDays(-1).Date).ToList().Sum(_ => _.Amount);
        }

        private void GetLastWeekData()
        {
            DateTime getLastweekDate = DateTime.Now.AddDays(-7);
            var lastWeek = ThisMonthData.OrderByDescending(_ => _.DateAdded!.Value.Date).TakeWhile(_ => _.DateAdded >= getLastweekDate).ToList();
            if (lastWeek is not null)
                LastWeekExpenses = lastWeek.Sum(_ => _.Amount);
        }

        //populate progressbar
        [ObservableProperty]
        private decimal yesterdayProgress;
        [ObservableProperty]
        private decimal lastWeekProgress;
        private void GetYesterdayAndLastWeekProgressbar()
        {
            ThisMonthExpenses = ThisMonthData.Sum(_ => _.Amount);

            // Last Week 
            if (LastWeekExpenses > 0 && ThisMonthExpenses > 0)
                LastWeekProgress = LastWeekExpenses / ThisMonthExpenses * 100;

            //Yesterday
            if (YesterdayExpenses > 0 && ThisMonthExpenses > 0)
                YesterdayProgress = YesterdayExpenses / ThisMonthExpenses * 100;
        }

        // Go to add Expenses page and open the add expense popup using [String Query]
        [RelayCommand]
        private async Task GotoAddExpensePage()
        {
            await Shell.Current.GoToAsync($"{nameof(ManageExpensesPage)}?id=1");
        }

    }
}
