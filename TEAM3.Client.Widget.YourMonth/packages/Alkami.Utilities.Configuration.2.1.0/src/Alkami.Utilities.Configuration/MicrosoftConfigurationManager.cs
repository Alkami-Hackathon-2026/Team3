#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Alkami.Utilities.Configuration
{
    internal class MicrosoftConfigurationManager : SettingsBase
    {
        private readonly IConfiguration _configuration;

        public MicrosoftConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected internal override string[] AllKeys()
        {
            return _configuration.AsEnumerable().Select(x => x.Key).ToArray();
        }

        protected internal override string Get(string name)
        {
            return _configuration[name];
        }
    }
}
#endif
