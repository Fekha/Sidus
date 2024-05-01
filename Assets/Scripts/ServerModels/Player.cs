using System;
using System.Collections.Generic;

namespace StarTaneousAPI.Models
{
    [Serializable]
    public class Player
    {
        public Guid StationId { get; set; }
        public List<Guid> FleetIds { get; set; }
    }
}