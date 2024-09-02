using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.BuildingRestrictions.Models
{
    public class BuildingStats
    {
        public int StructuresCount { get; set; }
        public int BarricadesCount { get; set; }
        public int PlayersCount { get; set; }
        public int BuildingsCount => StructuresCount + BarricadesCount;
    }
}
