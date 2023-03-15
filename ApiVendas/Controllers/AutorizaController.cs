using ApiVendas.DTOs;
using ApiVendas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiVendas.Controllers
{
	[Route("api/v1/[Controller]")]
	[ApiController]
	public class AutorizaController : ControllerBase
	{
		private readonly UserManager<ObrasUser> _userManager;
		private readonly SignInManager<ObrasUser> _signInManager;
		private readonly IConfiguration _configuration;

		public AutorizaController(UserManager<ObrasUser> userManager, SignInManager<ObrasUser> signInManager,
			IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
		}

		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpGet]
		public ActionResult<string> Get()
		{
			return "AutorizaController :: Acessado em : "
				+ DateTime.Now.ToLongTimeString();
		}

		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpPost("register")]
		public async Task<ActionResult> RegisterUser([FromBody] UsuarioDTO model)
		{

			var user = new ObrasUser
			{
				UserName = model.Usuario,
				Email = model.Email,
				Cod_Fornecedor = model.Cod_Fornecedor,
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if(!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			await _signInManager.SignInAsync(user, false);
			return Ok(GeraToken(model));
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] UsuarioLoginDTO userLogin)
		{
			//if(!ModelState.IsValid)
			//{
			//    return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
			//}

			ObrasUser userL = await _userManager.FindByEmailAsync(userLogin.Email);

			if(userL == null)
			{
				return BadRequest("Usuário/Senha inválidos");
			}

			var result = await _signInManager.PasswordSignInAsync(userL.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);

			UsuarioDTO userInfo = new UsuarioDTO()
			{
				Cod_Fornecedor = userL.Cod_Fornecedor,
				Email = userLogin.Email,
				Usuario = userL.UserName,
				Password = userLogin.Password,
				ConfirmPassword = userLogin.Password
			};

			if (result.Succeeded)
			{
				return Ok(GeraToken(userInfo));
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Login Inválido...");
				return BadRequest(ModelState);
			}
		}

		private UsuarioToken GeraToken(UsuarioDTO userInfo)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
				new Claim(ClaimTypes.Name, userInfo.Usuario),
				new Claim("Fornecedor", userInfo.Cod_Fornecedor.ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

			};

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
			var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var expiracao = _configuration["TokenConfiguration:ExpireHours"];
			var expiration = DateTime.UtcNow.AddHours(double.Parse(expiracao));

			JwtSecurityToken token = new JwtSecurityToken(
				issuer: _configuration["TokenConfiguration:Issuer"],
				audience: _configuration["TokenConfiguration:Audience"],
				claims: claims,
				expires: expiration,
				signingCredentials: credenciais
				);

			return new UsuarioToken()
			{
				Authenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = expiration,
				Message = "Token JWT OK"
			};
		}
	}
}
