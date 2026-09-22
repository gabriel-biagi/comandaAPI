namespace comandaAPI.Models.DTOs.Response
{
    public class PedidosResponse
    {
        public required string Item { get; set; }
        public required string Tamanho { get; set; }
        public required List<string> Acompanhamentos { get; set; }
    }
}

