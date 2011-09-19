// Type: Styx.WoWInternals.WoWObjects.WoWUnit
// Assembly: Honorbuddy, Version=2.0.0.5354, Culture=neutral, PublicKeyToken=50a565ab5c01ae50
// Assembly location: D:\Users\Public\Documents\hb\calc.exe

using Styx;
using Styx.Combat.CombatRoutine;
using Styx.Logic.Combat;
using Styx.Logic.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.Misc.DBC;
using Styx.WoWInternals.WoWCache;
using System;
using System.Collections.Generic;

namespace Styx.WoWInternals.WoWObjects
{
    public class WoWUnit : WoWObject, ILootableObject
    {
        public WoWUnit(uint baseAddress);
        public DateTime CurrentCastStartTime { get; }
        public DateTime CurrentCastEndTime { get; }
        public TimeSpan CurrentCastTimeLeft { get; }
        public uint CurrentCastId { get; }
        public bool CanInterruptCurrentSpellCast { get; }
        public Dictionary<string, WoWAura> Auras { get; }
        public Dictionary<string, WoWAura> PassiveAuras { get; }
        public Dictionary<string, WoWAura> ActiveAuras { get; }
        public Dictionary<string, WoWAura> Buffs { get; }
        public Dictionary<string, WoWAura> Debuffs { get; }
        public float BoundingHeight { get; }
        public override WoWPoint Location { get; }
        public override WoWPoint RelativeLocation { get; }
        public override float Rotation { get; }
        public override float InteractRange { get; }
        public bool IsWithinMeleeRange { get; }
        public ulong TransportGuid { get; }
        public WoWObject Transport { get; }
        public bool IsOnTransport { get; }
        public bool IsPlayerBehind { get; }
        public bool BehindTarget { get; }
        public bool IsMoving { get; }
        public bool IsFalling { get; }
        public bool IsSwimming { get; }
        public bool IsFlying { get; }
        public WoWMovementInfo MovementInfo { get; }
        public WoWObject ChannelObject { get; }
        public ulong ChannelObjectGuid { get; }
        public WoWUnit CharmedUnit { get; }
        public ulong CharmedUnitGuid { get; }
        public WoWUnit SummonedUnit { get; }
        public ulong SummonedUnitGuid { get; }
        public WoWUnit VanityPet { get; }
        public ulong VanityPetGuid { get; }
        public WoWUnit CharmedByUnit { get; }
        public ulong CharmedByUnitGuid { get; }
        public WoWUnit SummonedByUnit { get; }
        public ulong SummonedByUnitGuid { get; }
        public WoWUnit CreatedByUnit { get; }
        public ulong CreatedByUnitGuid { get; }
        public WoWUnit CurrentTarget { get; }
        public ulong CurrentTargetGuid { get; }
        public virtual WoWUnit OwnedByUnit { get; }
        public WoWUnit OwnedByRoot { get; }
        public WoWPlayer ControllingPlayer { get; }
        public bool GotTarget { get; }
        public uint CurrentPower { get; }
        public uint MaxPower { get; }
        public float PowerPercent { get; }
        public WoWUnit.PowerInfo CurrentPowerInfo { get; }
        public bool Dead { get; }
        public uint CurrentHealth { get; }
        public uint CurrentMana { get; }
        public uint CurrentRage { get; }
        public uint CurrentEnergy { get; }
        public uint CurrentFocus { get; }
        public uint CurrentHappiness { get; }
        public uint CurrentRunicPower { get; }
        public uint CurrentSoulShards { get; }
        public uint CurrentEclipse { get; }
        public uint CurrentHolyPower { get; }
        public uint MaxHealth { get; }
        public uint MaxMana { get; }
        public uint MaxRage { get; }
        public uint MaxEnergy { get; }
        public uint MaxFocus { get; }
        public uint MaxHappiness { get; }
        public uint MaxRunicPower { get; }
        public uint MaxSoulShards { get; }
        public uint MaxEclipse { get; }
        public uint MaxHolyPower { get; }
        public double EclipsePercent { get; }
        public double EnergyPercent { get; }
        public double FocusPercent { get; }
        public double HappinessPercent { get; }
        public double HealthPercent { get; }
        public double HolyPowerPercent { get; }
        public double ManaPercent { get; }
        public double RagePercent { get; }
        public double RunesPercent { get; }
        public double RunicPowerPercent { get; }
        public double SoulShardsPercent { get; }
        public WoWUnit.PowerInfo ManaInfo { get; }
        public WoWUnit.PowerInfo RageInfo { get; }
        public WoWUnit.PowerInfo EnergyInfo { get; }
        public WoWUnit.PowerInfo FocusInfo { get; }
        public WoWUnit.PowerInfo HappinessInfo { get; }
        public WoWUnit.PowerInfo RunesPowerInfo { get; }
        public WoWUnit.PowerInfo RunicPowerInfo { get; }
        public WoWUnit.PowerInfo SoulShardsInfo { get; }
        public WoWUnit.PowerInfo EclipseInfo { get; }
        public WoWUnit.PowerInfo HolyPowerInfo { get; }
        public int MaxItemLevel { get; }
        public uint[] VirtualItemSlotIds { get; }
        public uint Strength { get; }
        public uint StrengthPositiveModifier { get; }
        public uint StrengthNegativeModifier { get; }
        public uint Agility { get; }
        public uint AgilityPositiveModifier { get; }
        public uint AgilityNegativeModifier { get; }
        public uint Stamina { get; }
        public uint StaminaPositiveModifier { get; }
        public uint StaminaNegativeModifier { get; }
        public uint Intellect { get; }
        public uint IntellectPositiveModifier { get; }
        public uint IntellectNegativeModifier { get; }
        public uint Spirit { get; }
        public uint SpiritPositiveModifier { get; }
        public uint SpiritNegativeModifier { get; }
        public uint BaseMana { get; }
        public uint BaseHealth { get; }
        public uint AttackPower { get; }
        public float AttackPowerMultiplier { get; }
        public uint BaseAttackTime { get; }
        public uint BaseOffHandAttackTime { get; }
        public uint BaseRangedAttackTime { get; }
        public uint RangedAttackPower { get; }
        public float RangedAttackPowerMultiplier { get; }
        public float MinRangedDamage { get; }
        public float MaxRangedDamage { get; }
        public float CombatReach { get; }
        public uint MinDamage { get; }
        public float MaxDamage { get; }
        public float MinOffHandDamage { get; }
        public float MaxOffHandDamage { get; }
        public uint Armor { get; }
        public uint ResistHoly { get; }
        public uint ResistFire { get; }
        public uint ResistNature { get; }
        public uint ResistFrost { get; }
        public uint ResistShadow { get; }
        public uint ResistArcane { get; }
        public uint ResistFirePos { get; }
        public float MaxHealthModifier { get; }
        public uint PetNumber { get; }
        public uint PetNameTimestamp { get; }
        public uint PetExperience { get; }
        public uint PetNextLevelExperience { get; }
        public virtual WoWUnit Pet { get; }
        public bool IsPet { get; }
        public bool IsTargetingMeOrPet { get; }
        public bool IsTargetingAnyMinion { get; }
        public bool GotAlivePet { get; }
        public bool IsTargetingPet { get; }
        public int ChanneledCastingSpellId { get; }
        public int NonChanneledCastingSpellId { get; }
        public int CastingSpellId { get; }
        public WoWSpell CastingSpell { get; }
        public bool IsCasting { get; }
        public int Level { get; }
        public uint FactionId { get; }
        public WoWFaction Faction { get; }
        public WoWFactionTemplate FactionTemplate { get; }
        public float BoundingRadius { get; }
        public uint DisplayId { get; }
        public uint NativeDisplayId { get; }
        public uint MountDisplayId { get; }
        public virtual bool Mounted { get; }
        public float HoverHeight { get; }
        public float CastSpeedModifier { get; }
        public uint CreatedBySpellId { get; }
        public uint Flags { get; }
        public uint Flags2 { get; }
        public uint DynamicFlags { get; }
        public uint NpcFlags { get; }
        public EmoteState NpcEmoteState { get; }
        public uint AuraState { get; }
        public WoWClass Class { get; }
        public WoWRace Race { get; }
        public WoWPowerType PowerType { get; }
        public WoWGender Gender { get; }
        public WoWStateFlag StateFlag { get; }
        public PvPState PvPState { get; }
        public ShapeshiftForm Shapeshift { get; }
        public bool Combat { get; }
        public bool Skinnable { get; }
        public bool Dazed { get; }
        public bool Disarmed { get; }
        public bool Attackable { get; }
        public bool PvpFlagged { get; }
        public bool Fleeing { get; }
        public bool Pacified { get; }
        public bool Stunned { get; }
        public bool Rooted { get; }
        public bool CanSelect { get; }
        public bool Silenced { get; }
        public bool Possessed { get; }
        public bool Elite { get; }
        public bool Looting { get; }
        public bool PetInCombat { get; }
        public bool OnTaxi { get; }
        public bool PlayerControlled { get; }
        public bool TaggedByOther { get; }
        public bool TaggedByMe { get; }
        public bool Lootable { get; }
        public bool Tracked { get; }
        public bool RafLinked { get; }
        public bool TappedByAllThreatLists { get; }
        public bool CanGossip { get; }
        public bool IsQuestGiver { get; }
        public bool IsTrainer { get; }
        public bool IsClassTrainer { get; }
        public bool IsProfessionTrainer { get; }
        public bool IsAnyTrainer { get; }
        public bool IsAnyVendor { get; }
        public bool IsVendor { get; }
        public bool IsAmmoVendor { get; }
        public bool IsFoodVendor { get; }
        public bool IsPoisonVendor { get; }
        public bool IsPetitioner { get; }
        public bool IsReagentVendor { get; }
        public bool IsRepairMerchant { get; }
        public bool IsFlightMaster { get; }
        public bool IsSpiritHealer { get; }
        public bool IsSpiritGuide { get; }
        public bool IsInnkeeper { get; }
        public bool IsBanker { get; }
        public bool IsTabardDesigner { get; }
        public bool IsBattleMaster { get; }
        public bool IsAuctioneer { get; }
        public bool IsStableMaster { get; }
        public bool IsGuard { get; }
        public bool IsGuildBanker { get; }
        public CreatureFamily CreatureFamilyInfo { get; }
        public string SubName { get; }
        public WoWCreatureSkinType SkinType { get; }
        public WoWCreatureType CreatureType { get; }
        public bool IsBeast { get; }
        public bool IsCritter { get; }
        public bool IsDemon { get; }
        public bool IsDragon { get; }
        public bool IsElemental { get; }
        public bool IsGasCloud { get; }
        public bool IsGiant { get; }
        public bool IsHumanoid { get; }
        public bool IsMechanical { get; }
        public bool IsNonCombatPet { get; }
        public bool IsTotem { get; }
        public bool IsUndead { get; }
        public WoWUnitClassificationType CreatureRank { get; }
        public bool IsTameable { get; }
        public bool IsGhostVisible { get; }
        public bool IsExotic { get; }
        public UnitThreatInfo ThreatInfo { get; }
        public bool Aggro { get; }
        public bool PetAggro { get; }
        public WoWUnitReaction MyReaction { get; }
        public bool IsFriendly { get; }
        public bool IsHostile { get; }
        public bool IsNeutral { get; }
        public bool IsAutoAttacking { get; }
        public bool IsPlayer { get; }
        public bool IsUnit { get; }
        public virtual bool IsAlive { get; }
        public bool KilledByMe { get; }
        public float MyAggroRange { get; }
        public float MyStealthDetectionRange { get; }
        public bool CanSkin { get; }
        public bool IsTargetingMyPartyMember { get; }
        public bool IsTargetingMyRaidMember { get; }
        public bool IsStealthed { get; }

