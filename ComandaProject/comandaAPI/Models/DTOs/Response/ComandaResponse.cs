namespace comandaAPI.Models.DTOs.Response
{
    public class ComandaResponse
    {
        public required string Nome { get; set; }
        public required List<PedidosResponse> Pedidos { get; set; }
        public required string Valor { get; set; }
        public required string FormaDePagamento { get; set; }
        public required string Endereço { get; set; }
    }
}