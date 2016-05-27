using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrganizationMemoPlugin
{
    public class OrganizationShipInfo
    {
        public int Id { get; set; }

        public List<int> SlotIds { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public ShipInfo ShipInfo => KanColleClient.Current.Master.Ships.Where(s => s.Value.Id == Id).First().Value;

        [System.Xml.Serialization.XmlIgnore]
        public List<OrganizationSlotItemInfo> SlotItemInfos => SlotIds.Select((x, idx) => new {Idx = idx, Id = x })
                                                                        .Where(y => y.Id != 0)
                                                                        .Select(z => new OrganizationSlotItemInfo
                                                                        {
                                                                            SlotItemInfo = KanColleClient.Current.Master.SlotItems.Where(i => i.Value.Id.Equals(z.Id)).First().Value,
                                                                            Slot = ShipInfo.Slots[z.Idx]
                                                                        })
                                                                        .ToList();

        [System.Xml.Serialization.XmlIgnore]
        public List<OrganizationSlotItemInfo> SlotItemInfosFirstHalf => SlotItemInfos.Take(2).ToList();

        [System.Xml.Serialization.XmlIgnore]
        public List<OrganizationSlotItemInfo> SlotItemInfosLatterHalf => SlotItemInfos.Skip(2).Take(2).ToList();
    }
}
