using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiVendas.Models
{
	public class Pedido
	{
		public int NumPedido { get; set; }
		public string? Emissao { get; set; }
		public string? Loja { get; set; }
		public string? Situacao { get; set; }
		public string? Cod_Produto { get; set; }
		public string? Cod_Fornecedor { get; set; }
		public string? Eans { get; set; }
		public string? Descricao { get; set; }
		public string? Embalagem { get; set; }
		public string? Quantidade { get; set; }
		public double Valor_Unitario { get; set; }
		public double Valor_Item { get; set; }
		public double Icms_EmbItem { get; set; }
		public double Ipi_EmbItem { get; set; }
		public double Custo_Bruto { get; set; }
	}
}