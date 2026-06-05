#region File Description
//-----------------------------------------------------------------------------
// Crew.cs
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
    public enum Experience
    {
        Legendary,
        Elite,
        Veteran,
        Qualified,
        Rookie
    }
    public class Crew
    {
        #region Crew Variables
        bool stunned;
        Experience experience;
        Skill piloting;
        Skill gunnery;
        Skill electronicWarfare;
        int count;
        int skill;
        #endregion

        #region Properites
        public Experience Experience
        {
            get
            {
                return experience;
            }
        }
        public bool Stunned
        {
            get
            {
                return stunned;
            }
            set
            {
                stunned = value;
            }
        }
        public Skill Piloting
        {
            get
            {
                return piloting;
            }
        }
        public Skill Gunnery
        {
            get
            {
                return gunnery;
            }
        }
        public Skill ElectronicWarfare
        {
            get
            {
                return electronicWarfare;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public int Skill
        {
            get
            {
                return skill;
            }
        }
        #endregion

        public Crew(Experience experience, int count)
        {
            switch (experience)
            {
                case Experience.Rookie:
                    piloting = new Skill(1, 0);
                    gunnery = new Skill(1, 1);
                    electronicWarfare = new Skill(1, 0);
                    break;
                case Experience.Qualified:
                    piloting = new Skill(2, 0);
                    gunnery = new Skill(2, 1);
                    electronicWarfare = new Skill(2, 0);
                    break;
                case Experience.Veteran:
                    piloting = new Skill(3, 1);
                    gunnery = new Skill(3, 1);
                    electronicWarfare = new Skill(3, 1);
                    break;
                case Experience.Elite:
                    piloting = new Skill(4, 1);
                    gunnery = new Skill(4, 2);
                    electronicWarfare = new Skill(4, 1);
                    break;
            }
            this.experience = experience;
            this.count = count;
            skill = (int)experience;
            stunned = false;
        }

        #region Defaults
        public static Crew Rookie
        {
            get
            {
                return new Crew(Experience.Rookie, 1);
            }
        }
        public static Crew Qualified
        {
            get
            {
                return new Crew(Experience.Qualified, 1);
            }
        }
        public static Crew Veteran
        {
            get
            {
                return new Crew(Experience.Veteran, 1);
            }
        }
        public static Crew Elite
        {
            get
            {
                return new Crew(Experience.Elite, 1);
            }
        }
        #endregion
    }
}

