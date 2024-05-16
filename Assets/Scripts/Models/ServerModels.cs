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
        public List<ServerModule>? MarketModules { get; set; }
        public Player[]? Players { get; set; }
    }

    [Serializable]
    public class Player
    {
        public ServerUnit? Station { get; set; }
        public List<ServerUnit>? Fleets { get; set; }
        public List<ServerAction>? Actions { get; set; }
        public List<Guid>? ModulesGuids { get; set; }
        public int? Credits { get; set; }
    }

    [Serializable]
    public class ServerAction
    {
        public int PlayerId { get; set; }
        public int ActionOrder { get; set; }
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public List<ServerCoords>? SelectedCoords { get; set; }
        public ServerModule? SelectedModule { get; set; }
        public Guid? GeneratedGuid { get; set; }
    }

    [Serializable]
    public class ServerUnit
    {
        public Guid? UnitGuid { get; set; }
        public ServerCoords? Location { get; set; }
        public Direction Facing { get; set; }
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
