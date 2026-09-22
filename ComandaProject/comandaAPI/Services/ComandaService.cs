using comandaAPI.Models.DTOs.Request;
using comandaAPI.Models.DTOs.Response;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using comandaAPI.Services.Interfaces;
using comandaAPI.Domain.Exception;

namespace comandaAPI.Services;

public class ComandaService : IComandaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ComandaService> _logger;
    public ComandaService(HttpClient httpClient, IConfiguration configuration, ILogger<ComandaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task<ComandaResponse> ProcessarComanda(ComandaRequest comandaRequest)
    {
        string apiKey = _configuration["GroqApiKey"];
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        var requestBody = new
        {
            model = "openai/gpt-oss-120b",
            messages = new[]
            {
                new { role = "system", content = @"Você é um extrator de pedidos de uma lanchonete/restaurante.

Analise TODAS as mensagens, considerando o contexto e a ordem em que foram enviadas. Uma mensagem pode complementar outra.

Responda EXCLUSIVAMENTE com JSON válido, sem markdown ou explicações, seguindo este formato:

{
  ""Nome"": """",
  ""Pedidos"": [
    {
      ""Item"": """",
      ""Tamanho"": """",
      ""Acompanhamentos"": []
    }
  ],
  ""Valor"": """",
  ""FormaDePagamento"": """",
  ""Endereço"": """"
}

REGRAS:
1. Nome
- Extraia o nome do cliente.
- Se não informado: ""Não Informado"".
2. Pedidos
- Sempre use um array.
- Cada produto diferente deve ser um objeto separado.
- Cada pedido possui seu próprio Item, Tamanho e Acompanhamentos.
- Não misture informações entre pedidos.
3. Item
- Extraia o produto solicitado, preservando a informação fornecida.
- Exemplos: Copo, Marmita, Açaí, Pizza.
- Se não puder identificar: ""Não Informado"".
4. Tamanho
- Extraia o tamanho quando estiver explicitamente informado junto ao pedido.
- Preserve o valor informado.
- Se não estiver claramente informado, use ""Não Informado"".
- Não tente inferir o tamanho com base no contexto.
5. Acompanhamentos
- Cada pedido possui seu próprio array.
- Associe os acompanhamentos ao pedido correto usando contexto e ordem das mensagens.
- Separe múltiplos acompanhamentos em elementos diferentes.
- Se não houver: [].
- Não invente acompanhamentos.
6. Contexto e múltiplos pedidos
- Considere todas as mensagens como uma única comanda.
- Mensagens curtas ou incompletas podem complementar pedidos anteriores, principalmente para acompanhamentos.
- Não use mensagens separadas para inferir tamanhos.
- Quando houver vários produtos, associe os acompanhamentos ao pedido correto usando contexto e ordem das mensagens.
7. Valor
- Extraia o valor total informado.
- Preserve o valor encontrado.
- Se não informado: ""Não Informado"".
8. FormaDePagamento
- Extraia formas como Pix, dinheiro, cartão etc.
- Se não informado: ""Não Informado"".
9. Endereço
- Extraia rua, número, bairro, complemento, cidade etc.
- Preserve as informações fornecidas.
- Se não informado: ""Não Informado"".
10. Não invente
- Use somente informações presentes nas mensagens ou claramente determinadas pelo contexto.
- Quando uma informação não puder ser determinada com segurança, use ""Não Informado"" ou []." },
                new { role = "user", content = comandaRequest.Text }
            },
            response_format = new { type = "json_object" }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Erro ao processar na API do Groq. Status Code: {StatusCode}, Reason: {ReasonPhrase}, Response: {Response}",
                response.StatusCode,
                response.ReasonPhrase,
                errorContent);

            throw new ExternalServiceException(
                "Erro ao processar na API da Groq.",
                response.StatusCode);
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        string resultadoText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        _logger.LogDebug("Processing comanda request");

        var resultadoJson = JsonSerializer.Deserialize<ComandaResponse>(resultadoText ?? throw new InvalidOperationException());
        return resultadoJson;
    }
}