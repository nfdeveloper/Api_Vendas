using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.DAO;
using ApiVendas.Db;
using ApiVendas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVendas.Controllers
{
	[Authorize(AuthenticationSchemes = "Bearer")]
	[ApiController]
	[Route("api/v1/[controller]")]
	public class VendaController : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<List<Venda>>> Venda()
		{
			VendaDAO oracle = new VendaDAO();
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Venda> vendas = await oracle.vendas(int.Parse(cod_fornecedor.Value));
			
			if(vendas is null)
			{
				return NotFound("Nenhuma venda Encontrada!");
			}

			return vendas;
		}
		
		[HttpGet("Loja/{num_loja}")]
		public async Task<ActionResult<List<Venda>>> VendaPorLoja(int num_loja)
		{
			VendaDAO oracle = new VendaDAO();
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Venda> vendas = await oracle.vendasPorLoja(int.Parse(cod_fornecedor.Value), num_loja);

			return vendas;
		}
		
	}
}