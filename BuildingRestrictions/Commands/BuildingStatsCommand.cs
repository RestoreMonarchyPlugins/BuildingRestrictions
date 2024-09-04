using RestoreMonarchy.BuildingRestrictions.Models;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace RestoreMonarchy.BuildingRestrictions.Commands
{
    public class BuildingStatsCommand : IRocketCommand
    {
        private BuildingRestrictionsPlugin pluginInstance => BuildingRestrictionsPlugin.Instance;
        private BuildingRestrictionsConfiguration configuration => pluginInstance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            string structuresCount;
            string barricadesCount;
            string buildingsCount;

            if (caller is ConsolePlayer && command.Length == 0)
            {
                BuildingStats buildingStats = pluginInstance.Database.GetBuildingStats();

                string playersCount = buildingStats.PlayersCount.ToString("N0");
                structuresCount = buildingStats.StructuresCount.ToString("N0");
                barricadesCount = buildingStats.BarricadesCount.ToString("N0");
                buildingsCount = buildingStats.BuildingsCount.ToString("N0");

                pluginInstance.SendMessageToPlayer(caller, "BuildingStats", playersCount, barricadesCount, structuresCount, buildingsCount);
                return;
            }

            ulong steamId = 0;
            string playerName = null;
            UnturnedPlayer player = null;
            if (command.Length > 0)
            {
                if (!caller.HasPermission("buildingstats.other"))
                {
                    pluginInstance.SendMessageToPlayer(caller, "BuildingStatsOtherNoPermission");
                    return;
                }

                string playerNameOrSteamId = command[0];

                player = UnturnedPlayer.FromName(playerNameOrSteamId);
                if (player != null)
                {
                    steamId = player.CSteamID.m_SteamID;
                    playerName = player.DisplayName;
                } else if (playerNameOrSteamId.Length != 17)
                {
                    ulong.TryParse(playerNameOrSteamId, out steamId);
                }

                if (steamId == 0)
                {
                    pluginInstance.SendMessageToPlayer(caller, "PlayerNotFound", playerNameOrSteamId);
                    return;
                }
            } else
            {
                player = (UnturnedPlayer)caller;
                steamId = player.CSteamID.m_SteamID;
            }

            PlayerBuildingStats playerBuildingStats = pluginInstance.Database.GetPlayerBuildingStats(steamId);

            buildingsCount = playerBuildingStats.BuildingsCount.ToString("N0");
            structuresCount = playerBuildingStats.StructuresCount.ToString("N0");
            barricadesCount = playerBuildingStats.BarricadesCount.ToString("N0");

            decimal multiplier = pluginInstance.GetPlayerBuildingsMultiplier((IRocketPlayer)player ?? new RocketPlayer(steamId.ToString()));
            string buildingsLimit = configuration.EnableMaxBuildings ? $"/{(int)(configuration.MaxBuildings * multiplier):N0}" : "";
            string structuresLimit = configuration.EnableMaxStructures ? $"/{(int)(configuration.MaxStructures * multiplier):N0}" : "";
            string barricadesLimit = configuration.EnableMaxBarricades ? $"/{(int)(configuration.MaxBarricades * multiplier):N0}" : "";

            if (caller.Id == steamId.ToString())
            {
                pluginInstance.SendMessageToPlayer(caller, "PlayerBuildingStatsYou", barricadesCount, barricadesLimit, structuresCount, structuresLimit, buildingsCount, buildingsLimit);
            } else
            {
                string name = playerName ?? steamId.ToString();
                pluginInstance.SendMessageToPlayer(caller, "PlayerBuildingStats", name, barricadesCount, barricadesLimit, structuresCount, structuresLimit, buildingsCount, buildingsLimit);
            }            
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "buildingstats";

        public string Help => "";

        public string Syntax => "[player]";

        public List<string> Aliases => new();

        public List<string> Permissions => new();
    }
}
