import re, os

path = '/home/justinr/Dev/hg-tactics/src/HeavyGear.Core/HeavyGearManager.cs'
txt = open(path).read()

# 1. Replace using block
old_usings = """using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HeavyGear.GameLogic;
using HeavyGear.GameScreens;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
using HeavyGear.Shaders;
using Texture = HeavyGear.Graphics.Texture;
using HeavyGear.Properties;"""
new_usings = """using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using HeavyGear.GameLogic;
using HeavyGear.GameScreens;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Net;
using HeavyGear.Sounds;
using HeavyGear.Shaders;
using HeavyGear.Properties;
using Texture = HeavyGear.Graphics.Texture;"""
txt = txt.replace(old_usings, new_usings, 1)
print("1. usings:", "OK" if old_usings not in txt else "MISSED")

# 2. Replace network variable declarations
old_vars = "        //Network variables\n        private static NetworkSession networkSession;\n\n        private static PacketWriter packetWriter = new PacketWriter();\n        private static PacketReader packetReader = new PacketReader();\n\n        private static string errorMessage;"
new_vars = "        // Transport abstraction -- replaced by LiteNetLib implementation in Phase 6.\n        private static ITransport transport = NullTransport.Instance;\n\n        // Reusable packet buffers\n        private static MemoryStream packetStream = new MemoryStream(1024);\n        private static BinaryWriter packetWriter;\n        private static BinaryReader packetReader;\n\n        // Stored so packet handlers can reference current frame time\n        private static GameTime lastGameTime = new GameTime();\n\n        private static string errorMessage;"
txt = txt.replace(old_vars, new_vars, 1)
print("2. vars:", "OK" if "NetworkSession networkSession" not in txt else "MISSED")

# 3. Replace NetworkSession property with transport property
old_nsprop = "        public static NetworkSession NetworkSession\n        {\n            get\n            {\n                return networkSession;\n            }\n        }"
new_nsprop = "        public static ITransport Transport => transport;"
txt = txt.replace(old_nsprop, new_nsprop, 1)
print("3. NetworkSession prop:", "OK" if "NetworkSession NetworkSession" not in txt else "MISSED")

# 4. NumberOfPlayers
old_np = "        public static int NumberOfPlayers\n        {\n            get\n            {\n                if (networkSession == null)\n                    return players.Length;\n                else\n                    return networkSession.AllGamers.Count;\n            }\n        }"
new_np = "        public static int NumberOfPlayers\n        {\n            get\n            {\n                if (players != null) return players.Length;\n                return transport.PlayerCount;\n            }\n        }"
txt = txt.replace(old_np, new_np, 1)
print("4. NumberOfPlayers:", "OK" if "networkSession.AllGamers.Count" not in txt else "MISSED")

# 5. Player(int index) method
old_pm = "        public static Player Player(int index)\n        {\n            if (networkSession == null)\n                return players[index];\n            else\n                return networkSession.AllGamers[index].Tag as Player;\n        }"
new_pm = "        public static Player Player(int index) => players[index];"
txt = txt.replace(old_pm, new_pm, 1)
print("5. Player method:", "OK" if "networkSession.AllGamers[index]" not in txt else "MISSED")

# 6. Drop GamerServicesComponent from constructor
old_gc = "#if DEBUG\n            this.Components.Add(new GamerServicesComponent(this));\n#endif\n            //TODO: Start playing the menu music"
new_gc = "            //TODO: Start playing the menu music"
txt = txt.replace(old_gc, new_gc, 1)
print("6. GamerServicesComponent:", "OK" if "GamerServicesComponent" not in txt else "MISSED")

# 7. Initialize packet buffers in constructor
old_base = '        public HeavyGearManager()\n            : base("HeavyGear")\n        {'
new_base = '        public HeavyGearManager()\n            : base("HeavyGear")\n        {\n            packetWriter = new BinaryWriter(packetStream);\n            packetReader = new BinaryReader(packetStream);'
txt = txt.replace(old_base, new_base, 1)
print("7. packet buffers init:", "OK" if "new BinaryWriter(packetStream)" in txt else "MISSED")

