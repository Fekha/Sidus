using Models;
using System;
using System.Collections.Generic;

public static class Globals{
    internal static GameMatch GameMatch;
    internal static Account Account;
    internal static int localStationIndex;
    internal static bool Online = true;
    internal static bool HasBeenToLobby = false;
    internal static bool IsCPUGame = false;
    internal static int Teams = 2;
    internal static bool DebugMode = false;
}