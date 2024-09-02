using SDG.Unturned;
using UnityEngine;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class PlayerBarricade
    {
        public ushort ItemId { get; set; }
        public EBuild Build { get; set; }
        public Transform Transform { get; set; }
    }
}
