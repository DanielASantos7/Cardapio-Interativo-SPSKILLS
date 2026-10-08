using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CardapioInterativo.MOBILE.Service
{
    public static class ApiService
    {

        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri("http://10.106.130.81:5054/api/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        private static readonly JsonSerializerOptions _jsongOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static string Erro { get; private set; } = string.Empty;

        public static async Task<TRes?> PostAsync<TReq, TRes>(string endpoint, TReq dados)
        {
            Erro = string.Empty;

            try
            {
                var response = await Http.PostAsJsonAsync(endpoint, dados);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<TRes>();

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Credenciais inválidas.",
                    HttpStatusCode.NotFound => "Endereço não encontado na API.",
                    HttpStatusCode.BadRequest => "Dados preenchidos incorretamente.",
                    _ => $"Erro no servidor (Código: {(int)response.StatusCode}"
                };
                return default;
            }
            catch (Exception ex)
            {
                Erro = $"Falha de conexão: {ex.Message}";
                return default;
            }
        }
    }
}
