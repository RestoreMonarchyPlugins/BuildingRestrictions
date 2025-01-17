using HarmonyLib;
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

        public const string HarmonyId = "com.restoremonarchy.buildingrestrictions";
        private Harmony harmony;

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);

            Database = new BuildingsDatabase();

            harmony = new(HarmonyId);
            harmony.PatchAll();

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
            BarricadeManager.onDeployBarricadeRequested -= OnDeployBarricadeRequested;
            StructureManager.onDeployStructureRequested -= OnDeployStructureRequested;
            BarricadeManager.onBarricadeSpawned -= OnBarricadeSpawned;
            StructureManager.onStructureSpawned -= OnStructureSpawned;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "BuildingsRestriction", "You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] buildings." },
            { "BarricadesRestriction", "You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] barricades." },
            { "StructuresRestriction", "You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1}[[/b]] structures." },
            { "SpecificRestriction", "You can't build [[b]]{0}[[/b]] because you have reached the limit of max [[b]]{1} {2}[[/b]]." },
            { "SpecificRestrictionInfo", "You have placed [[b]]{0}/{1} {2}[[/b]]." },
            { "PlayerBuildingStatsYou", "You have placed [[b]]{0}{1}[[/b]] barricades and [[b]]{2}{3}[[/b]] structures, so in total [[b]]{4}{5}[[/b]] buildings." },
            { "PlayerBuildingStats", "[[b]]{0}[[/b]] have placed [[b]]{1}{2}[[/b]] barricades and [[b]]{3}{4}[[/b]] structures, so in total [[b]]{5}{6}[[/b]] buildings." },
            { "BuildingStats", "[[b]]{0}[[/b]] players have built [[b]]{1}[[/b]] barricades and [[b]]{2}[[/b]] structures, so in total [[b]]{3}[[/b]] buildings." },
            { "PlayerNotFound", "Player [[b]]{0}[[/b]] not found." },
            { "BuildingStatsOtherNoPermission", "You don't have permission to check other player building stats." },
            { "MaxBarricadeHeightRestriction", "You can't build [[b]]{0}[[/b]] because it's higher than max [[b]]{1}m[[/b]] height above the ground." },
            { "MaxStructureHeightRestriction", "You can't build [[b]]{0}[[/b]] because it's higher than max [[b]]{1}m[[/b]] height above the ground." }
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
            LogDebug($"Loading {stats.BuildingsCount:N0} buildings took {seconds:N2} seconds.");
        }

        private void LogDebug(string message)
        {
            if (Configuration.Instance.Debug)
            {
                Logger.Log($"Debug >> {message}");
            }
        }

        public decimal GetPlayerBuildingsMultiplier(IRocketPlayer player)
        {
            decimal multiplier = 1;
            if (Configuration.Instance.Multipliers != null && Configuration.Instance.Multipliers.Length > 0)
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

            if (Configuration.Instance.BypassAdmin && unturnedPlayer.IsAdmin)
            {
                return;
            }

            float groundHeight = LevelGround.getHeight(point);
            float barricadeHeight = point.y - groundHeight;
            if (Configuration.Instance.EnableMaxBarricadeHeight && barricadeHeight > Configuration.Instance.MaxBarricadeHeight)
            {
                SendMessageToPlayer(unturnedPlayer, "MaxBarricadeHeightRestriction", asset.itemName, Configuration.Instance.MaxBarricadeHeight.ToString("N0"));
                shouldAllow = false;
                return;
            }

            decimal multiplier = GetPlayerBuildingsMultiplier(unturnedPlayer);
            PlayerBuildings playerBuildings = Database.GetPlayerBuildings(owner);

            string itemName = barricade.asset.itemName;

            if (Configuration.Instance.EnableMaxBuildings)
            {
                int buildingsCount = (playerBuildings?.Barricades.Count ?? 0) + (playerBuildings?.Structures.Count ?? 0);
                int maxBuildings = (int)(Configuration.Instance.MaxBuildings * multiplier);

                if (maxBuildings <= buildingsCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "BuildingsRestriction", itemName, maxBuildings);
                    shouldAllow = false;
                    return;
                }
            }
            
            if (Configuration.Instance.EnableMaxBarricades)
            {
                int barricadesCount = playerBuildings?.Barricades.Count ?? 0;
                int maxBarricades = (int)(Configuration.Instance.MaxBarricades * multiplier);

                if (maxBarricades <= barricadesCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "BarricadesRestriction", itemName, maxBarricades);
                    shouldAllow = false;
                    return;
                }
            }

            if (Configuration.Instance.Barricades != null && Configuration.Instance.Barricades.Any())
            {
                int itemIdCount = playerBuildings?.Barricades.Count(x => x.ItemId == barricade.asset.id) ?? 0;
                int buildCount = playerBuildings?.Barricades.Count(x => x.Build == barricade.asset.build) ?? 0;

                BuildingRestriction itemIdRestriction = Configuration.Instance.Barricades.FirstOrDefault(x => x.ItemId != 0 && x.ItemId == barricade.asset.id);
                if (itemIdRestriction != null)
                {
                    int max = (int)(itemIdRestriction.Max * multiplier);
                    if (max <= itemIdCount)
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, itemIdRestriction.Name);
                        shouldAllow = false;
                    }
                    else
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", itemIdCount + 1, max, itemIdRestriction.Name);
                    }
                    return;
                }

                BuildingRestriction buildRestriction = Configuration.Instance.Barricades.FirstOrDefault(x => x.Build != null && x.Build.Equals(barricade.asset.build.ToString()));
                if (buildRestriction != null)
                {
                    int max = (int)(buildRestriction.Max * multiplier);
                    if (max <= buildCount)
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, buildRestriction.Name);
                        shouldAllow = false;
                    }
                    else
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", buildCount + 1, max, buildRestriction.Name);
                    }
                    return;
                }
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

            if (Configuration.Instance.BypassAdmin && unturnedPlayer.IsAdmin)
            {
                return;
            }

            float groundHeight = LevelGround.getHeight(point);
            float structureHeight = point.y - groundHeight;
            if (Configuration.Instance.EnableMaxStructureHeight && structureHeight > Configuration.Instance.MaxStructureHeight)
            {
                SendMessageToPlayer(unturnedPlayer, "MaxStructureHeightRestriction", asset.itemName, Configuration.Instance.MaxStructureHeight.ToString("N0"));
                shouldAllow = false;
                return;
            }

            decimal multiplier = GetPlayerBuildingsMultiplier(unturnedPlayer);
            PlayerBuildings playerBuildings = Database.GetPlayerBuildings(owner);

            string itemName = structure.asset.itemName;

            if (Configuration.Instance.EnableMaxBuildings)
            {
                int buildingsCount = ((playerBuildings?.Structures.Count ?? 0) + (playerBuildings?.Barricades.Count ?? 0));
                int maxBuildings = (int)(Configuration.Instance.MaxBuildings * multiplier);

                if (maxBuildings <= buildingsCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "BuildingsRestriction", itemName, maxBuildings);
                    shouldAllow = false;
                    return;
                }
            }

            if (Configuration.Instance.EnableMaxStructures)
            {
                int structuresCount = playerBuildings?.Structures.Count ?? 0;
                int maxStructures = (int)(Configuration.Instance.MaxStructures * multiplier);

                if (maxStructures <= structuresCount)
                {
                    SendMessageToPlayer(unturnedPlayer, "StructuresRestriction", itemName, maxStructures);
                    shouldAllow = false;
                    return;
                }
            }

            if (Configuration.Instance.Structures != null && Configuration.Instance.Structures.Any())
            {
                int itemIdCount = playerBuildings?.Structures.Count(x => x.ItemId == structure.asset.id) ?? 0;
                int constructCount = playerBuildings?.Structures.Count(x => x.Construct == structure.asset.construct) ?? 0;

                BuildingRestriction itemIdRestriction = Configuration.Instance.Structures.FirstOrDefault(x => x.ItemId != 0 && x.ItemId == structure.asset.id);
                if (itemIdRestriction != null)
                {
                    int max = (int)(itemIdRestriction.Max * multiplier);
                    if (max <= itemIdCount)
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, itemIdRestriction.Name);
                        shouldAllow = false;
                    }
                    else
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", itemIdCount + 1, max, itemIdRestriction.Name);
                    }
                    return;
                }

                BuildingRestriction structRestriction = Configuration.Instance.Structures.FirstOrDefault(x => x.Construct != null && x.Construct.Equals(structure.asset.construct.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (structRestriction != null)
                {
                    int max = (int)(structRestriction.Max * multiplier);
                    if (max <= constructCount)
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestriction", itemName, max, structRestriction.Name);
                        shouldAllow = false;
                    }
                    else
                    {
                        SendMessageToPlayer(unturnedPlayer, "SpecificRestrictionInfo", constructCount + 1, max, structRestriction.Name);
                    }
                    return;
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

        internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            if (player == null)
            {
                return;
            }

            string msg = Translate(translationKey, placeholder);
            msg = msg.Replace("[[", "<").Replace("]]", ">");
            if (player is ConsolePlayer)
            {
                Logger.Log(msg);
                return;
            }

            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
            if (unturnedPlayer != null)
            {
                ChatManager.serverSendMessage(msg, MessageColor, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, Configuration.Instance.MessageIconUrl, true);
            }
        }
    }
}
