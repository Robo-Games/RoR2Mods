using System;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.AddressableAssets;
using UncappedChances;

namespace UncappedChances.Effects
{
    public class Crit
    {
        internal static bool EnableDefault = true;
        internal static bool HarderSuccessiveDefault = false;
        internal static bool MultiplicativeCritDefault = true;
        internal static bool Enable = EnableDefault;
        internal static bool HarderSuccessive = HarderSuccessiveDefault;
        internal static bool MultiplicativeCrit = MultiplicativeCritDefault;
        public Crit()
        {
            if (!Enable)
            {
                return;
            };
            Hooks();
        }
        private void Hooks()
        {
            MainPlugin.ModLogger.LogInfo("Applying Crit IL modifications");
            IL.RoR2.HealthComponent.TakeDamage += new ILContext.Manipulator(IL_TakeDamage);
        }

        private void IL_TakeDamage(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            if (!ilcursor.TryGotoNext
            (
                x => x.MatchLdloc(6),
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt(typeof(RoR2.CharacterBody), "get_critMultiplier"),
                x => x.MatchMul(),
                x => x.MatchStloc(6)
            ))
            {
                MainPlugin.ModLogger.LogError("Crit - IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldloc_1);
            ilcursor.EmitDelegate<Func<RoR2.CharacterBody, float>>((body) =>
            {
                float rolls = 1f;
                float critChance = body.crit - 100f;
                float critMultiplier = body.critMultiplier;
                while (critChance > 0)
                {
                    if (HarderSuccessive)
                    {
                        rolls += 1f;
                    }
                    if (Util.CheckRoll(critChance / rolls, body.master))
                    {
                        if (MultiplicativeCrit)
                        {
                            critMultiplier *= body.critMultiplier;
                        }
                        else
                        {
                            critMultiplier += body.critMultiplier - 1;
                        }
                    }
                    critChance -= rolls * 100f;
                }
                return critMultiplier;
            });
        }
    }
}
