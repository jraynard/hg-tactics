#region File Description
//-----------------------------------------------------------------------------
// Vehicle.cs
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
    public class Vehicle : Unit
    {
        #region Variables

        #region Vehicle variables
        /// <summary>
        /// Crew of this unit
        /// </summary>
        private Crew crew;
        /// <summary>
        /// combat speed in movement points
        /// </summary>
        private int combatSpeed;
        /// <summary>
        /// the unit's top speed in movement points
        /// </summary>
        private int topSpeed;
        /// <summary>
        /// How easy or hard this vehicle is to manuever. 0 average attribute
        /// </summary>
        private int manuever;
        /// <summary>
        /// Quality of onboard sensor systems
        /// </summary>
        private int sensors;
        /// <summary>
        /// range in Hex for this unit's sensors
        /// </summary>
        private int sensorRange;
        /// <summary>
        /// Quality of onboard fire control systems
        /// </summary>
        private int fireControl;
        /// <summary>
        /// Overall toughness of the vehicle
        /// </summary>
        private int armor;
        /// <summary>
        /// The currently selected speed for this unit
        /// </summary>
        private SpeedType speed;
        /// <summary>
        /// Whether this unit has used it's sensors last turn
        /// </summary>
        private bool sensorsActive;
        private bool sensorsDestroyed = false;
        private bool communicationsDestroyed = false;
        private bool fireControlDestroyed = false;

        private Unit acquiredUnit = null;

        #endregion

        #region Vehicle Perks
        /// <summary>
        /// The rating of a equipped target designator
        /// </summary>
        private int targetDesignator = 0;
        /// <summary>
        /// Backup sensors absorb first sensor hit
        /// </summary>
        private bool backupSensors = false;
        /// <summary>
        /// Backup comms absorb first comms hit
        /// </summary>
        private bool backupComms = false;
        /// <summary>
        /// Abosrbs first crew hit
        /// </summary>
        private bool reinforcedCrewCompartment = false;
        /// <summary>
        /// -2 from ammo/fuel hit rolls
        /// </summary>
        private bool ammoFuelContainmentSystem = false;
        /// <summary>
        /// -1 to hex cost for Hex that cost more than 1
        /// </summary>
        private bool improvedOffRoad = false;
        /// <summary>
        /// Reinforced armor gets added to any attack from the front
        /// </summary>
        private int reinforcedFrontArmor = 0;
        /// <summary>
        /// Improved rear defense reduces defense roll penalties by 1 for rear and rear flank attacks
        /// </summary>
        private bool improvedRearDefense = false;

        #endregion

        #region Vehicle Flaws
        /// <summary>
        /// Large sensor profile gets subtracted from concealment
        /// </summary>
        private int largeSensorProfile = 0;
        /// <summary>
        /// Attacks from the rear halve this unit armor if it has a weak rear facing
        /// </summary>
        private bool weakRearFacing = false;
        #endregion

        #endregion

        #region Properties

        #region Vehicle Perks
        /// <summary>
        /// The rating of a equipped target designator
        /// </summary>
        public int TargetDesignator
        {
            get
            {
                return targetDesignator;
            }
            set
            {
                targetDesignator = value;
            }
        }
        /// <summary>
        /// Backup sensors absorb first sensor hit
        /// </summary>
        public bool BackupSensors
        {
            get
            {
                return backupSensors;
            }
            set
            {
                backupSensors = value;
            }
        }
        public bool BackupComms
        {
            get
            {
                return backupComms;
            }
            set
            {
                backupComms = value;
            }
        }
        /// <summary>
        /// Abosrbs first crew hit
        /// </summary>
        public bool ReinforcedCrewCompartment
        {
            get
            {
                return reinforcedCrewCompartment;
            }
            set
            {
                reinforcedCrewCompartment = value;
            }
        }
        /// <summary>
        /// -2 from ammo/fuel hit rolls
        /// </summary>
        public bool AmmoFuelContainmentSystem
        {
            get
            {
                return ammoFuelContainmentSystem;
            }
            set
            {
                ammoFuelContainmentSystem = value;
            }
        }
        /// <summary>
        /// -1 to hex cost for Hex that cost more than 1
        /// </summary>
        public bool ImprovedOffRoad
        {
            get
            {
                return improvedOffRoad;
            }
            set
            {
                improvedOffRoad = value;
            }
        }
        public bool ImprovedRearDefense
        {
            get
            {
                return improvedRearDefense;
            }
            set
            {
                improvedRearDefense = value;
            }
        }
        /// <summary>
        /// Reinforced armor gets added to any attack from the front
        /// </summary>
        public int ReinforcedFrontArmor
        {
            get
            {
                return reinforcedFrontArmor;
            }
            set
            {
                reinforcedFrontArmor = value;
            }
        }

        #endregion

        #region Vehicle Flaws
        /// <summary>
        /// Large sensor profile gets subtracted from concealment
        /// </summary>
        public int LargeSensorProfile
        {
            get
            {
                return largeSensorProfile;
            }
            set
            {
                largeSensorProfile = value;
            }
        }
        /// <summary>
        /// Attacks from the rear halve this unit armor if it has a weak rear facing
        /// </summary>
        public bool WeakRearFacing
        {
            get
            {
                return weakRearFacing;
            }
            set
            {
                weakRearFacing = value;
            }
        }
        #endregion

        public override int Skill
        {
            get
            {
                return crew.Skill;
            }
        }

        public override Experience Experience
        {
            get
            {
                return crew.Experience;
            }
        }

        public Unit AcquiredUnit
        {
            get
            {
                return acquiredUnit;
            }
            set
            {
                acquiredUnit = value;
            }
        }

        public bool CommsDestroyed
        {
            get
            {
                return communicationsDestroyed;
            }
        }

        public bool SensorsDestroyed
        {
            get
            {
                return sensorsDestroyed;
            }
        }

        public Crew Crew
        {
            get
            {
                return crew;
            }
        }

        public int CombatSpeed
        {
            get
            {
                return this.combatSpeed;
            }
        }

        public int TopSpeed
        {
            get
            {
                return this.topSpeed;
            }
        }

        public SpeedType Speed
        {
            get
            {
                return this.speed;
            }
            set
            {
                this.speed = value;
            }
        }

        public int Manuever
        {
            get
            {
                return this.manuever;
            }
            set
            {
                this.manuever = value;
            }
        }

        public int Sensors
        {
            get
            {
                return this.sensors;
            }
        }

        public int FireControl
        {
            get
            {
                return this.fireControl;
            }
        }

        public int Armor
        {
            get
            {
                return this.armor;
            }
            set
            {
                this.armor = value;
            }
        }

        public int SensorRange
        {
            get
            {
                return sensorRange;
            }
        }

        public bool SensorsActive
        {
            get
            {
                return sensorsActive;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a vehicle unit
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
        public Vehicle(UnitType unitType, string Name, int threatValue, List<Weapon> Weapons, int playerIndex, int armyIndex, int squadIndex, 
            int unitIndex, int combatSpeed, int topSpeed, int manuever, int sensors, int fireControl, int armor, Crew crew, int sensorRange,
            bool squadLeader) 
            : base(unitType, Name, threatValue, Weapons, playerIndex, armyIndex, squadIndex, unitIndex, squadLeader)
        {
            this.combatSpeed = combatSpeed;
            this.topSpeed = topSpeed;
            this.manuever = manuever;
            this.sensors = sensors;
            this.fireControl = fireControl;
            this.armor = armor;
            this.crew = crew;
            this.sensorRange = sensorRange;
            this.speed = SpeedType.CombatSpeed;
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

                //skip turn if the crew was stunned by a hit
                if (Crew.Stunned)
                {
                    Crew.Stunned = false;
                    IsTurn = false;
                    HeavyGearManager.MessageLogAdd(Name + "'s crew is no longer stunned.");
                }

                if (MP >= 0)
                    HasMoved = true;
                
                /*
                if (!HasTurned && TimesTurned > 1)
                {
                    HasTurned = true;
                    MPUsed++;
                }*/

                if (ActionsUsed >= crew.Count)
                    HasUsedActions = true;

                if (HasMoved && HasUsedActions)
                    IsTurn = false;

                #endregion
            }
            if (sensorsDestroyed && (sensorsActive || acquiredUnit != null))
            {
                sensorsActive = false;
                acquiredUnit = null;
            }
        }

        #endregion

        #region StartTurn

        public override void StartTurn()
        {
            base.StartTurn();
            if (speed == SpeedType.CombatSpeed)
                MP = combatSpeed;
            else
                MP = topSpeed;
        }

        #endregion

        #region ChangeSpeed
        public void ChangeSpeed()
        {
            if (speed == SpeedType.CombatSpeed)
            {
                speed = SpeedType.TopSpeed;
                HeavyGearManager.MessageLogAdd(Name + " shifts to top speed");
            }
            else
            {
                speed = SpeedType.CombatSpeed;
                HeavyGearManager.MessageLogAdd(Name + " shifts to combat speed");
            }
            ActionsUsed++;
        }
        #endregion

        #region ToggleSensors
        public bool ToggleSensors()
        {
            if (!sensorsDestroyed)
            {
                sensorsActive = !sensorsActive;
                if (sensorsActive)
                    HeavyGearManager.MessageLogAdd(Name + " turns on active sensors");
                else
                    HeavyGearManager.MessageLogAdd(Name + " turns off active sensors");
                ActionsUsed++;
                return true;
            }
            else
            {
                HeavyGearManager.MessageLogAdd(Name + "'s sensors are destroyed");
                return false;
            }
        }
        #endregion

        #region AcquireUnit
        /// <summary>
        /// Attempts to acquire a unit using active sensors, which can then be fired at using guided
        /// weapons even if out of normal LOS
        /// </summary>
        /// <param name="target"></param>
        /// <returns>whether the action was successful</returns>
        public bool AcquireUnit(Unit target)
        {
            if (sensorsDestroyed)
            {
                HeavyGearManager.MessageLogAdd(Name + "'s sensors are destroyed");
                return false;
            }

            if (!sensorsActive)
            {
                HeavyGearManager.MessageLogAdd(Name + "'s sensors are inactive");
                return false;
            }

            Skill skill = crew.ElectronicWarfare;
            skill.Level -= HeavyGearManager.Player(PlayerIndex).Squads[SquadIndex].MoralePenalty;

            int detectionRoll = Dice.Roll(skill);
            //modified by vehicle sensors
            detectionRoll += sensors;

            int threshold = HeavyGearManager.Map.GetObscurement(this.MapPosition, target.MapPosition);
            //Modified by Hex moved and number of weapons fired
            threshold -= target.HexesMoved;
            foreach (Weapon Weapon in target.Weapons)
            {
                if (Weapon.HasFired)
                    threshold--;
            }

            if (detectionRoll - threshold > 0)
            {
                acquiredUnit = target;
                HeavyGearManager.MessageLogAdd(Name + " obtains a sensor lock on " + target.Name);
                ActionsUsed++;
                return true;
            }
            else
            {
                acquiredUnit = null;
                HeavyGearManager.MessageLogAdd(Name + " fails to acquire a lock on " + target.Name);
                ActionsUsed++;
                return false;
            }
        }

        #endregion

        #region Attack

        public override void Attack(Unit target)
        {
            Weapon weapon = Weapons[Weapon];
            if (weapon.CurrentAmmo > 0)
            {
                int range = HeavyGearManager.Map.GetRange(MapPosition, target.MapPosition);
                if (range <= weapon.Range * 8)
                {
                    if (target.UnitType != UnitType.Infantry)
                        AttackVehicle((Vehicle)target, range);
                    else
                        AttackInfantry((InfantrySquad)target, range);
                    base.Attack(target);
                }
                else
                    HeavyGearManager.MessageLogAdd(target.Name + " is out of range.");
            }
            else
                HeavyGearManager.MessageLogAdd(weapon.Name + " is out of ammo.");
        }

        #endregion

        #region AttackVehicle
        private void AttackVehicle(Vehicle target, int range)
        {
            Weapon weapon = Weapons[Weapon];

            Skill attackerSkill = crew.Gunnery;
            Skill defenderSkill = target.Crew.Piloting;

            attackerSkill.Level -= HeavyGearManager.Player(PlayerIndex).Squads[SquadIndex].MoralePenalty;
            defenderSkill.Level -= HeavyGearManager.Player(target.PlayerIndex).Squads[target.SquadIndex].MoralePenalty;

            int attackerResult = Dice.Roll(attackerSkill);
            int defenderResult = Dice.Roll(defenderSkill);

            #region Attacker Modifiers
            //guided missile modifier
            if (weapon.Guided)
            {
                foreach (Unit unit in HeavyGearManager.Player(PlayerIndex).Units)
                {
                    if (unit.TaggedUnit != null)
                    {
                        if (unit.TaggedUnit == target)
                            attackerResult += 2;
                    }
                }
            }

            //accuracy
            int accuracyModifier = 0;
            if (fireControlDestroyed)
                accuracyModifier = weapon.Accuracy - 5;
            else
                accuracyModifier = fireControl + weapon.Accuracy;
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


            //Speed
            int attackerSpeedModifier = 0;
            if ((speed == SpeedType.CombatSpeed && MP >= combatSpeed) || (speed == SpeedType.TopSpeed && MP >= topSpeed))
                attackerSpeedModifier = 1;
            else if (speed == SpeedType.TopSpeed)
                attackerSpeedModifier = -3;
            else if (speed == SpeedType.CombatSpeed)
                attackerSpeedModifier = 0;

            #region Apply Modifiers

            attackerResult += rangeModifier;
            attackerResult += obscurementModifier;
            attackerResult += accuracyModifier;
            attackerResult += attackerSpeedModifier;

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
            int defenseArcModifier = CheckDefenseArc(target.Rotation, MapPosition, target.MapPosition);

            //Add +1 if improved rear defense perk is available and this is a rear or rear flank attack
            if (defenseArcModifier < 0 && improvedRearDefense)
                defenseArcModifier++;

            #region Apply Modifiers

            defenderResult += target.Manuever;
            defenderResult += defenderSpeedModifer;
            defenderResult += defenseArcModifier;

            #endregion

            #endregion

            int marginOfSuccess = attackerResult - defenderResult;

            int effectiveDamage = weapon.Damage;
            if (weapon.RateOfFire > 0)
            {
                effectiveDamage += weapon.RateOfFire;
            }

            if (marginOfSuccess > 0)
            {
                // TODO : play hit sound
                int totalDamage = effectiveDamage * marginOfSuccess;
                DamageType damage;

                HitTarget = true;

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
                        //DestroyedTarget = true;
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

                    DamageToApply = damage;
                }
            }
            else
            {
                // TODO : play miss sound + appropriate animation
                HeavyGearManager.MessageLogAdd(Name + " misses " + target.Name);
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
        #endregion

        #region AttackInfantry

        private void AttackInfantry(InfantrySquad target, int range)
        {
            Skill attackerSkill = crew.Gunnery;
            int defenderSkill = target.Skill;

            attackerSkill.Level -= HeavyGearManager.Player(PlayerIndex).Squads[SquadIndex].MoralePenalty;
            defenderSkill -= HeavyGearManager.Player(target.PlayerIndex).Squads[target.SquadIndex].MoralePenalty;

            int attackerResult = Dice.Roll(attackerSkill);
            int defenderResult = Dice.Roll(defenderSkill);

            Weapon weapon = Weapons[Weapon];

            #region Attacker Modifiers
            //accuracy
            int accuracyModifier = fireControl + weapon.Accuracy;
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

            //Speed
            int attackerSpeedModifier = 0;
            if ((speed == SpeedType.CombatSpeed && MP >= combatSpeed) || (speed == SpeedType.TopSpeed && MP >= topSpeed))
                attackerSpeedModifier = 1;
            else if (speed == SpeedType.TopSpeed)
                attackerSpeedModifier = -3;
            else if (speed == SpeedType.CombatSpeed)
                attackerSpeedModifier = 0;

            #region Apply Modifiers

            if (!weapon.AntiInfantry)
                attackerResult -= 2;

            attackerResult += rangeModifier;
            attackerResult += obscurementModifier;
            attackerResult += accuracyModifier;
            attackerResult += attackerSpeedModifier;

            #endregion

            #endregion

            #region Defender Modifiers

            //defense arc modifier
            int defenseArcModifier = CheckDefenseArc((int)target.Rotation, MapPosition, target.MapPosition);

            #region Apply Modifiers

            defenderResult += target.Skill;
            defenderResult -= target.Encumberance;
            defenderResult += defenseArcModifier;

            #endregion

            #endregion

            int marginOfSuccess = attackerResult - defenderResult;

            int effectiveDamage = weapon.Damage;

            if (marginOfSuccess > 0)
            {
                if (weapon.RateOfFire > 0)
                {
                    marginOfSuccess += weapon.RateOfFire;
                }

                // TODO : play hit sound
                HitTarget = true;
                int totalDamage = effectiveDamage * marginOfSuccess;

                HeavyGearManager.MessageLogAdd(Name + " hits " + target.Name + " for " + totalDamage.ToString() + " damage");

                target.ApplyDamage(totalDamage);
            }
            else
            {
                // TODO : play miss sound + appropriate animation
                HeavyGearManager.MessageLogAdd(Name + " misses " + target.Name);
            }


            if (weapon.RateOfFire == 0)
                weapon.CurrentAmmo--;
            else
            {
                if (weapon.WeaponType == WeaponType.Missile || weapon.WeaponType == WeaponType.Rocket)
                    weapon.CurrentAmmo -= (int)Math.Pow(2, weapon.RateOfFire);
                else
                    weapon.CurrentAmmo -= weapon.RateOfFire * 10;
            }

            weapon.HasFired = true;
            ActionsUsed++;
        }
        
        #endregion

        #region ObserveUnit
        public override bool ObserveUnit(Unit target)
        {
            if (!communicationsDestroyed)
            {
                if (base.ObserveUnit(target))
                {
                    //also laser designate the target if a designator is equipped and functioning
                    if (targetDesignator > 0)
                    {
                        TaggedUnit = target;
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region ApplyDamage

        public override void ApplyDamage(DamageType damage)
        {
            base.ApplyDamage(damage);

            ApplySystemsDamage(damage);

            if (HeavyGearManager.Transport.IsConnected)
                HeavyGearManager.SendTargetPacket(
                    HeavyGearManager.Player(PlayerIndex).Units.IndexOf(this));
        }

        #endregion

        #region Apply Systems Damage

        private void ApplySystemsDamage(DamageType damage)
        {
            if (damage == DamageType.Overkill || damage == DamageType.None)
                return;

            int damageTable = Dice.Roll(1, 5);

            //Fire Control
            if (damageTable == 1)
                ApplyFireControlDamage(damage);

            //Structure
            if (damageTable == 2)
                ApplyStructureDamage(damage);

            //Crew
            if (damageTable == 3)
            {
                if (reinforcedCrewCompartment)
                {
                    HeavyGearManager.MessageLogAdd(Name + "'s reinforced crew compartment absorbs a hit");
                    reinforcedCrewCompartment = false;
                }
                else if (damage == DamageType.Light)
                {
                    crew.Stunned = true;
                    HeavyGearManager.MessageLogAdd(Name + "'s crew is stunned");
                }
                else if (damage == DamageType.Heavy)
                {
                    crew.Count -= 1;
                    HeavyGearManager.MessageLogAdd(Name + " suffers crew casualties");
                    if (crew.Count == 0)
                        Destroy();
                }
            }

            //Movement
            if (damageTable == 4)
            {
                if (damage == DamageType.Light)
                {
                    if (combatSpeed > 0)
                        combatSpeed--;
                    if (topSpeed > 0)
                        topSpeed--;
                    HeavyGearManager.MessageLogAdd(Name + "'s movement is hit, -1 penalty");
                }
                if (damage == DamageType.Heavy)
                {
                    combatSpeed /= 2;
                    topSpeed /= 2;
                    manuever -= 2;
                    HeavyGearManager.MessageLogAdd(Name + "'s movement is crippled, speed halved, manuever decreased");
                }
            }
            //Auxillary Systems
            if (damageTable == 5)
            {
                if (damage == DamageType.Light)
                {
                    //one auxillary system is damaged
                    if (targetDesignator > 0)
                    {
                        //choose between sensors target designator
                        int roll = Dice.Roll(1, 2);
                        //Sensors
                        if (roll == 1)
                        {
                            if (backupSensors)
                            {
                                backupSensors = false;
                                HeavyGearManager.MessageLogAdd(Name + "'s back up sensors are now active");
                            }
                            else
                            {
                                sensors -= 1;
                                HeavyGearManager.MessageLogAdd(Name + "'s sensors take damage");
                            }
                        }
                        //TD
                        if (roll == 2)
                        {
                            HeavyGearManager.MessageLogAdd(Name + "'s target designator takes damage");
                            targetDesignator -= 1;
                        }
                    }
                    else
                    {
                        //sensor damage
                        if (backupSensors)
                        {
                            backupSensors = false;
                            HeavyGearManager.MessageLogAdd(Name + "'s back up sensors are now active");
                        }
                        else
                        {
                            sensors -= 1;
                            HeavyGearManager.MessageLogAdd(Name + "'s sensors take damage");
                        }
                    }
                }
                else if (damage == DamageType.Heavy)
                {
                    //one auxillary system destroyed
                    if (targetDesignator > 0)
                    {
                        //choose between sensors, comms, target designator
                        int roll = Dice.Roll(1, 3);
                        //Sensors
                        if (roll == 1)
                        {
                            if (backupSensors)
                            {
                                backupSensors = false;
                                HeavyGearManager.MessageLogAdd(Name + "'s back up sensors are now active");
                            }
                            else
                            {
                                sensorsDestroyed = true;
                                HeavyGearManager.MessageLogAdd(Name + "'s sensors are destroyed");
                            }
                        }
                        //Comms
                        if (roll == 2)
                        {
                            if (backupComms)
                            {
                                backupComms = false;
                                HeavyGearManager.MessageLogAdd(Name + "'s back up comms are now active");
                            }
                            else
                            {
                                communicationsDestroyed = true;
                                HeavyGearManager.MessageLogAdd(Name + "'s comms are destroyed");
                            }
                        }
                        if (roll == 3)
                        {
                            targetDesignator = 0;
                            HeavyGearManager.MessageLogAdd(Name + "'s target designator is destroyed");
                        }
                    }
                    else
                    {
                        //sensor damage
                        if (backupSensors)
                        {
                            backupSensors = false;
                            HeavyGearManager.MessageLogAdd(Name + "'s back up sensors are now active");
                        }
                        else
                        {
                            sensorsDestroyed = true;
                            HeavyGearManager.MessageLogAdd(Name + "'s sensors are destroyed");
                        }
                    }
                }
            }


        }
        #endregion

        #region Apply Fire Control Damage
        private void ApplyFireControlDamage(DamageType damage)
        {
            int result = Dice.Roll(1, 6);
            if (damage == DamageType.Heavy)
                result++;

            if (ammoFuelContainmentSystem && result > 5)
            {
                result -= 2;
            }

            int roll;

            switch (result)
            {
                case 1:
                    //-1 to single Weapon
                    roll = Dice.Roll(0, Weapons.Count);
                    Weapons[roll].Accuracy--;
                    HeavyGearManager.MessageLogAdd(Name + "'s " + Weapons[roll].Name + " is damaged");
                    break;
                case 2:
                    //-2 to single Weapon
                    roll = Dice.Roll(0, Weapons.Count);
                    Weapons[roll].Accuracy -= 2;
                    HeavyGearManager.MessageLogAdd(Name + "'s " + Weapons[roll].Name + " is heavily damaged");
                    break;
                case 3:
                    //All Weapons -1
                    fireControl--;
                    HeavyGearManager.MessageLogAdd(Name + "'s fire control is damaged");
                    break;
                case 4:
                    //Single Weapon destroyed
                    
                    roll = Dice.Roll(0, Weapons.Count);
                    HeavyGearManager.MessageLogAdd(Name + "'s " + Weapons[roll].Name + " is destroyed");
                    Weapons.RemoveAt(roll);
                    break;
                case 5:
                    //Fire control system destroyed
                    fireControlDestroyed = true;
                    HeavyGearManager.MessageLogAdd(Name + "'s fire control is destroyed");
                    break;
                case 6:
                    //Ammo/Fuel Tank rupture, cannot move or fire
                    combatSpeed = 0;
                    topSpeed = 0;
                    Weapons.Clear();
                    HeavyGearManager.MessageLogAdd(Name + "'s fuel tank ruptures");
                    break;
                case 7:
                    //Ammo fuel explosion
                    HeavyGearManager.MessageLogAdd(Name + " suffers a critical fuel/ammo explosion");
                    Destroy();
                    break;
            }
        }
        #endregion

        #region Apply Structure Damage
        private void ApplyStructureDamage(DamageType damage)
        {
            int result = Dice.Roll(1, 6);
            if (damage == DamageType.Heavy)
                result++;

            switch (result)
            {
                case 1:
                    //-1 MP
                    combatSpeed--;
                    topSpeed--;
                    HeavyGearManager.MessageLogAdd(Name + "'s movement system is damaged");
                    break;
                case 2:
                    //1/2 remaining mp
                    combatSpeed /= 2;
                    topSpeed /= 2;
                    HeavyGearManager.MessageLogAdd(Name + "'s Weapon is crippled");
                    break;
                case 3:
                    //-1 to manuever
                    manuever--;
                    HeavyGearManager.MessageLogAdd(Name + "'s manueverability is damaged");
                    break;
                case 4:
                    //-2 to manuever
                    HeavyGearManager.MessageLogAdd(Name + "'s manuever is severly damaged");
                    manuever -= 2;
                    break;
                case 5:
                    //Power transfer failure
                    HeavyGearManager.MessageLogAdd(Name + " suffers a power transfer failure");
                    combatSpeed = 0;
                    topSpeed = 0;
                    break;
                case 6:
                    //Crew compartment failure
                    HeavyGearManager.MessageLogAdd(Name + " suffers crew compartment failure");
                    crew.Count--;
                    if (crew.Count == 0)
                        Destroy();
                    break;
                case 7:
                    //Complete structural failure
                    HeavyGearManager.MessageLogAdd(Name + " suffers complete structural failure");
                    Destroy();
                    break;
            }
        }
        #endregion
    }

}