# 8. Update method - replace networkSession checks
old_upd_check = "            if (networkSession != null)\n            {\n                UpdateNetworkSession(gameTime);\n            }\n\n            if (InGame)\n            {\n                \n                if (networkSession == null)\n                {"
new_upd_check = "            lastGameTime = gameTime;\n            if (transport.IsConnected)\n            {\n                UpdateNetworkSession(gameTime);\n            }\n\n            if (InGame)\n            {\n                if (!transport.IsConnected)\n                {"
txt = txt.replace(old_upd_check, new_upd_check, 1)
print("8. Update checks:", "OK" if "networkSession != null" not in txt else "MISSED")

# 9. Replace entire #region Network Methods block
net_start = txt.find("        #region Network Methods")
net_end = txt.find("        #endregion", net_start) + len("        #endregion")
if net_start != -1 and net_end != -1:
    new_net_region = """        #region Network Methods

        private static void UpdateNetworkSession(GameTime gameTime)
        {
            transport.Update();
        }

        public static void SendMenuPacket(int playerIndex)
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Menu);
            string mapName = map?.Name ?? "";
            packetWriter.Write(mapName);
            packetWriter.Write(playerIndex);
            transport.SendToAll(packetStream.ToArray());
        }

        private static void SendStartTurnPacket(int playerIndex)
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.StartTurn);
            packetWriter.Write(playerIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendStartGamePacket()
        {
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.StartGame);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendAttackPacket(int attackingUnitIndex)
        {
            Player player = localPlayer;
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Attack);
            packetWriter.Write(messageLog[0]);
            packetWriter.Write(messageLog[1]);
            packetWriter.Write(messageLog[2]);
            packetWriter.Write(messageLog[3]);
            player.WriteAttackPacket(packetWriter, attackingUnitIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        public static void SendTargetPacket(int targetedUnitIndex)
        {
            Player player = localPlayer;
            packetStream.SetLength(0);
            packetWriter.Write((byte)PacketType.Target);
            packetWriter.Write(messageLog[0]);
            packetWriter.Write(messageLog[1]);
            packetWriter.Write(messageLog[2]);
            packetWriter.Write(messageLog[3]);
            player.WriteTargetPacket(packetWriter, targetedUnitIndex);
            transport.SendToAll(packetStream.ToArray(), reliable: true);
        }

        private static void OnPacketReceived(BinaryReader reader, int senderIndex)
        {
            PacketType packetType = (PacketType)reader.ReadByte();
            Player sender = players != null && senderIndex < players.Length ? players[senderIndex] : null;

            switch (packetType)
            {
                case PacketType.Menu:
                    string mapName = reader.ReadString();
                    int playerIndex = reader.ReadInt32();
                    if (playerIndex >= 0) PlayerReady[playerIndex] = !PlayerReady[playerIndex];
                    if (!string.IsNullOrEmpty(mapName))
                    {
                        for (int i = 0; i < maps.Length; i++)
                        {
                            if (maps[i].Name == mapName) { map = new Map(mapFileNames[i]); break; }
                        }
                    }
                    break;
                case PacketType.StartTurn:
                    int pi = reader.ReadInt32();
                    if (deployMode) deployMode = false;
                    activePlayer = players[pi];
                    if (localPlayer?.PlayerIndex == pi)
                    {
                        foreach (Unit u in activePlayer.Units) if (u.IsAlive) u.StartTurn();
                        NextPlayer = true;
                    }
                    break;
                case PacketType.StartGame:
                    startGame = true;
                    break;
                case PacketType.Normal:
                    sender?.ReadNormalPacket(reader, lastGameTime, TimeSpan.Zero);
                    break;
                case PacketType.Deploy:
                    sender?.ReadDeployPacket(reader, lastGameTime, TimeSpan.Zero);
                    break;
                case PacketType.Attack:
                    messageLog[0] = reader.ReadString();
                    messageLog[1] = reader.ReadString();
                    messageLog[2] = reader.ReadString();
                    messageLog[3] = reader.ReadString();
                    int attackIdx = reader.ReadByte();
                    UnitType tType = (UnitType)reader.ReadByte();
                    int tPlayer = reader.ReadByte();
                    int tUnit = reader.ReadByte();
                    int infDmg = 0;
                    DamageType dmg = DamageType.None;
                    if (tType == UnitType.Infantry) infDmg = reader.ReadByte();
                    else dmg = (DamageType)reader.ReadByte();
                    WeaponType wpn = (WeaponType)reader.ReadByte();
                    if (sender != null)
                    {
                        sender.Units[attackIdx].TargetedUnit = Player(tPlayer).Units[tUnit];
                        sender.Units[attackIdx].AddProjectiles(wpn, 0);
                    }
                    if (localPlayer?.PlayerIndex == tPlayer)
                    {
                        if (tType == UnitType.Infantry) Player(tPlayer).ApplyInfantryDamage(tUnit, infDmg);
                        else Player(tPlayer).ApplyDamage(tUnit, dmg);
                    }
                    else { Player(tPlayer).Units[tUnit].StartAnimation(AnimationType.Hit); }
                    break;
                case PacketType.Target:
                    messageLog[0] = reader.ReadString();
                    messageLog[1] = reader.ReadString();
                    messageLog[2] = reader.ReadString();
                    messageLog[3] = reader.ReadString();
                    int targetIdx = reader.ReadByte();
                    bool destroyed = reader.ReadBoolean();
                    if (destroyed && sender != null) sender.Units[targetIdx].Destroy();
                    break;
            }
        }

        public static void JoinSession(ITransport newTransport)
        {
            transport = newTransport;
            transport.PacketReceived += OnPacketReceived;
            transport.SessionEnded += () => { transport = NullTransport.Instance; };
        }

        public static void EndSession()
        {
            transport?.Dispose();
            transport = NullTransport.Instance;
        }

        public static void CreateSession(ITransport newTransport)
        {
            try { JoinSession(newTransport); }
            catch (Exception e) { errorMessage = e.Message; }
        }

        public static void HookSessionEvents() { /* handled by ITransport events */ }

        public static int GetBits(byte b, int offset, int count)
        {
            return (b >> offset) & ((1 << count) - 1);
        }
        #endregion
"""
    txt = txt[:net_start] + new_net_region + txt[net_end:]
    print("9. Network region replaced: OK")
