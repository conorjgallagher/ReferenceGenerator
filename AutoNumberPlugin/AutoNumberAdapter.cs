using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoNumberPlugin
{
    public class AutoNumberDataMapper
    {
        private readonly IOrganizationService _organizationService;

        public AutoNumberDataMapper(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public void Populate(Entity target)
        {
            // Get all the auto numbers up front so that we hold any transactions open for as short as possible
            var autoNumbers = ScanForAutoNumbers(target);
            var numberGenerator = new NumberGenerator(_organizationService);
            foreach (var autoNumberAttribute in autoNumbers.Keys)
            {
                var autoNumber = autoNumbers[autoNumberAttribute];
                var number = numberGenerator.Execute(autoNumber);
                var pattern = autoNumber.GetAttributeValue<string>("smp_pattern");
                var padLength = autoNumber.GetAttributeValue<int>("smp_padlength");
                var paddedNumber = number.ToString(CultureInfo.InvariantCulture).PadLeft(padLength, '0');
                var reference = pattern.Replace("{0}", paddedNumber);

                target[autoNumberAttribute] = reference;
            }
        }

        private Dictionary<string, Entity> ScanForAutoNumbers(Entity target)
        {
            var autoNumberList = new Dictionary<string, Entity>();
            foreach (var attribute in target.Attributes.Where(a => a.Key != "id"))
            {
                var queryByAttribute = new QueryByAttribute("smp_autonumber");
                queryByAttribute.AddAttributeValue("smp_entity", target.LogicalName);
                queryByAttribute.AddAttributeValue("smp_attribute", attribute.Key);
                queryByAttribute.ColumnSet = new ColumnSet("smp_name", "smp_nextnumber", "smp_sharednumber", "smp_pattern");
                var autoNumbers = _organizationService.RetrieveMultiple(queryByAttribute).Entities;

                if (autoNumbers.Count > 1)
                {
                    throw new InvalidPluginExecutionException(string.Format("Invalid autonumber configuration. {0} records found for the attribute {1} on {2}", autoNumbers.Count, attribute.Key, target.LogicalName));
                }
                if (autoNumbers.Count > 0)
                {
                    autoNumberList[attribute.Key] = autoNumbers[0];
                }
            }
            return autoNumberList;
        }
    }
}