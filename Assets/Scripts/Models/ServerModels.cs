using System.Collections.Generic;
using System;

namespace Models
{
    [Serializable]
    public class Account
    {
        public string AccountId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public Guid PlayerGuid { get; set; }
    }

    [Serializable]
    public class GameMatch
    {
        public int MaxPlayers { get; set; }
        public int NumberOfModules { get; set; }
        public Guid GameGuid { get; set; }
        public string? GameSettings { get; set; }
        public Guid Winner { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime HealthCheck { get; set; }
        public virtual ICollection<GameTurn> GameTurns { get; set; } = new HashSet<GameTurn>();
    }

    [Serializable]
    public class GameTurn
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public string? ModulesForMarket { get; set; }
        public string? MarketModuleGuids { get; set; }
        public bool TurnIsOver { get; set; }
        public virtual ICollection<GamePlayer> Players { get; set; } = new HashSet<GamePlayer>();
        public virtual ICollection<ServerModule> AllModules { get; set; } = new HashSet<ServerModule>();
        public virtual ICollection<ServerNode> AllNodes { get; set; } = new HashSet<ServerNode>();
    }

    [Serializable]
    public class ServerNode
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsRift { get; set; }
        public int MaxCredits { get; set; }
        public int Minerals { get; set; }
        public int CreditRegin { get; set; }
        public Guid OwnedByGuid { get; set; }
    }

    [Serializable]
    public class GamePlayer
    {
        public Guid GameGuid { get; set; }
        public Guid PlayerGuid { get; set; }
        public int TurnNumber { get; set; }
        public int PlayerColor { get; set; }
        public string? ModulesGuids { get; set; }
        public int Credits { get; set; }
        public int MaxActions { get; set; }
        public int FleetCount { get; set; }
        public int BonusKinetic { get; set; }
        public int BonusThermal { get; set; }
        public int BonusExplosive { get; set; }
        public int BonusHP { get; set; }
        public int BonusMining { get; set; }
        public int Score { get; set; }
        public virtual ICollection<ServerUnit> Units { get; set; } = new HashSet<ServerUnit>();
        public virtual ICollection<ServerAction> Actions { get; set; } = new HashSet<ServerAction>();
        public virtual ICollection<ServerTechnology> Technology { get; set; } = new HashSet<ServerTechnology>();
    }

    [Serializable]
    public class ServerTechnology
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public Guid PlayerGuid { get; set; }
        public int TechnologyId { get; set; }
        public int Level { get; set; }
        public int CurrentAmount { get; set; }
        public int NeededAmount { get; set; }
        public string? EffectText { get; set; }
        public string? CurrentEffectText { get; set; }
        public string? RequirementText { get; set; }
    }

    [Serializable]
    public class ServerAction
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public Guid PlayerGuid { get; set; }
        public int ActionOrder { get; set; }
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public string? XList { get; set; }
        public string? YList { get; set; }
        public Guid? SelectedModuleGuid { get; set; }
        public int? PlayerBid { get; set; }
        public Guid? GeneratedGuid { get; set; }
    }

    [Serializable]
    public class ServerUnit
    {
        public bool IsStation { get; set; }
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public Guid PlayerGuid { get; set; }
        public Guid UnitGuid { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int Facing { get; set; }
        public string? UnitName { get; set; }
        public int PlayerColor { get; set; }
        public int TeamId { get; set; }
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int MaxMovement { get; set; }
        public int MovementLeft { get; set; }
        public int KineticPower { get; set; }
        public int ThermalPower { get; set; }
        public int ExplosivePower { get; set; }
        public int KineticDamageModifier { get; set; }
        public int ThermalDamageModifier { get; set; }
        public int ExplosiveDamageModifier { get; set; }
        public int MaxMining { get; set; }
        public int MiningLeft { get; set; }
        public double SupportValue { get; set; }
        public int Level { get; set; }
        public int GlobalCreditGain { get; set; }
        public int MaxAttachedModules { get; set; }
        public string? AttachedModules { get; set; }
        public string? ModuleEffects { get; set; }
    }

    [Serializable]
    public class ServerModule
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public Guid ModuleGuid { get; set; }
        public int ModuleId { get; set; }
        public int MidBid { get; set; }
        public int TurnsLeft { get; set; }
    }
}