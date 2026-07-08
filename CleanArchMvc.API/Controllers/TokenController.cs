using CleanArchMvc.API.Models;
using CleanArchMvc.Domain.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchMvc.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IAuthenticate _authentication;
    private readonly IConfiguration _configuration;

    public TokenController(IAuthenticate authentication, IConfiguration configuration)
    {
        _authentication = authentication ??
            throw new ArgumentNullException(nameof(authentication));
        _configuration = configuration;
    }

    [HttpPost("LoginUsser")]
    public async Task<ActionResult<UserToken>> Login([FromBody] LoginModel userInfo)
    {
        var result = await _authentication.Authenticate(userInfo.Email, userInfo.Password);

        if(result)
        {
            return GenerateToken(userInfo);
            return Ok($"User {userInfo.Email} login successfully");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return BadRequest(ModelState);
        }

    }

    [HttpPost("CreateUser")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult> CreateUser([FromBody] LoginModel userInfo)
    {
        var result = await _authentication.RegisterUser(userInfo.Email, userInfo.Password);

        if (result)
        {
            return Ok($"User {userInfo.Email} created successfully");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return BadRequest(ModelState);
        }
    }

    private UserToken GenerateToken(LoginModel userInfo)
    {
        //declarações do usuário

        var claims = new[]
        {
            new Claim("email", userInfo.Email),
            new Claim("meuvalor", "o que você quiser"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        //Gerar chave privada para assinar o token
        var privateKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

        //Gerar a assinatura digital
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

        //Definir o tempo de expiração do token
        var expiration = DateTime.UtcNow.AddMinutes(10);

        //Gerar o token
        JwtSecurityToken token = new JwtSecurityToken(
            //Emissor do token
            issuer: _configuration["Jwt:Issuer"],
            //(Audiencia) Destinatário do token
            audience: _configuration["Jwt:Audience"],
            //Claims do usuário
            claims: claims,
            //Tempo de expiração
            expires: expiration,
            //Assinatura digital do token
            signingCredentials: credentials);

        var secret = _configuration["Jwt:SecretKey"];

        return new UserToken()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration
        };
    }
}
