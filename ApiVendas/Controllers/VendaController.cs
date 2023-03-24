using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.DAO;
using ApiVendas.Db;
using ApiVendas.DTOs;
using ApiVendas.Models;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVendas.Controllers
{
	[Authorize(AuthenticationSchemes = "Bearer")]
	[ApiController]
	[Route("api/v1/[controller]")]
	public class VendaController : ControllerBase
	{
		private readonly IWebHostEnvironment _webHostEnv;

		public VendaController(IWebHostEnvironment webHostEnv)
		{
			_webHostEnv = webHostEnv;
		}

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
		
		[HttpGet("{data_inicial}/{data_final}")]	
		public async Task<ActionResult<List<Venda>>> VendaPorData(string data_inicial,string data_final)
		{
			DateTime data_inicial_check = DateTime.Parse(data_inicial);
			DateTime data_final_check = DateTime.Parse(data_final);
			
			if(data_final_check < data_inicial_check)
			{
				return BadRequest("A data final não pode ser menor que a data inicial");
			}
			
			if(data_inicial_check.Year < 2022)
			{
				return BadRequest("Data inicial não pode ser menor que 01/01/2021");
			}
			
			VendaDAO oracle = new VendaDAO();
			var cod_fornecedor = User.Claims.First(c => c.Type == "Fornecedor");
			List<Venda> vendas = await oracle.vendasPorData(int.Parse(cod_fornecedor.Value), data_inicial,data_final);
			
			return vendas;
			
		}
		
		// [HttpGet("Pdf/Pr")]
		// public async Task<ActionResult<List<VendaLojaDTO>>> VendaPreparePdf()
		// {
		// 	VendaDAO oracle = new VendaDAO();
		// 	List<Venda> vendas = await oracle.vendas(10);
		// 	List<VendaLojaDTO> vendasPdf = await VendaParaPdf(vendas);
		// 	return vendasPdf;
		// }

		private async Task<List<VendaLojaDTO>> VendaParaPdf(List<Venda> vendas)
		{
			List<VendaLojaDTO> vendaLojaPdf = new List<VendaLojaDTO>();
			LojaDAO dbLoja = new LojaDAO();
		
			List<Loja> lojas = await dbLoja.Lojas();
			
			//Populando o DTO
			foreach(Loja loja in lojas)
			{
				VendaLojaDTO venda = new VendaLojaDTO()
				{
					Loja = loja.Nome,
					Vendas = new List<Venda>()
				};
				
				vendaLojaPdf.Add(venda);
			}
			
			foreach(Venda vend in vendas)
			{	
				foreach(VendaLojaDTO v in vendaLojaPdf)
				{
					if(vend.Loja == v.Loja)
					{
						v.Vendas?.Add(vend);
					}
				}
			}
			
			vendaLojaPdf.RemoveAll(x => x.Vendas?.Count() == 0);
			
			return vendaLojaPdf;
			
			
		}
		
		[HttpGet("GerarTemplatePdf")]
		public async Task<ActionResult> GerarTemplatePdf()
		{
			VendaDAO oracle = new VendaDAO();
			var caminhoReport = Path.Combine("Reports\\VendaFornecedorPdf.frx");
			var reportFile = caminhoReport;
			var freport = new FastReport.Report();
			var vendas = await oracle.vendas(10);
			List<VendaLojaDTO> vendasPdf = await VendaParaPdf(vendas);
			freport.Dictionary.RegisterBusinessObject(vendasPdf, "vendasPdf", 10,true);
			freport.Report.Save(reportFile);
			return Ok($" Relatório gerado : {caminhoReport}");
		}
		
		[HttpGet("Gerar/Pdf")]
		public async Task<ActionResult> GerarPdf()
		{
			VendaDAO oracle = new VendaDAO();
			var caminhoReport = Path.Combine("Reports\\VendaFornecedorPdf.frx");
			var reportFile = caminhoReport;
			
			var freport = new FastReport.Report();
			var vendas = await oracle.vendas(10);
			List<VendaLojaDTO> vendasPdf = await VendaParaPdf(vendas);
			freport.Report.Load(reportFile);
			freport.Dictionary.RegisterBusinessObject(vendasPdf, "vendasPdf", 10,true);
			freport.Prepare();
			
			//var pdfExport = new PDFSimpleExport();
			// using MemoryStream ms = new MemoryStream();
			// pdfExport.Export(freport, ms);
			// ms.Flush();
			// return File(ms.ToArray(), "application/pdf");
			byte[] reportArray = null;
			
			using (MemoryStream ms = new MemoryStream())
			{
				var pdfExport = new PDFSimpleExport();
				pdfExport.Export(freport.Report,ms);
				ms.Flush();
				reportArray	= ms.ToArray();
			}
			
			return File(reportArray, "application/pdf","Venda.pdf");
		}
	}
}