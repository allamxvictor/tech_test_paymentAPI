using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tech_test_payment_api.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public string StatusDaVenda { get; set; }
        public string IdentificadorDoPedido { get; set; }
        public DateTime Data { get; set; }
        public List<ItemVendido> ProdutosVendidos { get; set; }
        public Vendedor Vendedor { get; set; }
    }
}