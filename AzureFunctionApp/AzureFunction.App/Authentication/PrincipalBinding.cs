using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunction.App.Authentication
{
    public class PrincipalBinding : IBinding
    {
        private readonly string _validAudience;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private TokenValidationParameters _tokenValidationParameters;

        public PrincipalBinding(string metadataAddress, string validAudience)
        {
            _validAudience = validAudience;
            
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, 
                new OpenIdConnectConfigurationRetriever(), 
                new HttpDocumentRetriever {RequireHttps = true});
        }

        public async Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            await PrepareTokenValidationParametersAsync();
            if (value is HttpRequest httpRequest)
            {
                var binding = new PrincipalValueProvider(httpRequest, _tokenValidationParameters);
                return binding;
            }
            throw new InvalidOperationException("Value must be an HttpRequest");
        }

        private async Task PrepareTokenValidationParametersAsync()
        {
            if (_tokenValidationParameters != null)
            {
                return;
            }
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = _validAudience
            };
            var configuration = await _configurationManager.GetConfigurationAsync();
            var issuers = new[] {configuration.Issuer};
            _tokenValidationParameters.ValidIssuers = _tokenValidationParameters.ValidIssuers?.Concat(issuers) ?? issuers;
            _tokenValidationParameters.IssuerSigningKeys = _tokenValidationParameters.IssuerSigningKeys?.Concat(configuration.SigningKeys) ?? configuration.SigningKeys;
        }

        public async Task<IValueProvider> BindAsync(BindingContext context)
        {
            await PrepareTokenValidationParametersAsync();
            if (context.BindingData.TryGetValue("$request", out var request) && request is HttpRequest httpRequest)
            {
                var binding = new PrincipalValueProvider(httpRequest, _tokenValidationParameters);
                return binding;
            }
            throw new InvalidOperationException("Expected context binding data to contain an HttpRequest");
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }

        public bool FromAttribute => false;
    }
}