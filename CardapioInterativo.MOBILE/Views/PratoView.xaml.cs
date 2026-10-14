using CardapioInterativo.MOBILE.Service;

namespace CardapioInterativo.MOBILE.Views;

public partial class PratoView : ContentPage
{
    public PratoView()
    {
        InitializeComponent();
    }

    // Carrega as categorias ao abrir a tela.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarCategorias();
    }

    private async Task CarregarCategorias()
    {
        var categorias =
            await ApiService.GetAsync<List<CategoriaPratoDto>>("categorias");

        if (categorias == null)
        {
            await DisplayAlert(
                "Erro",
                string.IsNullOrWhiteSpace(ApiService.Erro)
                    ? "Não foi possível carregar as categorias."
                    : ApiService.Erro,
                "OK");

            return;
        }

        pkCategoria.ItemsSource = categorias;
    }

    // Valida os campos e cadastra o prato na API.
    private async void Salvar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text) ||
            string.IsNullOrWhiteSpace(txtDescricao.Text) ||
            string.IsNullOrWhiteSpace(txtIngredientes.Text) ||
            string.IsNullOrWhiteSpace(txtTempoPreparo.Text) ||
            pkCategoria.SelectedItem == null)
        {
            await DisplayAlert(
                "Atenção",
                "Preencha todos os campos obrigatórios.",
                "OK");

            return;
        }

        // Garante que o tempo de preparo seja um número inteiro positivo.
        if (!int.TryParse(txtTempoPreparo.Text, out int tempo) || tempo <= 0)
        {
            await DisplayAlert(
                "Atenção",
                "Informe um tempo de preparo válido, em minutos.",
                "OK");

            return;
        }

        var categoria = (CategoriaPratoDto)pkCategoria.SelectedItem;

        var novoPrato = new PratoCriacaoRequestDto
        {
            Nome = txtNome.Text.Trim(),
            Descricao = txtDescricao.Text.Trim(),
            Ingredientes = txtIngredientes.Text.Trim(),
            TempoPreparo = tempo,
            CategoriaId = categoria.Id
        };

        btnSalvar.IsEnabled = false;

        try
        {
            var resultado =
                await ApiService.PostAsync<PratoCriacaoRequestDto, bool>(
                    "pratos",
                    novoPrato);

            if (resultado != true)
            {
                await DisplayAlert(
                    "Erro",
                    string.IsNullOrWhiteSpace(ApiService.Erro)
                        ? "Não foi possível cadastrar o prato."
                        : ApiService.Erro,
                    "OK");

                return;
            }

            await DisplayAlert(
                "Sucesso",
                "Prato cadastrado com sucesso!",
                "OK");

            // Volta à listagem, que recarrega os pratos ao aparecer.
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            btnSalvar.IsEnabled = true;
        }
    }

    // Retorna para a listagem.
    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

// Dados de cada categoria recebidos da API.
public class CategoriaPratoDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;
}

// Dados enviados para cadastrar um prato.
public class PratoCriacaoRequestDto
{
    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Ingredientes { get; set; } = string.Empty;

    public int TempoPreparo { get; set; }

    public int CategoriaId { get; set; }
}