using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoNumberPlugin
{
    class NumberGenerator
    {
        private readonly IOrganizationService _organizationService;

        public NumberGenerator(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public decimal Execute(Entity autoNumber)
        {

            // Need to handle shared numbers and we're almost done!!!

            var internalAutoNumber = new Entity("smp_autonumber") {Id = autoNumber.Id};
            internalAutoNumber["name"] = autoNumber["name"];
            _organizationService.Update(internalAutoNumber);

            // Once here we have a transaction open. Get the autonumber again in case the number has moved
            var queryByAttribute = new QueryByAttribute("smp_autonumber");
            queryByAttribute.AddAttributeValue("smp_name", autoNumber["name"]);
            queryByAttribute.ColumnSet = new ColumnSet("smp_name", "smp_nextnumber");
            internalAutoNumber = _organizationService.RetrieveMultiple(queryByAttribute).Entities[0];
            decimal number = internalAutoNumber.GetAttributeValue<decimal>("smp_nextnumber");

            // Update the new number
            internalAutoNumber["smp_nextnumber"] = number + 1;
            _organizationService.Update(internalAutoNumber);

            return number;
        }
    }
}
