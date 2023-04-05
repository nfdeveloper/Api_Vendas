using ApiVendas.DAO;
using ApiVendas.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiVendas.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LojaController : Controller
    {
        /// <summary>
        /// Listagem de todas as Lojas.
        /// </summary>
        [HttpGet]
        public async Task<List<Loja>> Lojas()
        {
            LojaDAO db = new LojaDAO();    
            List<Loja> lojas = await db.Lojas();    
            return lojas;
        }
    }
}
