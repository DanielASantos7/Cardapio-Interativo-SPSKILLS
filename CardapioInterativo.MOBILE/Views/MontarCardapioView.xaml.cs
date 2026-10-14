using CardapioInterativo.MOBILE.Service;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CardapioInterativo.MOBILE.Views;

[QueryProperty(nameof(RestauranteId), "restauranteId")]
public partial class MontarCardapioView : ContentPage
{
    public int RestauranteId { get; set; }

    private readonly ObservableCollection<PratoDto> disponiveis = new();
    private readonly ObservableCollection<PratoDto> menu = new();

    // Rastreamento infalível em memória para Android/iOS
    private PratoDto? _pratoEmArrasto;
    private string? _origemEmArrasto;

    public MontarCardapioView()
    {
        InitializeComponent();

        cvPratosDisponiveis.ItemsSource = disponiveis;
        cvMenuRestaurante.ItemsSource = menu;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarCardapio();
    }

    private async Task CarregarCardapio()
    {
        var cardapio = await ApiService.GetAsync<CardapioDto>(
            $"cardapio/{RestauranteId}");

        if (cardapio == null)
        {
            await DisplayAlert("Erro", ApiService.Erro, "OK");
            return;
        }

        disponiveis.Clear();
        menu.Clear();

        foreach (var prato in cardapio.Disponiveis)
            disponiveis.Add(prato);

        foreach (var prato in cardapio.Menu)
            menu.Add(prato);
    }

    private void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void DragGestureRecognizer_DragStarting(
        object sender, DragStartingEventArgs e)
    {
        var prato = ObterPratoDoSender(sender);

        if (prato == null)
        {
            e.Cancel = true;
            return;
        }

        // Armazena no estado em memória do app
        _pratoEmArrasto = prato;
        _origemEmArrasto = "Disponiveis";

        e.Data.Text = $"Disponiveis|{prato.Id}";
        e.Data.Properties["PratoId"] = prato.Id;
        e.Data.Properties["Origem"] = "Disponiveis";
    }

    private void DragGestureRecognizer_DragStarting_1(
        object sender, DragStartingEventArgs e)
    {
        var prato = ObterPratoDoSender(sender);

        if (prato == null)
        {
            e.Cancel = true;
            return;
        }

        // Armazena no estado em memória do app
        _pratoEmArrasto = prato;
        _origemEmArrasto = "Menu";

        e.Data.Text = $"Menu|{prato.Id}";
        e.Data.Properties["PratoId"] = prato.Id;
        e.Data.Properties["Origem"] = "Menu";
    }

    private async void DropGestureRecognizer_Drop(
        object sender, DropEventArgs e)
    {
        string? origem = _origemEmArrasto;
        PratoDto? prato = _pratoEmArrasto;

        // Limpa referências temporárias de memória
        _pratoEmArrasto = null;
        _origemEmArrasto = null;

        // Fallback secundário via e.Data caso o estado de memória estivesse limpo
        if (prato == null)
        {
            int pratoId = 0;
            try
            {
                string text = await e.Data.GetTextAsync();
                if (!string.IsNullOrEmpty(text) && text.Contains('|'))
                {
                    var partes = text.Split('|');
                    origem = partes[0];
                    int.TryParse(partes[1], out pratoId);
                }
            }
            catch { }

            if (pratoId != 0)
                prato = disponiveis.FirstOrDefault(p => p.Id == pratoId);
        }

        if (origem != "Disponiveis" || prato == null)
            return;

        string? resposta = await DisplayPromptAsync(
            "Adicionar ao cardápio",
            "Informe o valor do prato:",
            accept: "Salvar",
            cancel: "Cancelar",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(resposta))
            return;

        string valorTexto = resposta.Trim().Replace(',', '.');

        if (!decimal.TryParse(
                valorTexto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal valor) || valor < 0)
        {
            await DisplayAlert(
                "Valor inválido",
                "Digite um valor numérico válido.",
                "OK");
            return;
        }

        var pedido = new CardapioRequestDto
        {
            RestauranteId = RestauranteId,
            PratoId = prato.Id,
            Valor = valor
        };

        var resultado = await ApiService.PostAsync<CardapioRequestDto, bool>(
            "cardapio", pedido);

        if (resultado != true)
        {
            await DisplayAlert("Erro", ApiService.Erro, "OK");
            return;
        }

        disponiveis.Remove(prato);
        menu.Add(prato);
    }

    private async void Lixeira_Drop(object sender, DropEventArgs e)
    {
        string? origem = _origemEmArrasto;
        PratoDto? prato = _pratoEmArrasto;

        // Limpa referências temporárias de memória
        _pratoEmArrasto = null;
        _origemEmArrasto = null;

        // Fallback via e.Data
        if (prato == null)
        {
            int pratoId = 0;
            try
            {
                string text = await e.Data.GetTextAsync();
                if (!string.IsNullOrEmpty(text) && text.Contains('|'))
                {
                    var partes = text.Split('|');
                    origem = partes[0];
                    int.TryParse(partes[1], out pratoId);
                }
            }
            catch { }

            if (pratoId != 0)
                prato = menu.FirstOrDefault(p => p.Id == pratoId);
        }

        if (origem != "Menu" || prato == null)
            return;

        bool sucesso = await ApiService.DeleteAsync(
            $"cardapio/{RestauranteId}/{prato.Id}");

        if (!sucesso)
        {
            await DisplayAlert("Erro", ApiService.Erro, "OK");
            return;
        }

        menu.Remove(prato);
        disponiveis.Add(prato);
    }

    private PratoDto? ObterPratoDoSender(object sender)
    {
        if (sender is Element element)
        {
            if (element.BindingContext is PratoDto p) return p;
            if (element.Parent is Element parent && parent.BindingContext is PratoDto pParent) return pParent;
        }
        return null;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

public class CardapioDto
{
    public List<PratoDto> Disponiveis { get; set; } = new();
    public List<PratoDto> Menu { get; set; } = new();
}

public class PratoDto
{
    public int Id { get; set; }

    public string Foto =>
        $"prato{((Math.Max(1, Id) - 1) % 5) + 1}.jpg";
}

public class CardapioRequestDto
{
    public int RestauranteId { get; set; }
    public int PratoId { get; set; }
    public decimal Valor { get; set; }
}