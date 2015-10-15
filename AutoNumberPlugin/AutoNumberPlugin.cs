using Microsoft.Xrm.Sdk;

namespace AutoNumberPlugin
{
    public class AutoNumberPlugin : BasePlugin
    {
        public override void Execute(ILocalPluginContext localContext)
        {
            localContext.Trace("Entered AutoNumberPlugin");

            var organizationService = localContext.OrganizationService;


            localContext.Trace("Exiting AutoNumberPlugin");
        }
    }
}