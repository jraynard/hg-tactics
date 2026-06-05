#region File Description
//-----------------------------------------------------------------------------
// Units.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameLogic
{
    public static class Units
    {
        public static Rectangle[] animationRects = new Rectangle[]
            {
                new Rectangle(0, 0, 275, 275),
                new Rectangle(275,0,275,275),
                new Rectangle(0,275,275,275),
                new Rectangle(275,275,275,275)
            };

        public static void Draw(int armyIndex, int unitIndex, Rectangle destRect, Rectangle sourceRect, float rotation)
        {
            BaseGame.UI.Unit[armyIndex][unitIndex].RenderOnScreen(destRect, sourceRect, Color.White, rotation);
        }

        public static void DrawAnimation(int armyIndex, int unitIndex, Rectangle destRect, Rectangle animationRect, float rotation, int frame)
        {
            Rectangle sourceRect = animationRects[frame];
            sourceRect.X += animationRect.X;
            sourceRect.Y += animationRect.Y;
            BaseGame.UI.Unit[armyIndex][unitIndex].RenderOnScreen(destRect, sourceRect, Color.White, rotation);
        }

        #region Vehicles

        #region Northern Units

        public static Vehicle Cheetah(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {

            Weapon m25 = new Weapon("M25 Pack Gun", -1, 8, 2, 30, 2, WeaponType.AutoCannon);
            Weapon rp109 = new Weapon("RP-109 PepperBox", -1, 12, 1, 24, 3, WeaponType.Rocket);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            rp109.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m25);
            weapons.Add(rp109);
            weapons.Add(m2aGrenade);

            int threatValue = 625;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle cheetah = new Vehicle(UnitType.Walker, "Cheetah", threatValue, weapons, playerIndex, 0, squadIndex, 0, 
                                    6, 11, 2, 5, 0, 10, new Crew(experience, 1), 100, squadLeader);

            cheetah.TargetDesignator = 2;

            return cheetah;
        }

        public static Vehicle StrikeCheetah(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {

            Weapon m25 = new Weapon("M25 Pack Gun", -1, 8, 2, 30, 2, WeaponType.AutoCannon);
            Weapon rfl2 = new Weapon("RFL2 Soothsayer", 0, 14, 1, 60, 2, WeaponType.AutoCannon);

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m25);
            weapons.Add(rfl2);

            int threatValue = 668;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle cheetah = new Vehicle(UnitType.Walker, "Strike Cheetah", threatValue, weapons, playerIndex, 0, squadIndex, 0,
                                    6, 11, 2, 5, 0, 14, new Crew(experience, 1), 100, squadLeader);

            return cheetah;
        }

        public static Vehicle Hunter(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {

            Weapon m222 = new Weapon("M222 Autocannon", 0, 8, 2, 60, 2, WeaponType.AutoCannon);
            Weapon rp109 = new Weapon("RP-109 PepperBox", -1, 12, 1, 24, 3, WeaponType.Rocket);
            Weapon mkIVGrenadeLauncher = new Weapon("MK IV Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            rp109.IndirectFire = true;
            mkIVGrenadeLauncher.AntiInfantry = true;
            mkIVGrenadeLauncher.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m222);
            weapons.Add(rp109);
            weapons.Add(mkIVGrenadeLauncher);
            weapons.Add(m2aGrenade);

            int threatValue = 380;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle hunter = new Vehicle(UnitType.Walker, "Hunter", threatValue, weapons, playerIndex, 0, squadIndex, 1,
                                    4, 7, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            return hunter;
        }

        public static Vehicle HeadHunter(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {

            Weapon m222 = new Weapon("M222 Autocannon", 0, 8, 2, 60, 2, WeaponType.AutoCannon);
            Weapon rp109 = new Weapon("RP-109 PepperBox", -1, 12, 1, 24, 3, WeaponType.Rocket);
            Weapon mkIVGrenadeLauncher = new Weapon("MK IV Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            rp109.IndirectFire = true;
            mkIVGrenadeLauncher.AntiInfantry = true;
            mkIVGrenadeLauncher.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m222);
            weapons.Add(rp109);
            weapons.Add(mkIVGrenadeLauncher);
            weapons.Add(m2aGrenade);

            int threatValue = 392;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle hunter = new Vehicle(UnitType.Walker, "Headhunter", threatValue, weapons, playerIndex, 0, squadIndex, 1,
                                    4, 7, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            hunter.BackupComms = true;

            return hunter;
        }

        public static Vehicle AssaultHunter(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {

            Weapon lgpcSnubCannon = new Weapon("LGPC Snub Cannon", -1, 28, 1, 6, 0, WeaponType.FieldGun);
            Weapon mkIVGrenadeLauncher = new Weapon("MK IV Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            mkIVGrenadeLauncher.AntiInfantry = true;
            mkIVGrenadeLauncher.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(lgpcSnubCannon);
            weapons.Add(mkIVGrenadeLauncher);
            weapons.Add(m2aGrenade);

            int threatValue = 435;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle hunter = new Vehicle(UnitType.Walker, "Assault Hunter", threatValue, weapons, playerIndex, 0, squadIndex, 1,
                                    4, 7, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            return hunter;
        }

        public static Vehicle Jaguar(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon mr25 = new Weapon("MR25 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon rp111 = new Weapon("RP-111 PepperBox II", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon mkIVGrenadeLauncher = new Weapon("MK IV Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            rp111.IndirectFire = true;
            mkIVGrenadeLauncher.AntiInfantry = true;
            mkIVGrenadeLauncher.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(mr25);
            weapons.Add(rp111);
            weapons.Add(mkIVGrenadeLauncher);
            weapons.Add(m2aGrenade);

            int threatValue = 628;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jaguar = new Vehicle(UnitType.Walker, "Jaguar", threatValue, weapons, playerIndex, 0, squadIndex, 2,
                                    5, 9, 1, 3, 1, 16, new Crew(experience, 1), 60, squadLeader);

            return jaguar;
        }

        public static Vehicle FireJaguar(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon mr25 = new Weapon("MR25 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon gh16a = new Weapon("GH-16 Rocket Pod", -1, 18, 2, 36, 4, WeaponType.Rocket);
            Weapon gh16b = new Weapon("GH-16 Rocket Pod", -1, 18, 2, 36, 4, WeaponType.Rocket);

            gh16a.IndirectFire = true;
            gh16b.IndirectFire = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(mr25);
            weapons.Add(gh16a);
            weapons.Add(gh16b);

            int threatValue = 694;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jaguar = new Vehicle(UnitType.Walker, "Fire Jaguar", threatValue, weapons, playerIndex, 0, squadIndex, 2,
                                    5, 9, 1, 3, 0, 16, new Crew(experience, 1), 60, squadLeader);

            jaguar.ReinforcedFrontArmor = 1;
            jaguar.ImprovedRearDefense = true;

            return jaguar;
        }

        public static Vehicle StrikeJaguar(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon b300 = new Weapon("Riley B300 Bazooka", 0, 20, 2, 20, 0, WeaponType.AutoCannon);
            Weapon irp20 = new Weapon("IRP Incendiary Rocket Pod", -1, 13, 1, 20, 2, WeaponType.Rocket);
            Weapon mkIVGrenadeLauncher = new Weapon("MK IV Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon m2aGrenade = new Weapon("M-2A Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            irp20.IndirectFire = true;
            irp20.Incendiary = true;
            mkIVGrenadeLauncher.AntiInfantry = true;
            mkIVGrenadeLauncher.IndirectFire = true;
            m2aGrenade.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(b300);
            weapons.Add(irp20);
            weapons.Add(mkIVGrenadeLauncher);
            weapons.Add(m2aGrenade);

            int threatValue = 880;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jaguar = new Vehicle(UnitType.Walker, "Strike Jaguar", threatValue, weapons, playerIndex, 0, squadIndex, 2,
                                    5, 9, 1, 3, 1, 16, new Crew(experience, 1), 60, squadLeader);

            return jaguar;
        }

        public static Vehicle Grizzly(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon m225 = new Weapon("M225 Autocannon", 0, 12, 3, 30, 1, WeaponType.AutoCannon);
            Weapon gh81 = new Weapon("GH-8 Rocket Pod", -1, 18, 2, 18, 3, WeaponType.Rocket);
            Weapon gh82 = new Weapon("GH-8 Rocket Pod", -1, 18, 2, 18, 3, WeaponType.Rocket);
            Weapon gu10 = new Weapon("GU-10 Gatling Unit", 0, 4, 1, 300, 3, WeaponType.MachineGun);
            Weapon m25 = new Weapon("M25 Pack Gun", -1, 8, 2, 30, 2, WeaponType.AutoCannon);
            Weapon td76 = new Weapon("TD-76 Mortar", -1, 20, 5, 12, 0, WeaponType.Mortar);

            gh81.IndirectFire = true;
            gh82.IndirectFire = true;
            gu10.AntiInfantry = true;
            td76.Guided = true;
            td76.MinimumRange = 5;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m225);
            weapons.Add(gh81);
            weapons.Add(gh82);
            weapons.Add(gu10);
            weapons.Add(m25);
            weapons.Add(td76);

            int threatValue = 888;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle grizzly = new Vehicle(UnitType.Walker, "Grizzly", threatValue, weapons, playerIndex, 0, squadIndex, 3, 
                                    3, 6, -1, 2, 0, 18, new Crew(experience, 1), 40, squadLeader);

            grizzly.ReinforcedFrontArmor = 2;
            grizzly.LargeSensorProfile = 1;

            return grizzly;
        }

        public static Vehicle Mammoth(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon gu20 = new Weapon("GU-20 Autocannon", 0, 10, 3, 200, 1, WeaponType.AutoCannon);
            Weapon fireball = new Weapon("Fireball II Launcher", 1, 25, 3, 8, 0, WeaponType.Missile);
            Weapon kj161 = new Weapon("KJ-16 Minigun", 0, 3, 1, 600, 4, WeaponType.MachineGun);
            Weapon kj162 = new Weapon("KJ-16 Minigun", 0, 3, 1, 600, 4, WeaponType.MachineGun);
            Weapon sb90 = new Weapon("SB-90 Assault Gun", -1, 28, 1, 20, 0, WeaponType.FieldGun);

            fireball.Guided = true;
            kj161.AntiInfantry = true;
            kj162.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(gu20);
            weapons.Add(fireball);
            weapons.Add(kj161);
            weapons.Add(kj162);
            weapons.Add(sb90);

            int threatValue = 1500;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle mammoth = new Vehicle(UnitType.Walker, "Mammoth", threatValue, weapons, playerIndex, 0, squadIndex, 4, 
                                    3, 5, -2, 3, 0, 25, new Crew(experience, 2), 60, squadLeader);

            mammoth.AmmoFuelContainmentSystem = true;
            mammoth.BackupSensors = true;
            mammoth.ImprovedOffRoad = true;
            mammoth.ReinforcedFrontArmor = 2;

            mammoth.LargeSensorProfile = 2;

            return mammoth;
        }
        
        public static Vehicle Scorpion(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon m230 = new Weapon("M230-F Autocannon", 0, 10, 3, 320, 1, WeaponType.AutoCannon);
            Weapon at1 = new Weapon("AT-6 Damocles", 1, 25, 3, 4, 0, WeaponType.Missile);
            Weapon at2 = new Weapon("AT-6 Damocles", 1, 25, 3, 4, 0, WeaponType.Missile);
            Weapon gh81 = new Weapon("GH-8A Rocket Pod", -1, 18, 2, 18, 3, WeaponType.Rocket);
            Weapon gh82 = new Weapon("GH-8A Rocket Pod", -1, 18, 2, 18, 3, WeaponType.Rocket);

            at1.Guided = true;
            at2.Guided = true;
            gh81.IndirectFire = true;
            gh82.IndirectFire = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(m230);
            weapons.Add(at1);
            weapons.Add(at2);
            weapons.Add(gh81);
            weapons.Add(gh82);

            int threatValue = 9625;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle scorpion = new Vehicle(UnitType.Hover, "Scorpion", threatValue, weapons, playerIndex, 0, squadIndex, 5,
                                    7, 13, 1, 1, 1, 11, new Crew(experience, 2), 40, squadLeader);

            scorpion.TargetDesignator = 4;

            return scorpion;
        }
        
        public static Vehicle Aller(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon thor = new Weapon("THOR Heavy Railgun", 0, 35, 10, 24, 0, WeaponType.RailGun);
            Weapon kj16 = new Weapon("KJ-16 Minigun", 0, 3, 1, 600, 4, WeaponType.MachineGun);

            kj16.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(thor);
            weapons.Add(kj16);

            int threatValue = 5000;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle aller = new Vehicle(UnitType.Hover, "Aller", threatValue, weapons, playerIndex, 0, squadIndex, 6, 
                                    3, 5, -2, 2, 0, 20, new Crew(experience, 3), 40, squadLeader);

            aller.ReinforcedCrewCompartment = true;
            aller.ImprovedOffRoad = true;
            aller.LargeSensorProfile = 2;
            aller.WeakRearFacing = true;

            return aller;
        }
        
        #endregion

        #region Southern Units

        public static Vehicle Iguana(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon mpgu22 = new Weapon("MPGU-22 Pack Gun", -1, 8, 2, 30, 2, WeaponType.AutoCannon);
            Weapon vogel7 = new Weapon("Vogel-7 Rocket Pod", -1, 12, 1, 24, 3, WeaponType.Rocket);

            vogel7.IndirectFire = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(mpgu22);
            weapons.Add(vogel7);

            int threatValue = 584;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle iguana = new Vehicle(UnitType.Walker, "Iguana", threatValue, weapons, playerIndex, 1, squadIndex, 0, 
                5, 9, 1, 4, 0, 14, new Crew(experience, 1), 80, squadLeader);

            iguana.TargetDesignator = 3;
            iguana.BackupSensors = true;

            return iguana;
        }

        public static Vehicle BlitzIguana(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr25 = new Weapon("PR-25 Autocannon", 0, 8, 2, 40, 2, WeaponType.AutoCannon);
            Weapon vogel8 = new Weapon("Vogel 8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);

            vogel8.IndirectFire = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr25);
            weapons.Add(vogel8);

            int threatValue = 444;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle iguana = new Vehicle(UnitType.Walker, "Blitz Iguana", threatValue, weapons, playerIndex, 1, squadIndex, 0,
                5, 9, 1, 4, 0, 14, new Crew(experience, 1), 80, squadLeader);

            iguana.BackupSensors = true;

            return iguana;
        }

        public static Vehicle Jager(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr25 = new Weapon("PR-25 Autocannon", 0, 8, 2, 60, 2, WeaponType.AutoCannon);
            Weapon vogel6 = new Weapon("Vogel-6 Rocket Pod", -1, 12, 1, 24, 3, WeaponType.Rocket);
            Weapon hlb16 = new Weapon("HLB-16 AP G Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon hg2 = new Weapon("HG-2 Hand Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            vogel6.IndirectFire = true;
            hlb16.AntiInfantry = true;
            hlb16.IndirectFire = true;
            hg2.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr25);
            weapons.Add(vogel6);
            weapons.Add(hlb16);
            weapons.Add(hg2);

            int threatValue = 380;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jager = new Vehicle(UnitType.Walker, "Jager", threatValue, weapons, playerIndex, 1, squadIndex, 1, 
                                    4, 7, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            return jager;
        }

        public static Vehicle JagerCommand(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr25 = new Weapon("PR-25 Autocannon", 0, 8, 2, 60, 2, WeaponType.AutoCannon);
            Weapon vogel6 = new Weapon("Vogel-6 Rocket Pod", -1, 12, 1, 24, 3, WeaponType.Rocket);
            Weapon hlb16 = new Weapon("HLB-16 AP G Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon hg2 = new Weapon("HG-2 Hand Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            vogel6.IndirectFire = true;
            hlb16.AntiInfantry = true;
            hlb16.IndirectFire = true;
            hg2.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr25);
            weapons.Add(vogel6);
            weapons.Add(hlb16);
            weapons.Add(hg2);

            int threatValue = 392;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jager = new Vehicle(UnitType.Walker, "Jager Command", threatValue, weapons, playerIndex, 1, squadIndex, 1,
                                    4, 7, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            jager.BackupComms = true;

            return jager;
        }

        public static Vehicle BlitzJager(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr55 = new Weapon("PR-55 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon atml1 = new Weapon("ATML-1 Missile Launcher", 1, 25, 3, 1, 0, WeaponType.Missile);
            Weapon hlb16 = new Weapon("HLB-16 AP G Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon hg2 = new Weapon("HG-2 Hand Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            atml1.Guided = true;
            atml1.IndirectFire = true;
            hlb16.AntiInfantry = true;
            hlb16.IndirectFire = true;
            hg2.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr55);
            weapons.Add(atml1);
            weapons.Add(hlb16);
            weapons.Add(hg2);

            int threatValue = 983;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle jager = new Vehicle(UnitType.Walker, "Blitz Jager", threatValue, weapons, playerIndex, 1, squadIndex, 1,
                                    4, 6, 0, 2, 0, 15, new Crew(experience, 1), 40, squadLeader);

            return jager;
        }

        public static Vehicle BlackMamba(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr55 = new Weapon("PR-55 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon vogel8 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon gl011 = new Weapon("GL-01 Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon gl012 = new Weapon("GL-01 Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon hgc4 = new Weapon("HG-C4 Hand Grenade", -1, 15, 1, 3, 0, WeaponType.Grenade);

            vogel8.IndirectFire = true;
            gl011.AntiInfantry = true;
            gl011.IndirectFire = true;
            gl012.AntiInfantry = true;
            gl012.IndirectFire = true;
            hgc4.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr55);
            weapons.Add(vogel8);
            weapons.Add(gl011);
            weapons.Add(gl012);
            weapons.Add(hgc4);

            int threatValue = 671;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle blackMamba = new Vehicle(UnitType.Walker, "Black Mamba", threatValue, weapons, playerIndex, 1, squadIndex, 2, 
                                    5, 9, 1, 3, 1, 17, new Crew(experience, 1), 40, squadLeader);

            blackMamba.WeakRearFacing = true;

            return blackMamba;
        }

        public static Vehicle RazorFangBlackMamba(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr55 = new Weapon("PR-55 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon vogel8 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon gl011 = new Weapon("GL-01 Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon gl012 = new Weapon("GL-01 Grenade Launcher", -1, 3, 1, 6, 0, WeaponType.GrenadeLauncher);
            Weapon hgc4 = new Weapon("HG-C4 Hand Grenade", -1, 15, 1, 3, 0, WeaponType.Grenade);

            vogel8.IndirectFire = true;
            gl011.AntiInfantry = true;
            gl011.IndirectFire = true;
            gl012.AntiInfantry = true;
            gl012.IndirectFire = true;
            hgc4.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr55);
            weapons.Add(vogel8);
            weapons.Add(gl011);
            weapons.Add(gl012);
            weapons.Add(hgc4);

            int threatValue = 797;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle blackMamba = new Vehicle(UnitType.Walker, "Razor Fang Black Mamba", threatValue, weapons, playerIndex, 1, squadIndex, 2,
                                    5, 8, 1, 3, 1, 17, new Crew(experience, 1), 40, squadLeader);

            blackMamba.BackupComms = true;

            blackMamba.WeakRearFacing = true;

            return blackMamba;
        }

        public static Vehicle LongFangBlackMamba(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon pr55 = new Weapon("PR-55 Autocannon", 0, 10, 3, 40, 1, WeaponType.AutoCannon);
            Weapon b12a = new Weapon("Vogel B-12 Rocket Pod", -1, 18, 2, 36, 4, WeaponType.Rocket);
            Weapon b12b = new Weapon("Vogel B-12 Rocket Pod", -1, 18, 2, 36, 4, WeaponType.Rocket);
            Weapon hgc4 = new Weapon("HG-C4 Hand Grenade", -1, 15, 1, 3, 0, WeaponType.Grenade);

            b12a.IndirectFire = true;
            b12b.IndirectFire = true;
            hgc4.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(pr55);
            weapons.Add(b12a);
            weapons.Add(b12b);
            weapons.Add(hgc4);

            int threatValue = 1188;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle blackMamba = new Vehicle(UnitType.Walker, "Long Fang Black Mamba", threatValue, weapons, playerIndex, 1, squadIndex, 2,
                                    5, 8, 1, 3, 1, 17, new Crew(experience, 1), 40, squadLeader);

            blackMamba.WeakRearFacing = true;

            return blackMamba;
        }

        public static Vehicle SpittingCobra(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon mr60 = new Weapon("MR60 Autocannon", 0, 12, 3, 30, 1, WeaponType.AutoCannon);
            Weapon fsrp = new Weapon("FSRP-36 Rocket Pod", -1, 18, 2, 18, 3, WeaponType.Rocket);
            Weapon scrp = new Weapon("SCRP-98 Rocket Pod", -1, 20, 3, 48, 4, WeaponType.Rocket);
            Weapon mgu77 = new Weapon("MGU-77 Minigun", 0, 3, 1, 400, 4, WeaponType.MachineGun);
            Weapon vogelH = new Weapon("Vogel-H Mortar", -1, 15, 3, 10, 0, WeaponType.Mortar);
            Weapon hg2 = new Weapon("HG-2 Hand Grenade", -1, 15, 1, 4, 0, WeaponType.Grenade);

            fsrp.IndirectFire = true;
            scrp.IndirectFire = true;
            mgu77.AntiInfantry = true;
            vogelH.Guided = true;
            vogelH.MinimumRange = 3;
            hg2.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(mr60);
            weapons.Add(fsrp);
            weapons.Add(scrp);
            weapons.Add(mgu77);
            weapons.Add(vogelH);
            weapons.Add(hg2);

            int threatValue = 818;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle spittingCobra = new Vehicle(UnitType.Walker, "Spitting Cobra", threatValue, weapons, playerIndex, 1, squadIndex, 3,
                                    3, 6, -1, 2, 0, 21, new Crew(experience, 1), 40, squadLeader);

            spittingCobra.ReinforcedCrewCompartment = true;
            spittingCobra.LargeSensorProfile = 1;

            return spittingCobra;
        }

        public static Vehicle Naga(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon mt30 = new Weapon("MT-30 Autocannon", 0, 10, 3, 200, 1, WeaponType.AutoCannon);
            Weapon pilum1 = new Weapon("Pilum-VI Missile", 1, 25, 3, 4, 0, WeaponType.Missile);
            Weapon pilum2 = new Weapon("Pilum-VI Missile", 1, 25, 3, 4, 0, WeaponType.Missile);

            pilum1.Guided = true;
            pilum2.Guided = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(mt30);
            weapons.Add(pilum1);
            weapons.Add(pilum2);

            int threatValue = 1645;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle naga = new Vehicle(UnitType.Walker, "Naga", threatValue, weapons, playerIndex, 1, squadIndex, 4,
                                    4, 7, -2, 4, 0, 23, new Crew(experience, 2), 80, squadLeader);

            naga.TargetDesignator = 1;
            naga.LargeSensorProfile = 1;

            return naga;
        }

        public static Vehicle Titan(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon prf25 = new Weapon("PRF-25 Autocannon", 0, 8, 2, 2400, 2, WeaponType.AutoCannon);
            Weapon at1 = new Weapon("AT-6 Damocles", 1, 25, 3, 4, 0, WeaponType.Missile);
            Weapon at2 = new Weapon("AT-6 Damocles", 1, 25, 3, 4, 0, WeaponType.Missile);
            Weapon vogel1 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon vogel2 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon vogel3 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);
            Weapon vogel4 = new Weapon("Vogel-8 Rocket Pod", -1, 12, 1, 32, 4, WeaponType.Rocket);

            at1.Guided = true;
            at2.Guided = true;
            vogel1.IndirectFire = true;
            vogel2.IndirectFire = true;
            vogel3.IndirectFire = true;
            vogel4.IndirectFire = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(prf25);
            weapons.Add(at1);
            weapons.Add(at2);
            weapons.Add(vogel1);
            weapons.Add(vogel2);
            weapons.Add(vogel3);
            weapons.Add(vogel4);

            int threatValue = 3898;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle titan = new Vehicle(UnitType.Hover, "Titan", threatValue, weapons, playerIndex, 1, squadIndex, 5,
                                    6, 11, 0, 0, 0, 15, new Crew(experience, 2), 40, squadLeader);

            titan.AmmoFuelContainmentSystem = true;
            titan.TargetDesignator = 4;

            titan.LargeSensorProfile = 1;

            return titan;
        }

        public static Vehicle Stonewall(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon magister = new Weapon("140mm Magister II Cannon", 0, 28, 8, 40, 0, WeaponType.FieldGun);
            Weapon mgu77 = new Weapon("MGU-77 Minigun", 0, 3, 1, 400, 4, WeaponType.MachineGun);

            magister.IndirectFire = true;
            mgu77.AntiInfantry = true;

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(magister);
            weapons.Add(mgu77);

            int threatValue = 5000;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            Vehicle stonewall = new Vehicle(UnitType.Ground, "Stonewall", threatValue, weapons, playerIndex, 1, squadIndex, 6,
                3, 5, -2, 2, 0, 21, new Crew(experience, 3), 40, squadLeader);

            stonewall.ImprovedOffRoad = true;
            stonewall.LargeSensorProfile = 2;
            stonewall.WeakRearFacing = true;

            return stonewall;
        }

        #endregion

        #endregion

        #region Infantry

        #region Northern Squads

        public static InfantrySquad NorthAntiHGSquad(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon ar = new Weapon("7mm Assault Rifle", 0, 2, 1, 500, 1, WeaponType.AssaultRifle);
            Weapon agr = new Weapon("24mm Anti-HG Rifle", 1, 7, 3, 80, 0, WeaponType.AntiHGRifle);

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(ar);
            weapons.Add(agr);

            int threatValue = 200;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            InfantrySquad northernSquad = new InfantrySquad("Anti-HG Squad", threatValue, weapons, playerIndex, 0, squadIndex, 7,
                                    experience, ArmorType.HeavyFlak, 4, squadLeader);

            return northernSquad;
        }

        public static InfantrySquad NorthRifleSquad(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon ar = new Weapon("7mm Assault Rifle", 0, 2, 1, 500, 1, WeaponType.AssaultRifle);
            Weapon agr = new Weapon("9mm Chaingun", 0, 3, 1, 800, 4, WeaponType.MachineGun);

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(ar);
            weapons.Add(agr);

            int threatValue = 150;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            InfantrySquad northernSquad = new InfantrySquad("Rifle Squad", threatValue, weapons, playerIndex, 0, squadIndex, 7,
                                    experience, ArmorType.HeavyFlak, 9, squadLeader);

            return northernSquad;
        }

        #endregion

        #region Southern Squads

        public static InfantrySquad SouthAntiHGSquad(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon ar = new Weapon("7mm Assault Rifle", 0, 2, 1, 500, 1, WeaponType.AssaultRifle);
            Weapon agr = new Weapon("24mm Anti-HG Rifle", 1, 7, 3, 80, 0, WeaponType.AntiHGRifle);

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(ar);
            weapons.Add(agr);

            int threatValue = 200;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            InfantrySquad southAntiHGSquad = new InfantrySquad("Anti-HG Squad", threatValue, weapons, playerIndex, 1, squadIndex, 7,
                                    experience, ArmorType.HeavyFlak, 4, squadLeader);

            return southAntiHGSquad;
        }

        public static InfantrySquad SouthRifleSquad(int playerIndex, int squadIndex, Experience experience, bool squadLeader)
        {
            Weapon ar = new Weapon("7mm Assault Rifle", 0, 2, 1, 500, 1, WeaponType.AssaultRifle);
            Weapon agr = new Weapon("9mm Chaingun", 0, 3, 1, 800, 4, WeaponType.MachineGun);

            List<Weapon> weapons = new List<Weapon>();
            weapons.Add(ar);
            weapons.Add(agr);

            int threatValue = 150;
            if (experience == Experience.Rookie)
                threatValue /= 4;
            else if (experience == Experience.Veteran)
                threatValue *= 2;
            else if (experience == Experience.Elite)
                threatValue *= 4;
            else if (experience == Experience.Legendary)
                threatValue *= 8;

            InfantrySquad southRifleSquad = new InfantrySquad("Rifle Squad", threatValue, weapons, playerIndex, 1, squadIndex, 7, 
                                    experience, ArmorType.HeavyFlak, 9, squadLeader);

            return southRifleSquad;
        }

        #endregion

        #endregion
    }
}
