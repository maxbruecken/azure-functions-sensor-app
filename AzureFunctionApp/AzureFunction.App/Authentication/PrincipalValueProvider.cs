using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunction.App.Authentication
{
    public class PrincipalValueProvider : IValueProvider
    {
        private const string AuthHeaderName = "Authorization";
        private const string BearerPrefix = "Bearer ";
        
        private readonly HttpRequest _request;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public PrincipalValueProvider(HttpRequest request, TokenValidationParameters tokenValidationParameters)
        {
            _request = request;
            _tokenValidationParameters = tokenValidationParameters;
        }
            
        public Task<object> GetValueAsync()
        {
            if (_request.Headers.ContainsKey(AuthHeaderName) &&
                _request.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix))
            {
                var token = _request.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var result = handler.ValidateToken(token, _tokenValidationParameters, out _);

                    return Task.FromResult((object) result);
                }
                catch (Exception)
                {
                    return Task.FromResult((object)new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }

            return Task.FromResult((object)new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public string ToInvokeString() => string.Empty;

        public Type Type => typeof(ClaimsPrincipal);
    }
}