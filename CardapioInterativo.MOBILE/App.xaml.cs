using CardapioInterativo.MOBILE.Views;

namespace CardapioInterativo.MOBILE
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new SplashScreenView();
        }
    }
}