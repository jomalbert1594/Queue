using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QueueingApi.Helpers
{
    public class TokenIssuer
    {
        /// <summary>
        /// Generates Access Token for the User
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public string GenerateToken(string deviceId,string baseUrl)
        {
            var userClaims = new List<Claim>
            {
				new Claim(ClaimTypes.Name, deviceId.ToString())
            };

            var secretKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));

            var signingCredentials =
                new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: baseUrl,
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        /// <summary>
        /// Generates a Random Refresh Token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Retrieves the Claims inside the Expired Access Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345")),
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters,
                    out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken)
                    || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityException();

                return principal;
            }
            catch (Exception)
            {
                throw new SecurityException();
            }
        }
    }
}
