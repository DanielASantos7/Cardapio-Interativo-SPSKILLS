using CardapioInterativo.MOBILE.Service;

namespace CardapioInterativo.MOBILE.Views;

public partial class ListagemPratosView : ContentPage
{
    public ListagemPratosView()
    {
        InitializeComponent();
    }

    // Carrega os pratos sempre que a tela é aberta.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarPratos();
    }

    // Busca os pratos na API e agrupa por categoria.
    private async Task CarregarPratos()
    {
        var resposta = await ApiService.GetAsync<List<PratoListagemDto>>(
            "pratos"
        );

        if (resposta == null)
        {
            await DisplayAlert(
                "Erro",
                string.IsNullOrWhiteSpace(ApiService.Erro)
                    ? "Não foi possível carregar os pratos."
                    : ApiService.Erro,
                "OK"
            );

            return;
        }

        // Ordena as categorias e os pratos alfabeticamente.
        var grupos = resposta
            .OrderBy(p => p.CategoriaNome)
            .ThenBy(p => p.Nome)
            .GroupBy(p => p.CategoriaNome)
            .Select(g => new CategoriaPratosGrupo(g.Key, g))
            .ToList();

        // Exibe os grupos no CollectionView do XAML.
        cvPratos.ItemsSource = grupos;
    }

    // Abre a tela de cadastro de pratos.
    private async void NovoPrato_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PratoView));
    }
}

// Dados recebidos da API e imagem calculada localmente.
public class PratoListagemDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Ingredientes { get; set; } = string.Empty;

    public string CategoriaNome { get; set; } = "Geral";

    public int TotalCurtidas { get; set; }

    // Repete as cinco imagens locais conforme o ID.
    public string Foto =>
        $"prato{((Math.Max(1, Id) - 1) % 5) + 1}.jpg";
}

// Representa uma categoria e os pratos pertencentes a ela.
public class CategoriaPratosGrupo : List<PratoListagemDto>
{
    public string Categoria { get; }

    public CategoriaPratosGrupo(
        string categoria,
        IEnumerable<PratoListagemDto> pratos
    ) : base(pratos)
    {
        Categoria = categoria;
    }
}