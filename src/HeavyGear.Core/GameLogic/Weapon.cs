#region File Description
//-----------------------------------------------------------------------------
// Weapon.cs
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameLogic
{
    public enum WeaponType
    {
        AssaultRifle,
        AntiHGRifle,
        AutoCannon,
        Grenade,
        GrenadeLauncher,
        Rocket,
        Missile,
        MachineGun,
        RailGun,
        FieldGun,
        Mortar
    }
  
    public class Weapon
    {
        #region Constants
        #endregion

        #region Variables

        private WeaponType weaponType;

        #region Weapon variables
        /// <summary>
        /// Name of weapon
        /// </summary>
        private string name;
        /// <summary>
        /// General indication of a weapon's accuracy
        /// </summary>
        private int accuracy;
        /// <summary>
        /// How many damage points this weapon does
        /// </summary>
        private int damage;
        /// <summary>
        /// The base range of this weapon in tiles
        /// </summary>
        private int range;
        /// <summary>
        /// Clip size of this weapon
        /// </summary>
        private int ammunition;
        /// <summary>
        /// the maximum rate of fire this weapon can attain
        /// </summary>
        private int maxRateOfFire;
        /// <summary>
        /// The currently selected rate of fire for this weapon
        /// </summary>
        private int rateOfFire;

        #endregion

        #region Weapon Characteristics
        private bool antiInfantry = false;
        
        private bool frag = false;
        private bool guided = false;
        private bool haywire = false;
        private bool incendiary = false;
        private bool indirectFire = false;
        private int areaEffect = 0;
        private int minimumRange = 0;
        private int damageLostPerRangeBand = 0;
        #endregion
        /// <summary>
        /// How much ammo is left in this weapon
        /// </summary>
        private int currentAmmo;

        /// <summary>
        /// Whether this weapon has fired this turn
        /// </summary>
        private bool hasFired = false;

        
        #endregion

        #region Properties

        public bool AntiInfantry
        {
            get
            {
                return antiInfantry;
            }
            set
            {
                antiInfantry = value;
            }
        }
        public int AreaEffect
        {
            get
            {
                return areaEffect;
            }
            set
            {
                areaEffect = value;
            }
        }
        public bool Frag
        {
            get
            {
                return frag;
            }
            set
            {
                frag = value;
            }
        }
        public bool Guided
        {
            get
            {
                return guided;
            }
            set
            {
                guided = value;
            }
        }
        public bool Haywire
        {
            get
            {
                return haywire;
            }
            set
            {
                haywire = value;
            }
        }
        public bool Incendiary
        {
            get
            {
                return incendiary;
            }
            set
            {
                incendiary = value;
            }
        }
        public bool IndirectFire
        {
            get
            {
                return indirectFire;
            }
            set
            {
                indirectFire = value;
            }
        }
        public int MinimumRange
        {
            get
            {
                return minimumRange;
            }
            set
            {
                minimumRange = value;
            }
        }
        public int DamageLostPerRangeBand
        {
            get
            {
                return damageLostPerRangeBand;
            }
            set
            {
                damageLostPerRangeBand = value;
            }
        }

        public WeaponType WeaponType
        {
            get
            {
                return weaponType;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
        public int Accuracy
        {
            get
            {
                return this.accuracy;
            }
            set
            {
                this.accuracy = value;
            }
        }

        public int Damage
        {
            get
            {
                return this.damage;
            }
        }

        public int Range
        {
            get
            {
                return this.range;
            }
        }

        public int Ammunition
        {
            get
            {
                return this.ammunition;
            }
        }
        public int MaxRateOfFire
        {
            get
            {
                return this.maxRateOfFire;
            }
        }
        public int RateOfFire
        {
            get
            {
                return this.rateOfFire;
            }
            set
            {
                this.rateOfFire = value;
            }
        }

        public int CurrentAmmo
        {
            get
            {
                return this.currentAmmo;
            }
            set
            {
                currentAmmo = value;
            }
        }

        public bool HasFired
        {
            get
            {
                return hasFired;
            }
            set
            {
                hasFired = value;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create car physics controller
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        public Weapon(string name, int accuracy, int damage, int range, int ammunition, int maxRateOfFire, WeaponType weaponType)
        {
            this.name = name;
            this.accuracy = accuracy;
            this.damage = damage;
            this.range = range;
            this.ammunition = ammunition;
            this.currentAmmo = ammunition;
            this.maxRateOfFire = maxRateOfFire;
            this.rateOfFire = maxRateOfFire;
            this.weaponType = weaponType;
        }
        #endregion

        #region Reset
        /// <summary>
        /// Reset all player entries for restarting a game, just resets the
        /// car speed here.
        /// </summary>
        public void Reset()
        {
            hasFired = false;
        }
       #endregion

    }
}
