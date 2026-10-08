namespace CardapioInterativo.MOBILE.Views;

public partial class SplashScreenView : ContentPage
{
    public SplashScreenView()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var cores = new[]
        {
            Color.FromArgb("#A4C2D6"),
            Color.FromArgb("#C75D4D"),
            Color.FromArgb("#7A8C69"),
        };

        for (int i = 0; i <= 12; i++)
        {
            await Task.Delay(1000);

            pgBar.Progress = i / 12.0;

            if (i % 2 == 0)
            {
                int p = i / 2;
                Topo.BackgroundColor = cores[p % 3];
                Meio.BackgroundColor = cores[(p + 1) % 3];
                baixo.BackgroundColor = cores[(p + 2) % 3];
            }
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Application.Current!.MainPage = new AppShell();
        });
    }
}