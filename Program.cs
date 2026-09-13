using CS2;

Renderer renderer = new Renderer();

Thread renderThread = new(() => renderer.Start().Wait());
renderThread.IsBackground = true;
renderThread.Start();

while (true)
{
    renderer._game.Update();
    renderer._aimFeature.Update();
    renderer._noRecoilFeature.Update();
    renderer._triggetFeature.Update();
    renderer._antibangFeature.Update();
    Thread.Sleep(1);
}


////void FovChanger()
////{
////    uint desiredFov = (uint)renderer.fov;
////    IntPtr pCameraServices = memory.ReadPointer(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_pCameraServices);

////    uint CurrentFov = memory.ReadUInt(pCameraServices + Offsets.CameraService.m_iFOV);
////    bool isScoped = memory.ReadBool(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_bIsScoped);
////    if (!isScoped && CurrentFov != desiredFov)
////    {
////        memory.WriteUInt(pCameraServices + Offsets.CameraService.m_iFOV, desiredFov);
////    }
////}



////void TriggerBot()
////{
////    if (!renderer.Triggerbot.active)
////        return;
////    int entityId = memory.ReadInt(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_iIDEntIndex);
////    if (entityId != -1)
////    {
////        IntPtr entityList = memory.ReadPointer(clientDLL + Offsets.ClientDll.dwEntityList);
////        IntPtr listEntry = memory.ReadPointer(entityList + (0x8 * ((entityId & 0x7FFF) >> 9) + 0x10));
////        IntPtr entityPawn = memory.ReadPointer(listEntry + (0x70 * (entityId & 0x1FF)));
////        if (entityPawn == IntPtr.Zero)
////            return;
////        uint lifeState = memory.ReadUInt(entityPawn + Offsets.CSPlayerPawn.m_lifeState);
////        if (lifeState != 256)
////            return;
////        int entTeam = memory.ReadInt(entityPawn + Offsets.CSPlayerPawn.m_iTeamNum);
////        int health = memory.ReadInt(entityPawn + Offsets.CSPlayerPawn.m_iHealth);
////        if (entTeam != localPlayer.team && health > 0)
////        {
////            memory.WriteInt(clientDLL + Offsets.ClientDll.attack, 65537);
////            Thread.Sleep(10);
////            memory.WriteInt(clientDLL + Offsets.ClientDll.attack, 256);
////            Thread.Sleep(1000);
////        }
////    }
////}

////Vector3 velocity;

////void AirShot()
////{
////    if (GetAsyncKeyState(0x06) >= 0)
////        return;
////    int fFlag = memory.ReadInt(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_fFlags);
////    if (fFlag == 65664)
////    {
////        Thread.Sleep(100);
////        velocity = memory.ReadVec(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_vecAbsVelocity);
////        while (velocity.Z > 18f || velocity.Z < -18f)
////        {
////            velocity = memory.ReadVec(localPlayer.pawnAddress + Offsets.CSPlayerPawn.m_vecAbsVelocity);
////        }
////        memory.WriteInt(clientDLL + Offsets.ClientDll.attack, 65537);
////        Thread.Sleep(150);
////        memory.WriteInt(clientDLL + Offsets.ClientDll.attack, 256);
////        Thread.Sleep(1000);
////    }
////}


////Thread airThread = new Thread(() =>
////{
////    while (true)
////    {
////        AirShot();
////        Thread.Sleep(1);
////    }
////});

////airThread.IsBackground = true;
////airThread.Start();

//while (true)
//{
//    //UpdateLocalPlayer();
//    //FovChanger();
//    //TriggerBot();
//    //if (renderer._aimbotEnabled)
//    //{
//    //    AimBot();
//    //}
//}
