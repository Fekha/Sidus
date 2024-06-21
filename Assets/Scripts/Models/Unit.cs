using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class Unit : Node
{
    internal string unitName;
    internal Guid unitGuid;
    internal Guid playerGuid;
    internal PlayerColor playerColor;
    internal int teamId;
    internal int maxHP;
    internal int HP;
    internal int maxMovement;
    internal int movementLeft;
    internal int kineticPower;
    internal int thermalPower;
    internal int explosivePower;
    internal int kineticDamageModifier;
    internal int thermalDamageModifier;
    internal int explosiveDamageModifier;
    internal int maxMining;
    internal int miningLeft;
    internal double supportValue;
    internal int level;
    internal int globalCreditGain;
    internal int maxAttachedModules; // 1+ station.level
    internal Direction facing;
    internal List<Module> attachedModules = new List<Module>();
    internal List<ModuleEffect> moduleEffects = new List<ModuleEffect>();
    internal TextMeshPro HPText;
    internal TextMeshPro statText;
    internal GameObject selectIcon;
    internal GameObject inCombatIcon;
    internal Transform unitImage;
    internal List<Tuple<int, int>> _minedPath = new List<Tuple<int, int>>();
    internal bool hasMoved = false;

    public void InitializeUnit(int _x, int _y, int _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid, int _mining, Direction _direction)
    {
        teamId = _color % Globals.Teams;
        facing = _direction;
        unitGuid = _unitGuid;
        location = new Coords(_x, _y);
        maxHP = _hp;
        HP = _hp;
        playerColor = (PlayerColor)_color;
        maxMovement = _range;
        movementLeft = _range;
        kineticPower = _electricAttack;
        thermalPower = _thermalAttack;
        explosivePower = _voidAttack;
        maxMining = _mining;
        miningLeft = _mining;
        level = 1;
        supportValue = .5;
        globalCreditGain = 0;
        maxAttachedModules = 1;
        GetUIComponents();
    }
    internal void InitializeUnit(ServerUnit unit)
    {
        unitGuid = unit.UnitGuid;
        facing = (Direction)unit.Facing;
        location = new Coords(unit.X,unit.Y);
        unitName = unit.UnitName;
        playerColor = (PlayerColor)unit.PlayerColor;
        playerGuid = unit.PlayerGuid;
        teamId = unit.TeamId;
        maxHP = unit.MaxHP;
        HP = unit.HP;
        maxMovement = unit.MaxMovement;
        movementLeft = unit.MovementLeft;
        kineticPower = unit.KineticPower;
        thermalPower = unit.ThermalPower;
        explosivePower = unit.ExplosivePower;
        kineticDamageModifier = unit.KineticDamageModifier;
        thermalDamageModifier = unit.ThermalDamageModifier;
        explosiveDamageModifier = unit.ExplosiveDamageModifier;
        maxMining = unit.MaxMining;
        miningLeft = unit.MiningLeft;
        supportValue = unit.SupportValue;
        level = unit.Level;
        globalCreditGain = unit.GlobalCreditGain;
        maxAttachedModules = unit.MaxAttachedModules;
        attachedModules = GameManager.i.AllModules.Where(x => unit.AttachedModules.Contains(x.moduleGuid.ToString())).ToList();
        moduleEffects = unit.ModuleEffects.Select(x => (ModuleEffect)x).ToList();
        GetUIComponents();
    }
    private void GetUIComponents()
    {
        currentPathNode.unitOnPath = this;
        transform.position = currentPathNode.transform.position;
        HPText = transform.Find("HP").GetComponent<TextMeshPro>();
        statText = transform.Find("HP/Stats").GetComponent<TextMeshPro>();
        selectIcon = transform.Find("Select").gameObject;
        inCombatIcon = transform.Find("InCombat").gameObject;
        unitImage = transform.Find("Unit");
        SpriteRenderer unitSprite = unitImage.GetComponent<SpriteRenderer>();
        unitSprite.color = GridManager.i.playerColors[(int)playerColor];
        if (this is Station)
        {
            unitSprite.sprite = level == 1 ? GridManager.i.stationSprites[(int)playerColor,level-1] : level == 2 ? GridManager.i.stationSprites[(int)playerColor,level-1] : level == 3 ? GridManager.i.stationSprites[(int)playerColor,level-1] : GridManager.i.stationSprites[(int)playerColor,level-1];
        }
        else
        {
            unitSprite.sprite = level == 1 ? GridManager.i.fleetSprites[(int)playerColor,level-1] : level == 2 ? GridManager.i.fleetSprites[(int)playerColor,level-1] : level == 3 ? GridManager.i.fleetSprites[(int)playerColor,level-1] : GridManager.i.fleetSprites[(int)playerColor,level-1];
        }
        unitImage.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270 - (facing == Direction.TopRight ? -60 : (int)facing * 60));
        GameManager.i.AllUnits.Add(this);
    }

    public void RegenHP(int regen, bool queuing = false)
    {
        if (moduleEffects.Contains(ModuleEffect.DoubleHeal))
            regen *= 2;
        if (regen != 0 && HP < maxHP && !queuing)
            StartCoroutine(GameManager.i.FloatingTextAnimation($"+{Math.Min(regen,maxHP-HP)} HP",transform,this));
        HP += regen;
    }
    public void TakeDamage(int damage,bool maxHpDamage = false)
    {
        HP -= Mathf.Max(damage,0);
        HP = Mathf.Max(0, HP);
        if (maxHpDamage)
        {
            maxHP -= Mathf.Max(damage, 0);
        }
        ShowHPText(true);
    }
    internal void IncreaseMaxHP(int hp)
    {
        maxHP += hp;
    }
    internal void IncreaseMaxMining(int mining)
    {
        maxMining += mining;
        miningLeft += mining;
    }
    internal void IncreaseMaxMovement(int movement)
    {
        maxMovement += movement;
        movementLeft += movement;
    }
    
    internal void resetMovementRange()
    {
        movementLeft = maxMovement;
    }
    internal void resetMining()
    {
        miningLeft = maxMining;
    }

    internal void clearMovementRange()
    {
        movementLeft = 0;
    }

    internal int getMovementRange()
    {
        return movementLeft;
    }

    internal int getMaxMovementRange()
    {
        return maxMovement;
    }

    internal void subtractMovement(int i)
    {
        movementLeft -= i;
    }
    internal void ShowHPText(bool value)
    {
        if (teamId != GameManager.i.MyStation.teamId && moduleEffects.Contains(ModuleEffect.HiddenStats))
        {
            HPText.text = $"?";
            statText.text = $"?|?|?";
        }
        else
        {
            HPText.text = $"{Mathf.Min(maxHP, HP)}";
            statText.text = $"{kineticPower}|{thermalPower}|{explosivePower}";
        }
        HPText.gameObject.SetActive(value);
    }
    internal void EditModule(int id, int modifer = 1)
    {
        switch (id)
        {
            case 0:
                kineticPower += (5 * modifer);
                thermalDamageModifier += (-1 * modifer);
                break;
            case 1:
                thermalPower += (5 * modifer);
                IncreaseMaxHP(1 * modifer);
                break;
            case 2:
                explosivePower += (5 * modifer);
                kineticDamageModifier += (1 * modifer);
                break;
            case 3:
                IncreaseMaxMovement(1 * modifer);
                kineticPower += (1 * modifer);
                break;
            case 4:
                IncreaseMaxMovement(1 * modifer);
                thermalPower += (1 * modifer);
                break;
            case 5:
                IncreaseMaxMovement(1 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 6:
                kineticPower += (2 * modifer);
                explosivePower += (3 * modifer);
                break;
            case 7:
                kineticPower += (2 * modifer);
                thermalPower += (3 * modifer);
                break;
            case 8:
                kineticPower += (6 * modifer);
                explosivePower += (-1 * modifer);
                thermalDamageModifier += (-3 * modifer);
                break;
            case 9:
                thermalPower += (6 * modifer);
                kineticPower += (-1 * modifer);
                explosiveDamageModifier += (-2 * modifer);
                break;
            case 10:
                explosivePower += (6 * modifer);
                thermalPower += (-1 * modifer);
                kineticDamageModifier += (-1 * modifer);
                break;
            case 11:
                kineticPower += (1 * modifer);
                thermalPower += (2 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 12:
                thermalPower += (3 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 13:
                IncreaseMaxMining(2 * modifer);
                kineticDamageModifier += (3 * modifer);
                break;
            case 14:
                IncreaseMaxMining(2 * modifer);
                thermalDamageModifier += (3 * modifer);
                break;
            case 15:
                IncreaseMaxMining(2 * modifer);
                explosiveDamageModifier += (3 * modifer);
                break;
            case 16:
                kineticPower += (3 * modifer);
                thermalPower += (-2 * modifer);
                explosivePower += (4 * modifer);
                break;
            case 17:
                kineticPower += (3 * modifer);
                thermalPower += (4 * modifer);
                explosivePower += (-2 * modifer);
                break;
            case 18:
                kineticPower += (-2 * modifer);
                thermalPower += (3 * modifer);
                explosivePower += (4 * modifer);
                break;
            case 19:
                thermalPower += (-2 * modifer);
                explosivePower += (5 * modifer);
                IncreaseMaxHP(5 * modifer);
                break;
            case 20:
                IncreaseMaxMovement(1 * modifer);
                kineticDamageModifier += (2 * modifer); 
                break;
            case 21:
                IncreaseMaxMovement(1 * modifer);
                thermalDamageModifier += (2 * modifer);
                break;
            case 22:
                IncreaseMaxMovement(1 * modifer);
                explosiveDamageModifier += (2 * modifer);
                break;
            case 23:
                kineticPower += (3 * modifer);
                thermalDamageModifier += (2 * modifer);
                explosiveDamageModifier += (2 * modifer);
                break;
            case 24:
                thermalPower += (3 * modifer);
                explosiveDamageModifier += (2 * modifer);
                kineticDamageModifier += (2 * modifer);
                break;
            case 25:
                explosivePower += (3 * modifer);
                kineticDamageModifier += (2 * modifer);
                thermalDamageModifier += (2 * modifer);
                break;
            case 26:
                IncreaseMaxHP(5 * modifer);
                kineticDamageModifier += (-4 * modifer);
                break;
            case 27:
                IncreaseMaxHP(5 * modifer);
                thermalDamageModifier += (-4 * modifer);
                break;
            case 28:
                IncreaseMaxHP(5 * modifer);
                explosiveDamageModifier += (-4 * modifer);
                break;
            case 29:
                IncreaseMaxHP(5 * modifer);
                kineticPower += (2 * modifer);
                break;
            case 30:
                IncreaseMaxHP(5 * modifer);
                thermalPower += (2 * modifer);
                break;
            case 31:
                IncreaseMaxHP(5 * modifer);
                explosivePower += (2 * modifer);
                break;  
            case 32:
                globalCreditGain += (1 * modifer);
                break; 
            case 33:
                globalCreditGain += (2 * modifer);
                IncreaseMaxMining(-1 * modifer);
                break; 
            case 34:
                globalCreditGain += (4 * modifer);
                IncreaseMaxMovement(-1 * modifer);
                break;
            case 35:
                IncreaseMaxMining(3 * modifer);
                kineticPower += (3 * modifer);
                thermalPower += (-1 * modifer);
                break; 
            case 36:
                IncreaseMaxMining(3 * modifer);
                thermalPower += (2 * modifer);
                break; 
            case 37:
                IncreaseMaxMining(3 * modifer);
                explosivePower += (3 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 38:
                kineticPower += (7 * modifer);
                thermalPower += (-2 * modifer);
                explosiveDamageModifier += (-3 * modifer);
                break;
            case 39:
                thermalPower += (7 * modifer);
                explosivePower += (-2 * modifer);
                kineticDamageModifier += (-2 * modifer);
                break;
            case 40:
                kineticPower += (-2 * modifer);
                explosivePower += (7 * modifer);
                thermalDamageModifier += (-1 * modifer);
                break; 
            case 41:
                kineticPower += (2 * modifer);
                supportValue = modifer == 1 ? 1 : .5;
                break;
            case 42:
                thermalPower += (3 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.ReduceMaxHp); }
                else { moduleEffects.Remove(ModuleEffect.ReduceMaxHp); }
                break;
            case 43:
                explosivePower += (4 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.DoubleHeal); }
                else { moduleEffects.Remove(ModuleEffect.DoubleHeal); }
                break;
            case 44:
                explosivePower += (2 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatHeal3); }
                else { moduleEffects.Remove(ModuleEffect.CombatHeal3); }
                break;
            case 45:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatKinetic2); }
                else { moduleEffects.Remove(ModuleEffect.CombatKinetic2); }
                break;
            case 46:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatThermal2); }
                else { moduleEffects.Remove(ModuleEffect.CombatThermal2); }
                break;
            case 47:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatExplosive2); }
                else { moduleEffects.Remove(ModuleEffect.CombatExplosive2); }
                break;
            case 48:
                IncreaseMaxMining(1 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidCredits3); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidCredits3); }
                break;
            case 49:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.HiddenStats); }
                else { moduleEffects.Remove(ModuleEffect.HiddenStats); }
                break;
            case 50:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.SelfDestruct); }
                else { moduleEffects.Remove(ModuleEffect.SelfDestruct); }
                break; 
            case 51:
                IncreaseMaxMining(1 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidHP5); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidHP5); }
                break;
            default:
                break;
        }
    }

    internal ServerUnit ToServerUnit()
    {
        return new ServerUnit()
        {
            IsStation = this is Station,
            GameGuid = Globals.GameMatch.GameGuid,
            TurnNumber = GameManager.i.TurnNumber,
            PlayerGuid = playerGuid,
            UnitGuid = unitGuid,
            Facing = (int)facing,
            X = location.x,
            Y = location.y,
            UnitName = unitName,
            PlayerColor = (int)playerColor,
            TeamId = teamId,
            MaxHP = maxHP,
            HP = HP,
            MaxMovement = maxMovement,
            MovementLeft = movementLeft,
            KineticPower = kineticPower,
            ThermalPower = thermalPower,
            ExplosivePower = explosivePower,
            KineticDamageModifier = kineticDamageModifier,
            ThermalDamageModifier = thermalDamageModifier,
            ExplosiveDamageModifier = explosiveDamageModifier,
            MaxMining = maxMining,
            MiningLeft = miningLeft,
            SupportValue = supportValue,
            Level = level,
            GlobalCreditGain = globalCreditGain,
            MaxAttachedModules = maxAttachedModules,
            AttachedModules = String.Join(",",attachedModules.Select(x=>x.moduleGuid)),
            ModuleEffects = String.Join(",", moduleEffects.Select(x=>(int)x).ToList()),
        };
    }


    internal void ClearMinedPath(List<PathNode> selectedPath)
    {
        if (selectedPath != null)
        {
            foreach (var item in _minedPath)
            {
                selectedPath[item.Item1].AwardCredits(this, item.Item2 * -1,true);
            }
            _minedPath.Clear();
        }
    }

    internal void AddMinedPath(List<PathNode> selectedPath)
    {
        ClearMinedPath(selectedPath);
        for (int i = 0; i < selectedPath.Count; i++)
        {
            if (selectedPath[i].isAsteroid)
            {
                int j = i;
                var minedAmount = selectedPath[i].MineCredits(this, true);
                _minedPath.Add(new Tuple<int, int>(i, minedAmount));
            }
        }
    }
}