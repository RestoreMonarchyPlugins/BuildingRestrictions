using SDG.Unturned;
using System.Xml.Serialization;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class BuildingRestriction
    {
        public BuildingRestriction(string name, ushort itemId, string build, string construct, int max)
        {
            Name = name;
            ItemId = itemId;
            Build = build;
            Construct = construct;
            Max = max;
        }

        public BuildingRestriction() { }

        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public ushort ItemId { get; set; }
        [XmlAttribute]
        public string Build { get; set; }
        [XmlAttribute]
        public string Construct { get; set; }
        [XmlAttribute]
        public int Max { get; set; }

        public bool ShouldSerializeItemId() => ItemId != 0;
        public bool ShouldSerializeBuild() => !string.IsNullOrEmpty(Build);
        public bool ShouldSerializeConstruct() => !string.IsNullOrEmpty(Construct);
    }
}
