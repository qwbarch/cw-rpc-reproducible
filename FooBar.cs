using BepInEx;
using HarmonyLib;
using UnityEngine;
using MyceliumNetworking;
using Photon.Pun;
using System;

namespace FooBar
{
    [BepInPlugin("qwbarch.FooBar", "FooBar", "1.0.0")]
    class FooBar : BaseUnityPlugin
    {
        void Awake()
        {
            var harmony = new Harmony("qwbarch.FooBar");
            harmony.PatchAll(typeof(RegisterPrefab));
        }
    }

    static class RegisterPrefab
    {
        static bool Registered = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundSpawner), "Start")]
        static void RegisterPrefabForEnemies(RoundSpawner __instance)
        {
            // This runs every round, but we only want add our prefabs to the enemy prefabs once.
            if (!Registered)
            {
                Registered = true;
                foreach (var spawn in __instance.possibleSpawns)
                {
                    // Filter out spawns that aren't "monster groups".
                    if (spawn.GetComponent<MonsterGroupClose>() == null)
                    {
                        spawn.AddComponent<MyNetworkObject>();
                    }
                }
            }
        }
    }

    class MyNetworkObject : MonoBehaviour
    {
        private static readonly uint ModId = 12345;

        void Awake()
        {
            MyceliumNetwork.RegisterNetworkObject(this, ModId, GetComponent<PhotonView>().ViewID);
        }

        void OnDestroy()
        {
            MyceliumNetwork.DeregisterNetworkObject(this, ModId, GetComponent<PhotonView>().ViewID);
        }

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Console.WriteLine("Hello world! (host)");
                MyceliumNetwork.RPC(ModId, nameof(SayHelloWorldRPC), ReliableType.Reliable);
            }
        }

        [CustomRPC]
        void SayHelloWorldRPC()
        {
            Console.WriteLine("Hello world! (non-host)");
        }
    }
}