using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using comandaAPI.Models.DTOs.Request;
using comandaAPI.Services.Interfaces;   

namespace comandaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComandaController : ControllerBase
{
    private readonly IComandaService _comandaService;

    public ComandaController(IComandaService comandaService)
    {
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