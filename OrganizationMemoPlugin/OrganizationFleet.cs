using System;
using System.Collections.Generic;

namespace OrganizationMemoPlugin
{
    public class OrganizationFleet
    {
        public string FirstFleetName { get; set; }
        public string SecondFleetName { get; set; }
        public string DisplayFleetName => SecondFleetName == null ? FirstFleetName : FirstFleetName + " - " + SecondFleetName;
        public DateTime Time { get; set; }
        public List<OrganizationShipInfo> FirstFleet { get; set; }
        public List<OrganizationShipInfo> SecondFleet { get; set; }
        public int MaxAirSuperiorityPotential => FirstFleet == null ? 0 : FirstFleet.CalcMaxAirSuperiorityPotential();
        public int MinAirSuperiorityPotential => FirstFleet == null ? 0 : FirstFleet.CalcMinAirSuperiorityPotential();

    }
}
