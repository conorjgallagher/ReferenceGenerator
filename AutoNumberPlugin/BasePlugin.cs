using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace AutoNumberPlugin
{
    public abstract class BasePlugin : IPlugin
    {
        private readonly string _className;

        protected BasePlugin()
        {
            _className = GetType().Name;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            // Construct the Local plug-in context.
            var localContext = new LocalPluginContext(serviceProvider);

            localContext.Trace("Entered {0}.Execute()", _className);

            try
            {
                Execute(localContext);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                // Trace the exception before bubbling so that we ensure everything we need hits the log
                localContext.Trace(e);

                // Bubble the exception
                throw;
            }
            finally
            {
                localContext.Trace("Exiting {0}.Execute()", _className);
            }
        }

        public abstract void Execute(ILocalPluginContext localContext);
    } 
}
