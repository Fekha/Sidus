using System.ComponentModel;

public enum ModuleEffect
{
    [Description("")]
    None,
    [Description("Damage dealt reduces Max HP")]
    ReduceMaxHp,
    [Description("Double all repairing")]
    DoubleHeal,
    [Description("When entering combat:\nRepair 3 HP")]
    CombatHeal3,
    [Description("When entering combat:\nRepair 4 HP")]
    CombatHeal4,
    [Description("When entering combat:\nRepair 5 HP")]
    CombatHeal5,
    [Description("When entering combat:\nGain 3 Kinetic Power")]
    CombatKinetic3,
    [Description("When entering combat:\nGain 3 Thermal Power")]
    CombatThermal3,
    [Description("When entering combat:\nGain 3 Explosive Power")]
    CombatExplosive3,
    [Description("After destroying an asteroid:\nGain 3 Credits")]
    AsteroidCredits3,
    [Description("After destroying an asteroid:\nGain 5 Credits")]
    AsteroidCredits5,
    [Description("After destroying an asteroid:\nGain 7 Credits")]
    AsteroidCredits7,
    [Description("Stats are hidden from enemies")]
    HiddenStats,
    [Description("If destroyed while defending\nalso destroy the attacking bomber\nand this module")]
    SelfDestruct,
    [Description("After destroying an asteroid:\nGain 1 Mining Power")]
    AsteroidMining1,
    [Description("After destroying an asteroid:\nGain 2 Mining Power")]
    AsteroidMining2,
    [Description("After destroying an asteroid:\nGain 3 Mining Power")]
    AsteroidMining3,
    [Description("While supporting:\nProvide full Kinetic Power")]
    FullKineticSupport,
    [Description("While supporting:\nProvide full Thermal Power")]
    FullThermalSupport,
    [Description("While supporting:\nProvide full Explosive Power")]
    FullExplosiveSupport,
}