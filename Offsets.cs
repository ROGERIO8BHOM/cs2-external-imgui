using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS2
{
    internal class Offsetssd
    {
        public static class ClientDll
        {
            public const nint attack = 0x2065A90;
            public const nint jump = 0x2065FA0;
            public const nint lookatweapon = 0x2355FE0;
            public const nint zoom = 0x2355F50;
            public const nint dwCSGOInput = 0x23560C0;
            public const nint dwEntityList = 0x24E7680;
            public const nint dwGameEntitySystem = 0x24E7680;
            public const nint dwGameEntitySystem_highestEntityIndex = 0x2090;
            public const nint dwGameRules = 0x2340FE8;
            public const nint dwGlobalVars = 0x20616D0;
            public const nint dwGlowManager = 0x233DDE0;
            public const nint dwLocalPlayerController = 0x2320570;
            public const nint dwLocalPlayerPawn = 0x2341528;
            public const nint dwPlantedC4 = 0x234FE28;
            public const nint dwPrediction = 0x2341430;
            public const nint dwSensitivity = 0x233E8F8;
            public const nint dwSensitivity_sensitivity = 0x58;
            public const nint dwViewAngles = 0x2356748;
            public const nint dwViewMatrix = 0x23469C0;
            public const nint dwViewRender = 0x2346D70;
            public const nint dwWeaponC4 = 0x22BED18;
        }

        public static class MoneyService
        {
            public const nint m_pInGameMoneyServices = 0x808;
            public const nint m_iAccount = 0x40;
            public const nint m_iTotalCashSpent = 0x48;
            public const nint m_iCashSpentThisRound = 0x4C;
        }

        public class CBaseController
        {
            public const nint m_iszPlayerName = 0x6F4;
            public const nint m_steamID = 0x780;
        }

        public static class CSPlayerPawn
        {
            public const nint m_pCameraServices = 0x1218;
            public const nint m_bIsScoped = 0x1C50; // bool
            public const nint m_iTeamNum = 0x3EB;
            public const nint m_lifeState = 0x354;
            public const nint m_vOldOrigin = 0x1390;
            public const nint m_hPlayerPawn = 0x90C;
            public const nint m_iHealth = 0x34C;
            public const nint m_vecViewOffset = 0xE70;
            public const nint m_skeletonInstance = 0x80;
            public const nint m_pGameSceneNode = 0x330;
            public const nint m_modelState = 0x150;
            //public const nint m_entitySpottedState = 0x1CC8; C4 State
            public const nint m_entitySpottedState = 0x1C38;
            public const nint m_bSpotted = 0x8;
            public const nint m_bSpottedByMask = 0xC;
            public const nint m_vecAbsVelocity = 0x3FC;
            public const nint m_fFlags = 0x3F8;
            public const nint m_iIDEntIndex = 0x33FC;
            public const nint m_hController = 0x13A8;
        }

        public static class CameraService
        {
            public const nint m_iFOV = 0x290;
        }

    }
}
