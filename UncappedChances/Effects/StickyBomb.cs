using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using System;
using R2API.Utils;

namespace UncappedChances.Effects
{
    public class StickyBomb
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool SingleBombDefault = true;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        internal static bool SingleBomb = SingleBombDefault;
        public StickyBomb()
        {
            if (!Enable)
            {
                return;
            }
            Hooks();
        }

        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Sticky IL modifications");
            IL.RoR2.GlobalEventManager.OnHitEnemy += new ILContext.Manipulator(IL_OnHitEnemy);
        }

        private void IL_OnHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "StickyBomb")
            ))
            {
                MainPlugin.ModLogger.LogError("Sticky - IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;

            ilcursor.Emit(OpCodes.Ldloc_1);
            ilcursor.Emit(OpCodes.Ldloc_2);
            ilcursor.Emit(OpCodes.Ldloc, 14);
            ilcursor.Emit(OpCodes.Ldarg_1);
            ilcursor.EmitDelegate<Action<CharacterBody, CharacterBody, int, DamageInfo>>((attackerBody, victimBody, stacks, damageInfo) =>
            {
                if (stacks <= 0)
                {
                    return;
                }
                bool alive = victimBody.healthComponent.alive;
                float num9 = 5f;
                Vector3 position = damageInfo.position;
                Vector3 forward = victimBody.corePosition - position;
                float magnitude = forward.magnitude;
                float damageCoefficient7 = 1.8f;
                float damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient7);

                float bombChance = (float)stacks * 5f * damageInfo.procCoefficient;
                float rolls = 1f;
                int bombs = 0;
                while (bombChance > 0)
                {
                    if (Util.CheckRoll(bombChance / rolls, attackerBody.master))
                    {
                        if (!SingleBomb)
                        {
                            Quaternion rotation = ((magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform);
                            ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num9) : (-1f));
                        }
                        else
                        {
                            bombs += 1;
                        }
                    }
                    bombChance -= rolls * 100f;
                    if (HarderSuccessive)
                    {
                        rolls += 1f;
                    }
                }
                if (SingleBomb && bombs > 0)
                {
                    Quaternion rotation = ((magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform);
                    ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, (float)bombs * damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num9) : (-1f));
                }
            });
            ilcursor.Index += 1;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }
    }
}