using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComandaController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ComandaController> _logger;
    private readonly IComandaService _comandaService;

    public ComandaController(HttpClient httpClient, IConfiguration configuration, ILogger<ComandaController> logger, IComandaService comandaService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _comandaService = comandaService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProcessarComanda([FromBody] ComandaRequest request)
    {
        var resultadoJson = await _comandaService.ProcessarComanda(request);
        return Ok(resultadoJson);
    }
}