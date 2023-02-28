using Microsoft.AspNetCore.Identity;

namespace ApiVendas.Models
{
    public class ObrasUser : IdentityUser
    {
        public int Cod_Fornecedor { get; set; }
    }
}
