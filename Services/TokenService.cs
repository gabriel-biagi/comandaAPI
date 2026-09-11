using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using comandaAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace comandaAPI.Services;

public class TokenService : ITokenService
{
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration config)
    {
        var key = config.GetSection("JWT:SecretKey").Value ?? 
                  throw new InvalidOperationException("Invalid Secret Key");
        
        var privateKey = Encoding.UTF8.GetBytes(key);
        
        var signingCredentials =
            new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(config.GetSection("JWT:TokenValidityInMinutes").Value)),

            Audience = config.GetSection("JWT:ValidAudience").Value,
            Issuer = config.GetSection("JWT:ValidIssuer").Value,

            SigningCredentials = signingCredentials
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        var secureRandomBytes = new Byte[128];
        
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        
        randomNumberGenerator.GetBytes(secureRandomBytes);
        
        return Convert.ToBase64String(secureRandomBytes);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config)
    {
        var secretKey = config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid Key");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityException("Invalid Token");
        }
        
        return principal;
    }
}