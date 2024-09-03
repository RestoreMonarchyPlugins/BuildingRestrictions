namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class PlayerBuildingStats
    {
        public ulong SteamId { get; set; }
        public int StructuresCount { get; set; }
        public int BarricadesCount { get; set; }
        public int BuildingsCount => StructuresCount + BarricadesCount;
    }
}
