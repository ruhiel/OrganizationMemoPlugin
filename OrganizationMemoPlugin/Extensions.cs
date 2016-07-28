using Grabacr07.KanColleWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganizationMemoPlugin
{

	[Flags]
	public enum AirSuperiorityCalculationOptions
	{
		Default = Maximum,

		Minimum = LevelMin | ProficiencyMin | InternalProficiencyMinValue | Fighter,
		Medium = LevelMin | ProficiencyMax | InternalProficiencyMaxValue | Fighter | Attacker | SeaplaneBomber,
		Maximum = LevelMax | ProficiencyMax | InternalProficiencyMaxValue | Fighter | Attacker | SeaplaneBomber,

		/// <summary>艦上戦闘機、水上戦闘機。</summary>
		Fighter = 0x0001,

		/// <summary>艦上攻撃機、艦上爆撃機。</summary>
		Attacker = 0x0002,

		/// <summary>水上爆撃機。</summary>
		SeaplaneBomber = 0x0004,

		/// <summary>内部熟練度最小値による計算。</summary>
		InternalProficiencyMinValue = 0x0100,

		/// <summary>内部熟練度最大値による計算。</summary>
		InternalProficiencyMaxValue = 0x0200,

		/// <summary>
		/// 熟練度最小値による計算
		/// </summary>
		ProficiencyMin = 0x4000,

		/// <summary>
		/// 熟練度最大値による計算
		/// </summary>
		ProficiencyMax = 0x8000,

		/// <summary>
		/// 改修レベル最大値による計算
		/// </summary>
		LevelMax = 0x10000,

		/// <summary>
		/// 改修レベル最小値による計算
		/// </summary>
		LevelMin = 0x20000,
	}

	public static class AirSuperiorityPotential
	{
		public static int CalcMaxAirSuperiorityPotential(this List<OrganizationShipInfo> fleet)
		{
			return fleet.Sum(x => 
						x.SlotItemInfos
							.Sum(y => y.GetAirSuperiorityPotential(AirSuperiorityCalculationOptions.Maximum)));			
		}

		public static int CalcMinAirSuperiorityPotential(this List<OrganizationShipInfo> fleet)
		{
			return fleet.Sum(x =>
						x.SlotItemInfos
							.Sum(y => y.GetAirSuperiorityPotential(AirSuperiorityCalculationOptions.Minimum)));
		}

		public static int CalcMediumAirSuperiorityPotential(this List<OrganizationShipInfo> fleet)
		{
			return fleet.Sum(x =>
						x.SlotItemInfos
							.Sum(y => y.GetAirSuperiorityPotential(AirSuperiorityCalculationOptions.Medium)));
		}

		public static int GetAirSuperiorityPotential(this OrganizationSlotItemInfo info, AirSuperiorityCalculationOptions options)
		{
			return info.SlotItemInfo.GetAirSuperiorityPotential(info.Slot, options);
		}

		/// <summary>
		/// 艦娘の制空能力を計算します。
		/// </summary>
		public static int GetAirSuperiorityPotential(this Ship ship, AirSuperiorityCalculationOptions options = AirSuperiorityCalculationOptions.Default)
		{
			return ship.EquippedItems
				.Select(x => GetAirSuperiorityPotential(x.Item.Info, x.Current, options))
				.Sum();
		}

		/// <summary>
		/// 装備と搭載数を指定して、スロット単位の制空能力を計算します。
		/// </summary>
		public static int GetAirSuperiorityPotential(this SlotItemInfo slotItem, int onslot, AirSuperiorityCalculationOptions options = AirSuperiorityCalculationOptions.Default)
		{
			var calculator = slotItem.GetCalculator();
			return options.HasFlag(calculator.Options) && onslot >= 1
				? calculator.GetAirSuperiority(slotItem, onslot, options)
				: 0;
		}

		private static AirSuperiorityCalculator GetCalculator(this SlotItemInfo slotItem)
		{
			switch (slotItem.Type)
			{
				case SlotItemType.艦上戦闘機:
				case SlotItemType.水上戦闘機:
					return new FighterCalculator();

				case SlotItemType.艦上攻撃機:
				case SlotItemType.艦上爆撃機:
					return new AttackerCalculator();

				case SlotItemType.水上爆撃機:
					return new SeaplaneBomberCalculator();

				default:
					return EmptyCalculator.Instance;
			}
		}

		private abstract class AirSuperiorityCalculator
		{
			public abstract AirSuperiorityCalculationOptions Options { get; }

			public int GetAirSuperiority(SlotItemInfo slotItem, int onslot, AirSuperiorityCalculationOptions options)
			{
				// 装備の対空値とスロットの搭載数による制空値
				var airSuperiority = this.GetAirSuperiorityInternal(slotItem, onslot, options);

				// 装備の熟練度による制空値ボーナス
				airSuperiority += this.GetProficiencyBonus(slotItem, options);

				return (int)airSuperiority;
			}

			protected virtual double GetAirSuperiorityInternal(SlotItemInfo slotItem, int onslot, AirSuperiorityCalculationOptions options)
			{
				return slotItem.AA * Math.Sqrt(onslot);
			}

			protected abstract double GetProficiencyBonus(SlotItemInfo slotItem, AirSuperiorityCalculationOptions options);
		}

		#region AirSuperiorityCalculator 派生型

		private class FighterCalculator : AirSuperiorityCalculator
		{
			public override AirSuperiorityCalculationOptions Options => AirSuperiorityCalculationOptions.Fighter;

			protected override double GetAirSuperiorityInternal(SlotItemInfo slotItem, int onslot, AirSuperiorityCalculationOptions options)
			{	
				int level;
				if (options.HasFlag(AirSuperiorityCalculationOptions.LevelMax)) level = 10;
				else if (options.HasFlag(AirSuperiorityCalculationOptions.LevelMin)) level = 0;
				else level = 5;
				// 装備改修による対空値加算 (★ x 0.2)
				return (slotItem.AA + level * 0.2) * Math.Sqrt(onslot);
			}

			protected override double GetProficiencyBonus(SlotItemInfo slotItem, AirSuperiorityCalculationOptions options)
			{
				var proficiency = slotItem.GetProficiency(options);
				return Math.Sqrt(proficiency.GetInternalValue(options) / 10.0) + proficiency.FighterBonus;
			}
		}

		private class AttackerCalculator : AirSuperiorityCalculator
		{
			public override AirSuperiorityCalculationOptions Options => AirSuperiorityCalculationOptions.Attacker;

			protected override double GetProficiencyBonus(SlotItemInfo slotItem, AirSuperiorityCalculationOptions options)
			{
				var proficiency = slotItem.GetProficiency(options);
				return Math.Sqrt(proficiency.GetInternalValue(options) / 10.0);
			}
		}

		private class SeaplaneBomberCalculator : AirSuperiorityCalculator
		{
			public override AirSuperiorityCalculationOptions Options => AirSuperiorityCalculationOptions.SeaplaneBomber;

			protected override double GetProficiencyBonus(SlotItemInfo slotItem, AirSuperiorityCalculationOptions options)
			{
				var proficiency = slotItem.GetProficiency(options);
				return Math.Sqrt(proficiency.GetInternalValue(options) / 10.0) + proficiency.SeaplaneBomberBonus;
			}
		}

		private class EmptyCalculator : AirSuperiorityCalculator
		{
			public static EmptyCalculator Instance { get; } = new EmptyCalculator();

			public override AirSuperiorityCalculationOptions Options => ~AirSuperiorityCalculationOptions.Default;
			protected override double GetAirSuperiorityInternal(SlotItemInfo slotItem, int onslot, AirSuperiorityCalculationOptions options) => .0;
			protected override double GetProficiencyBonus(SlotItemInfo slotItem, AirSuperiorityCalculationOptions options) => .0;

			private EmptyCalculator() { }
		}

		#endregion

		private class Proficiency
		{
			private readonly int internalMinValue;
			private readonly int internalMaxValue;

			public int FighterBonus { get; }
			public int SeaplaneBomberBonus { get; }

			public Proficiency(int internalMin, int internalMax, int fighterBonus, int seaplaneBomberBonus)
			{
				this.internalMinValue = internalMin;
				this.internalMaxValue = internalMax;
				this.FighterBonus = fighterBonus;
				this.SeaplaneBomberBonus = seaplaneBomberBonus;
			}

			/// <summary>
			/// 内部熟練度値を取得します。
			/// </summary>
			public int GetInternalValue(AirSuperiorityCalculationOptions options)
			{
				if (options.HasFlag(AirSuperiorityCalculationOptions.InternalProficiencyMinValue)) return this.internalMinValue;
				if (options.HasFlag(AirSuperiorityCalculationOptions.InternalProficiencyMaxValue)) return this.internalMaxValue;
				return (this.internalMaxValue + this.internalMinValue) / 2; // <- めっちゃ適当
			}
		}

		private static readonly Dictionary<int, Proficiency> proficiencies = new Dictionary<int, Proficiency>()
		{
			{ 0, new Proficiency(0, 9, 0, 0) },
			{ 1, new Proficiency(10, 24, 0, 0) },
			{ 2, new Proficiency(25, 39, 2, 1) },
			{ 3, new Proficiency(40, 54, 5, 1) },
			{ 4, new Proficiency(55, 69, 9, 1) },
			{ 5, new Proficiency(70, 84, 14, 3) },
			{ 6, new Proficiency(85, 99, 14, 3) },
			{ 7, new Proficiency(100, 120, 22, 6) },
		};

		private static Proficiency GetProficiency(this SlotItemInfo slotItem, AirSuperiorityCalculationOptions options)
		{
			int lv;
			if (options.HasFlag(AirSuperiorityCalculationOptions.ProficiencyMin)) lv = 0;
			else if (options.HasFlag(AirSuperiorityCalculationOptions.ProficiencyMax)) lv = 7;
			else lv = 4;

			return proficiencies[Math.Max(Math.Min(lv, 7), 0)];
		}
	}
}
