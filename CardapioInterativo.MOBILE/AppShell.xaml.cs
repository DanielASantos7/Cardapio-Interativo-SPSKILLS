using CardapioInterativo.MOBILE.Views;

namespace CardapioInterativo.MOBILE
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RestaurantesView), typeof(RestaurantesView));
            Routing.RegisterRoute(nameof(MontarCardapioView), typeof(MontarCardapioView));
            Routing.RegisterRoute(nameof(VisualizarCardapioView), typeof(VisualizarCardapioView));
            Routing.RegisterRoute(nameof(ListagemPratosView), typeof(ListagemPratosView));
            Routing.RegisterRoute(nameof(PratoView), typeof(PratoView));
            Routing.RegisterRoute(nameof(RankingView), typeof(RankingView));
        }
    }
}
