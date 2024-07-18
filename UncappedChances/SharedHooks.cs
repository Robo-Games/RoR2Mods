using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UncappedChances
{
    public class SharedHooks
    {
        public delegate void Handle_GlobalHitEvent(GameObject victim, CharacterBody attackerBody, DamageInfo damageInfo);
        public static Handle_GlobalHitEvent Handle_GlobalHitEvent_Actions;

        public static void Setup()
        {
            if (Handle_GlobalHitEvent_Actions != null)
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_HitEnemy;
            }
        }

        internal static void GlobalEventManager_HitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (!NetworkServer.active)
            {
                return;
            }
            if (victim && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    //MainPlugin.ModLogger.LogInfo("Handle_GlobalHitEvent_Actions");
                    Handle_GlobalHitEvent_Actions.Invoke(victim, attackerBody, damageInfo);
                }
            }
        }
    }
}