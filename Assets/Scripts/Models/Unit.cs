using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Unit : Node
{
    internal string unitName;
    internal string color;
    internal Guid unitGuid;
    internal int stationId;
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
    internal double supportValue = .5;
    internal int level = 1;
    internal int globalCreditGain = 0;
    internal bool hasMoved = false;
    internal List<Module> attachedModules = new List<Module>();
    internal List<ModuleEffect> moduleEffects = new List<ModuleEffect>();
    internal TextMeshPro HPText;
    internal TextMeshPro statText;
    internal GameObject selectIcon;
    internal GameObject inCombatIcon;
    internal Direction facing;
    internal Transform unitImage;
    internal int maxAttachedModules = 1; // 1+ station.level
    internal List<Tuple<int, int>> _minedPath = new List<Tuple<int, int>>();

    public void InitializeUnit(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid, int _mining, Direction _direction)
    {
        teamId = stationId % Globals.Teams;
        facing = _direction;
        unitGuid = _unitGuid;
        location = new Coords(_x, _y);
        color = _color;
        maxHP = _hp;
        HP = _hp;
        maxMovement = _range;
        movementLeft = _range;
        kineticPower = _electricAttack;
        thermalPower = _thermalAttack;
        explosivePower = _voidAttack;
        maxMining = _mining;
        currentPathNode.structureOnPath = this;
        transform.position = currentPathNode.transform.position;
        resetMining();
        resetMovementRange();
        SetNodeColor();
        HPText = transform.Find("HP").GetComponent<TextMeshPro>();
        statText = transform.Find("HP/Stats").GetComponent<TextMeshPro>();
        selectIcon = transform.Find("Select").gameObject;
        inCombatIcon = transform.Find("InCombat").gameObject;
        unitImage = transform.Find("Unit");
        unitImage.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270 - (facing == Direction.TopRight ? -60 : (int)facing * 60));
        GameManager.i.AllUnits.Add(this);
    }
    public void RegenHP(int regen)
    {
        if (moduleEffects.Contains(ModuleEffect.DoubleHeal))
            regen *= 2;
        if (HP < maxHP)
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
    internal void GainHP(int hp)
    {
        maxHP += hp;
    }
    public void SetNodeColor()
    {
        currentPathNode.transform.Find("Node").GetComponent<SpriteRenderer>().material.color = GridManager.i.tileColors[stationId];
        currentPathNode.ownedById = stationId;
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
                kineticPower += (3 * modifer);
                thermalDamageModifier += (-1 * modifer);
                break;
            case 1:
                thermalPower += (3 * modifer);
                GainHP(1 * modifer);
                break;
            case 2:
                explosivePower += (3 * modifer);
                kineticDamageModifier += (1 * modifer);
                break;
            case 3:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 4:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                thermalPower += (-1 * modifer);
                break;
            case 5:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                explosivePower += (-1 * modifer);
                break;
            case 6:
                kineticPower += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 7:
                kineticPower += (1 * modifer);
                thermalPower += (2 * modifer);
                break;
            case 8:
                kineticPower += (-1 * modifer);
                explosivePower += (4 * modifer);
                break;
            case 9:
                kineticPower += (3 * modifer);
                explosiveDamageModifier += (-1 * modifer);
                break;
            case 10:
                explosivePower += (3 * modifer);
                kineticDamageModifier += (1 * modifer);
                break;
            case 11:
                kineticPower += (1 * modifer);
                thermalPower += (1 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 12:
                thermalPower += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 13:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                kineticDamageModifier += (2 * modifer);
                break;
            case 14:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                thermalDamageModifier += (2 * modifer);
                break;
            case 15:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                explosiveDamageModifier += (2 * modifer);
                break;
            case 16:
                kineticPower += (3 * modifer);
                thermalPower += (-2 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 17:
                kineticPower += (2 * modifer);
                thermalPower += (3 * modifer);
                explosivePower += (-2 * modifer);
                break;
            case 18:
                kineticPower += (-2 * modifer);
                thermalPower += (4 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 19:
                thermalPower += (-2 * modifer);
                explosivePower += (4 * modifer);
                GainHP(3 * modifer);
                break;
            case 20:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                kineticDamageModifier += (-2 * modifer); 
                break;
            case 21:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                thermalDamageModifier += (-2 * modifer);
                break;
            case 22:
                maxMovement += (1 * modifer);
                movementLeft += (1 * modifer);
                explosiveDamageModifier += (-2 * modifer);
                break;
            case 23:
                kineticPower += (1 * modifer);
                thermalDamageModifier += (2 * modifer);
                explosiveDamageModifier += (2 * modifer);
                break;
            case 24:
                thermalPower += (1 * modifer);
                explosiveDamageModifier += (2 * modifer);
                kineticDamageModifier += (2 * modifer);
                break;
            case 25:
                explosivePower += (1 * modifer);
                kineticDamageModifier += (2 * modifer);
                thermalDamageModifier += (2 * modifer);
                break;
            case 26:
                GainHP(3 * modifer);
                kineticDamageModifier += (-2 * modifer);
                break;
            case 27:
                GainHP(3 * modifer);
                thermalDamageModifier += (-2 * modifer);
                break;
            case 28:
                GainHP(3 * modifer);
                explosiveDamageModifier += (-2 * modifer);
                break;
            case 29:
                GainHP(3 * modifer);
                kineticPower += (1 * modifer);
                break;
            case 30:
                GainHP(3 * modifer);
                thermalPower += (1 * modifer);
                break;
            case 31:
                GainHP(3 * modifer);
                explosivePower += (1 * modifer);
                break;  
            case 32:
                globalCreditGain += (1 * modifer);
                explosivePower += (-2 * modifer);
                break; 
            case 33:
                globalCreditGain += (2 * modifer);
                maxMining += (-2 * modifer);
                miningLeft += (-2 * modifer);
                break; 
            case 34:
                globalCreditGain += (3 * modifer);
                maxMovement += (-2 * modifer);
                movementLeft += (-2 * modifer);
                break;
            case 35:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                kineticPower += (1 * modifer);
                break; 
            case 36:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                thermalPower += (1 * modifer);
                break; 
            case 37:
                maxMining += (2 * modifer);
                miningLeft += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 38:
                kineticPower += (5 * modifer);
                thermalPower += (-2 * modifer);
                explosiveDamageModifier += (-4 * modifer);
                break;
            case 39:
                thermalPower += (5 * modifer);
                explosivePower += (-2 * modifer);
                kineticDamageModifier += (-3 * modifer);
                break;
            case 40:
                kineticPower += (-2 * modifer);
                explosivePower += (5 * modifer);
                thermalDamageModifier += (-2 * modifer);
                break; 
            case 41:
                supportValue = modifer == 1 ? 1 : .5;
                break;
            case 42:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.ReduceMaxHp); }
                else { moduleEffects.Remove(ModuleEffect.ReduceMaxHp); }
                break;
            case 43:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.DoubleHeal); }
                else { moduleEffects.Remove(ModuleEffect.DoubleHeal); }
                break;
            case 44:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatHeal3); }
                else { moduleEffects.Remove(ModuleEffect.CombatHeal3); }
                break;
            case 45:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatKinetic1); }
                else { moduleEffects.Remove(ModuleEffect.CombatKinetic1); }
                break;
            case 46:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatThermal1); }
                else { moduleEffects.Remove(ModuleEffect.CombatThermal1); }
                break;
            case 47:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.CombatExplosive1); }
                else { moduleEffects.Remove(ModuleEffect.CombatExplosive1); }
                break;
            case 48:
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidMining2); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidMining2); }
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
                if (modifer == 1) { moduleEffects.Add(ModuleEffect.AsteroidHP3); }
                else { moduleEffects.Remove(ModuleEffect.AsteroidHP3); }
                break;
            default:
                break;
        }
    }

    internal ServerUnit ToServerUnit()
    {
        return new ServerUnit()
        {
            UnitGuid = unitGuid,
            Facing = facing,
            Location = location.ToServerCoords()
        };
    }

    internal void ClearMinedPath(List<PathNode> selectedPath)
    {
        if (selectedPath != null)
        {
            foreach (var item in _minedPath)
            {
                selectedPath[item.Item1].AwardCredits(this, item.Item2 * -1);
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