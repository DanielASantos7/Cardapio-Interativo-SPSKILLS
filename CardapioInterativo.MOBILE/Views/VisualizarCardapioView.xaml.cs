using CardapioInterativo.MOBILE.Service;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CardapioInterativo.MOBILE.Views;

[QueryProperty(nameof(RestauranteId), "restauranteId")]
[QueryProperty(nameof(UsuarioId), "usuarioId")]
public partial class VisualizarCardapioView : ContentPage
{
    public int RestauranteId { get; set; }
    public int UsuarioId { get; set; }

    // Mantém a lista original para reaplicar busca e ordenação.
    private readonly List<PratoClienteViewDto> pratos = new();

    public VisualizarCardapioView()
    {
        InitializeComponent();

        cvPratos.ItemsSource = pratos;
        pckOrdenacao.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carrega o cardápio ao abrir a tela.
        await CarregarCardapio();
    }

    private async Task CarregarCardapio()
    {
        // Busca o cardápio do restaurante e o estado das curtidas do cliente.
        var resposta = await ApiService.GetAsync<CardapioClienteViewResponse>(
            $"cardapio/cliente/{RestauranteId}?usuarioId={UsuarioId}");

        if (resposta == null)
        {
            await DisplayAlert(
                "Erro",
                string.IsNullOrWhiteSpace(ApiService.Erro)
                    ? "Não foi possível carregar o cardápio."
                    : ApiService.Erro,
                "OK");

            return;
        }

        // Preenche o cabeçalho com os dados do restaurante.
        lblNomeRestaurante.Text = resposta.NomeRestaurante;
        lblTipoRestaurante.Text = resposta.TipoRestaurante;

        pratos.Clear();
        pratos.AddRange(resposta.Pratos);

        AplicarFiltros();
    }

    private void Busca_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Atualiza os resultados conforme o cliente digita.
        AplicarFiltros();
    }

    private void Ordenacao_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Reordena os pratos conforme a opção escolhida.
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        string busca = txtBusca.Text?.Trim() ?? string.Empty;

        // Filtra pelo nome sem diferenciar maiúsculas e minúsculas.
        IEnumerable<PratoClienteViewDto> resultado = pratos
            .Where(p => p.Nome.Contains(
                busca, StringComparison.OrdinalIgnoreCase));

        // Aplica a ordenação selecionada.
        resultado = pckOrdenacao.SelectedIndex switch
        {
            1 => resultado.OrderBy(p => p.Preco),
            2 => resultado.OrderByDescending(p => p.Preco),
            _ => resultado
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nome)
        };

        // Atualiza a lista visual sem perder os dados originais.
        cvPratos.ItemsSource = resultado.ToList();
    }

    private async void Curtir_Clicked(object sender, EventArgs e)
    {
        // Recupera o prato associado ao botão clicado.
        if (sender is not Button botao ||
            botao.CommandParameter is not PratoClienteViewDto prato)
        {
            return;
        }

        bool novoEstado = !prato.IsCurtido;

        // Envia o novo estado da curtida para a API.
        var pedido = new CurtirPratoViewRequest
        {
            PratoId = prato.Id,
            ClienteId = UsuarioId,
            IsCurtido = novoEstado
        };

        var resultado = await ApiService.PostAsync<
            CurtirPratoViewRequest, bool>(
                "cardapio/curtir", pedido);

        // Só atualiza o coração se a API confirmar a operação.
        if (resultado != true)
        {
            await DisplayAlert(
                "Erro",
                string.IsNullOrWhiteSpace(ApiService.Erro)
                    ? "Não foi possível atualizar a curtida."
                    : ApiService.Erro,
                "OK");

            return;
        }

        prato.IsCurtido = novoEstado;
        AplicarFiltros();
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        // Retorna à tela anterior.
        await Shell.Current.GoToAsync("..");
    }
}

// DTO que representa a resposta completa do cardápio.
public class CardapioClienteViewResponse
{
    public string NomeRestaurante { get; set; } = string.Empty;
    public string TipoRestaurante { get; set; } = string.Empty;
    public List<PratoClienteViewDto> Pratos { get; set; } = new();
}

// DTO de cada prato exibido na tela.
public class PratoClienteViewDto : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Geral";
    public double? Preco { get; set; }

    private bool isCurtido;

    public bool IsCurtido
    {
        get => isCurtido;
        set
        {
            if (isCurtido == value)
                return;

            isCurtido = value;

            // Notifica a interface para atualizar o ícone do coração.
            OnPropertyChanged();
            OnPropertyChanged(nameof(IconeCurtida));
        }
    }

    // Exibe coração cheio ou vazio conforme o estado.
    public string IconeCurtida => IsCurtido ? "♥" : "♡";

    // Seleciona uma imagem local conforme o ID do prato.
    public string Foto =>
        $"prato{((Math.Max(1, Id) - 1) % 5) + 1}.jpg";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? nome = null)
    {
        PropertyChanged?.Invoke(
            this, new PropertyChangedEventArgs(nome));
    }
}

// DTO enviado à API para adicionar ou remover uma curtida.
public class CurtirPratoViewRequest
{
    public int PratoId { get; set; }
    public int ClienteId { get; set; }
    public bool IsCurtido { get; set; }
}