using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public static class Common
    {
        public const string ServerUrl = "https://localhost:7083/api/Matchmaking";

        public enum Role
        {
            None,
            Human,
            Cat,
            Random
        }
    }
}
