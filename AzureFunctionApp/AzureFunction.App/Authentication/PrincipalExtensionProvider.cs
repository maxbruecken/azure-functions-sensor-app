using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;

namespace AzureFunction.App.Authentication
{
    public class PrincipalExtensionProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;

        public PrincipalExtensionProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void Initialize(ExtensionConfigContext context)
        {
            var jwtMedataAddress = _configuration["JwtTokenMetadataAddress"];
            var jwtValidAudience = _configuration["JwtTokenValidAudience"];
            var provider = new PrincipalBindingProvider(jwtMedataAddress, jwtValidAudience);
            context.AddBindingRule<PrincipalAttribute>().Bind(provider);
        }
    }
}