using Models;
using System;
using System.Collections.Generic;

public static class Constants{
    internal static int ClientVersion = 25;
    internal static int TechAmount = 7;
    internal static int ShadowTechAmount = 1;
    internal static int MinTech = 11;
    internal static int MaxTech = MinTech+TechAmount;
    internal static int GridSize = 10;
    internal static int MaxActions = 5;
    internal static int MaxPlayers = 4;
    internal static int MaxUnitLevel = 4;
    internal static int MaxModules = 4;
    internal static int MinModules = 1;
    internal static List<int> SpyModules = new List<int>() { 49, 60, 61 };
    internal static int Create = 1;
    internal static int Remove = -1;
    internal static int StartingCredits = 5;
    internal static float MovementSpeed = .8f;
    internal static int HPGain = 2;
    internal static int NumberHelpPages = 9;
    internal static List<Guid> CPUGuids = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    internal static Random rnd = new Random();
}