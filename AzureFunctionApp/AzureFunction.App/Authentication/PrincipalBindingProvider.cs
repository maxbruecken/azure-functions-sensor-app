using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace AzureFunction.App.Authentication
{
    public class PrincipalBindingProvider : IBindingProvider
    {
        private readonly string _validAudience;
        private readonly bool _bypassAuthentication;
        private readonly string _metadataAddress;

        public PrincipalBindingProvider(string metadataAddress, string validAudience, bool bypassAuthentication)
        {
            _validAudience = validAudience;
            _bypassAuthentication = bypassAuthentication;
            _metadataAddress = metadataAddress;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            return Task.FromResult((IBinding) new PrincipalBinding(_metadataAddress, _validAudience, _bypassAuthentication));
        }
    }
}