using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Common
{
    public const string ServerUrl = "https://localhost:7083";
    public const string ServerGameHub = Common.ServerUrl + "/GameHub";

    public enum Role
    {
        None,
        Human,
        Cat,
        Random
    }
}
