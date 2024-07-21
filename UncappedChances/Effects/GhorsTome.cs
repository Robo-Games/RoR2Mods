using System;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.AddressableAssets;
using UncappedChances;
using UnityEngine;
using UnityEngine.Networking;

namespace UncappedChances.Effects
{
    public class GhorsTome
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        public GhorsTome()
        {
            if (!Enable)
            {
                return;
            };
            Hooks();
        }
        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Ghors IL modifications");
            IL.RoR2.GlobalEventManager.OnCharacterDeath += new ILContext.Manipulator(IL_OnCharacterDeath);
        }

        private void IL_OnCharacterDeath(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "BonusGoldPackOnKill")
            ))
            {
                MainPlugin.ModLogger.LogError("Ghors - IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;
            ilcursor.Emit(OpCodes.Ldloc, 16);
            ilcursor.Emit(OpCodes.Ldloc, 85);
            ilcursor.Emit(OpCodes.Ldloc, 6);
            ilcursor.Emit(OpCodes.Ldloc, 18);
            ilcursor.EmitDelegate<Action<CharacterMaster, int, Vector3, TeamIndex>>((attackerMaster, stacks, vector, attackerTeamIndex) =>
            {
                float rolls = 1f;
                float chance = 4f * stacks;
                while (chance > 0)
                {
                    if (Util.CheckRoll(chance / rolls, attackerMaster))
                    {
                        GameObject obj6 = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BonusMoneyPack"), vector, UnityEngine.Random.rotation);
                        TeamFilter component12 = obj6.GetComponent<TeamFilter>();
                        if ((bool)component12)
                        {
                            component12.teamIndex = attackerTeamIndex;
                        }
                        NetworkServer.Spawn(obj6);
                    }
                    chance -= rolls * 100f;
                    if (HarderSuccessive)
                    {
                        rolls += 1f;
                    }
                }
            });
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }
    }
}