else:
    print("9. Network region: MISSED - not found")

# 10. ReturnToMenu - replace networkSession = null with transport reset
old_return = "            networkSession = null;\n\n            gameScreens.Clear();"
new_return = "            transport?.Dispose();\n            transport = NullTransport.Instance;\n\n            gameScreens.Clear();"
txt = txt.replace(old_return, new_return, 1)
print("10. ReturnToMenu:", "OK" if "networkSession = null;" not in txt else "MISSED")

# 11. EndTurn - networkSession checks
old_et = "            if (networkSession == null)\n            {\n                foreach (Unit unit in activePlayer.Units)\n                {\n                    if (unit.IsAlive)\n                        unit.StartTurn();\n                }\n                \n                NextPlayer = true;\n            }\n            else\n            {\n                SendStartTurnPacket(activePlayer.PlayerIndex);\n            }"
new_et = "            if (!transport.IsConnected)\n            {\n                foreach (Unit unit in activePlayer.Units)\n                {\n                    if (unit.IsAlive)\n                        unit.StartTurn();\n                }\n                NextPlayer = true;\n            }\n            else\n            {\n                SendStartTurnPacket(activePlayer.PlayerIndex);\n            }"
txt = txt.replace(old_et, new_et, 1)
print("11. EndTurn:", "OK" if "networkSession == null" not in txt else "MISSED")

# 12. Fix PostUIRender - RenderState.DepthBufferEnable (XNA 3.0 API)
old_post = "            // Enable depth buffer again\n            BaseGame.Device.RenderState.DepthBufferEnable = true;"
new_post = "            // Enable depth buffer again\n            BaseGame.Device.DepthStencilState = Microsoft.Xna.Framework.Graphics.DepthStencilState.Default;"
txt = txt.replace(old_post, new_post, 1)
print("12. PostUIRender:", "OK" if "RenderState.DepthBufferEnable" not in txt else "MISSED")

open(path, 'w').write(txt)
print("\nDone. Remaining NetworkSession refs:", txt.count("NetworkSession"), "| PacketWriter refs:", txt.count("PacketWriter"), "| PacketReader refs:", txt.count("PacketReader"))
