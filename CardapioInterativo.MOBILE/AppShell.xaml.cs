using CardapioInterativo.MOBILE.Views;

namespace CardapioInterativo.MOBILE
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RestaurantesView), typeof(RestaurantesView));
        }
    }
}
