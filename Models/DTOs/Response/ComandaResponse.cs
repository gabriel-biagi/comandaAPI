namespace comandaAPI.Models.DTOs.Response
{
    public class ComandaResponse
    {
        public required string Nome { get; set; }
        public required string Pedido { get; set; }
        public required string Acompanhamentos { get; set; }
        public required string Valor { get; set; }
        public required string FormaDePagamento { get; set; }
        public required string Endereço { get; set; }
    }
}