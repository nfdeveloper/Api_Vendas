using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.DAO;
using ApiVendas.Db;
using ApiVendas.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiVendas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendaController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Venda>>> Venda()
        {
            VendaDAO oracle = new VendaDAO();

            List<Venda> vendas = await oracle.vendas();

            return vendas;
        }
    }
}