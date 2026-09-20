using BepInEx;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;

namespace ShadeFinderMod
{
    [BepInPlugin("com.2lazy.ShadowLocation", "ShadowLocation-dev", "1.0.0")]
    public class ShadeFinder : BaseUnityPlugin
    {
        private GameObject marker;
        private static bool forceMapDisplay;
        private static string forceMapBoolName;
        private static bool originalCompassValue;
        private static int originalCompassCost;
        private static bool compassOverrideActive;

        private void Awake()
        {
            Logger.LogInfo("PLUGIN LOADED");
            new Harmony("com.2lazy.ShadowLocation").PatchAll();
        }

        private void RemoveMarker()
        {
            if (marker != null)
            {
                Destroy(marker);
                marker = null;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "GetBool")]
        private static class CompassLocationPatch
        {
            private static bool Prefix(string boolName, ref bool __result)
            {
                if (forceMapDisplay && boolName == forceMapBoolName)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }

        private static void UpdateCompassOverride(PlayerData playerData, bool enabled)
        {
            if (enabled)
            {
                if (!compassOverrideActive)
                {
                    originalCompassValue = playerData.equippedCharm_2;
                    originalCompassCost = playerData.charmCost_2;
                    compassOverrideActive = true;
                }

                playerData.equippedCharm_2 = true;
                playerData.charmCost_2 = 0;
            }
            else if (compassOverrideActive)
            {
                playerData.equippedCharm_2 = originalCompassValue;
                playerData.charmCost_2 = originalCompassCost;
                compassOverrideActive = false;
            }
        }

        private static string GetMapBoolName(string zone)
        {
            switch (zone)
            {
                case "ABYSS":
                    return "mapAbyss";
                case "CITY":
                    return "mapCity";
                case "CLIFFS":
                    return "mapCliffs";
                case "CROSSROADS":
                    return "mapCrossroads";
                case "DEEP_NEST":
                    return "mapDeepnest";
                case "DIRTMOUTH":
                    return "mapDirtmouth";
                case "FOG_CANYON":
                    return "mapFogCanyon";
                case "FUNGAL_WASTES":
                    return "mapFungalWastes";
                case "GREEN_PATH":
                    return "mapGreenpath";
                case "MINES":
                    return "mapMines";
                case "OUTSKIRTS":
                    return "mapOutskirts";
                case "RESTING_GROUNDS":
                    return "mapRestingGrounds";
                case "ROYAL_GARDENS":
                    return "mapRoyalGardens";
                case "WATERWAYS":
                    return "mapWaterways";
                default:
                    return null;
            }
        }

        private void Update()
        {
            forceMapDisplay = false;
            forceMapBoolName = null;

            if (PlayerData.instance == null)
                return;

            MapZone playerZone =
                (MapZone)System.Enum.Parse(
                    typeof(MapZone),
                    GameManager.instance.GetCurrentMapZone()
                );

            string sceneName = PlayerData.instance.shadeScene;
            bool hasActiveShade =
                !string.IsNullOrEmpty(sceneName) && sceneName != "None";
            string shadeZone = PlayerData.instance.shadeMapZone;
            string mapBoolName = GetMapBoolName(playerZone.ToString());
            bool hasMap = mapBoolName != null && PlayerData.instance.GetBool(mapBoolName);

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Logger.LogInfo(
                    $"Shade zone: {shadeZone}, Player zone: {playerZone}, Has map: {hasMap}, Active shade: {hasActiveShade}"
                );
            }

            bool shouldShowCompass =
                hasActiveShade && shadeZone == playerZone.ToString() && !hasMap;

            UpdateCompassOverride(PlayerData.instance, shouldShowCompass);

            if (shouldShowCompass && mapBoolName != null)
            {
                forceMapDisplay = true;
                forceMapBoolName = mapBoolName;
            }

            if (string.IsNullOrEmpty(sceneName) || sceneName == "None")
            {
                RemoveMarker();
                return;
            }

            // Create marker once
            if (marker == null)
            {
                GameObject shade = GameObject.Find("Shade");

                if (shade == null)
                {
                    Logger.LogWarning("Could not find Shade object.");
                    return;
                }

                marker = Instantiate(shade);
                marker.name = "ShadeLocationMarker";
            }

            float x = PlayerData.instance.shadePositionX;
            float y = PlayerData.instance.shadePositionY;

            marker.transform.position = new Vector3(x, y, 0f);

        }

    }
}