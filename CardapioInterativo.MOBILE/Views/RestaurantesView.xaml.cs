using CardapioInterativo.MOBILE.Helpers;
using CardapioInterativo.MOBILE.Service;
using System.Windows.Input;

namespace CardapioInterativo.MOBILE.Views;

public partial class RestaurantesView : ContentPage
{

    private string termo = "";

    public ICommand ExcluirRestauranteCommand { get; }
    public RestaurantesView()
    {
        InitializeComponent();

        ExcluirRestauranteCommand =
            new Command<RestauranteRequestDto>(
                async restaurante => await ExcluirRestaurantAsync(restaurante));

        BindingContext = this;

        lblTabPratos.IsVisible = SessaoUsuario.PerfilId == 1;
    }

    private async Task ExcluirRestaurantAsync(RestauranteRequestDto restaurante)
    {
        if (SessaoUsuario.PerfilId != 1
            || restaurante == null
            || restaurante.Excluido)
            return;

        bool confirmou = await DisplayAlert(
            "Excluir restaurante",
            "Tem certeza de que deseja excluir?",
            "Excluir",
            "Cancelar"
            );

        if (!confirmou)
            return;

        string endpoint =
            $"restaurantes/{restaurante.Id}" +
            $"?usuarioId={SessaoUsuario.Idusuario}" +
            $"&perfilId={SessaoUsuario.PerfilId}";

        bool sucesso = await ApiService.DeleteAsync(endpoint);

        if (!sucesso)
        {
            await DisplayAlert("Erro", ApiService.Erro, "OK");
            return;
        }

        await CarregarRestaurantes();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarRestaurantes();
    }

    private async Task CarregarRestaurantes()
    {
        var restaurantes = await ApiService.GetAsync<List<RestauranteRequestDto>>(
            $"restaurantes?usuarioId={SessaoUsuario.Idusuario}" +
            $"&perfilId={SessaoUsuario.PerfilId}" +
            $"&pesquisa={Uri.EscapeDataString(termo)}"
            );

        if (restaurantes == null)
        {
            await DisplayAlert("Erro", ApiService.Erro, "OK");
            return;
        }

        foreach (var restaurante in restaurantes)
        {
            restaurante.EhProprietario = SessaoUsuario.PerfilId == 1;
        }

        cvRestaurantes.ItemsSource = restaurantes;
    }

    private async void pesquisa_TextChanged(object sender, TextChangedEventArgs e)
    {
        termo = e.NewTextValue ?? "";

        if (pesquisa.Text.Length >= 2 || pesquisa.Text.Length == 0)
        {
            termo = pesquisa.Text;
            await CarregarRestaurantes();
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button botao ||
            botao.BindingContext is not RestauranteRequestDto restaurante)
            return;

        if (restaurante.Excluido)
        {

            if (SessaoUsuario.PerfilId != 1)
                return;

            bool confirmou = await DisplayAlert(
                "Restaurar restaurante",
                "Deseja restaurar este restaurante?",
                "Restaurar",
                "Cancelar");

            if (!confirmou)
                return;

            string endpoint =
                $"restaurantes/{restaurante.Id}/restaurar" +
                $"?usuarioId={SessaoUsuario.Idusuario}" +
                $"&perfilId={SessaoUsuario.PerfilId}";

            bool sucesso = await ApiService.PostSemBodyAsync(endpoint);

            if (!sucesso)
            {
                await DisplayAlert(
                    "Erro ao restaurar",
                    ApiService.Erro,
                    "OK");

                return;
            }

            await DisplayAlert(
                "Sucesso",
                "Restaurante restaurado!",
                "OK");

            await CarregarRestaurantes();
            return;
        }

        if (SessaoUsuario.PerfilId == 1)
        {
            await Shell.Current.GoToAsync(
                $"{nameof(MontarCardapioView)}?restauranteId={restaurante.Id}");
        }
        else
        {
            await Shell.Current.GoToAsync(
                $"{nameof(VisualizarCardapioView)}?restauranteId={restaurante.Id}&usuarioId={SessaoUsuario.Idusuario}");
        }
    }

    private async void TabPratos_Tapped(object sender, TappedEventArgs e)
    {
        // Impede que clientes acessem a tela de gerenciamento.
        if (SessaoUsuario.PerfilId != 1)
            return;

        await Shell.Current.GoToAsync(nameof(ListagemPratosView));
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RankingView));
    }
}


public class RestauranteRequestDto
{
    public int Id { get; set; }

    public string Nome { get; set; }

    public string Descricao { get; set; }

    public string Cidade { get; set; }

    public string Tipo { get; set; }

    public int Avaliacao { get; set; }
    public bool Excluido { get; set; }

    public string Localizacao => $"{Cidade} / {Tipo}";
    public string TextoBotao =>
        Excluido ? "Restaurar" :
        EhProprietario ? "Montar Cardápio" :
        "Visualizar Cardápio";

    public bool EhProprietario { get; set; }

    // Existem somente 4 imagens de restaurantes.
    // O % 4 faz as imagens se repetirem:
    // 1 -> Restaurante1
    // 2 -> Restaurante2
    // 3 -> Restaurante3
    // 4 -> Restaurante4
    // 5 -> Restaurante1
    // 6 -> Restaurante2

    public string Imagem =>
        $"Restaurante{((Id - 1) % 4) + 1}.jpg";


    // ESTRELAS

    public string Estrela1 =>
        Avaliacao >= 1
            ? "estrelapreta.png"
            : "estrela.png";

    public string Estrela2 =>
        Avaliacao >= 2
            ? "estrelapreta.png"
            : "estrela.png";

    public string Estrela3 =>
        Avaliacao >= 3
            ? "estrelapreta.png"
            : "estrela.png";

    public string Estrela4 =>
        Avaliacao >= 4
            ? "estrelapreta.png"
            : "estrela.png";

    public string Estrela5 =>
        Avaliacao >= 5
            ? "estrelapreta.png"
            : "estrela.png";
}