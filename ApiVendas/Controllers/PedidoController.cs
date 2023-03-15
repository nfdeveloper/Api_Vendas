using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.DAO;
using ApiVendas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVendas.Controllers
{
	[Authorize(AuthenticationSchemes = "Bearer")]
	[ApiController]
	[Route("api/v1/[controller]")]
	public class PedidoController : ControllerBase
	{
		
		[HttpGet]
		public async Task<List<Pedido>> Pedidos()
		{
			PedidoDAO pedido = new DAO.PedidoDAO();			
			VendaDAO oracle = new VendaDAO();
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Pedido> pedidos = await pedido.PedidosPorFornecedor(int.Parse(cod_fornecedor.Value));
			
			return pedidos;
		}
		
		[HttpGet("{num_loja}")]
		public async Task<List<Pedido>> PedidosPorLoja(int num_loja)
		{
			PedidoDAO pedido = new DAO.PedidoDAO();			
			VendaDAO oracle = new VendaDAO();
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Pedido> pedidos = await pedido.PedidosPorFornecedorPorLoja(int.Parse(cod_fornecedor.Value), num_loja);
			
			return pedidos;
		}
		
	}
}