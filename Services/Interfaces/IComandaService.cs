using comandaAPI.Models.DTOs.Request;
using comandaAPI.Models.DTOs.Response;

namespace comandaAPI.Services.Interfaces;

public interface IComandaService
{
    Task<ComandaResponse> ProcessarComanda(ComandaRequest comandaRequest);
}