using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace StartaneousAPI.Models
{
    public class NewGame
    {
        public Guid ClientId { get; set; }
        public Guid GameId { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }

        public NewGame(Guid localStationGuid, int _maxPlayers)
        {
            ClientId = localStationGuid;
            MaxPlayers = _maxPlayers;
        }
    }
}
