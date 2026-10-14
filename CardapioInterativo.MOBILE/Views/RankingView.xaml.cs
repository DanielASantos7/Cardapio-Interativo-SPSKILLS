using CardapioInterativo.MOBILE.Service;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CardapioInterativo.MOBILE.Views;

public partial class RankingView : ContentPage
{
    private readonly ObservableCollection<PratoRankingMobileDto> pratos = new();

    private List<RestauranteRankingMobileDto> ranking = new();

    private bool mostrandoPratos;

    public RankingView()
    {
        InitializeComponent();

        cvPratosRanking.ItemsSource = pratos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarRanking();
    }

    private async Task CarregarRanking()
    {
        try
        {
            var resposta =
                await ApiService.GetAsync<List<RestauranteRankingMobileDto>>(
                    "restaurantes/ranking");

            if (resposta == null)
            {
                await DisplayAlert(
                    "Erro ao carregar ranking",
                    ApiService.Erro ?? "Não foi possível carregar o ranking.",
                    "OK");

                return;
            }

            // Ordena os restaurantes pela quantidade de curtidas.
            ranking = resposta
                .OrderByDescending(r => r.TotalCurtidas)
                .ThenBy(r => r.Nome)
                .Take(3)
                .ToList();

            // A cor é atribuída ao restaurante uma única vez.
            // Ela acompanha o card nas trocas de posição.
            for (int i = 0; i < ranking.Count; i++)
            {
                ranking[i].Posicao = i + 1;

                ranking[i].CorCard = i switch
                {
                    0 => "#D4A017", // Golden Wheat
                    1 => "#7A8C69", // Sage Green
                    2 => "#C75D4D", // Terracotta
                    _ => "#D4A017"
                };
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AtualizarCards();
                PosicionarCards();
            });

            // Solicita nova atualização das imagens depois que
            // o layout inicial já teve oportunidade de ser calculado.
            await Task.Delay(150);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                RecarregarImagens();

                layoutRanking.InvalidateMeasure();

                imgPrincipal.InvalidateMeasure();
                imgSegundo.InvalidateMeasure();
                imgTerceiro.InvalidateMeasure();
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Erro",
                $"Não foi possível carregar o ranking: {ex.Message}",
                "OK");
        }
    }

    // Calcula o tamanho e a posição dos cards para celular e tablet.
    private void LayoutRanking_SizeChanged(object? sender, EventArgs e)
    {
        PosicionarCards();
    }

    private void PosicionarCards()
    {
        double largura = layoutRanking.Width;
        double altura = layoutRanking.Height;

        if (largura <= 0 || altura <= 0)
            return;

        // Limita o tamanho para evitar cards exageradamente
        // grandes em tablets.
        double larguraCard = Math.Min(largura * 0.68, 410);

        double alturaCard = Math.Min(
            altura * 0.80,
            larguraCard * 1.32);

        double centroX = largura / 2;

        double xPrincipal = centroX - larguraCard / 2;
        double yPrincipal = (altura - alturaCard) / 2;

        double larguraSecundario = larguraCard * 1.02;
        double alturaSecundario = alturaCard * 0.96;
        double ySecundario = yPrincipal + alturaCard * 0.035;

        // Os cards laterais ficam atrás do principal.
        double xEsquerda = 0;
        double xDireita = largura - larguraSecundario;

        AbsoluteLayout.SetLayoutBounds(
            cardTerceiro,
            new Rect(
                xEsquerda,
                ySecundario,
                larguraSecundario,
                alturaSecundario));

        AbsoluteLayout.SetLayoutBounds(
            cardSegundo,
            new Rect(
                xDireita,
                ySecundario,
                larguraSecundario,
                alturaSecundario));

        AbsoluteLayout.SetLayoutBounds(
            cardPrincipal,
            new Rect(
                xPrincipal,
                yPrincipal,
                larguraCard,
                alturaCard));

        cardTerceiro.Rotation = -7;
        cardSegundo.Rotation = 7;
        cardPrincipal.Rotation = 0;

        // Redimensiona as imagens proporcionalmente ao card.
        imgPrincipal.HeightRequest = alturaCard * 0.36;
        imgSegundo.HeightRequest = alturaCard * 0.34;
        imgTerceiro.HeightRequest = alturaCard * 0.34;
    }

    private void AtualizarCards()
    {
        RestaurarFrente();

        PreencherCard(
            ranking.ElementAtOrDefault(0),
            imgPrincipal,
            lblNomePrincipal,
            lblDescricaoPrincipal,
            lblEnderecoPrincipal,
            lblPosicaoPrincipal,
            cardPrincipal);

        PreencherCard(
            ranking.ElementAtOrDefault(1),
            imgSegundo,
            lblNomeSegundo,
            lblDescricaoSegundo,
            lblEnderecoSegundo,
            lblPosicaoSegundo,
            cardSegundo);

        PreencherCard(
            ranking.ElementAtOrDefault(2),
            imgTerceiro,
            lblNomeTerceiro,
            lblDescricaoTerceiro,
            lblEnderecoTerceiro,
            lblPosicaoTerceiro,
            cardTerceiro);

        lblTotalCurtidas.Text = ranking.Count > 0
            ? $"♥ {ranking[0].TotalCurtidas} curtidas"
            : string.Empty;

        cardSegundo.IsVisible = ranking.Count >= 2;
        cardTerceiro.IsVisible = ranking.Count >= 3;
    }

    private static void PreencherCard(
        RestauranteRankingMobileDto? restaurante,
        Image imagem,
        Label nome,
        Label descricao,
        Label endereco,
        Label posicao,
        Border card)
    {
        if (restaurante == null)
        {
            card.BackgroundColor = Color.FromArgb("#D9D9D9");

            imagem.Source = ImageSource.FromFile("SemImagem.png");

            nome.Text = "Sem restaurante";
            descricao.Text = string.Empty;
            endereco.Text = string.Empty;
            posicao.Text = string.Empty;

            return;
        }

        // A cor vem do próprio restaurante, e não da posição visual.
        card.BackgroundColor = Color.FromArgb(restaurante.CorCard);

        imagem.Source = ImageSource.FromFile(
            string.IsNullOrWhiteSpace(restaurante.Imagem)
                ? "SemImagem.png"
                : restaurante.Imagem);

        nome.Text = restaurante.Nome;
        descricao.Text = restaurante.Descricao;
        endereco.Text = restaurante.Endereco;

        // A identificação original acompanha o card.
        posicao.Text = $"#{restaurante.Posicao}";
    }

    // Reaplica as imagens após o layout inicial.
    private void RecarregarImagens()
    {
        AplicarImagem(imgPrincipal, ranking.ElementAtOrDefault(0));
        AplicarImagem(imgSegundo, ranking.ElementAtOrDefault(1));
        AplicarImagem(imgTerceiro, ranking.ElementAtOrDefault(2));
    }

    private static void AplicarImagem(
        Image imagem,
        RestauranteRankingMobileDto? restaurante)
    {
        string arquivo =
            restaurante == null ||
            string.IsNullOrWhiteSpace(restaurante.Imagem)
                ? "SemImagem.png"
                : restaurante.Imagem;

        imagem.Source = ImageSource.FromFile(arquivo);
        imagem.InvalidateMeasure();
    }

    // Troca os restaurantes sem alterar suas cores originais.
    private void TrocarComPrincipal(int indice)
    {
        if (indice < 1 || indice >= ranking.Count)
            return;

        (ranking[0], ranking[indice]) =
            (ranking[indice], ranking[0]);

        AtualizarCards();
        PosicionarCards();
    }

    private void CardSegundo_Tapped(object sender, TappedEventArgs e)
    {
        TrocarComPrincipal(1);
    }

    private void CardTerceiro_Tapped(object sender, TappedEventArgs e)
    {
        TrocarComPrincipal(2);
    }

    // Mostra ou esconde o verso do card principal.
    private void CardPrincipal_Tapped(object sender, TappedEventArgs e)
    {
        if (ranking.Count == 0)
            return;

        mostrandoPratos = !mostrandoPratos;

        faceFrente.IsVisible = !mostrandoPratos;
        faceVerso.IsVisible = mostrandoPratos;

        if (!mostrandoPratos)
            return;

        pratos.Clear();

        var pratosOrdenados = (
            ranking[0].Pratos ?? new List<PratoRankingMobileDto>())
            .OrderByDescending(p => p.Curtidas)
            .ThenBy(p => p.Nome);

        foreach (var prato in pratosOrdenados)
        {
            pratos.Add(prato);
        }
    }

    private void RestaurarFrente()
    {
        mostrandoPratos = false;

        faceFrente.IsVisible = true;
        faceVerso.IsVisible = false;

        pratos.Clear();
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

// DTO dos restaurantes
public class RestauranteRankingMobileDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public int TotalCurtidas { get; set; }

    public List<PratoRankingMobileDto> Pratos { get; set; } = new();

    // Propriedades locais da interface, não provenientes da API.
    [JsonIgnore]
    public int Posicao { get; set; }

    [JsonIgnore]
    public string CorCard { get; set; } = "#D4A017";

    public string Imagem =>
        $"Restaurante{((Math.Max(1, Id) - 1) % 4) + 1}.jpg";
}

// DTO dos pratos exibidos no verso do card
public class PratoRankingMobileDto
{
    public string Nome { get; set; } = string.Empty;

    public int Curtidas { get; set; }
}