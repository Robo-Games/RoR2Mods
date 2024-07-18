using System;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Projectile;

namespace UncappedChances
{
    public class Collapse
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        public Collapse()
        {
            /*if (!Enable)
            {
                return;
            }*/
            Hooks();
        }

        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Collapse IL modifications");
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_HitEnemy;
            IL.RoR2.GlobalEventManager.OnHitEnemy += new ILContext.Manipulator(IL_OnHitEnemy);
        }

        private void IL_OnHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchLdarg(1),
                x => x.MatchLdflda(typeof(RoR2.DamageInfo), "procChainMask"),
                x => x.MatchLdcI4(19),
                x => x.MatchCall(typeof(RoR2.ProcChainMask), "HasProc")
            ))
            {
                MainPlugin.ModLogger.LogError("Collapse - IL Hook Failed");
                return;
            }
            ilcursor.Index += 4;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_1);
        }

        static void ApplyCollapse(CharacterBody attacker, GameObject victim, DamageInfo damageInfo, uint? maxStacksFromAttacker = null)
        {
            ProcChainMask procChainMask2 = damageInfo.procChainMask;
            procChainMask2.AddProc(ProcType.BleedOnHit);
            DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f, maxStacksFromAttacker);
        }

        internal static void GlobalEventManager_HitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (!NetworkServer.active)
            {
                return;
            }
            if (!victim || !damageInfo.attacker)
            {
                return;
            }
            if (damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
            {
                return;
            }

            uint? maxStacksFromAttacker = null;
            if ((bool)damageInfo?.inflictor)
            {
                ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                if ((bool)component && component.useDotMaxStacksFromAttacker)
                {
                    maxStacksFromAttacker = component.dotMaxStacksFromAttacker;
                }
            }

            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            int needleTickCount = attackerBody.inventory.GetItemCount(DLC1Content.Items.BleedOnHitVoid);
            needleTickCount += attackerBody.HasBuff(DLC1Content.Buffs.EliteVoid) ? 10 : 0;
            float collapseChance = (float)needleTickCount * 10f * damageInfo.procCoefficient;
            float rolls = 1f;
            while (collapseChance > 0)
            {
                if (Util.CheckRoll(collapseChance / rolls, attackerBody.master))
                {
                    ApplyCollapse(attackerBody, victim, damageInfo, maxStacksFromAttacker);
                }
                collapseChance -= rolls * 100f;
                if (HarderSuccessive)
                {
                    rolls += 1f;
                }
            }
        }
    }
}