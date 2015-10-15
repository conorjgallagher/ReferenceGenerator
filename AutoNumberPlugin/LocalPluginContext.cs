using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace AutoNumberPlugin
{
    public interface ILocalPluginContext
    {
        IOrganizationService OrganizationService { get; }
        IPluginExecutionContext PluginExecutionContext { get; }
        ITracingService TracingService { get; }
        void Trace(string message, params object[] o);
        void Trace(FaultException<OrganizationServiceFault> exception);
    }

    public class LocalPluginContext : ILocalPluginContext
    {
        private readonly IServiceProvider _serviceProvider;
        private IPluginExecutionContext _pluginExecutionContext;
        private ITracingService _tracingService;
        private IOrganizationServiceFactory _organizationServiceFactory;
        private IOrganizationService _organizationService;

        public IOrganizationService OrganizationService
        {
            get
            {
                return _organizationService ?? (_organizationService = OrganizationServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId));
            }
        }

        public IPluginExecutionContext PluginExecutionContext
        {
            get
            {
                return _pluginExecutionContext ??
                       (_pluginExecutionContext = (IPluginExecutionContext)_serviceProvider.GetService(typeof(IPluginExecutionContext)));
            }
        }

        public ITracingService TracingService
        {
            get
            {
                return _tracingService ?? (_tracingService = (ITracingService)_serviceProvider.GetService(typeof(ITracingService)));
            }
        }

        private IOrganizationServiceFactory OrganizationServiceFactory
        {
            get { return _organizationServiceFactory ?? (_organizationServiceFactory = (IOrganizationServiceFactory)_serviceProvider.GetService(typeof(IOrganizationServiceFactory))); }
        }

        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            _serviceProvider = serviceProvider;
        }

        public void Trace(string message, params object[] o)
        {
            if (PluginExecutionContext == null)
            {
                SafeTrace(message, o);
            }
            else
            {
                SafeTrace(
                    "{0}, Correlation Id: {1}, Initiating User: {2}",
                    string.Format(message, o),
                    PluginExecutionContext.CorrelationId,
                    PluginExecutionContext.InitiatingUserId);
            }
        }

        public void Trace(FaultException<OrganizationServiceFault> exception)
        {
            // Trace the first message using the embedded Trace to get the Correlation Id and User Id out.
            Trace("Exception: {0}", exception.Message);

            // From here on use the tracing service trace
            SafeTrace(exception.StackTrace);

            if (exception.Detail != null)
            {
                SafeTrace("Error Code: {0}", exception.Detail.ErrorCode);
                SafeTrace("Detail Message: {0}", exception.Detail.Message);
                if (!string.IsNullOrEmpty(exception.Detail.TraceText))
                {
                    SafeTrace("Trace: ");
                    SafeTrace(exception.Detail.TraceText);
                }

                foreach (var item in exception.Detail.ErrorDetails)
                {
                    SafeTrace("Error Details: ");
                    SafeTrace(item.Key);
                    SafeTrace(item.Value.ToString());
                }

                if (exception.Detail.InnerFault != null)
                {
                    Trace(new FaultException<OrganizationServiceFault>(exception.Detail.InnerFault));
                }
            }
        }

        private void SafeTrace(string message, params object[] o)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }
            TracingService.Trace(message, o);
        }
    }
}