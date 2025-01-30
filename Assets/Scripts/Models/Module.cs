using Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Module
{
    internal Guid moduleGuid;
    internal int moduleId;
    internal Sprite icon;
    internal string effectText;
    internal int minBid = 3;
    internal int currentBid = 3;
    internal int turnsLeftOnMarket = 5;
    internal ModuleStats moduleStats;
    public Module(int _moduleId, Guid _moduleGuid)
    {
        SetModule(_moduleId, _moduleGuid);
    }
    public Module(ServerModule? x)
    {
        if (x != null)
        {
            SetModule(x.ModuleId, (Guid)x.ModuleGuid);
            minBid = x.MidBid;
            turnsLeftOnMarket = x.TurnsLeft;
        }
    }

    internal ServerModule ToServerModule()
    {
        return new ServerModule()
        {
            GameGuid = Globals.GameMatch.GameGuid,
            TurnNumber = GameManager.i.TurnNumber,
            MidBid = minBid,
            ModuleGuid = moduleGuid,
            ModuleId = moduleId,
            TurnsLeft = turnsLeftOnMarket,
        };
    }
    private void SetModule(int _moduleId, Guid _moduleGuid)
    {
        moduleGuid = _moduleGuid;
        moduleId = _moduleId;
        moduleStats = GameManager.i.AllModulesStats.FirstOrDefault(x => x.ModuleId == moduleId);
        icon = Resources.Load<Sprite>($"Sprites/Modules/{moduleId}");
        if(!GameManager.i.AllModules.Any(x=>x.moduleGuid == _moduleGuid))
            GameManager.i.AllModules.Add(this);
        if(moduleStats == null)
        {
            Debug.LogError($"Module {moduleId} not found in AllModulesStats");
            effectText = $"Module {moduleId} not found in AllModulesStats";
            return;
        }
        if (moduleStats.MovementRange != 0)
        {
            var sign = moduleStats.MovementRange > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.MovementRange} Movement Range\n";
        }
        if (moduleStats.DeployRange != 0)
        {
            var sign = moduleStats.DeployRange > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.DeployRange} Deploy Range\n";
        }
        if (moduleStats.Credits != 0)
        {
            var sign = moduleStats.Credits > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.Credits} Credits per turn\n";
        }
        if (moduleStats.HP != 0)
        {
            var sign = moduleStats.HP > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.HP} Max HP\n";
        }
        if (moduleStats.MiningPower != 0)
        {
            var sign = moduleStats.MiningPower > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.MiningPower} Mining Power\n";
        }
        if (moduleStats.KineticPower != 0)
        {
            var sign = moduleStats.KineticPower > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.KineticPower} Kinetic Power\n";
        }
        //if (moduleStats.ThermalPower != 0)
        //{
        //    var sign = moduleStats.ThermalPower > 0 ? "+" : "";
        //    effectText += $"{sign}{moduleStats.ThermalPower} Thermal Power\n";
        //}
        if (moduleStats.ExplosivePower != 0)
        {
            var sign = moduleStats.ExplosivePower > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.ExplosivePower} Explosive Power\n";
        }
        if (moduleStats.KineticDamageTaken != 0)
        {
            var sign = moduleStats.KineticDamageTaken > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.KineticDamageTaken} Kinetic Damage Taken\n";
        }
        //if (moduleStats.ThermalDamageTaken != 0)
        //{
        //    var sign = moduleStats.ThermalDamageTaken > 0 ? "+" : "";
        //    effectText += $"{sign}{moduleStats.ThermalDamageTaken} Thermal Damage Taken\n";
        //}
        if (moduleStats.ExplosiveDamageTaken != 0)
        {
            var sign = moduleStats.ExplosiveDamageTaken > 0 ? "+" : "";
            effectText += $"{sign}{moduleStats.ExplosiveDamageTaken} Explosive Damage Taken\n";
        }
        if (moduleStats.AbilityId != 0)
        {
            effectText += $"{GetDescription(moduleStats.AbilityId)}\n";
        }
        effectText = effectText.Substring(0, effectText.Length - 1);
    }

    public void EditUnitStats(Unit unit, int modifier = 1)
    {
        if (moduleStats.MovementRange != 0)
            unit.IncreaseMaxMovement(moduleStats.MovementRange * modifier);
        if (moduleStats.DeployRange != 0)
            unit.deployRange += moduleStats.DeployRange * modifier;
        if (moduleStats.Credits != 0)
            unit.globalCreditGain += moduleStats.Credits * modifier;
        if (moduleStats.HP != 0)
            unit.IncreaseMaxHP(moduleStats.HP * modifier);
        if (moduleStats.MiningPower != 0)
            unit.IncreaseMaxMining(moduleStats.MiningPower * modifier);
        if (moduleStats.KineticPower != 0)
            unit.kineticPower += moduleStats.KineticPower * modifier;
        //if (moduleStats.ThermalPower != 0)
        //    unit.thermalPower += moduleStats.ThermalPower * modifier;
        if (moduleStats.ExplosivePower != 0)
            unit.explosivePower += moduleStats.ExplosivePower * modifier;
        if (moduleStats.KineticDamageTaken != 0)
            unit.kineticDamageTaken += moduleStats.KineticDamageTaken * modifier;
        //if (moduleStats.ThermalDamageTaken != 0)
        //    unit.thermalDamageTaken += moduleStats.ThermalDamageTaken * modifier;
        if (moduleStats.ExplosiveDamageTaken != 0)
            unit.explosiveDamageTaken += moduleStats.ExplosiveDamageTaken * modifier;
        if (moduleStats.AbilityId != 0)
        {
            if (modifier == 1) { unit.moduleEffects.Add((ModuleEffect)moduleStats.AbilityId); }
            else { unit.moduleEffects.Remove((ModuleEffect)moduleStats.AbilityId); }
        }
    }
    public string GetDescription(int value)
    {
        Type type = ModuleEffect.None.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
}