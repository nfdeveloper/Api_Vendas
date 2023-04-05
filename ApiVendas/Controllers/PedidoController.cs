using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.DAO;
using ApiVendas.DTOs;
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
        /// <summary>
        /// Lista os Pedidos de todas as Lojas feitos nos últimos 15 dias.
        /// </summary>
        [HttpGet]
		public async Task<List<PedidoDTO>> Pedidos()
		{
			PedidoDAO pedido = new DAO.PedidoDAO();		
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Pedido> pedidos = await pedido.PedidosPorFornecedor(int.Parse(cod_fornecedor.Value));
			
			List<PedidoDTO> pedidosRender = await ParsePedidoTodasLojas(pedidos);

			return pedidosRender;
		}

        /// <summary>
        /// Lista os Pedidos de uma Loja específica feitos nos últimos 15 dias.
        /// </summary>
        [HttpGet("{num_loja}")]
		public async Task<ActionResult<PedidoDTO>> PedidosPorLoja(int num_loja)
		{
			PedidoDAO pedido = new DAO.PedidoDAO();	
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Pedido> pedidos = await pedido.PedidosPorFornecedorPorLoja(int.Parse(cod_fornecedor.Value), num_loja);
			
			if(pedidos is null)
			{
				return BadRequest("Pedidos não encontrados.");
			}

			PedidoDTO pedidoRender = await ParsePedidoLoja(pedidos, num_loja);

			return pedidoRender;
		}

		private async Task<List<PedidoDTO>> ParsePedidoTodasLojas(List<Pedido> pedidos)
		{
			List<PedidoDTO> pedidosRender = new List<PedidoDTO>();	

			LojaDAO dbLoja = new LojaDAO();



			List<Loja> lojas = await dbLoja.Lojas();

			foreach(Loja lj in lojas)
			{
				PedidoDTO ped = new PedidoDTO()
				{
					Loja = lj,
					Pedidos = new List<Pedido>()
				};

				pedidosRender.Add(ped);	
			}

			foreach(Pedido ped in pedidos)
			{
				foreach(PedidoDTO pedDto in pedidosRender)
				{
					if(ped.Loja == pedDto.Loja.Cod_Loja)
					{
						pedDto.Pedidos.Add(ped);
					}
				}
			}

			return pedidosRender;	
		}
		
		private async Task<PedidoDTO> ParsePedidoLoja(List<Pedido> pedidos, int num_loja)
		{
			LojaDAO dbLoja = new LojaDAO();

			Loja loja = await dbLoja.LojaPorNumero(num_loja);

			PedidoDTO pedidosRender = new PedidoDTO()
			{
				Loja = loja,
				Pedidos = new List<Pedido>()
			};

			foreach(Pedido pedido in pedidos)
			{
				pedidosRender.Pedidos.Add(pedido);
			}

			return pedidosRender;
		}
	}
}