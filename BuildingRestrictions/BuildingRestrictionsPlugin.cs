using RestoreMonarchy.BuildingRestrictions.Helpers;
using RestoreMonarchy.BuildingRestrictions.Models;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RestoreMonarchy.BuildingRestrictions
{
    public class BuildingRestrictionsPlugin : RocketPlugin<BuildingRestrictionsConfiguration>
    {
        private List<PlayerBuildings> playerBuildings = new();

        public BuildingStats GetBuildingStats()
        {
            BuildingStats stats = new();
            stats.StructuresCount = playerBuildings.Sum(x => x.Structures.Count);
            stats.BarricadesCount = playerBuildings.Sum(x => x.Barricades.Count);
            stats.PlayersCount = playerBuildings.Count;

            return stats;
        }

        private PlayerBuildings GetPlayerBuildings(ulong steamId)
        {
            return playerBuildings.FirstOrDefault(x => x.SteamId == steamId);
        }

        private PlayerBuildings GetOrAddPlayerBuildings(ulong steamId)
        {
            PlayerBuildings playerBuildings = GetPlayerBuildings(steamId);
            if (playerBuildings == null)
            {
                playerBuildings = new()
                {
                    SteamId = steamId,
                    Structures = new(),
                    Barricades = new()
                };
                this.playerBuildings.Add(playerBuildings);
            }

            return playerBuildings;
        }

        private void AddPlayerStructure(ulong steamId, PlayerStructure structure)
        {
            PlayerBuildings playerBuildings = GetOrAddPlayerBuildings(steamId);
            playerBuildings.Structures.Add(structure);
        }

        private void AddPlayerBarricade(ulong steamId, PlayerBarricade barricade)
        {
            PlayerBuildings playerBuildings = GetOrAddPlayerBuildings(steamId);
            playerBuildings.Barricades.Add(barricade);
        }

        protected override void Load()
        {
            if (Level.isLoaded)
            {
                OnLevelLoaded(0);
            } else
            {
                Level.onLevelLoaded += OnLevelLoaded;
            }            
        }

        protected override void Unload()
        {
            Level.onLevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(int level)
        {
            LogDebug($"Loading buildings in to cache memory...");
            Stopwatch stopwatch = new();
            stopwatch.Start();
            playerBuildings.Clear();

            BarricadeRegion[,] bRegions = BarricadeManager.regions.Clone() as BarricadeRegion[,];
            foreach (BarricadeRegion region in bRegions)
            {
                List<BarricadeDrop> drops = region.drops.ToList();

                for (int i = 0; i < drops.Count; i++)
                {
                    BarricadeDrop drop = drops[i];
                    PlayerBarricade playerBarricade = new()
                    {
                        ItemId = drop.asset.id,
                        Build = drop.asset.build,
                        Transform = drop.model
                    };
                    BarricadeData data = drop.GetServersideData();
                    AddPlayerBarricade(data.owner, playerBarricade);
                }
            }

            StructureRegion[,] sRegions = StructureManager.regions.Clone() as StructureRegion[,];
            foreach (StructureRegion region in sRegions)
            {
                List<StructureDrop> drops = region.drops.ToList();

                for (int i = 0; i < drops.Count; i++)
                {
                    StructureDrop drop = drops[i];
                    PlayerStructure playerStructure = new()
                    {
                        ItemId = drop.asset.id,
                        Construct = drop.asset.construct,
                        Transform = drop.model
                    };
                    StructureData data = drop.GetServersideData();
                    AddPlayerStructure(data.owner, playerStructure);
                }
            }

            BuildingStats stats = GetBuildingStats();
            stopwatch.Stop();
            LogDebug($"Loading {stats.BuildingsCount} buildings took {Math.Round(stopwatch.Elapsed.TotalSeconds, 2).ToString("N2")} seconds.");
        }

        private void LogDebug(string message)
        {
            if (Configuration.Instance.Debug)
            {
                Logger.Log($"Debug >> {message}");
            }
        }
    }
}
