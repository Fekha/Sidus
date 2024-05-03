using System;
using System.Collections.Generic;

namespace StarTaneousAPI.Models
{
    [Serializable]
    public class Player
    {
        public Guid StationGuid { get; set; }
        public List<Guid> FleetGuids { get; set; }
    }
}