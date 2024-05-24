using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;

public static class Globals{
    internal static GameMatch GameMatch;
    internal static int localStationIndex;
    internal static Guid localStationGuid;
    internal static bool Online = true;
    internal static bool HasBeenToLobby = false;
    internal static bool IsCPUGame = false;
}