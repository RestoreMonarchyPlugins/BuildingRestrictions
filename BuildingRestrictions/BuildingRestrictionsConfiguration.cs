using RestoreMonarchy.BuildingRestrictions.Models;
using Rocket.API;
using SDG.Unturned;
using System.Xml.Serialization;

namespace RestoreMonarchy.BuildingRestrictions
{
    public class BuildingRestrictionsConfiguration : IRocketPluginConfiguration
    {
        public bool Debug { get; set; }
        public bool ShouldSerializeDebug() => Debug;
        public string MessageColor { get; set; }
        public int MaxBuildings { get; set; }
        public int MaxBarricades { get; set; }
        public int MaxStructures { get; set; }

        [XmlArrayItem("Barricade")]
        public BuildingRestriction[] Barricades { get; set; }
        [XmlArrayItem("Structure")]
        public BuildingRestriction[] Structures { get; set; }
        [XmlArrayItem("Multiplier")]
        public PermissionMultiplier[] Multipliers { get; set; }

        public void LoadDefaults()
        {
            Debug = false;
            MessageColor = "yellow";
            MaxBuildings = 200;
            MaxBarricades = 100;
            MaxStructures = 150;
            Barricades =
            [
                new("sentries", 0, EBuild.SENTRY.ToString(), null, 5),
                new("stereos", 0, EBuild.STEREO.ToString(), null, 1),
                new("campfires", 0, EBuild.CAMPFIRE.ToString(), null, 2),
            ];
            Structures =
            [
                new("roofs", 0, null, EConstruct.ROOF.ToString(), 30),
                new("floors", 0, null, EConstruct.FLOOR.ToString(), 30)
            ];
            Multipliers =
            [
                new("buildings.vip", 1.5m)
            ];
        }
    }
}
