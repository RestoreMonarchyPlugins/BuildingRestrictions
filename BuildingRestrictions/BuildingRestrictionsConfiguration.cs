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
        public string MessageIconUrl { get; set; }
        public bool EnableMaxBuildings { get; set; }
        public int MaxBuildings { get; set; }
        public bool EnableMaxBarricades { get; set; }
        public int MaxBarricades { get; set; }
        public bool EnableMaxStructures { get; set; }
        public int MaxStructures { get; set; }
        public bool BypassAdmin { get; set; }

        public bool EnableMaxBarricadeHeight { get; set; } = false;
        public float MaxBarricadeHeight { get; set; } = 100;
        public bool EnableMaxStructureHeight { get; set; } = false;
        public float MaxStructureHeight { get; set; } = 100;

        public bool EnableMaxBuildingsPerLocation { get; set; } = false;
        public float MaxBuildingsPerLocationHeight { get; set; } = 100;
        public int MaxBuildingsPerLocation { get; set; } = 10;

        [XmlArrayItem("Barricade")]
        public BuildingRestriction[] Barricades { get; set; } = [];
        [XmlArrayItem("Structure")]
        public BuildingRestriction[] Structures { get; set; } = [];
        [XmlArrayItem("Multiplier")]
        public PermissionMultiplier[] Multipliers { get; set; } = [];

        public void LoadDefaults()
        {
            Debug = false;
            MessageColor = "yellow";
            MessageIconUrl = "https://i.imgur.com/LlEcfBg.png";
            EnableMaxBuildings = false;
            MaxBuildings = 200;
            EnableMaxBarricades = false;
            MaxBarricades = 100;
            EnableMaxStructures = false;
            MaxStructures = 150;
            BypassAdmin = false;

            EnableMaxBarricadeHeight = false;
            MaxBarricadeHeight = 100;
            EnableMaxStructureHeight = false;
            MaxStructureHeight = 100;

            EnableMaxBuildingsPerLocation = false;
            MaxBuildingsPerLocationHeight = 100;
            MaxBuildingsPerLocation = 10;

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
