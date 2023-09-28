using DemoMauiExpensesTracker.ViewModels;

namespace DemoMauiExpensesTracker.Views;

public partial class ManageExpensesPage : ContentPage
{
    private readonly ManageExpensesPageViewModel manageExpensesPageViewModel;

    public ManageExpensesPage(ManageExpensesPageViewModel manageExpensesPageViewModel)
    {
        InitializeComponent();
        BindingContext = manageExpensesPageViewModel;
        this.manageExpensesPageViewModel = manageExpensesPageViewModel;
    }

    protected override void OnAppearing()
    {
        manageExpensesPageViewModel.GetExpensesCommand.Execute(this);
        manageExpensesPageViewModel.CheckForQueryStringAndNavigiateCommand.Execute(this);
    }
}