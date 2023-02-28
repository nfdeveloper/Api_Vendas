namespace ApiVendas.DTOs
{
    public class UsuarioDTO
    {
        public string? Usuario { get; set; }
        public string? Email { get; set; }  
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public int Cod_Fornecedor { get; set; }
    }
}