        #region ILootableObject Members

        public bool CanLoot { get; }

        #endregion

        public WoWAura GetAuraByName(string name);
        public WoWAura GetAuraById(int id);
        public bool HasAura(string name);
        public WoWAuraCollection GetAllAuras();
        public WoWPoint GetTraceLinePos();
        public uint GetCurrentPower(WoWPowerType type);
        public uint GetMaxPower(WoWPowerType type);
        public float GetPowerPercent(WoWPowerType type);
        public float GetPowerRegenFlat(WoWPowerType type);
        public float GetPowerRegenInterrupted(WoWPowerType type);
        public WoWUnit.PowerInfo GetPowerInfo(WoWPowerType type);
        public uint GetPowerCostModifier(WoWPowerType type);
        public float GetPowerCostMultiplier(WoWPowerType type);
        public bool GetCachedInfo(out WoWCache.CreatureCacheEntry info);
        public bool Behind(WoWUnit obj);
        public float GetStealthDetectionRange(WoWUnit to);
        public float GetAggroRange(WoWUnit to);
        public void Face();
        public void Target();
        public WoWUnitReaction GetReactionTowards(WoWUnit otherUnit);
        public UnitThreatInfo GetThreatInfoFor(WoWUnit otherUnit);

        #region Nested type: PowerInfo

        public struct PowerInfo
        {
            public WoWPowerType Type { get; }
            public uint Current { get; }
            public int CurrentI { get; }
            public uint Max { get; }
            public float Percent { get; }
            public float RegenFlatModifier { get; }
            public float RegenInterruptedFlatModifier { get; }
            public uint CostModifier { get; }
            public float CostMultiplier { get; }
        }

        #endregion
    }
}
