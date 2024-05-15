using System.Collections.Generic;
using System;

namespace StartaneousAPI.ServerModels
{
    [Serializable]
    public class GameMatch
    {
        public int MaxPlayers { get; set; }
        public Guid GameGuid { get; set; }
        public List<GameTurn>? GameTurns { get; set; }
        public List<string>? GameSettings { get; set; }
    }

    [Serializable]
    public class GameTurn
    {
        public Guid GameGuid { get; set; }
        public int TurnNumber { get; set; }
        public Player[]? Players { get; set; }
    }

    [Serializable]
    public class Player
    {
        public ServerUnit? Station { get; set; }
        public List<ServerUnit>? Fleets { get; set; }
        public List<ServerAction>? Actions { get; set; }
        public List<Guid>? ModulesGuids { get; set; }
        public int Credits { get; set; }
    }

    [Serializable]
    public class ServerAction
    {
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public List<Guid>? SelectedModulesGuids { get; set; }
        public List<ServerCoords>? SelectedCoords { get; set; }
        public int? GeneratedModuleId { get; set; }
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
    public class ServerCoords
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
