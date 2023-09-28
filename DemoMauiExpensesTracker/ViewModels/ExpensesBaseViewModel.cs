using CommunityToolkit.Mvvm.ComponentModel;
namespace DemoMauiExpensesTracker.ViewModels
{
    public partial class ExpensesBaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;
    }
}
