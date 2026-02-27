using System.Xml.Serialization;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class ExcludedItem
    {
        public ExcludedItem(ushort id, string name)
        {
            Id = id;
            Name = name;
        }

        public ExcludedItem() { }

        [XmlAttribute]
        public ushort Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
    }
}
