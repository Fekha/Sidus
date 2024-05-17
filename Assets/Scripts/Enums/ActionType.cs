using System.ComponentModel;

public enum ActionType
{
    [Description("Generate Module")]
    GenerateModule,
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
}

