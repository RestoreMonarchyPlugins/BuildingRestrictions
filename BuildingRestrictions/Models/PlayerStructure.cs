using SDG.Unturned;
using UnityEngine;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class PlayerStructure
    {
        public ushort ItemId { get; set; }
        public EConstruct Construct { get; set; }
        public Transform Transform { get; set; }
    }
}
