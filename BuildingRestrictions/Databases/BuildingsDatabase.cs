using RestoreMonarchy.BuildingRestrictions.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RestoreMonarchy.BuildingRestrictions.Databases
{
    public class BuildingsDatabase
    {
        private List<PlayerBuildings> playerBuildings = new();

        public void Clear()
        {
            playerBuildings.Clear();
        }

        public BuildingStats GetBuildingStats()
        {
            BuildingStats stats = new()
            {
                StructuresCount = playerBuildings.Sum(x => x.Structures.Count),
                BarricadesCount = playerBuildings.Sum(x => x.Barricades.Count),
                PlayersCount = playerBuildings.Count
            };

            return stats;
        }

        public PlayerBuildings GetPlayerBuildings(ulong steamId)
        {
            return playerBuildings.FirstOrDefault(x => x.SteamId == steamId);
        }

        public PlayerBuildings GetOrAddPlayerBuildings(ulong steamId)
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

        public void RemovePlayerBarricade(Transform transform)
        {
            for (int i = 0; i < this.playerBuildings.Count; i++)
            {
                PlayerBuildings playerBuildings = this.playerBuildings[i];
                for (int j = 0; j < playerBuildings.Barricades.Count; j++)
                {
                    PlayerBarricade playerBarricade = playerBuildings.Barricades[j];
                    if (playerBarricade.Transform == transform)
                    {
                        playerBuildings.Barricades.RemoveAt(j);
                        if (playerBuildings.Barricades.Count == 0 && playerBuildings.Structures.Count == 0)
                        {
                            this.playerBuildings.RemoveAt(i);
                        }
                        return;
                    }
                }
            }
        }

        public void RemovePlayerStructure(Transform transform)
        {
            for (int i = 0; i < this.playerBuildings.Count; i++)
            {
                PlayerBuildings playerBuildings = this.playerBuildings[i];
                for (int j = 0; j < playerBuildings.Structures.Count; j++)
                {
                    PlayerStructure playerStructure = playerBuildings.Structures[j];
                    if (playerStructure.Transform == transform)
                    {
                        playerBuildings.Structures.RemoveAt(j);
                        if (playerBuildings.Barricades.Count == 0 && playerBuildings.Structures.Count == 0)
                        {
                            this.playerBuildings.RemoveAt(i);
                        }
                        return;
                    }
                }
            }
        }

        public void AddPlayerStructure(ulong steamId, PlayerStructure structure)
        {
            PlayerBuildings playerBuildings = GetOrAddPlayerBuildings(steamId);
            playerBuildings.Structures.Add(structure);
        }

        public void AddPlayerBarricade(ulong steamId, PlayerBarricade barricade)
        {
            PlayerBuildings playerBuildings = GetOrAddPlayerBuildings(steamId);
            playerBuildings.Barricades.Add(barricade);
        }
    }
}
