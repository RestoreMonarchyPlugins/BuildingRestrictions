using System.Xml.Serialization;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class PermissionMultiplier
    {
        public PermissionMultiplier(string permission, decimal multiplier)
        {
            Permission = permission;
            Multiplier = multiplier;
        }

        public PermissionMultiplier() { }

        [XmlAttribute]
        public string Permission { get; set; }
        [XmlAttribute]
        public decimal Multiplier { get; set; }
    }
}
