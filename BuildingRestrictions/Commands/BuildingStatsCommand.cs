//using Rocket.API;
//using Rocket.Unturned.Player;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RestoreMonarchy.BuildingRestrictions.Commands
//{
//    public class BuildingStatsCommand : IRocketCommand
//    {
//        private BuildingRestrictionsPlugin pluginInstance => BuildingRestrictionsPlugin.Instance;

//        public void Execute(IRocketPlayer caller, string[] command)
//        {
//            string otherPlayerName = command.ElementAtOrDefault(0);
//            ulong steamId = 0;
//            if (otherPlayerName != null && caller.HasPermission("buildingstats.other"))
//            {
//                if (otherPlayerName.Length == 17 && ulong.TryParse(otherPlayerName, out steamId))
//                {

//                }



//                UnturnedPlayer.FromName(otherPlayerName);
//            }

//        }

//        public AllowedCaller AllowedCaller => AllowedCaller.Both;

//        public string Name => "buildingstats";

//        public string Help => "";

//        public string Syntax => "[player]";

//        public List<string> Aliases => new();

//        public List<string> Permissions => new();
//    }
//}
