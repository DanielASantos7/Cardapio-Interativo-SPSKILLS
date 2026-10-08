namespace CardapioInterativo.MOBILE.Views;

public partial class RestaurantesView : ContentPage
{
    public RestaurantesView()
    {
        InitializeComponent();

        CarregarRestaurantes();
    }


    private void CarregarRestaurantes()
    {
        var restaurantes = new List<RestauranteMock>
        {
            new RestauranteMock
            {
                Id = 1,
                Nome = "Restaurante 1",
                Descricao = "Restaurante especializado em pratos deliciosos.",
                Cidade = "São Paulo",
                Tipo = "Italiana",
                Avaliacao = 5
            },

            new RestauranteMock
            {
                Id = 2,
                Nome = "Restaurante 2",
                Descricao = "Ambiente agradável com diversas opções.",
                Cidade = "São Paulo",
                Tipo = "Brasileira",
                Avaliacao = 4
            },

            new RestauranteMock
            {
                Id = 3,
                Nome = "Restaurante 3",
                Descricao = "Pratos preparados com ingredientes selecionados.",
                Cidade = "Campinas",
                Tipo = "Japonesa",
                Avaliacao = 3
            },

            new RestauranteMock
            {
                Id = 4,
                Nome = "Restaurante 4",
                Descricao = "Uma experiência gastronômica completa.",
                Cidade = "Limeira",
                Tipo = "Hamburgueria",
                Avaliacao = 5
            },

            new RestauranteMock
            {
                Id = 5,
                Nome = "Restaurante 5",
                Descricao = "Sabores especiais para todos os momentos.",
                Cidade = "Santos",
                Tipo = "Mexicana",
                Avaliacao = 2
            },

            new RestauranteMock
            {
                Id = 6,
                Nome = "Restaurante 6",
                Descricao = "Comida saborosa e ambiente familiar.",
                Cidade = "São Paulo",
                Tipo = "Brasileira",
                Avaliacao = 4
            }
        };

        cvRestaurantes.ItemsSource = restaurantes;
    }
}


public class RestauranteMock
{
    public int Id { get; set; }

    public string Nome { get; set; }

    public string Descricao { get; set; }

    public string Cidade { get; set; }

    public string Tipo { get; set; }

    public int Avaliacao { get; set; }


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