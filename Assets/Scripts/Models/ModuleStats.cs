using System;

public class ModuleStats
{
    public int ModuleId { get; set; }
    public int MovementRange { get; set; }
    public int DeployRange { get; set; }
    public int Credits { get; set; }
    public int HP { get; set; }
    public int MiningPower { get; set; }
    public int KineticPower { get; set; }
    //public int ThermalPower { get; set; }
    public int ExplosivePower { get; set; }
    public int KineticDamageTaken { get; set; }
    //public int ThermalDamageTaken { get; set; }
    public int ExplosiveDamageTaken { get; set; }
    public string AbilityText { set { AbilityId = (int)Enum.Parse<ModuleEffect>(value); } }
    internal int AbilityId;
}