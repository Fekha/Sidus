using System.ComponentModel;

public enum ActionType
{
    [Description("Repair Fleet")]
    RepairFleet,
    [Description("Move Unit")]
    MoveUnit,
    [Description("Deploy Fleet")]
    DeployFleet,
    [Description("Upgrade Fleet")]
    UpgradeFleet,
    [Description("Upgrade Station")]
    UpgradeStation,
    [Description("Attach Module")]
    AttachModule,
    [Description("Swap Module")]
    SwapModule,
    [Description("Deploy Bomb")]
    DeployBomb,
    [Description("Move & Mine")]
    MoveAndMine,
    [Description("Bid on module")]
    BidOnModule,
    [Description("Unlock Action")]
    UnlockAction,
    [Description("Research Station Level")]
    ResearchStationLvl,
    [Description("Research Fleet Level")]
    ResearchFleetLvl,
    [Description("Research Max Fleets")]
    ResearchMaxFleets,
    [Description("Research Kinetic Power")]
    ResearchKinetic,
    [Description("Research HP")]
    ResearchHP,
    [Description("Research Mining Power")]
    ResearchMining,
    //[Description("Research Thermal Power")]
    //ResearchThermal,
    [Description("Research Explosive Power")]
    ResearchExplosive,
}

