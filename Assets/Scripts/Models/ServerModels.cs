using System.Collections.Generic;
using System;

namespace StartaneousAPI.ServerModels
{
    [Serializable]
    public class GameMatch
    {
        public int MaxPlayers { get; set; }
        public int NumberOfModules { get; set; }
        public Guid GameGuid { get; set; }
        public List<GameTurn>? GameTurns { get; set; }
        public List<string>? GameSettings { get; set; }
    }

    [Serializable]
    public class GameTurn
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public List<int> ModulesForMarket { get; set; }
        public List<ServerModule>? MarketModules { get; set; }
        public List<ServerModule>? AllModules { get; set; }
        public List<ServerNode>? AllNodes { get; set; }
        public Player[]? Players { get; set; }
    }

    [Serializable]
    public class ServerNode
    {
        public Guid UnitOnPath { get; set; }
        public bool IsRift { get; set; }
        public int MaxCredits { get; set; }
        public int Minerals { get; set; }
        public int CreditRegin { get; set; }
        public int OwnedById { get; set; }
        public ServerCoords Coords { get; set; }
    }

    [Serializable]
    public class Player
    {
        public ServerUnit? Station { get; set; }
        public List<ServerUnit>? Fleets { get; set; }
        public List<ServerAction>? Actions { get; set; }
        public List<ServerTechnology>? Technology { get; set; }
        public List<Guid>? ModulesGuids { get; set; }
        public int Credits { get; set; }
        public int MaxActions { get; set; }
        public int FleetCount { get; set; }
        public int BonusKinetic { get; set; }
        public int BonusThermal { get; set; }
        public int BonusExplosive { get; set; }
        public int BonusHP { get; set; }
        public int BonusMining { get; set; }
        public int Score { get; set; }
    }

    [Serializable]
    public class ServerTechnology
    {
        public int ResearchId { get; set; }
        public int Level { get; set; }
        public int CurrentAmount { get; set; }
        public int NeededAmount { get; set; }
        public string EffectText { get; set; }
        public string CurrentEffectText { get; set; }
        public string RequirementText { get; set; }
    }

    [Serializable]
    public class ServerAction
    {
        public int PlayerId { get; set; }
        public int ActionOrder { get; set; }
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public List<ServerCoords> SelectedCoords { get; set; }
        public ServerModule SelectedModule { get; set; }
        public Guid? GeneratedGuid { get; set; }
    }

    [Serializable]
    public class ServerUnit
    {
        public Guid UnitGuid { get; set; }
        public ServerCoords Location { get; set; }
        public Direction Facing { get; set; }
        public string UnitName{ get; set; }
        public int StationId{ get; set; }
        public int TeamId{ get; set; }
        public int MaxHP{ get; set; }
        public int HP{ get; set; }
        public int MaxMovement{ get; set; }
        public int MovementLeft{ get; set; }
        public int KineticPower{ get; set; }
        public int ThermalPower{ get; set; }
        public int ExplosivePower{ get; set; }
        public int KineticDamageModifier{ get; set; }
        public int ThermalDamageModifier{ get; set; }
        public int ExplosiveDamageModifier{ get; set; }
        public int MaxMining{ get; set; }
        public int MiningLeft{ get; set; }
        public double SupportValue { get; set; }
        public int Level { get; set; }
        public int GlobalCreditGain { get; set; }
        public int MaxAttachedModules { get; set; }
        public string Color { get; set; }
        public List<Guid> AttachedModules { get; set; }
        public List<int> ModuleEffects { get; set; }
    }

    [Serializable]
    public class ServerModule
    {
        public Guid? ModuleGuid { get; set; }
        public int ModuleId { get; set; }
        public int MidBid { get; set; }
        public int PlayerBid { get; set; }
        public int TurnsLeft { get; set; }
    }

    [Serializable]
    public class ServerCoords
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
