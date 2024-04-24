using System;
using System.Collections.Generic;

namespace StartaneousAPI.Models
{
    [Serializable]
    public class Turn
    {
        public Guid GameId { get; set; }
        public Guid ClientId { get; set; }
        public int TurnNumber { get; set; }
        public List<ActionIds>? Actions { get; set; }

        public Turn(Guid gameId, Guid clientId, int turnNumber, List<ActionIds> actions)
        {
            GameId = gameId;
            ClientId = clientId;
            TurnNumber = turnNumber;
            Actions = actions;
        }
    }
}