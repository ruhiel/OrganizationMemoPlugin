using Grabacr07.KanColleWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganizationMemoPlugin
{
    static class Extensions
    {
        #region 制空値計算

        /// <summary>
        /// 装備と搭載数を指定して、スロット単位の制空能力を計算します。
        /// </summary>
        /// <param name="slotItem">対空能力を持つ装備。</param>
        /// <param name="onslot">搭載数。</param>
        /// <returns></returns>
        public static int CalcAirSuperiorityPotential(this SlotItemInfo slotItem, int onslot)
        {
            if (slotItem.IsAirSuperiorityFighter)
            {
                return (int)(slotItem.AA * Math.Sqrt(onslot));
            }

            return 0;
        }

        /// <summary>
        /// 熟練度による制空能力ボーナス最小値を計算します。
        /// </summary>
        /// <param name="slotItem">対空能力を持つ装備。</param>
        /// <param name="onslot">搭載数。</param>
        /// <returns></returns>
        private static double CalcMinAirecraftAdeptBonus(this SlotItemInfo slotItem, int onslot)
        {
            if (onslot < 1) return 0;
            return slotItem.Type == SlotItemType.艦上戦闘機
                ? slotItem.CalcAirecraftAdeptBonusOfType() + slotItem.CalcMinInternalAirecraftAdeptBonus()
                : 0; // 艦戦以外は簡単に吹き飛ぶので最小値としては計算に入れない
        }

        /// <summary>
        /// 熟練度による制空能力ボーナス最大値を計算します。
        /// </summary>
        /// <param name="slotItem">対空能力を持つ装備。</param>
        /// <param name="onslot">搭載数。</param>
        /// <returns></returns>
        private static double CalcMaxAirecraftAdeptBonus(this SlotItemInfo slotItem, int onslot)
            => onslot < 1 ? 0
            : slotItem.CalcAirecraftAdeptBonusOfType() + slotItem.CalcMaxInternalAirecraftAdeptBonus();

        /// <summary>
        /// 各表記熟練度に対応した機種別熟練度ボーナスを計算します。
        /// </summary>
        /// <param name="slotItem"></param>
        /// <returns></returns>
        private static int CalcAirecraftAdeptBonusOfType(this SlotItemInfo slotItem)
            => slotItem.Type == SlotItemType.艦上戦闘機
                ? 22
            : slotItem.Type == SlotItemType.水上爆撃機
                ? 6
            : 0;

        /// <summary>
        /// 各表記熟練度に対応した艦載機内部熟練度ボーナスの最小値を計算します。
        /// </summary>
        /// <param name="slotItem"></param>
        /// <returns></returns>
        private static double CalcMinInternalAirecraftAdeptBonus(this SlotItemInfo slotItem)
        {
            return slotItem.IsAirSuperiorityFighter
                ? Math.Sqrt((0 != 0 ? (0 - 1) * 15 + 10 : 0) / 10d)
                : 0;
        }

        /// <summary>
        /// 各表記熟練度に対応した艦載機内部熟練度ボーナスの最大値を計算します。
        /// </summary>
        /// <param name="slotItem"></param>
        /// <returns></returns>
        private static double CalcMaxInternalAirecraftAdeptBonus(this SlotItemInfo slotItem)
        {
            if (!slotItem.IsAirSuperiorityFighter)
                return 0;
            return Math.Sqrt(120d / 10);
        }

        #endregion
        /// <summary>
        /// 指定した艦の制空能力の最大値を計算します。
        /// </summary>
        public static int CalcMaxAirSuperiorityPotential(this OrganizationShipInfo ship)
        {

            return Enumerable.Range(0, ship.SlotItemInfos.Count())
                .Select(i => new { SlotItem = ship.SlotItemInfos[i], Slot = ship.ShipInfo.Slots[i] })
                .Select(x => x.SlotItem.SlotItemInfo.CalcAirSuperiorityPotential(x.Slot)
                + x.SlotItem.SlotItemInfo.CalcMaxAirecraftAdeptBonus(x.Slot))
                .Select(x => (int)x)
                .Sum();
        }

        /// <summary>
        /// 指定した艦の制空能力の最小値を計算します。
        /// </summary>
        public static int CalcMinAirSuperiorityPotential(this OrganizationShipInfo ship)
        {
            return Enumerable.Range(0, ship.SlotItemInfos.Count())
                .Select(i => new { SlotItem = ship.SlotItemInfos[i], Slot = ship.ShipInfo.Slots[i] })
                .Select(x => (x.SlotItem.SlotItemInfo.Type == SlotItemType.艦上戦闘機
                                ? x.SlotItem.SlotItemInfo.CalcAirSuperiorityPotential(x.Slot)
                                : 0)
                                 + x.SlotItem.SlotItemInfo.CalcMinAirecraftAdeptBonus(x.Slot))
                .Select(x => (int)x)
                .Sum();
        }

        /// <summary>
        /// 艦リストの制空能力の最小値を計算します。
        /// </summary>
        public static int CalcMinAirSuperiorityPotential(this List<OrganizationShipInfo> ships)
        {
            return ships.Sum(x => x.CalcMinAirSuperiorityPotential());
        }

        /// <summary>
        /// 艦リストの制空能力の最大値を計算します。
        /// </summary>
        public static int CalcMaxAirSuperiorityPotential(this List<OrganizationShipInfo> ships)
        {
            return ships.Sum(x => x.CalcMaxAirSuperiorityPotential());
        }
    }
}
