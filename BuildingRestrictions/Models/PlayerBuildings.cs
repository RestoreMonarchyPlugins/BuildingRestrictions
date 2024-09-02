using System.Collections.Generic;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class PlayerBuildings
    {
        public ulong SteamId { get; set; }
        public List<PlayerBarricade> Barricades { get; set; }
        public List<PlayerStructure> Structures { get; set; }
    }
}
