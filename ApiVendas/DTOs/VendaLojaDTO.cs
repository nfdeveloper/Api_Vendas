using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.Models;

namespace ApiVendas.DTOs
{
	public class VendaLojaDTO
	{
		public string? Loja { get; set; }
		public List<Venda>? Vendas {get;set;}
	}
}