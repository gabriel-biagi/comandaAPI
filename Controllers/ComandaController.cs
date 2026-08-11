using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using comandaAPI.Models;

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComandaController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ComandaController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessarComanda([FromBody] ComandaRequest request)
    {
        string apiKey = _configuration["GroqApiKey"];

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "system", content = @"VVocê é um extrator de comandas. Extraia as informações do texto e responda EXCLUSIVAMENTE no formato abaixo, sem markdown (sem usar asteriscos), sem saudações, sem introduções e sem mensagens adicionais ao final:

Nome: 
Pedido: 
Acompanhamentos: 
Valor: 
Forma de pagamento: 
Endereço: 

Se alguma informação não estiver presente na mensagem, deixe o campo após os dois pontos em branco." },
                new { role = "user", content = request.Text }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody), 
            Encoding.UTF8, 
            "application/json"
        );

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Erro ao processar na API do Groq.");
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(jsonResponse);
        string resultadoText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Ok(new { result = resultadoText });
    }
}