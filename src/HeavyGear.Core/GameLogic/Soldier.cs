#region File Description
//-----------------------------------------------------------------------------
// Soldier.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace HeavyGear.GameLogic
{
    public class Soldier
    {
        #region Soldier Variables
        /// <summary>
        /// The total damage this soldier has taken
        /// </summary>
        private int damage;
        /// <summary>
        /// The weapon this soldier is holding
        /// </summary>
        private Weapon weapon;
        /// <summary>
        /// the posistion of this unit in the squad
        /// </summary>
        private int position;
        #endregion

        #region Properites
        public int Position
        {
            get
            {
                return position;
            }
        }
        public int Damage
        {
            get
            {
                return damage;
            }
            set
            {
                damage = value;
            }
        }
        public Weapon Weapon
        {
            get
            {
                return weapon;
            }
            set
            {
                weapon = value;
            }
        }
        #endregion

        public Soldier(Weapon weapon, int position)
        {
            this.weapon = weapon;
            this.position = position;
            this.damage = 0;
        }
    }
}

