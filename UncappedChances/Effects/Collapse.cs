using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using System;
using R2API.Utils;

namespace UncappedChances.Effects
{
    public class Collapse
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        public Collapse()
        {
            if (!Enable)
            {
                return;
            }
            Hooks();
        }

        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Collapse IL modifications");
            IL.RoR2.GlobalEventManager.OnHitEnemy += new ILContext.Manipulator(IL_OnHitEnemy);
        }

        private void IL_OnHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchLdsfld("RoR2.DLC1Content/Items", "BleedOnHitVoid")
            ))
            {
                MainPlugin.ModLogger.LogError("Collapse - IL Hook Failed");
                return;
            }
            ilcursor.Index += 13;

            ilcursor.Emit(OpCodes.Ldloc_1);
            ilcursor.Emit(OpCodes.Ldarg_2);
            ilcursor.Emit(OpCodes.Ldloc, 24);
            ilcursor.Emit(OpCodes.Ldarg_1);
            ilcursor.Emit(OpCodes.Ldloc_0);
            ilcursor.EmitDelegate<Action<CharacterBody, GameObject, int, DamageInfo, uint?>>((attackerBody, victim, stacks, damageInfo, maxStacksFromAttacker) =>
            {
                float collapseChance = (float)stacks * 10f * damageInfo.procCoefficient;
                float rolls = 1f;
                while (collapseChance > 0)
                {
                    if (Util.CheckRoll(collapseChance / rolls, attackerBody.master))
                    {
                        ApplyCollapse(victim, damageInfo, maxStacksFromAttacker);
                    }
                    collapseChance -= rolls * 100f;
                    if (HarderSuccessive)
                    {
                        rolls += 1f;
                    }
                }
            });
            ilcursor.Index += 1;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }

        static void ApplyCollapse(GameObject victim, DamageInfo damageInfo, uint? maxStacksFromAttacker = null)
        {
            ProcChainMask procChainMask2 = damageInfo.procChainMask;
            procChainMask2.AddProc(ProcType.BleedOnHit);
            DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f, maxStacksFromAttacker);
        }
    }
}