using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiVendas.Models
{
    public class Loja
    {
        public string? Cod_Loja { get; set; }
        public string? Nome { get; set; }
        public string? Cnpj { get; set; }
        public string? Cep { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Rua { get; set; }
    }
}