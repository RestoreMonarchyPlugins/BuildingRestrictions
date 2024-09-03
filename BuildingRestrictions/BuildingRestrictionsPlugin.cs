using RestoreMonarchy.BuildingRestrictions.Databases;
using RestoreMonarchy.BuildingRestrictions.Models;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
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
        public UnityEngine.Color MessageColor { get; set; }

        public BuildingsDatabase Database { get; private set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);

            Database = new BuildingsDatabase();

            if (Level.isLoaded)
            {
                OnLevelLoaded(0);
            } else
            {
                Level.onLevelLoaded += OnLevelLoaded;
            }

            BarricadeManager.onDeployBarricadeRequested += OnDeployBarricadeRequested;
            StructureManager.onDeployStructureRequested += OnDeployStructureRequested;
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

        public override TranslationList DefaultTranslations => new()
        {
            { "BuildingsRestriction", "You can't build {0} because you have reached the limit of max {1} buildings." },
            { "BarricadesRestriction", "You can't build {0} because you have reached the limit of max {1} barricades." },
            { "StructuresRestriction", "You can't build {0} because you have reached the limit of max {1} structures." },
            { "SpecificRestriction", "You can't build {0} because you have reached the limit of max {1} {2}." },
            { "SpecificRestrictionInfo", "You have placed {0}/{1} {2}." },
        };

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

            if (unturnedPlayer.IsAdmin)
            {
                return;
            }

            decimal multiplier = GetPlayerBuildingsMultiplier(unturnedPlayer);
            PlayerBuildings playerBuildings = Database.GetPlayerBuildings(owner);

            int buildingsCount = (playerBuildings.Barricades.Count + playerBuildings.Structures.Count);
            int barricadesCount = playerBuildings.Barricades.Count;
            int itemIdCount = playerBuildings.Barricades.Count(x => x.ItemId == barricade.asset.id);
            int buildCount = playerBuildings.Barricades.Count(x => x.Build == barricade.asset.build);
            string itemName = barricade.asset.itemName;

            int maxBuildings = (int)(Configuration.Instance.MaxBuildings * multiplier);
            int maxBarricades = (int)(Configuration.Instance.MaxBarricades * multiplier);

            if (Configuration.Instance.MaxBuildings * multiplier <= buildingsCount)
            {
                SendMessageToPlayer(unturnedPlayer, "BuildingsRestriction", itemName, maxBuildings);
                shouldAllow = false;
                return;
            }

            if (Configuration.Instance.MaxBarricades * multiplier <= barricadesCount)
            {
                SendMessageToPlayer(unturnedPlayer, "BarricadesRestriction", itemName, maxBarricades);
                shouldAllow = false;
                return;
            }

            BuildingRestriction itemIdRestriction = Configuration.Instance.Barricades.FirstOrDefault(x => x.ItemId != 0 && x.ItemId == barricade.asset.id);
            if (itemIdRestriction != null)
            {
                int max = (int)(itemIdRestriction.Max * multiplier);
                if (max <= itemIdCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, itemIdRestriction.Name);
                    shouldAllow = false;
                } else
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", itemIdCount, max, itemIdRestriction.Name);
                }
                return;
            }

            BuildingRestriction buildRestriction = Configuration.Instance.Barricades.FirstOrDefault(x => x.Build != null && x.Build.Equals(barricade.asset.build.ToString()));
            if (buildRestriction != null)
            {
                int max = (int)(buildRestriction.Max * multiplier);
                if (buildRestriction.Max <= buildCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, buildRestriction.Name);
                    shouldAllow = false;
                }
                else
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", buildCount, max, buildRestriction.Name);
                }
                return;
            }
        }

        private void OnDeployStructureRequested(Structure structure, ItemStructureAsset asset, ref UnityEngine.Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
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

            if (unturnedPlayer.IsAdmin)
            {
                return;
            }

            decimal multiplier = GetPlayerBuildingsMultiplier(unturnedPlayer);
            PlayerBuildings playerBuildings = Database.GetPlayerBuildings(owner);

            int buildingsCount = (playerBuildings.Structures.Count + playerBuildings.Structures.Count);
            int structuresCount = playerBuildings.Structures.Count;
            int itemIdCount = playerBuildings.Structures.Count(x => x.ItemId == structure.asset.id);
            int constructCount = playerBuildings.Structures.Count(x => x.Construct == structure.asset.construct);
            string itemName = structure.asset.itemName;

            int maxBuildings = (int)(Configuration.Instance.MaxBuildings * multiplier);
            int maxStructures = (int)(Configuration.Instance.MaxStructures * multiplier);

            if (Configuration.Instance.MaxBuildings * multiplier <= buildingsCount)
            {
                SendMessageToPlayer(unturnedPlayer, "BuildingsRestriction", itemName, maxBuildings);
                shouldAllow = false;
                return;
            }

            if (Configuration.Instance.MaxStructures * multiplier <= structuresCount)
            {
                SendMessageToPlayer(unturnedPlayer, "StructuresRestriction", itemName, maxStructures);
                shouldAllow = false;
                return;
            }

            BuildingRestriction itemIdRestriction = Configuration.Instance.Structures.FirstOrDefault(x => x.ItemId != 0 && x.ItemId == structure.asset.id);
            if (itemIdRestriction != null)
            {
                int max = (int)(itemIdRestriction.Max * multiplier);
                if (max <= itemIdCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, itemIdRestriction.Name);
                    shouldAllow = false;
                } else
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", itemIdCount, max, itemIdRestriction.Name);
                }
                return;
            }

            BuildingRestriction buildRestriction = Configuration.Instance.Structures.FirstOrDefault(x => x.Construct != null && x.Construct.Equals(structure.asset.construct.ToString()));
            if (buildRestriction != null)
            {
                int max = (int)(buildRestriction.Max * multiplier);
                if (buildRestriction.Max <= constructCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, buildRestriction.Name);
                    shouldAllow = false;
                }
                else
                {
                    SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", constructCount, max, buildRestriction.Name);
                }
                return;
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

        internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            string message = Translate(translationKey, placeholder);
            UnturnedChat.Say(player, message, MessageColor, true);
        }
    }
}
