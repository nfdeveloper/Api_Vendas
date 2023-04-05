using ApiVendas.Models;

namespace ApiVendas.DTOs
{
    public class PedidoDTO
    {
        public Loja? Loja { get; set; }
        public List<Pedido>? Pedidos { get; set; }
    }
}
