using CardapioInterativo.MOBILE.Helpers;
using CardapioInterativo.MOBILE.Service;

namespace CardapioInterativo.MOBILE.Views;

public partial class LoginView : ContentPage
{
    private int perfilIdUsuarioLogado;
    private int idUsuarioLogado;
    private int tokenAleatorio;
    public LoginView()
    {
        InitializeComponent();
        btnContinuar.IsVisible = false;
        btnContinuar.IsEnabled = false;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(email.Text))
        {
            lblError.TextColor = Colors.Red;
            lblError.Text = "Erro: Preencha o campo de Email.";
            lblError.IsVisible = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(senha.Text))
        {
            lblError.TextColor = Colors.Red;
            lblError.Text = "Erro: Preencha o campo de senha.";
            lblError.IsVisible = true;
            return;
        }

        var emailTratado = email.Text.Trim();
        var senhaTratada = senha.Text.Trim();

        var loginRequest = new loginRequestDto();
        loginRequest.Email = emailTratado;
        loginRequest.Password = senhaTratada;

        var response = await ApiService.PostAsync<loginRequestDto, LoginResponseDTO>("auth/login", loginRequest);

        if (response == null)
        {
            lblError.Text = ApiService.Erro;
            lblError.TextColor = Colors.Red;
            lblError.IsVisible = true;
            return;
        }

        perfilIdUsuarioLogado = response.PerfilId;
        idUsuarioLogado = response.Id;

        stackEmail.IsVisible = false;
        stackSenha.IsVisible = false;
        stackToken.IsVisible = true;
        btnLogin.IsVisible = false;
        btnLogin.IsEnabled = false;

        btnContinuar.IsVisible = true;
        btnContinuar.IsEnabled = true;

        tokenAleatorio = Random.Shared.Next(100, 1000);
        lblExibirToken.Text = $"Seu token de acesso é: {tokenAleatorio}";

    }

    private async void btnContinuar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(lblToken.Text))
        {
            lblError.TextColor = Colors.Red;
            lblError.Text = "Erro: Preencha o campo de token.";
            lblError.IsVisible = true;
            return;
        }
        if (lblToken.Text != tokenAleatorio.ToString())
        {
            lblError.TextColor = Colors.Red;
            lblError.Text = "Erro: Credenciais inválidas.";
            lblError.IsVisible = true;
            return;
        }

        SessaoUsuario.PerfilId = perfilIdUsuarioLogado;
        SessaoUsuario.Idusuario = idUsuarioLogado;

        await Shell.Current.GoToAsync(nameof(RestaurantesView));
    }


    public class loginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponseDTO
    {
        public string Email { get; set; }
        public int PerfilId { get; set; }
        public int Id { get; set; }
    }
}