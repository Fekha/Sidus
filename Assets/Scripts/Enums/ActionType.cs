using System.ComponentModel;

public enum ActionType
{
    [Description("Repair Fleet")]
    RepairFleet,
    [Description("Move Unit")]
    MoveUnit,
    [Description("Create Fleet")]
    CreateFleet,
    [Description("Upgrade Fleet")]
    UpgradeFleet,
    [Description("Upgrade Station")]
    UpgradeStation,
    [Description("Attach Module")]
    AttachModule,
    [Description("Swap Module")]
    SwapModule,
    [Description("Mine Asteroid")]
    MineAsteroid,
    [Description("Move & Mine")]
    MoveAndMine,
    [Description("Bid on module")]
    BidOnModule,
    [Description("Gain 1 Credit")]
    GainCredit,
    [Description("Research Station Level")]
    ResearchStationLvl,
    [Description("Research Fleet Level")]
    ResearchFleetLvl,
    [Description("Research Max Fleets")]
    ResearchMaxFleets,
    [Description("Research HP")]
    ResearchHP,
    [Description("Research Kinetic Power")]
    ResearchKinetic,
    [Description("Research Thermal Power")]
    ResearchThermal,
    [Description("Research Explosive Power")]
    ResearchExplosive,
    [Description("Research Mining Power")]
    ResearchMining,
}

