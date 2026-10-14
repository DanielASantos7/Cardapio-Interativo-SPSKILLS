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

        public static async Task<bool> PostSemBodyAsync(string endpoint)
        {
            Erro = string.Empty;

            try
            {
                var response = await Http.PostAsync(endpoint, null);

                if (response.IsSuccessStatusCode)
                    return true;

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "Você não tem permissão para realizar esta ação.",
                    HttpStatusCode.NotFound => "Restaurante não encontrado",
                    HttpStatusCode.BadRequest => "O restaurante já está ativo ou a operação é inválida.",
                    _ => $"Erro no servidor. Código:{(int)response.StatusCode}"
                };
                return default;
            }
            catch (Exception ex)
            {
                Erro = $"Falha de conexão: {ex.Message}";
                return default;
            }
        }
        public static async Task<T?> GetAsync<T>(string endpoint)
        {
            Erro = string.Empty;

            try
            {
                var response = await Http.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsongOptions);
                }


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

        public static async Task<bool> DeleteAsync(string endpoint)
        {
            Erro = string.Empty;

            try
            {
                var response = await Http.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                    return true;

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "Você não tem permissão para realizar esta ação.",
                    HttpStatusCode.NotFound => "Restaurante não encontrado",
                    HttpStatusCode.BadRequest => "O restaurante já foi excluído ou a operação é inválida.",
                    _ => $"Erro no servidor. Código:{(int)response.StatusCode}"
                };
                return false;
            }
            catch (Exception ex)
            {
                Erro = $"Falha de conexão: {ex.Message}";
                return false;
            }
        }
    }
}
