using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Route("api")]
    public class AuthController : MainController
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, 
                              IOptions<AppSettings> appSettings ,INotificador notificador) : base (notificador)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("nova-conta")]
        public async Task<IActionResult> Registrar(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = registerViewModel.Email, //poderia ser o nome do usuário, login...etc, estamos usando o email para simplificar
                Email = registerViewModel.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            if (result.Succeeded)
            {
                //Faz o login do usuário se o cadastro ocorrer com sucesso.
                await _signInManager.SignInAsync(user, false); //persiste=false => Para lembrar o usuário no próximo login. Padrão: false (Não se aplica para API)
                
                return CustomResponse(await GerarToken(registerViewModel.Email));
            }

            foreach (var item in result.Errors)
            {
                NotificarErro(item.Description);
            }

            return CustomResponse(registerViewModel);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserViewModel login)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            //persiste=false => Para lembrar o usuário no próximo login. Padrão: false (Não se aplica para API)
            //lockoutOnFailUre=true => Bloqueia o usuário depois de "x" tentativas inválidas.
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, true);

            if (result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas.");
                return CustomResponse(login);
            }

            if (result.Succeeded)
                return CustomResponse(await GerarToken(login.Email));

            NotificarErro("Usuário ou Senha inválidas.");
            return CustomResponse(login);
        }

        private async Task<LoginResponseViewModel> GerarToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            //As claims podem ser adicionadas conforme a necessidade. Essas são apenas um exemplo.
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id)); //Id do usuário
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email)); //Email do usuário
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); //Id do token
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString())); //Não válido antes da data.  (Segundos que relativam a data)
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)); //Data de geração do token. (Segundos que relativam a data)

            //Add as roles do usuário nas claims
            foreach (var role in roles)
                claims.Add(new Claim("role", role));

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = identityClaims, //No Subject adicionamos a lista de claims do usuário no token.
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);

            /* Se for retornar apenas o token, retornar apenas o encodedToken 
               return encodedToken */

            var response = new LoginResponseViewModel
            {
                AccessToken = encodedToken,
                ExpiresIn = Convert.ToInt64(TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds),
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        //Metodo padrão 
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
