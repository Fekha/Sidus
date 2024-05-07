using StarTaneousAPI.Models;
using System;
using System.Collections.Generic;

public static class Globals{
    internal static Player[] Players;
    internal static int localStationIndex;
    internal static Guid GameId;
    internal static Guid localStationGuid;
    internal static bool Online = true;
    internal static List<string> GameSettings = new List<string>();
}