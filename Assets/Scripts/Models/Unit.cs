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
    //internal int thermalPower;
    internal int explosivePower;
    internal int kineticSupportPower;
    //internal int thermalSupportPower;
    internal int explosiveSupportPower;
    internal int kineticDamageTaken;
    //internal int thermalDamageTaken;
    internal int explosiveDamageTaken;
    internal int kineticDeployPower;
    //internal int thermalDeployPower;
    internal int explosiveDeployPower;
    internal int maxMining;
    internal int miningLeft;
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
    internal Transform unitImage;
    internal TrailRenderer trail;
    internal List<Tuple<int, int>> _minedPath = new List<Tuple<int, int>>();
    internal List<PathNode> _selectedPath = new List<PathNode>();
    internal bool hasMoved = false;
    internal bool hasMined = false;
    internal bool hasTakenDamage = false;

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
        //thermalPower = _thermalAttack;
        explosivePower = _voidAttack;
        maxMining = _mining;
        miningLeft = _mining;
        level = 1;
        globalCreditGain = 0;
        maxAttachedModules = 1;
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
        //thermalPower = unit.ThermalPower;
        explosivePower = unit.ExplosivePower;
        kineticDamageTaken = unit.KineticDamageModifier;
        //thermalDamageTaken = unit.ThermalDamageModifier;
        explosiveDamageTaken = unit.ExplosiveDamageModifier;
        kineticDeployPower = unit.KineticDeployPower;
        //thermalDeployPower = unit.ThermalDeployPower;
        explosiveDeployPower = unit.ExplosiveDeployPower;
        maxMining = unit.MaxMining;
        miningLeft = unit.MiningLeft;
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
        if (currentPathNode.unitOnPath == null)
        {
            currentPathNode.unitOnPath = this;
        }
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

    public void RegenHP(int regen, bool queuing = false, bool staggered = false)
    {
        if (regen > 0)
        {
            if (moduleEffects.Contains(ModuleEffect.DoubleHeal))
                regen *= 2;
            if (HP < maxHP && !queuing)
                StartCoroutine(GameManager.i.FloatingTextAnimation($"+{Math.Min(regen, maxHP - HP)} HP", transform, this, staggered));
            HP += regen;
        }
    }
    public void TakeDamage(int damage, Unit unit)
    {
        if (damage > 0)
        {
            hasTakenDamage = true;
            StartCoroutine(GameManager.i.FloatingTextAnimation($"-{damage} HP", transform, this));
            HP -= damage;
            HP = Mathf.Max(0, HP);
            if (unit.moduleEffects.Contains(ModuleEffect.ReduceMaxHp))
            {
                maxHP -= damage;
            }
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
        movementLeft = Mathf.Min(movementLeft, maxMovement);
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
            if (unitType != UnitType.Bomb)
            {
                //statText.text = $"{kineticPower}|{thermalPower}|{explosivePower}";
                statText.text = $"{kineticPower}|{explosivePower}";
            }
            else
                statText.text = $"{explosivePower}";
        }
        if(unitType != UnitType.Bomb)
            HPText.gameObject.SetActive(value);
        statText.gameObject.SetActive(value);
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
            //ThermalPower = thermalPower,
            ExplosivePower = explosivePower,
            KineticDamageModifier = kineticDamageTaken,
            //ThermalDamageModifier = thermalDamageTaken,
            ExplosiveDamageModifier = explosiveDamageTaken,
            KineticDeployPower = kineticDeployPower,
            //ThermalDeployPower = thermalDeployPower,
            ExplosiveDeployPower = explosiveDeployPower,
            DeployRange = deployRange,
            MaxMining = maxMining,
            MiningLeft = miningLeft,
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
        if (this is Bomber)
        {
            GameManager.i.GetStationByGuid(playerGuid).fleets.Remove(this as Bomber);
            GameManager.i.GetStationByGuid(playerGuid).modules.AddRange(attachedModules);
        }
        else if (this is Bomb)
        {
            GameManager.i.GetStationByGuid(playerGuid).bombs.Remove(this as Bomb);
        }
        GameManager.i.AllUnits.Remove(this);
        if (currentPathNode?.unitOnPath?.unitGuid == unitGuid)
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
                GameManager.i.Winner = playerGuid;
            }
        }
        if (HP <= 0 || (unitOnPathDestroyed && this is Bomber && unitOnPath.moduleEffects.Contains(ModuleEffect.SelfDestruct)))
        {
            unitDestroyed = true;
            Debug.Log($"{unitOnPath.unitName} destroyed {unitName}");
            if (this is Station)
            {
                GameManager.i.Winner = unitOnPath.playerGuid;
            }
        }
        if(unitOnPathDestroyed)
            unitOnPath.DestroyUnit();
        if (unitDestroyed)
            DestroyUnit();
    }

    internal void SetSupportValues(bool forPrediction = false, PathNode _currentPathNode = null)
    {
        if(unitType == UnitType.Bomb)
            return;
        if(_currentPathNode == null)
            _currentPathNode = currentPathNode;
        var neighbors = GridManager.i.GetNeighbors(_currentPathNode, true);
        var supportingFleets = neighbors.Select(x => x.unitOnPath).Where(x => x != null && x.teamId == teamId && x.unitType != UnitType.Bomb);
        if (forPrediction)
            supportingFleets = GameManager.i.AllUnits.Where(x => x.teamId == teamId && x.unitType != UnitType.Bomb && x != this && (x._selectedPath.LastOrDefault() == null ? neighbors.Contains(x.currentPathNode) : neighbors.Contains(x._selectedPath.LastOrDefault())));
        kineticSupportPower = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.kineticPower * (x.moduleEffects.Contains(ModuleEffect.FullKineticSupport) ? 1 : .5))));
        //thermalSupportPower = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.thermalPower * (x.moduleEffects.Contains(ModuleEffect.FullThermalSupport) ? 1 : .5))));
        explosiveSupportPower = Convert.ToInt32(supportingFleets.Sum(x => Math.Floor(x.explosivePower * (x.moduleEffects.Contains(ModuleEffect.FullExplosiveSupport) ? 1 : .5))));
    }

    internal int GetDamage(Unit attackedUnit, AttackType type)
    {
        if (type == AttackType.Kinetic)
            return (attackedUnit.kineticPower + attackedUnit.kineticSupportPower) - (kineticPower + kineticSupportPower);
        //else if (type == AttackType.Thermal)
        //    return (attackedUnit.thermalPower + attackedUnit.thermalSupportPower) - (thermalPower + thermalSupportPower);
        else
            return (attackedUnit.explosivePower + attackedUnit.explosiveSupportPower) - (explosivePower + explosiveSupportPower);
    }
}