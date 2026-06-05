#region File Description
//-----------------------------------------------------------------------------
// Squad.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using HeavyGear.Helpers;

namespace HeavyGear.GameLogic
{
    /// <summary>
    /// Contains a list of units in the squad, one of which is the squad leader
    /// </summary>
    public class Squad
    {
        #region Variables
        int unitsDestroyed = 0;
        /// <summary>
        /// List of all units in the squad besides the leader
        /// </summary>
        private List<Unit> units;
        /// <summary>
        /// The squad's leader index
        /// </summary>
        private int squadLeader;
        /// <summary>
        /// Total threat value of all units in this squad
        /// </summary>
        private int threatValue;
        private int moraleThreshold;
        /// <summary>
        /// The morale penalty of the squad
        /// </summary>
        private int moralePenalty;

        #endregion

        #region Properties
        public int MoralePenalty
        {
            get
            {
                return moralePenalty;
            }
        }
        public List<Unit> Units
        {
            get
            {
                return units;
            }
        }

        public int SquadLeader
        {
            get
            {
                return squadLeader;
            }
        }

        public int ThreatValue
        {
            get
            {
                return threatValue;
            }
        }

        public int UnitsDestroyed
        {
            get
            {
                return unitsDestroyed;
            }
            set
            {
                unitsDestroyed++;
            }
        }

        #endregion

        #region Constructor
        public Squad(List<Unit> units)
        {
            this.units = units;
            moralePenalty = 0;
            moraleThreshold = 0;
            threatValue = 0;
            unitsDestroyed = 0;
            for(int i = 0; i < units.Count ; i++)
            {
                threatValue += units[i].ThreatValue;
                moraleThreshold += units[i].Skill;
                if (units[i].IsSquadLeader)
                    squadLeader = i;
            }

            GetMoraleThreshold();
        }
        #endregion
        public void GetMoraleThreshold()
        {
            foreach (Unit unit in units)
                moraleThreshold += unit.Skill;

            moraleThreshold /= (units.Count + 1);

            foreach (Unit unit in units)
            {
                int skill = unit.Skill;
                int result = Dice.Roll(skill);
                if (result < moraleThreshold)
                    moralePenalty++;
            }
        }
        public void Draw()
        {
            foreach (Unit unit in units)
                unit.Draw();
        }

        public void Update()
        {
            foreach (Unit unit in units)
                unit.Update();
        }
        public void StartTurn()
        {
            unitsDestroyed = 0;
            foreach (Unit unit in units)
                unit.StartTurn();
        }

        #region Methods
        public void MoraleCheck(bool unitDestroyed)
        {
            int skill = 0;
            if (units[squadLeader].IsAlive)
                skill = units[squadLeader].Skill;
            else
            {
                foreach (Unit unit in units)
                {
                    if (unit.IsAlive && unit.Skill > skill)
                        skill = unit.Skill;
                }
            }

            skill -= moralePenalty;

            if (unitsDestroyed > 0)
                skill -= unitsDestroyed;

            //Note this only applies unit destroyed modifer after the first unit
            if (unitDestroyed)
                unitsDestroyed++;

            int result = Dice.Roll(skill);
            if (result < moraleThreshold)
            {
                moralePenalty++;
                HeavyGearManager.MessageLogAdd("Squad morale has decreased");
            }
        }

        public void Rally()
        {
            if (!units[squadLeader].IsAlive)
                return;

            #region Check for enemy LOS
            //Check to see if all units are out of enemy LOS
            bool noLOSfound = true;
            foreach (Unit unit in units)
            {
                for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
                {
                    Player player = HeavyGearManager.Player(i);

                    foreach (Unit enemy in player.Units)
                    {
                        if (HeavyGearManager.Map.CheckLOS(enemy, unit))
                        {
                            noLOSfound = false;
                            break;
                        }
                    }
                    if (!noLOSfound)
                        break;
                }
                if (!noLOSfound)
                    break;
            }
            #endregion

            #region Get Skill
            int skill = 0;
            if (units[squadLeader].IsAlive)
                skill = units[squadLeader].Skill;
            else
            {
                foreach (Unit unit in units)
                {
                    if (unit.IsAlive && unit.Skill > skill)
                        skill = unit.Skill;
                }
            }

            skill -= moralePenalty;
            #endregion

            if (noLOSfound)
            {
                moralePenalty = 0;
            }
            else
            {
                int result = Dice.Roll(skill);
                if (result >= moraleThreshold)
                {
                    if (moralePenalty > 0)
                        moralePenalty--;
                }
            }
        }
        #endregion

    }
}