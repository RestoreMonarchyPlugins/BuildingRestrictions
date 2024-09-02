using RestoreMonarchy.BuildingRestrictions.Databases;
using RestoreMonarchy.BuildingRestrictions.Models;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RestoreMonarchy.BuildingRestrictions
{
    public class BuildingRestrictionsPlugin : RocketPlugin<BuildingRestrictionsConfiguration>
    {
        public static BuildingRestrictionsPlugin Instance { get; private set; }
        public BuildingsDatabase Database { get; private set; }

        protected override void Load()
        {
            Instance = this;
            Database = new BuildingsDatabase();

            if (Level.isLoaded)
            {
                OnLevelLoaded(0);
            } else
            {
                Level.onLevelLoaded += OnLevelLoaded;
            }

            BarricadeManager.onDeployBarricadeRequested += OnDeployBarricadeRequested;
            BarricadeManager.onBarricadeSpawned += OnBarricadeSpawned;
            StructureManager.onStructureSpawned += OnStructureSpawned;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log("Check out more Unturned plugins at restoremonarchy.com");
        }

        protected override void Unload()
        {
            Level.onLevelLoaded -= OnLevelLoaded;
            BarricadeManager.onBarricadeSpawned -= OnBarricadeSpawned;
            StructureManager.onStructureSpawned -= OnStructureSpawned;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        private void OnLevelLoaded(int level)
        {
            LogDebug($"Loading buildings in to cache memory...");
            Stopwatch stopwatch = new();
            stopwatch.Start();
            Database.Clear();

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
                    Database.AddPlayerBarricade(data.owner, playerBarricade);
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
                    Database.AddPlayerStructure(data.owner, playerStructure);
                }
            }

            BuildingStats stats = Database.GetBuildingStats();
            stopwatch.Stop();
            double seconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2);
            LogDebug($"Loading {stats.BuildingsCount} buildings took {seconds:N2} seconds.");
        }

        private void LogDebug(string message)
        {
            if (Configuration.Instance.Debug)
            {
                Logger.Log($"Debug >> {message}");
            }
        }

        private decimal GetPlayerBuildingsMultiplier(UnturnedPlayer player)
        {
            decimal multiplier = 1;
            if (Configuration.Instance.Multipliers.Length > 0)
            {
                foreach (PermissionMultiplier permissionMultiplier in Configuration.Instance.Multipliers.OrderByDescending(x => x.Multiplier))
                {
                    if (player.HasPermission(permissionMultiplier.Permission))
                    {
                        multiplier = permissionMultiplier.Multiplier;
                        return multiplier;
                    }
                }
            }

            return multiplier;
        }

        private void OnDeployBarricadeRequested(Barricade barricade, ItemBarricadeAsset asset, UnityEngine.Transform hit, ref UnityEngine.Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow)
            {
                return;
            }

            CSteamID steamID = new(owner);
            Player player = PlayerTool.getPlayer(steamID);
            if (player == null)
            {
                return;
            }

            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            decimal multiplier = GetPlayerBuildingsMultiplier(unturnedPlayer);
            PlayerBuildings playerBuildings = Database.GetPlayerBuildings(owner);

            int buildingsCount = (playerBuildings.Barricades.Count + playerBuildings.Structures.Count);
            int barricadesCount = playerBuildings.Barricades.Count;
            int itemIdCount = playerBuildings.Barricades.Count(x => x.ItemId == barricade.asset.id);
            int buildCount = playerBuildings.Barricades.Count(x => x.Build == barricade.asset.build);

            if (Configuration.Instance.MaxBuildings * multiplier <= buildingsCount)
            {

                shouldAllow = false;
                return;
            }

            if (Configuration.Instance.MaxBarricades * multiplier <= barricadesCount)
            {

                shouldAllow = false;
                return;
            }

            BuildingRestriction itemIdRestriction = Configuration.Instance.Barricades.FirstOrDefault(x => x.ItemId == barricade.asset.id);
            if (itemIdRestriction != null)
            {
                if (itemIdRestriction.Max <= )
                {

                }
            }


        }

        private void OnBarricadeSpawned(BarricadeRegion region, BarricadeDrop drop)
        {
            BarricadeData barricadeData = drop.GetServersideData();
            PlayerBarricade playerBarricade = new()
            {
                ItemId = drop.asset.id,
                Build = drop.asset.build,
                Transform = drop.model
            };
            Database.AddPlayerBarricade(barricadeData.owner, playerBarricade);
        }

        private void OnStructureSpawned(StructureRegion region, StructureDrop drop)
        {
            StructureData structureData = drop.GetServersideData();
            PlayerStructure playerStructure = new()
            {
                ItemId = drop.asset.id,
                Construct = drop.asset.construct,
                Transform = drop.model
            };
            Database.AddPlayerStructure(structureData.owner, playerStructure);
        }
    }
}
