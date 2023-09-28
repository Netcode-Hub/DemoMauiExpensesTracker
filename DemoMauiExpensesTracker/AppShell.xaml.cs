using DemoMauiExpensesTracker.Views;

namespace DemoMauiExpensesTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ManageExpensesPage), typeof(ManageExpensesPage));
        }
    }
}
