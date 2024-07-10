using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    internal int deployRange;
    internal int kineticPower;
    internal int thermalPower;
    internal int explosivePower;
    internal int kineticDamageTaken;
    internal int thermalDamageTaken;
    internal int explosiveDamageTaken;
    internal int kineticDeployPower;
    internal int thermalDeployPower;
    internal int explosiveDeployPower;
    internal int maxMining;
    internal int miningLeft;
    internal double supportValue;
    internal int level;
    internal int globalCreditGain;
    internal int maxAttachedModules; // 1+ station.level
    internal Direction facing;
    internal List<Module> attachedModules = new List<Module>();
    internal List<ModuleEffect> moduleEffects = new List<ModuleEffect>();
    internal UnitType unitType;
    internal TextMeshPro HPText;
    internal TextMeshPro statText;
    internal GameObject selectIcon;
    internal GameObject inCombatIcon;
    internal Transform unitImage;
    internal TrailRenderer trail;
    internal List<Tuple<int, int>> _minedPath = new List<Tuple<int, int>>();
    internal bool hasMoved = false;

    public void InitializeUnit(int _x, int _y, int _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid, int _mining, Direction _direction, UnitType _unitType)
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
        deployRange = 1;
        unitType = _unitType;
        GetUIComponents();
    }
    internal void InitializeUnit(ServerUnit unit)
    {
        unitGuid = unit.UnitGuid;
        unitType = (UnitType)unit.UnitType;
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
        kineticDamageTaken = unit.KineticDamageModifier;
        thermalDamageTaken = unit.ThermalDamageModifier;
        explosiveDamageTaken = unit.ExplosiveDamageModifier;
        kineticDeployPower = unit.KineticDeployPower;
        thermalDeployPower = unit.ThermalDeployPower;
        explosiveDeployPower = unit.ExplosiveDeployPower;
        maxMining = unit.MaxMining;
        miningLeft = unit.MiningLeft;
        supportValue = unit.SupportValue;
        level = unit.Level;
        globalCreditGain = unit.GlobalCreditGain;
        maxAttachedModules = unit.MaxAttachedModules;
        deployRange = unit.DeployRange;
        attachedModules = GameManager.i.AllModules.Where(x => unit.AttachedModules.Contains(x.moduleGuid.ToString())).ToList();
        if (!String.IsNullOrEmpty(unit.ModuleEffects))
        {
            var moduleEffectList = unit.ModuleEffects.Split(",");
            if (moduleEffectList.Any())
                moduleEffects = moduleEffectList.Select(x => (ModuleEffect)int.Parse(x)).ToList();
        }
        GetUIComponents();
    }
    private void GetUIComponents()
    {
        currentPathNode.unitOnPath = this;
        transform.position = currentPathNode.transform.position;
        HPText = transform.Find("HP").GetComponent<TextMeshPro>();
        statText = transform.Find("Stats").GetComponent<TextMeshPro>();
        if (unitType == UnitType.Bomb)
        {
            GetComponent<SpriteRenderer>().color = GridManager.i.playerColors[(int)playerColor];
        }
        else
        {
            selectIcon = transform.Find("Select").gameObject;
            inCombatIcon = transform.Find("InCombat").gameObject;
            unitImage = transform.Find("Unit");
            SpriteRenderer unitSprite = unitImage.GetComponent<SpriteRenderer>();
            unitSprite.color = GridManager.i.playerColors[(int)playerColor];
            if (this is Station)
            {
                unitSprite.sprite = GridManager.i.stationSprites[level - 1];
                unitSprite.color = GridManager.i.playerColors[(int)playerColor];
            }
            else
            {
                unitSprite.sprite = GridManager.i.fleetSprites[(int)playerColor, level - 1];
            }
            trail = unitImage.GetComponent<TrailRenderer>();
            unitImage.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270 - (facing == Direction.TopRight ? -60 : (int)facing * 60));
        }
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
    public void TakeDamage(int damage, Unit unit)
    {
        StartCoroutine(GameManager.i.FloatingTextAnimation($"-{damage} HP", transform, this));
        HP -= Mathf.Max(damage,0);
        HP = Mathf.Max(0, HP);
        if (unit.moduleEffects.Contains(ModuleEffect.ReduceMaxHp))
        {
            maxHP -= Mathf.Max(damage, 0);
        }
        ShowHPText(true);
    }
    internal void IncreaseMaxHP(int hp)
    {
        maxHP += hp;
    }
    internal void IncreaseHP(int hp)
    {
        IncreaseMaxHP(hp);
        HP += hp;
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
        if(unitType != UnitType.Bomb)
            HPText.gameObject.SetActive(value);
        statText.gameObject.SetActive(value);
    }
    internal void EditModule(int id, int modifer = 1)
    {
        switch (id)
        {
            case 0:
                kineticPower += (5 * modifer);
                thermalDamageTaken += (-1 * modifer);
                break;
            case 1: //Roughly balancing everything on this module
                thermalPower += (5 * modifer);
                break;
            case 2:
                explosivePower += (5 * modifer);
                kineticDamageTaken += (1 * modifer);
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
                thermalDamageTaken += (-2 * modifer);
                break;
            case 9:
                thermalPower += (6 * modifer);
                kineticPower += (-1 * modifer);
                explosiveDamageTaken += (-1 * modifer);
                break;
            case 10:
                explosivePower += (6 * modifer);
                thermalPower += (-1 * modifer);
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
                kineticPower += (2 * modifer);
                kineticDamageTaken += (2 * modifer);
                break;
            case 14:
                IncreaseMaxMining(2 * modifer);
                thermalPower += (2 * modifer);
                thermalDamageTaken += (2 * modifer);
                break;
            case 15:
                IncreaseMaxMining(2 * modifer);
                explosivePower += (2 * modifer);
                explosiveDamageTaken += (2 * modifer);
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
                kineticDamageTaken += (2 * modifer); 
                break;
            case 21:
                IncreaseMaxMovement(1 * modifer);
                thermalDamageTaken += (2 * modifer);
                break;
            case 22:
                IncreaseMaxMovement(1 * modifer);
                explosiveDamageTaken += (2 * modifer);
                break;
            case 23:
                kineticPower += (3 * modifer);
                thermalDamageTaken += (2 * modifer);
                explosiveDamageTaken += (1 * modifer);
                break;
            case 24:
                thermalPower += (3 * modifer);
                explosiveDamageTaken += (2 * modifer);
                kineticDamageTaken += (1 * modifer);
                break;
            case 25:
                explosivePower += (3 * modifer);
                kineticDamageTaken += (2 * modifer);
                thermalDamageTaken += (1 * modifer);
                break;
            case 26:
                IncreaseMaxHP(5 * modifer);
                kineticDamageTaken += (1 * modifer);
                thermalPower += (3 * modifer);
                break;
            case 27:
                IncreaseMaxHP(5 * modifer);
                kineticDamageTaken += (2 * modifer);
                explosivePower += (3 * modifer);
                break;
            case 28:
                IncreaseMaxHP(5 * modifer);
                kineticPower += (3 * modifer);
                break;
            case 29:
                IncreaseMaxHP(5 * modifer);
                kineticPower += (1 * modifer);
                explosiveDeployPower += (1 * modifer);
                break;
            case 30:
                IncreaseMaxHP(5 * modifer);
                thermalPower += (1 * modifer);
                thermalDeployPower += (1 * modifer);
                break;
            case 31:
                IncreaseMaxHP(5 * modifer);
                kineticDeployPower += (1 * modifer);
                explosivePower += (1 * modifer);
                break;  
            case 32:
                globalCreditGain += (1 * modifer);
                break; 
            case 33:
                globalCreditGain += (2 * modifer);
                IncreaseMaxMining(-2 * modifer);
                break; 
            case 34:
                globalCreditGain += (4 * modifer);
                IncreaseMaxMovement(-1 * modifer);
                break;
            case 35:
                IncreaseMaxMining(3 * modifer);
                kineticPower += (2 * modifer);
                thermalPower += (-1 * modifer);
                break; 
            case 36:
                IncreaseMaxMining(3 * modifer);
                thermalPower += (1 * modifer);
                break; 
            case 37:
                IncreaseMaxMining(3 * modifer);
                explosivePower += (2 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 38:
                kineticPower += (7 * modifer);
                thermalPower += (-2 * modifer);
                explosiveDamageTaken += (-2 * modifer);
                break;
            case 39:
                thermalPower += (7 * modifer);
                explosivePower += (-2 * modifer);
                kineticDamageTaken += (-1 * modifer);
                break;
            case 40:
                kineticPower += (-2 * modifer);
                explosivePower += (7 * modifer);
                break; 
            case 41:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.FullKineticSupport); }
                else { moduleEffects.Remove(ModuleEffect.FullKineticSupport); }
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
                explosivePower += (3 * modifer);
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
                IncreaseMaxMining(2 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidCredits4); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidCredits4); }
                break;
            case 49:
                kineticPower += (2 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.HiddenStats); }
                else { moduleEffects.Remove(ModuleEffect.HiddenStats); }
                break;
            case 50:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.SelfDestruct); }
                else { moduleEffects.Remove(ModuleEffect.SelfDestruct); }
                break; 
            case 51:
                IncreaseMaxMining(1 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidMining2); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidMining2); }
                break;
            case 52:
                kineticPower += (4 * modifer);
                explosivePower += (1 * modifer);
                thermalDamageTaken += (-1 * modifer);
                break;
            case 53:
                thermalPower += (4 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 54:
                kineticPower += (1 * modifer);
                explosivePower += (4 * modifer);
                thermalDamageTaken += (1 * modifer);
                break;
            case 55:
                kineticPower += (3 * modifer);
                thermalDamageTaken += (1 * modifer);
                explosiveDamageTaken += (2 * modifer);
                break;
            case 56:
                thermalPower += (3 * modifer);
                explosiveDamageTaken += (1 * modifer);
                kineticDamageTaken += (2 * modifer);
                break;
            case 57:
                explosivePower += (3 * modifer);
                kineticDamageTaken += (1 * modifer);
                thermalDamageTaken += (2 * modifer);
                break;
            case 58:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.FullThermalSupport); }
                else { moduleEffects.Remove(ModuleEffect.FullThermalSupport); }
                break;
            case 59:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.FullExplosiveSupport); }
                else { moduleEffects.Remove(ModuleEffect.FullExplosiveSupport); }
                break;
            case 60:
                thermalPower += (2 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.HiddenStats); }
                else { moduleEffects.Remove(ModuleEffect.HiddenStats); }
                break;
            case 61:
                explosivePower += (2 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.HiddenStats); }
                else { moduleEffects.Remove(ModuleEffect.HiddenStats); }
                break;
            case 62:
                kineticPower += (5 * modifer);
                explosiveDamageTaken += (-1 * modifer);
                break;
            case 63:
                explosivePower += (5 * modifer);
                thermalDamageTaken += (1 * modifer);
                break;
            case 64:
                kineticPower += (6 * modifer);
                thermalPower += (-1 * modifer);
                explosiveDamageTaken += (-2 * modifer);
                break;
            case 65:
                thermalPower += (6 * modifer);
                explosivePower += (-1 * modifer);
                kineticDamageTaken += (-1 * modifer);
                break;
            case 66:
                explosivePower += (6 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 67:
                deployRange += (1 * modifer);
                kineticDeployPower += (-1 * modifer);
                break;
            case 68:
                deployRange += (1 * modifer);
                thermalDeployPower += (-1 * modifer);
                break;
            case 69:
                deployRange += (1 * modifer);
                explosiveDeployPower += (-1 * modifer);
                break;
            case 70:
                kineticDeployPower += (2 * modifer);
                explosivePower += (1 * modifer);
                thermalDamageTaken += (-1 * modifer);
                break;
            case 71:
                thermalDeployPower += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 72:
                kineticPower += (1 * modifer);
                explosiveDeployPower += (2 * modifer);
                thermalDamageTaken += (1 * modifer);
                break;
            case 73:
                kineticPower += (3 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.ReduceMaxHp); }
                else { moduleEffects.Remove(ModuleEffect.ReduceMaxHp); }
                break;
            case 74:
                explosivePower += (3 * modifer);
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.ReduceMaxHp); }
                else { moduleEffects.Remove(ModuleEffect.ReduceMaxHp); }
                break;
            case 75:
                IncreaseMaxHP(5 * modifer);
                kineticDeployPower += (1 * modifer);
                explosiveDamageTaken += (2 * modifer);
                break;
            case 76:
                IncreaseMaxHP(5 * modifer);
                thermalDeployPower += (1 * modifer);
                kineticDamageTaken += (2 * modifer);
                break;
            case 77:
                IncreaseMaxHP(5 * modifer);
                explosiveDeployPower += (1 * modifer);
                thermalDamageTaken += (2 * modifer);
                break;
            case 78:
                kineticPower += (6 * modifer);
                thermalDeployPower += (-1 * modifer);
                thermalDamageTaken += (-1 * modifer);
                break;
            case 79:
                thermalPower += (6 * modifer);
                explosiveDeployPower += (-1 * modifer);
                explosiveDamageTaken += (-1 * modifer);
                break;
            case 80:
                explosivePower += (6 * modifer);
                kineticDeployPower += (-1 * modifer);
                kineticDamageTaken += (-1 * modifer);
                break;
            case 81:
                kineticDeployPower += (3 * modifer);
                thermalDeployPower += (-1 * modifer);
                thermalDamageTaken += (1 * modifer);
                break;
            case 82:
                thermalDeployPower += (3 * modifer);
                explosiveDeployPower += (-1 * modifer);
                explosiveDamageTaken += (1 * modifer);
                break;
            case 83:
                explosiveDeployPower += (3 * modifer);
                kineticDeployPower += (-1 * modifer);
                kineticDamageTaken += (1 * modifer);
                break;
            default:
                break;
        }
    }

    internal ServerUnit ToServerUnit()
    {
        return new ServerUnit()
        {
            UnitType = (int)unitType,
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
            KineticDamageModifier = kineticDamageTaken,
            ThermalDamageModifier = thermalDamageTaken,
            ExplosiveDamageModifier = explosiveDamageTaken,
            KineticDeployPower = kineticDeployPower,
            ThermalDeployPower = thermalDeployPower,
            ExplosiveDeployPower = explosiveDeployPower,
            DeployRange = deployRange,
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

    internal void DestroyUnit()
    {
        GameManager.i.AllUnits.Remove(this);
        currentPathNode.unitOnPath = null;
        if (unitType != UnitType.Station)
            Destroy(gameObject);
    }

    internal void CheckDestruction(Unit unitOnPath)
    {
        var unitOnPathDestroyed = false;
        var unitDestroyed = false;
        if (unitOnPath.HP <= 0)
        {
            unitOnPathDestroyed = true;
            Debug.Log($"{unitName} destroyed {unitOnPath.unitName}");
            if (unitOnPath is Station)
            {
                var station = (unitOnPath as Station);
                //while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
                GameManager.i.Winner = playerGuid;
            }
            else if (unitOnPath is Fleet)
            {
                GameManager.i.GetStationByGuid(unitOnPath.playerGuid).fleets.Remove(unitOnPath as Fleet);
                GameManager.i.GetStationByGuid(unitOnPath.playerGuid).modules.AddRange(unitOnPath.attachedModules.Where(x => x.moduleId != 50));
            }
            else if (this is Bomb)
            {
                GameManager.i.GetStationByGuid(playerGuid).bombs.Remove(this as Bomb);
            }
        }
        if (HP <= 0 || (unitOnPathDestroyed && this is Fleet && unitOnPath.moduleEffects.Contains(ModuleEffect.SelfDestruct)))
        {
            unitDestroyed = true;
            Debug.Log($"{unitOnPath.unitName} destroyed {unitName}");
            if (this is Station)
            {
                var station = (this as Station);
                //while (station.fleets.Count > 0) { AllUnits.Remove(station.fleets[0]); Destroy(station.fleets[0].gameObject); station.fleets.RemoveAt(0); }
                GameManager.i.Winner = unitOnPath.playerGuid;
            }
            else if (this is Fleet)
            {
                GameManager.i.GetStationByGuid(playerGuid).fleets.Remove(this as Fleet);
                GameManager.i.GetStationByGuid(playerGuid).modules.AddRange(attachedModules);
            }
            else if (this is Bomb)
            {
                GameManager.i.GetStationByGuid(playerGuid).bombs.Remove(this as Bomb);
            }
        }
        if(unitOnPathDestroyed)
            unitOnPath.DestroyUnit();
        if (unitDestroyed)
            DestroyUnit();
    }
}