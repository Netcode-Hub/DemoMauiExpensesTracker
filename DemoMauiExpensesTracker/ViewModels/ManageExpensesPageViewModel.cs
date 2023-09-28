using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoMauiExpensesTracker.Models;
using DemoMauiExpensesTracker.Services;
using System.Collections.ObjectModel;

namespace DemoMauiExpensesTracker.ViewModels
{
    [QueryProperty(nameof(NavigationId), "id")]
    public partial class ManageExpensesPageViewModel : ExpensesBaseViewModel
    {
        //Recieve the incoming query string and open Add Expenses popup.
        [ObservableProperty]
        private int navigationId;

        private readonly ServiceInterface service;
        public ManageExpensesPageViewModel(ServiceInterface service)
        {
            this.service = service;
            ShowPopup = false;
            SelectedDate = DateTime.Now;
        }

        [RelayCommand]
        private void CheckForQueryStringAndNavigiate()
        {
            if (NavigationId == 1)
            {
                ExpenseModel = new Expenses();
                ShowPopup = true;
                NavigationId = 0;
            }
        }

        [ObservableProperty]
        private Expenses expenseModel;

        [ObservableProperty]
        private bool showPopup;

        //Handle date selection
        [ObservableProperty]
        private DateTime? selectedDate;
        partial void OnSelectedDateChanged(DateTime? oldValue, DateTime? newValue)
        {
            if (newValue is null) return;
            SelectedDate = newValue;
        }

        // Show Expenses popup
        [RelayCommand]
        private void ShowAddExpensePopup()
        {
            ExpenseModel = new Expenses();
            ShowPopup = true;
        }


        //Popup message section; 
        [ObservableProperty]
        private string popupMessage;

        [ObservableProperty]
        private string popupMessageTitle;

        [ObservableProperty]
        private bool showMessagePopup;

        [RelayCommand]
        private void CloseMessagePopup()
        {
            ShowPopup = true;
            ShowMessagePopup = false;
        }


        //Save Expenses command handler
        [RelayCommand]
        private async Task SaveExpenses()
        {
            ExpenseModel.DateAdded = SelectedDate.Value.Date;
            if (string.IsNullOrEmpty(ExpenseModel.Name) || ExpenseModel.Amount <= 0 || ExpenseModel.DateAdded is null)
            {
                PopupMessage = $"Sorry, make sure you have provided all the information needed.{Environment.NewLine}Name should not be NULL, {Environment.NewLine}Amount shouldn't be NULL,{Environment.NewLine}Date must be greater or equal to Today {DateTime.Now.Date}, {Environment.NewLine}Thank You!{Environment.NewLine}";
                PopupMessageTitle = "Alert";
                ShowPopup = false;
                ShowMessagePopup = true;
                return;
            }

            var (flag, message, newData) = await service.AddOrUpdateExpensesAsync(ExpenseModel);
            if (flag)
            {
                //let check if added or updated, we can do that by checking the incoming message.
                if (message.ToLower().Equals("saved"))
                {
                    //inserted : and if inserted, then add the data to the list we have
                    ExpensesData.Add(newData);
                }
                else
                {//updated : and if updated then remove the old one and add the updated one to the list we have
                    var oldData = ExpensesData.FirstOrDefault(_ => _.Id == ExpenseModel.Id);
                    ExpensesData.Remove(oldData); ExpensesData.Add(newData);
                }
                ExpenseModel = new();
                ShowPopup = false;
            }
            else
            {
                PopupMessage = message;
                PopupMessageTitle = "Alert";
                ShowPopup = false;
                ShowMessagePopup = true;
            }
        }


        public ObservableCollection<Expenses> ExpensesData { get; set; } = new();
        // Getting all expenses from the service.
        [RelayCommand]
        private async Task GetExpenses()
        {
            var result = await service.GetAllExpensesAsync();
            if (ExpensesData.Count > 0) return;

            if (result is null)
            {
                ExpensesData?.Clear();
                return;
            }

            foreach (var expense in result.OrderByDescending(_ => _.DateAdded))
                ExpensesData.Add(expense);
        }


        // Handling row Selection
        [ObservableProperty]
        private Expenses selectedRowData;
        partial void OnSelectedRowDataChanged(Expenses oldValue, Expenses newValue)
        {
            if (newValue != null)
                SelectedRowData = newValue;

            ManageSelectedData(SelectedRowData);
        }

        private async Task ManageSelectedData(Expenses SelectedRowData)
        {
            if (SelectedRowData != null || SelectedRowData.Id != 0)
            {
                string action = await Shell.Current.DisplayActionSheet("Action: Choose an Option", "Cancel", null, "Edit", "Delete");
                if (string.IsNullOrEmpty(action) || string.IsNullOrWhiteSpace(action)) return;

                if (action.Equals("Cancel")) return;

                if (action.Equals("Edit"))
                {
                    ExpenseModel = new Expenses();
                    ExpenseModel = SelectedRowData;
                    PopupMessageTitle = $"Update {SelectedRowData.Name} information.";
                    ShowPopup = true;
                }

                if (action.Equals("Delete"))
                {
                    bool answer = await Shell.Current.DisplayAlert("Confirm Operation", $"Are you sure you want to delete {SelectedRowData.Name}?", "Yes", "No");
                    if (!answer) return;

                    //using tupple to handle the result
                    var (flag, message) = await service.DeleteExpensesAsync(SelectedRowData);
                    if (flag)
                    {
                        // After deleted, lets then remove from the list we have
                        var getDeletedData = ExpensesData.Where(_ => _.Id == SelectedRowData.Id).FirstOrDefault();
                        ExpensesData.Remove(getDeletedData);
                        SelectedRowData = new Expenses();
                    }
                    await Shell.Current.DisplayAlert("Alert", message, "Ok");
                    SelectedRowData = new Expenses();
                }
            }
        }
    }
}
