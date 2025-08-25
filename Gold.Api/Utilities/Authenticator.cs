using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Gold.Api.Utilities
{
    public class Authenticator
    {
        public static string Key { get; set; }

        public static string GetToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Authenticator.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var res = tokenHandler.WriteToken(token);
            return res;
        }

        public static string GetVendorToken(string vendorId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Authenticator.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, vendorId)
                }),
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var res = tokenHandler.WriteToken(token);
            return res;
        }


        private static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;
                var key = Encoding.ASCII.GetBytes(Authenticator.Key);
                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                return principal;
            }

            catch (Exception)
            {
                return null;
            }
        }
        

        

        public static bool ValidateToken(string token, out string id)
        {
            id = null;

            var simplePrinciple = GetPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            id = usernameClaim?.Value;

            if (string.IsNullOrEmpty(id))
                return false;

            return true;
        }

        public static async Task<ValidateTokenAsyncResult> ValidateTokenAsync(string token)
        {

            return await Task.Run(() =>
            {
                var result = new ValidateTokenAsyncResult
                {
                    Id = null,
                    Validated = false
                };

                var simplePrinciple = GetPrincipal(token);
                var identity = simplePrinciple.Identity as ClaimsIdentity;

                if (identity == null)
                {
                    result.Validated = false;
                    return result;
                }

                if (!identity.IsAuthenticated)
                {
                    result.Validated = false;
                    return result;
                }

                var usernameClaim = identity.FindFirst(ClaimTypes.Name);
                result.Id = usernameClaim?.Value;

                if (string.IsNullOrEmpty(result.Id))
                {
                    result.Validated = false;
                    return result;
                }
                result.Validated = true;
                return result;
            });
        }


        public class ValidateTokenAsyncResult
        {
            public bool Validated { get; set; }
            public string Id { get; set; }
        }
    }
}
