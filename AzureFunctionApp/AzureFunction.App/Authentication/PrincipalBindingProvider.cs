using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace AzureFunction.App.Authentication
{
    public class PrincipalBindingProvider : IBindingProvider
    {
        private readonly string _validAudience;
        private readonly string _metadataAddress;

        public PrincipalBindingProvider(string metadataAddress, string validAudience)
        {
            _validAudience = validAudience;
            _metadataAddress = metadataAddress;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            return Task.FromResult((IBinding) new PrincipalBinding(_metadataAddress, _validAudience));
        }
    }
}