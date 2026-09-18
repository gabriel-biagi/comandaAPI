using comandaAPI.Models.DTOs.Requests;
using comandaAPI.Models.DTOs.Response;

namespace comandaAPI.Services.Interfaces;

public interface IComandaService
{
    Task<ComandaResponse> ProcessarComanda(ComandaRequest comandaRequest);
}