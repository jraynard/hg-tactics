#region File Description
//-----------------------------------------------------------------------------
// InfantrySquad.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
using HeavyGear.GameScreens;
#endregion

namespace HeavyGear.GameLogic
{
    public enum ArmorType
    {
        LightFlak,
        HeavyFlak,
        Turtleshell
    }
    public class InfantrySquad : Unit
    {
        #region Variables

        #region Squad variables

        private List<Soldier> soldiers;
        /// <summary>
        /// Skill of this unit
        /// </summary>
        private int skill;
        /// <summary>
        /// Stamina + armor value
        /// </summary>
        private int damageResistance;
        /// <summary>
        /// Penalty due to armor and inexperience
        /// </summary>
        private int encumberance;

        #endregion

        private Experience experience;

        #endregion

        #region Properties
        public override Experience Experience
        {
            get
            {
                return experience;
            }
        }
        public override int Skill
        {
            get
            {
                return skill;
            }
        }

        public List<Soldier> Soldiers
        {
            get
            {
                return soldiers;
            }
        }

        public int DamageResistance
        {
            get
            {
                return damageResistance;
            }
        }
        public int Encumberance
        {
            get
            {
                return encumberance;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs an infantry squad unit
        /// </summary>
        /// <param Name="unitType">The type of unit</param>
        /// <param Name="Name">Name of the unit</param>
        /// <param Name="threatValue">Threat value</param>
        /// <param Name="Weapons">List of Weapons mounted on vehicle</param>
        /// <param Name="playerIndex">The player index of this unit</param>
        /// <param Name="MapPosition">Position on map</param>
        /// <param Name="idleFrame">Texture to use when idle</param>
        /// <param Name="destroyedTexture">Texture when destroyed</param>
        /// <param Name="moveAnimation">Move animation</param>
        /// <param Name="fireAnimation"></param>
        /// <param Name="hitAnimation"></param>
        /// <param Name="destroyedAnimation"></param>
        /// <param Name="combatSpeed">Combat Speed</param>
        /// <param Name="topSpeed">Top Speed</param>
        /// <param Name="manuever">Manuever Rating (0 average)</param>
        /// <param Name="sensors">Sensor Rating (0 average)</param>
        /// <param Name="fireControl">Fire Control Rating (0 average)</param>
        /// <param Name="armor">Base Armor Rating</param>
        /// <param Name="crew">The crew of this unit</param>
        /// <param Name="sensorRange">Sensor range in Hex</param>
        public InfantrySquad(string Name, int threatValue, List<Weapon> Weapons, int playerIndex, int armyIndex, int squadIndex,
            int unitIndex, Experience experienceLevel, ArmorType armor, int numberOfStandardWeapons, bool squadLeader)
            : base(UnitType.Infantry, Name, threatValue, Weapons, playerIndex, armyIndex, squadIndex, unitIndex, squadLeader)
        {
            this.experience = experienceLevel;
            switch (experienceLevel)
            {
                case Experience.Rookie:
                    damageResistance = 3;
                    skill = 1;
                    if (armor == ArmorType.LightFlak)
                    {
                        encumberance = 0;
                        damageResistance += 2;
                    }
                    else if (armor == ArmorType.HeavyFlak)
                    {
                        encumberance = 1;
                        damageResistance += 4;
                    }
                    else
                    {
                        encumberance = 2;
                        damageResistance += 6;
                    }
                    break;
                case Experience.Qualified:
                    damageResistance = 3;
                    skill = 2;
                    if (armor == ArmorType.LightFlak)
                    {
                        encumberance = 0;
                        damageResistance += 2;
                    }
                    else if (armor == ArmorType.HeavyFlak)
                    {
                        encumberance = 1;
                        damageResistance += 4;
                    }
                    else
                    {
                        encumberance = 2;
                        damageResistance += 6;
                    }
                    break;
                case Experience.Veteran: 
                    damageResistance = 4;
                    skill = 3;
                    if (armor == ArmorType.LightFlak)
                    {
                        encumberance = 0;
                        damageResistance += 2;
                    }
                    else if (armor == ArmorType.HeavyFlak)
                    {
                        encumberance = 0;
                        damageResistance += 4;
                    }
                    else
                    {
                        encumberance = 1;
                        damageResistance += 6;
                    }
                    break;
                case Experience.Elite:
                    damageResistance = 4;
                    skill = 4;
                    if (armor == ArmorType.LightFlak)
                    {
                        encumberance = 0;
                        damageResistance += 2;
                    }
                    else if (armor == ArmorType.HeavyFlak)
                    {
                        encumberance = 0;
                        damageResistance += 4;
                    }
                    else
                    {
                        encumberance = 0;
                        damageResistance += 6;
                    }
                    break;
            }

            //create soldiers
            soldiers = new List<Soldier>();
            int standardWeaponCount = 0;
            int heavyWeaponCount = 0;
            for (int i = 0; i < 10; i++)
            {
                int roll = Dice.Roll(1, 2);
                if (roll == 1)
                {
                    if (standardWeaponCount < numberOfStandardWeapons)
                    {
                        standardWeaponCount++;
                        soldiers.Add(new Soldier(Weapons[0], i + 1));
                    }
                    else
                    {
                        soldiers.Add(new Soldier(Weapons[1], i + 1));
                    }
                }
                else
                {
                    if (heavyWeaponCount < 10 - numberOfStandardWeapons)
                    {
                        heavyWeaponCount++;
                        soldiers.Add(new Soldier(Weapons[1], i + 1));
                    }
                    else
                    {
                        soldiers.Add(new Soldier(Weapons[0], i + 1));
                    }
                }
            }

        }
        #endregion

        #region Update
        /// <summary>
        /// Update game logic for our unit.
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (IsTurn)
            {
                #region Update flags

                if (MP <= 0)
                    HasMoved = true;

                if (ActionsUsed >= 1)
                    HasUsedActions = true;

                if (HasMoved && HasUsedActions)
                    IsTurn = false;

                #endregion
            }
        }
        #endregion

        #region StartTurn
        public override void StartTurn()
        {
            base.StartTurn();
            MP = 2;
        }
        #endregion

        #region ObserveUnit
        public override bool ObserveUnit(Unit target)
        {
            if (base.ObserveUnit(target))
            {
                //also laser designate the target, infantry are assumed to always have a designator
                TaggedUnit = target;
                return true;
            }
            return false;
        }
        #endregion

        #region Attack
        public override void Attack(Unit target)
        {
            if (target.UnitType != UnitType.Infantry)
                AttackVehicle((Vehicle)target);
            else
                AttackInfantry((InfantrySquad)target);
            base.Attack(target);
        }
        #endregion

        #region AttackVehicle
        private void AttackVehicle(Vehicle target)
        {
            //HitTarget = DestroyedTarget = false;
            Weapon weapon = Weapons[Weapon];
            if (weapon.CurrentAmmo > 0)
            {
                int range = HeavyGearManager.Map.GetRange(MapPosition, target.MapPosition);
                if (range <= weapon.Range * 8)
                {
                    int attackerSkill = skill;
                    Skill defenderSkill = target.Crew.Piloting;

                    attackerSkill -= HeavyGearManager.Player(PlayerIndex).Squads[SquadIndex].MoralePenalty;
                    defenderSkill.Level -= HeavyGearManager.Player(target.PlayerIndex).Squads[target.SquadIndex].MoralePenalty;

                    int attackerResult = Dice.Roll(attackerSkill);
                    int defenderResult = Dice.Roll(defenderSkill);

                    int numberOfTroopersWithWeapon = 0;
                    foreach (Soldier soldier in soldiers)
                    {
                        if (soldier.Weapon == weapon)
                            numberOfTroopersWithWeapon++;
                    }
                    int rofBonus = 0;
                    if (numberOfTroopersWithWeapon >= 8)
                        rofBonus = 3;
                    else if (numberOfTroopersWithWeapon >= 4)
                        rofBonus = 2;
                    else if (numberOfTroopersWithWeapon >= 2)
                        rofBonus = 1;

                    #region Attacker Modifiers
                    //accuracy
                    int accuracyModifier = weapon.Accuracy;
                    //obscurement
                    int obscurementModifier = HeavyGearManager.Map.GetObscurement(MapPosition, target.MapPosition);
                    //range
                    int rangeModifier = 0;
                    if (range == 0)
                    {
                        //Point blank
                        rangeModifier = 1;
                    }
                    else if (range > weapon.Range * 4)
                    {
                        //Extreme range
                        rangeModifier = -3;
                    }
                    else if (range > weapon.Range * 2)
                    {
                        //Long range
                        rangeModifier = -2;
                    }
                    else if (range > weapon.Range)
                    {
                        //Medium Range
                        rangeModifier = -1;
                    }
                    else if (range > 0)
                    {
                        //Short range
                        rangeModifier = 0;
                    }

                    #region Apply Modifiers

                    attackerResult += rangeModifier;
                    attackerResult += obscurementModifier;
                    attackerResult += accuracyModifier;
                    attackerResult -= encumberance;

                    #endregion

                    #endregion

                    #region Defender Modifiers

                    //defender speed modifier
                    int defenderSpeedModifer = 0;
                    if (target.HexesMoved >= 10)
                        defenderSpeedModifer = 2;
                    else if (target.HexesMoved >= 7)
                        defenderSpeedModifer = 1;
                    else if (target.HexesMoved >= 5)
                        defenderSpeedModifer = 0;
                    else if (target.HexesMoved >= 3)
                        defenderSpeedModifer = -1;
                    else if (target.HexesMoved >= 1)
                        defenderSpeedModifer = -2;
                    else
                        defenderSpeedModifer = -3;

                    //defense arc modifier
                    int defenseArcModifier = CheckDefenseArc((int)target.Rotation, MapPosition, target.MapPosition);

                    #region Apply Modifiers

                    defenderResult += target.Manuever;
                    defenderResult += defenderSpeedModifer;
                    defenderResult += defenseArcModifier;

                    #endregion

                    #endregion

                    int marginOfSuccess = attackerResult - defenderResult;

                    int effectiveDamage = weapon.Damage + rofBonus;
                    if (weapon.RateOfFire > 0)
                    {
                        effectiveDamage += weapon.RateOfFire;
                    }

                    if (marginOfSuccess > 0)
                    {
                        // TODO : play hit sound
                        int totalDamage = effectiveDamage * marginOfSuccess;
                        DamageType damage;

                        int effectiveArmor = target.Armor;

                        if (target.ReinforcedFrontArmor > 0)
                        {
                            if (CheckDefenseArc((int)target.Rotation, MapPosition, target.MapPosition) == 0)
                                effectiveArmor += target.ReinforcedFrontArmor;
                        }

                        if (target.WeakRearFacing)
                        {
                            if (CheckDefenseArc((int)target.Rotation, MapPosition, target.MapPosition) > 0)
                                effectiveArmor /= 2;
                        }

                        if (totalDamage >= effectiveArmor * 3)
                            damage = DamageType.Overkill;
                        else if (totalDamage >= effectiveArmor * 2)
                            damage = DamageType.Heavy;
                        else if (totalDamage >= effectiveArmor)
                            damage = DamageType.Light;
                        else
                            damage = DamageType.None;

                        if (damage == DamageType.None)
                        {
                            HeavyGearManager.MessageLogAdd(Name + " hits " + target.Name + " for no damage");
                        }
                        else
                        {
                            //check to see if the target was destroyed
                            if (damage == DamageType.Overkill)
                                HeavyGearManager.MessageLogAdd(Name + " over kills " + target.Name);

                            HitTarget = true;

                            //armor degradation
                            if (damage == DamageType.Heavy)
                            {
                                target.Armor -= 2;
                                HeavyGearManager.MessageLogAdd(Name + " hits " + target.Name + " for heavy damage");
                            }
                            else if (damage == DamageType.Light)
                            {
                                target.Armor -= 1;
                                HeavyGearManager.MessageLogAdd(Name + " hits " + target.Name + " for light damage");
                            }

                            target.ApplyDamage(damage);
                        }

                    }
                    else
                    {
                        // TODO : play miss sound + appropriate animation
                        HeavyGearManager.MessageLogAdd(Name + " misses " + target.Name);
                    }
                }

                if (weapon.RateOfFire == 0)
                    weapon.CurrentAmmo--;
                else
                {
                    if (weapon.WeaponType == WeaponType.Missile || weapon.WeaponType == WeaponType.Rocket)
                        weapon.CurrentAmmo -= weapon.RateOfFire * 2;
                    else
                        weapon.CurrentAmmo -= weapon.RateOfFire * 10;
                }

                weapon.HasFired = true;
                ActionsUsed++;
            }
            else
            {
                HeavyGearManager.MessageLogAdd(weapon.Name + " is out of ammo.");
            }
        }
        #endregion

        #region AttackInfantry
        private void AttackInfantry(InfantrySquad target)
        {
            Weapon weapon = Weapons[Weapon];
            //HitTarget = DestroyedTarget = false;
            if (weapon.CurrentAmmo > 0)
            {
                StartAnimation(AnimationType.Fire);
                int range = HeavyGearManager.Map.GetRange(MapPosition, target.MapPosition);
                if (range <= weapon.Range * 8)
                {
                    int attackerSkill = skill;
                    int defenderSkill = target.Skill;

                    attackerSkill -= HeavyGearManager.Player(PlayerIndex).Squads[SquadIndex].MoralePenalty;
                    defenderSkill -= HeavyGearManager.Player(target.PlayerIndex).Squads[target.SquadIndex].MoralePenalty;

                    int attackerResult = Dice.Roll(attackerSkill);
                    int defenderResult = Dice.Roll(defenderSkill);

                    int numberOfTroopersWithWeapon = 0;
                    foreach (Soldier soldier in soldiers)
                    {
                        if (soldier.Weapon == weapon)
                            numberOfTroopersWithWeapon++;
                    }
                    int rofBonus = 0;
                    if (numberOfTroopersWithWeapon >= 8)
                        rofBonus = 3;
                    else if (numberOfTroopersWithWeapon >= 4)
                        rofBonus = 2;
                    else if (numberOfTroopersWithWeapon >= 2)
                        rofBonus = 1;

                    #region Attacker Modifiers
                    //accuracy
                    int accuracyModifier = weapon.Accuracy;
                    //obscurement
                    int obscurementModifier = HeavyGearManager.Map.GetObscurement(MapPosition, target.MapPosition);
                    //range
                    int rangeModifier = 0;
                    if (range == 0)
                    {
                        //Point blank
                        rangeModifier = 1;
                    }
                    else if (range > weapon.Range * 4)
                    {
                        //Extreme range
                        rangeModifier = -3;
                    }
                    else if (range > weapon.Range * 2)
                    {
                        //Long range
                        rangeModifier = -2;
                    }
                    else if (range > weapon.Range)
                    {
                        //Medium Range
                        rangeModifier = -1;
                    }
                    else if (range > 0)
                    {
                        //Short range
                        rangeModifier = 0;
                    }

                    #region Apply Modifiers

                    attackerResult += rangeModifier;
                    attackerResult += obscurementModifier;
                    attackerResult += accuracyModifier;
                    attackerResult -= encumberance;

                    #endregion

                    #endregion

                    #region Defender Modifiers

                    //defense arc modifier
                    int defenseArcModifier = CheckDefenseArc((int)target.Rotation, MapPosition, target.MapPosition);

                    #region Apply Modifiers

                    defenderResult += target.Skill;
                    defenderResult += defenseArcModifier;

                    #endregion

                    #endregion

                    int marginOfSuccess = attackerResult - defenderResult;

                    if (marginOfSuccess > 0)
                    {
                        marginOfSuccess += rofBonus;

                        int effectiveDamage = weapon.Damage;
                        if (weapon.RateOfFire > 0)
                        {
                            marginOfSuccess += weapon.RateOfFire;
                        }

                        HitTarget = true;
                        int totalDamage = effectiveDamage * marginOfSuccess;
                        if (weapon.AntiInfantry && (weapon.WeaponType == WeaponType.GrenadeLauncher || weapon.WeaponType == WeaponType.Grenade))
                            target.ApplyAreaEffectDamage(totalDamage);
                        else
                            target.ApplyDamage(totalDamage);

                        HeavyGearManager.MessageLogAdd(Name + " hits " + target.Name + " for " + totalDamage.ToString() + " damage");
                    }

                    else
                    {
                        // TODO : play miss sound + appropriate animation
                        HeavyGearManager.MessageLogAdd(Name + " misses " + target.Name);
                    }
                }

                if (weapon.RateOfFire == 0)
                    weapon.CurrentAmmo--;
                else
                {
                    if (weapon.WeaponType == WeaponType.Missile || weapon.WeaponType == WeaponType.Rocket)
                        weapon.CurrentAmmo -= weapon.RateOfFire * 2;
                    else
                        weapon.CurrentAmmo -= weapon.RateOfFire * 10;
                }

                weapon.HasFired = true;
                ActionsUsed++;
            }
            else
            {
                HeavyGearManager.MessageLogAdd(weapon.Name + " is out of ammo.");
            }
        }
        #endregion

        #region Apply Damage
        public override void ApplyDamage(int totalDamage)
        {
            int damageLeft = totalDamage;
            int soldiersLost = 0;

            int startPosition = 1;
            bool down = true;

            int roll = Dice.Roll(1, 6);
            switch (roll)
            {
                case 1:
                    startPosition = 1;
                    down = true;
                    break;
                case 2:
                    startPosition = 3;
                    down = true;
                    break;
                case 3:
                    startPosition = 5;
                    down = true;
                    break;
                case 4:
                    startPosition = 6;
                    down = false;
                    break;
                case 5:
                    startPosition = 8;
                    down = false;
                    break;
                case 6:
                    startPosition = 10;
                    down = false;
                    break;
            }

            List<Soldier> soldiersToRemove = new List<Soldier>();
            foreach (Soldier soldier in soldiers)
            {
                if (damageLeft > 0)
                {
                    if (down && startPosition >= soldier.Position)
                    {

                        if (damageLeft + soldier.Damage >= damageResistance)
                        {
                            //this soldier is dead
                            if (!soldiersToRemove.Contains(soldier))
                            {
                                soldiersToRemove.Add(soldier);
                                soldiersLost++;
                                damageLeft -= damageResistance - soldier.Damage;
                            }
                        }
                        else
                        {
                            //soldier is hurt
                            soldier.Damage += damageLeft;
                            damageLeft = 0;
                        }
                    }
                    else if (startPosition <= soldier.Position)
                    {
                        if (damageLeft + soldier.Damage >= damageResistance)
                        {
                            if (!soldiersToRemove.Contains(soldier))
                            {
                                //this soldier is dead
                                soldiersToRemove.Add(soldier);
                                soldiersLost++;
                                damageLeft -= damageResistance - soldier.Damage;
                            }
                        }
                        else
                        {
                            //soldier is hurt
                            soldier.Damage += damageLeft;
                            damageLeft = 0;
                        }
                    }
                }
            }

            //if there is still any damage left to be applied, start at the top and work down
            if (damageLeft > 0 && soldiers.Count > 0)
            {
                foreach (Soldier soldier in soldiers)
                {
                    if (damageLeft > 0)
                    {
                        if (damageLeft + soldier.Damage >= damageResistance)
                        {
                            if (!soldiersToRemove.Contains(soldier))
                            {
                                //this soldier is dead
                                soldiersToRemove.Add(soldier);
                                soldiersLost++;
                                damageLeft -= damageResistance - soldier.Damage;
                            }
                        }
                        else
                        {
                            //soldier is hurt
                            soldier.Damage += damageLeft;
                            damageLeft = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < soldiersToRemove.Count; i++)
            {
                soldiers.Remove(soldiersToRemove[i]);
            }

            if (soldiers.Count == 0)
            {
                HeavyGearManager.MessageLogAdd(Name + " was wiped out");
                Destroy();
            }

            if (soldiersLost > 0)
            {
                HeavyGearManager.MessageLogAdd(Name + " loses " + soldiersLost.ToString() + " soldiers");
                StartAnimation(AnimationType.Hit);
            }

            if (HeavyGearManager.Transport.IsConnected)
                HeavyGearManager.SendTargetPacket(HeavyGearManager.Player(PlayerIndex).Units.IndexOf(this));

        }
        #endregion

        #region Apply Area Effect Damage
        public void ApplyAreaEffectDamage(int totalDamage)
        {
            int soldiersLost = 0;

            foreach (Soldier soldier in soldiers)
            {
                if (totalDamage + soldier.Damage >= damageResistance)
                {
                    //this soldier is dead
                    soldiers.Remove(soldier);
                    soldiersLost++;
                }
                else
                {
                    //soldier is hurt
                    soldier.Damage += totalDamage;
                }
            }
            if (soldiers.Count == 0)
            {
                HeavyGearManager.MessageLogAdd(Name + " was wiped out");
                Destroy();
            }

            if (soldiersLost > 0)
                HeavyGearManager.MessageLogAdd(Name + " loses " + soldiersLost.ToString() + " soldiers");
            
            if (HeavyGearManager.Transport.IsConnected)
                HeavyGearManager.SendTargetPacket(HeavyGearManager.Player(PlayerIndex).Units.IndexOf(this));
        }        
        #endregion
    }
}
