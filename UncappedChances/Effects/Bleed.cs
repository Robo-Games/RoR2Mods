using System;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Projectile;
using UncappedChances;

namespace UncappedChances.Effects
{
    public class Bleed
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        public Bleed()
        {
            if (!Enable)
            {
                return;
            }
            Hooks();
        }

        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Bleed IL modifications");
            SharedHooks.Handle_GlobalHitEvent_Actions += GlobalEventManager_HitEnemy;
            IL.RoR2.GlobalEventManager.OnHitEnemy += new ILContext.Manipulator(IL_OnHitEnemy);
        }

        private void IL_OnHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchStloc(22),
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt(typeof(RoR2.CharacterBody), "get_bleedChance"),
                x => x.MatchLdcR4(0f),
                x => x.MatchCgt(),
                x => x.MatchLdloc(22),
                x => x.MatchOr(),
                x => x.MatchBrfalse(out var _)
            ))
            {
                MainPlugin.ModLogger.LogError("Bleed - IL Hook Failed");
                return;
            }
            ilcursor.Index += 7;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }

        static void ApplyBleed(CharacterBody attacker, GameObject victim, DamageInfo damageInfo, uint? maxStacksFromAttacker = null)
        {
            ProcChainMask procChainMask2 = damageInfo.procChainMask;
            procChainMask2.AddProc(ProcType.BleedOnHit);
            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
        }

        private void GlobalEventManager_HitEnemy(GameObject victim, CharacterBody attackerBody, DamageInfo damageInfo)
        {
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

            float bleedChance = attackerBody.bleedChance * damageInfo.procCoefficient;
            if ((damageInfo.damageType & DamageType.BleedOnHit) != 0)
            {
                ApplyBleed(attackerBody, victim, damageInfo, maxStacksFromAttacker);
            }
            if (!attackerBody.inventory)
            {
                return;
            }
            if (attackerBody.inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode) > 0 && damageInfo.crit)
            {
                ApplyBleed(attackerBody, victim, damageInfo, maxStacksFromAttacker);
            }
            float rolls = 1f;
            while (bleedChance > 0)
            {
                if (Util.CheckRoll(bleedChance / rolls, attackerBody.master))
                {
                    ApplyBleed(attackerBody, victim, damageInfo, maxStacksFromAttacker);
                }
                bleedChance -= rolls * 100f;
                if (HarderSuccessive)
                {
                    rolls += 1f;
                }
            }
        }
    }
}