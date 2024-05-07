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
        public List<string> GameSettings { get; set; }

        public NewGame(Guid _clientId, int _maxPlayers, List<string> _gameSettings)
        {
            ClientId = _clientId;
            MaxPlayers = _maxPlayers;
            GameSettings = _gameSettings;
        }
    }
}
