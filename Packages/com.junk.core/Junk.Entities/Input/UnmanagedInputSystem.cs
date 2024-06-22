using Unity.Entities;
using UnityEditor;
using UnityEngine.Device;
using UnityEngine.InputSystem;

namespace Junk.Entities
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    internal partial class UnmanagedInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var inputEntityArchetype = EntityManager.CreateArchetype(typeof(UnmanagedMouse), typeof(UnmanagedKeyboard));
            var inputEntity = EntityManager.CreateEntity(inputEntityArchetype);
            EntityManager.SetName(inputEntity, "InputEntity");
        }

        protected override void OnUpdate()
        {
            var mouseQuery    = SystemAPI.GetSingleton<UnmanagedMouse>();
            var keyboardQuery = SystemAPI.GetSingleton<UnmanagedKeyboard>();
            // check for null
            if (Mouse.current == null)
            {
                mouseQuery                = default;
                mouseQuery.IsDisconnected = true;
                SystemAPI.SetSingleton(mouseQuery);
            }

            if (Keyboard.current == null)
            {
                keyboardQuery                = default;
                keyboardQuery.IsDisconnected = true;
                SystemAPI.SetSingleton(keyboardQuery);
            }

            if (Mouse.current != null)
            {
                mouseQuery.IsDisconnected = Mouse.current == null;
                mouseQuery.delta        = Mouse.current.delta.ReadValue();
                mouseQuery.position     = Mouse.current.position.ReadValue();
                mouseQuery.scroll       = Mouse.current.scroll.ReadValue();
                mouseQuery.leftButton = new ButtonQuery
                {
                    isPressed            = Mouse.current.leftButton.isPressed,
                    wasPressedThisFrame  = Mouse.current.leftButton.wasPressedThisFrame,
                    wasReleasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame
                };
                mouseQuery.rightButton = new ButtonQuery
                {
                    isPressed            = Mouse.current.rightButton.isPressed,
                    wasPressedThisFrame  = Mouse.current.rightButton.wasPressedThisFrame,
                    wasReleasedThisFrame = Mouse.current.rightButton.wasReleasedThisFrame
                };
                mouseQuery.middleButton = new ButtonQuery
                {
                    isPressed            = Mouse.current.middleButton.isPressed,
                    wasPressedThisFrame  = Mouse.current.middleButton.wasPressedThisFrame,
                    wasReleasedThisFrame = Mouse.current.middleButton.wasReleasedThisFrame
                };
                SystemAPI.SetSingleton(mouseQuery);
            }

            if (Keyboard.current != null)
            {
                var keyboard = Keyboard.current;
            #if UNITY_EDITOR
                var pause = Keyboard.current.pKey.isPressed || Keyboard.current.pauseKey.isPressed;// || Mouse.current.middleButton.isPressed;
                if(pause)
                    EditorApplication.isPaused = !EditorApplication.isPaused;
            #endif
                
                keyboardQuery.IsDisconnected = Keyboard.current == null;

                keyboardQuery.aKey = new ButtonQuery
                {
                    isPressed            = keyboard.aKey.isPressed,
                    wasPressedThisFrame  = keyboard.aKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.aKey.wasReleasedThisFrame
                };
                keyboardQuery.bKey = new ButtonQuery
                {
                    isPressed            = keyboard.bKey.isPressed,
                    wasPressedThisFrame  = keyboard.bKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.bKey.wasReleasedThisFrame
                };
                keyboardQuery.cKey = new ButtonQuery
                {
                    isPressed            = keyboard.cKey.isPressed,
                    wasPressedThisFrame  = keyboard.cKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.cKey.wasReleasedThisFrame
                };
                keyboardQuery.dKey = new ButtonQuery
                {
                    isPressed            = keyboard.dKey.isPressed,
                    wasPressedThisFrame  = keyboard.dKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.dKey.wasReleasedThisFrame
                };
                keyboardQuery.eKey = new ButtonQuery
                {
                    isPressed            = keyboard.eKey.isPressed,
                    wasPressedThisFrame  = keyboard.eKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.eKey.wasReleasedThisFrame
                };
                keyboardQuery.fKey = new ButtonQuery
                {
                    isPressed            = keyboard.fKey.isPressed,
                    wasPressedThisFrame  = keyboard.fKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.fKey.wasReleasedThisFrame
                };
                keyboardQuery.gKey = new ButtonQuery
                {
                    isPressed            = keyboard.gKey.isPressed,
                    wasPressedThisFrame  = keyboard.gKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.gKey.wasReleasedThisFrame
                };
                keyboardQuery.hKey = new ButtonQuery
                {
                    isPressed            = keyboard.hKey.isPressed,
                    wasPressedThisFrame  = keyboard.hKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.hKey.wasReleasedThisFrame
                };
                keyboardQuery.iKey = new ButtonQuery
                {
                    isPressed            = keyboard.iKey.isPressed,
                    wasPressedThisFrame  = keyboard.iKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.iKey.wasReleasedThisFrame
                };
                keyboardQuery.jKey = new ButtonQuery
                {
                    isPressed            = keyboard.jKey.isPressed,
                    wasPressedThisFrame  = keyboard.jKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.jKey.wasReleasedThisFrame
                };
                keyboardQuery.kKey = new ButtonQuery
                {
                    isPressed            = keyboard.kKey.isPressed,
                    wasPressedThisFrame  = keyboard.kKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.kKey.wasReleasedThisFrame
                };
                keyboardQuery.lKey = new ButtonQuery
                {
                    isPressed            = keyboard.lKey.isPressed,
                    wasPressedThisFrame  = keyboard.lKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.lKey.wasReleasedThisFrame
                };
                keyboardQuery.mKey = new ButtonQuery
                {
                    isPressed            = keyboard.mKey.isPressed,
                    wasPressedThisFrame  = keyboard.mKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.mKey.wasReleasedThisFrame
                };
                keyboardQuery.nKey = new ButtonQuery
                {
                    isPressed            = keyboard.nKey.isPressed,
                    wasPressedThisFrame  = keyboard.nKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.nKey.wasReleasedThisFrame
                };
                keyboardQuery.oKey = new ButtonQuery
                {
                    isPressed            = keyboard.oKey.isPressed,
                    wasPressedThisFrame  = keyboard.oKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.oKey.wasReleasedThisFrame
                };
                keyboardQuery.pKey = new ButtonQuery
                {
                    isPressed            = keyboard.pKey.isPressed,
                    wasPressedThisFrame  = keyboard.pKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.pKey.wasReleasedThisFrame
                };
                keyboardQuery.qKey = new ButtonQuery
                {
                    isPressed            = keyboard.qKey.isPressed,
                    wasPressedThisFrame  = keyboard.qKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.qKey.wasReleasedThisFrame
                };
                keyboardQuery.rKey = new ButtonQuery
                {
                    isPressed            = keyboard.rKey.isPressed,
                    wasPressedThisFrame  = keyboard.rKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rKey.wasReleasedThisFrame
                };
                keyboardQuery.sKey = new ButtonQuery
                {
                    isPressed            = keyboard.sKey.isPressed,
                    wasPressedThisFrame  = keyboard.sKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.sKey.wasReleasedThisFrame
                };
                keyboardQuery.tKey = new ButtonQuery
                {
                    isPressed            = keyboard.tKey.isPressed,
                    wasPressedThisFrame  = keyboard.tKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.tKey.wasReleasedThisFrame
                };
                keyboardQuery.uKey = new ButtonQuery
                {
                    isPressed            = keyboard.uKey.isPressed,
                    wasPressedThisFrame  = keyboard.uKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.uKey.wasReleasedThisFrame
                };
                keyboardQuery.vKey = new ButtonQuery
                {
                    isPressed            = keyboard.vKey.isPressed,
                    wasPressedThisFrame  = keyboard.vKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.vKey.wasReleasedThisFrame
                };
                keyboardQuery.wKey = new ButtonQuery
                {
                    isPressed            = keyboard.wKey.isPressed,
                    wasPressedThisFrame  = keyboard.wKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.wKey.wasReleasedThisFrame
                };
                keyboardQuery.xKey = new ButtonQuery
                {
                    isPressed            = keyboard.xKey.isPressed,
                    wasPressedThisFrame  = keyboard.xKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.xKey.wasReleasedThisFrame
                };
                keyboardQuery.yKey = new ButtonQuery
                {
                    isPressed            = keyboard.yKey.isPressed,
                    wasPressedThisFrame  = keyboard.yKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.yKey.wasReleasedThisFrame
                };
                keyboardQuery.zKey = new ButtonQuery
                {
                    isPressed            = keyboard.zKey.isPressed,
                    wasPressedThisFrame  = keyboard.zKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.zKey.wasReleasedThisFrame
                };
                keyboardQuery.spaceKey = new ButtonQuery
                {
                    isPressed            = keyboard.spaceKey.isPressed,
                    wasPressedThisFrame  = keyboard.spaceKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.spaceKey.wasReleasedThisFrame
                };
                keyboardQuery.leftShiftKey = new ButtonQuery
                {
                    isPressed            = keyboard.leftShiftKey.isPressed,
                    wasPressedThisFrame  = keyboard.leftShiftKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.leftShiftKey.wasReleasedThisFrame
                };
                keyboardQuery.rightShiftKey = new ButtonQuery
                {
                    isPressed            = keyboard.rightShiftKey.isPressed,
                    wasPressedThisFrame  = keyboard.rightShiftKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rightShiftKey.wasReleasedThisFrame
                };
                keyboardQuery.leftCtrlKey = new ButtonQuery
                {
                    isPressed            = keyboard.leftCtrlKey.isPressed,
                    wasPressedThisFrame  = keyboard.leftCtrlKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.leftCtrlKey.wasReleasedThisFrame
                };
                keyboardQuery.rightCtrlKey = new ButtonQuery
                {
                    isPressed            = keyboard.rightCtrlKey.isPressed,
                    wasPressedThisFrame  = keyboard.rightCtrlKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rightCtrlKey.wasReleasedThisFrame
                };
                keyboardQuery.leftAltKey = new ButtonQuery
                {
                    isPressed            = keyboard.leftAltKey.isPressed,
                    wasPressedThisFrame  = keyboard.leftAltKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.leftAltKey.wasReleasedThisFrame
                };
                keyboardQuery.rightAltKey = new ButtonQuery
                {
                    isPressed            = keyboard.rightAltKey.isPressed,
                    wasPressedThisFrame  = keyboard.rightAltKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rightAltKey.wasReleasedThisFrame
                };
                keyboardQuery.leftWindowsKey = new ButtonQuery
                {
                    isPressed            = keyboard.leftWindowsKey.isPressed,
                    wasPressedThisFrame  = keyboard.leftWindowsKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.leftWindowsKey.wasReleasedThisFrame
                };
                keyboardQuery.rightWindowsKey = new ButtonQuery
                {
                    isPressed            = keyboard.rightWindowsKey.isPressed,
                    wasPressedThisFrame  = keyboard.rightWindowsKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rightWindowsKey.wasReleasedThisFrame
                };
                keyboardQuery.numLockKey = new ButtonQuery
                {
                    isPressed            = keyboard.numLockKey.isPressed,
                    wasPressedThisFrame  = keyboard.numLockKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.numLockKey.wasReleasedThisFrame
                };
                keyboardQuery.scrollLockKey = new ButtonQuery
                {
                    isPressed            = keyboard.scrollLockKey.isPressed,
                    wasPressedThisFrame  = keyboard.scrollLockKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.scrollLockKey.wasReleasedThisFrame
                };
                keyboardQuery.insertKey = new ButtonQuery
                {
                    isPressed            = keyboard.insertKey.isPressed,
                    wasPressedThisFrame  = keyboard.insertKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.insertKey.wasReleasedThisFrame
                };
                keyboardQuery.deleteKey = new ButtonQuery
                {
                    isPressed            = keyboard.deleteKey.isPressed,
                    wasPressedThisFrame  = keyboard.deleteKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.deleteKey.wasReleasedThisFrame
                };
                keyboardQuery.homeKey = new ButtonQuery
                {
                    isPressed            = keyboard.homeKey.isPressed,
                    wasPressedThisFrame  = keyboard.homeKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.homeKey.wasReleasedThisFrame
                };
                keyboardQuery.endKey = new ButtonQuery
                {
                    isPressed            = keyboard.endKey.isPressed,
                    wasPressedThisFrame  = keyboard.endKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.endKey.wasReleasedThisFrame
                };
                keyboardQuery.pageUpKey = new ButtonQuery
                {
                    isPressed            = keyboard.pageUpKey.isPressed,
                    wasPressedThisFrame  = keyboard.pageUpKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.pageUpKey.wasReleasedThisFrame
                };
                keyboardQuery.pageDownKey = new ButtonQuery
                {
                    isPressed            = keyboard.pageDownKey.isPressed,
                    wasPressedThisFrame  = keyboard.pageDownKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.pageDownKey.wasReleasedThisFrame
                };
                keyboardQuery.upArrowKey = new ButtonQuery
                {
                    isPressed            = keyboard.upArrowKey.isPressed,
                    wasPressedThisFrame  = keyboard.upArrowKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.upArrowKey.wasReleasedThisFrame
                };
                keyboardQuery.downArrowKey = new ButtonQuery
                {
                    isPressed            = keyboard.downArrowKey.isPressed,
                    wasPressedThisFrame  = keyboard.downArrowKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.downArrowKey.wasReleasedThisFrame
                };
                keyboardQuery.leftArrowKey = new ButtonQuery
                {
                    isPressed            = keyboard.leftArrowKey.isPressed,
                    wasPressedThisFrame  = keyboard.leftArrowKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.leftArrowKey.wasReleasedThisFrame
                };
                keyboardQuery.rightArrowKey = new ButtonQuery
                {
                    isPressed            = keyboard.rightArrowKey.isPressed,
                    wasPressedThisFrame  = keyboard.rightArrowKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.rightArrowKey.wasReleasedThisFrame
                };
                keyboardQuery.f1Key = new ButtonQuery
                {
                    isPressed            = keyboard.f1Key.isPressed,
                    wasPressedThisFrame  = keyboard.f1Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f1Key.wasReleasedThisFrame
                };
                keyboardQuery.f2Key = new ButtonQuery
                {
                    isPressed            = keyboard.f2Key.isPressed,
                    wasPressedThisFrame  = keyboard.f2Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f2Key.wasReleasedThisFrame
                };
                keyboardQuery.f3Key = new ButtonQuery
                {
                    isPressed            = keyboard.f3Key.isPressed,
                    wasPressedThisFrame  = keyboard.f3Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f3Key.wasReleasedThisFrame
                };
                keyboardQuery.f4Key = new ButtonQuery
                {
                    isPressed            = keyboard.f4Key.isPressed,
                    wasPressedThisFrame  = keyboard.f4Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f4Key.wasReleasedThisFrame
                };
                keyboardQuery.f5Key = new ButtonQuery
                {
                    isPressed            = keyboard.f5Key.isPressed,
                    wasPressedThisFrame  = keyboard.f5Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f5Key.wasReleasedThisFrame
                };
                keyboardQuery.f6Key = new ButtonQuery
                {
                    isPressed            = keyboard.f6Key.isPressed,
                    wasPressedThisFrame  = keyboard.f6Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f6Key.wasReleasedThisFrame
                };
                keyboardQuery.f7Key = new ButtonQuery
                {
                    isPressed            = keyboard.f7Key.isPressed,
                    wasPressedThisFrame  = keyboard.f7Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f7Key.wasReleasedThisFrame
                };
                keyboardQuery.f8Key = new ButtonQuery
                {
                    isPressed            = keyboard.f8Key.isPressed,
                    wasPressedThisFrame  = keyboard.f8Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f8Key.wasReleasedThisFrame
                };
                keyboardQuery.f9Key = new ButtonQuery
                {
                    isPressed            = keyboard.f9Key.isPressed,
                    wasPressedThisFrame  = keyboard.f9Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f9Key.wasReleasedThisFrame
                };
                keyboardQuery.f10Key = new ButtonQuery
                {
                    isPressed            = keyboard.f10Key.isPressed,
                    wasPressedThisFrame  = keyboard.f10Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f10Key.wasReleasedThisFrame
                };
                keyboardQuery.f11Key = new ButtonQuery
                {
                    isPressed            = keyboard.f11Key.isPressed,
                    wasPressedThisFrame  = keyboard.f11Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f11Key.wasReleasedThisFrame
                };
                keyboardQuery.f12Key = new ButtonQuery
                {
                    isPressed            = keyboard.f12Key.isPressed,
                    wasPressedThisFrame  = keyboard.f12Key.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.f12Key.wasReleasedThisFrame
                };
                keyboardQuery.enterKey = new ButtonQuery
                {
                    isPressed            = keyboard.enterKey.isPressed,
                    wasPressedThisFrame  = keyboard.enterKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.enterKey.wasReleasedThisFrame
                };
                keyboardQuery.escapeKey = new ButtonQuery
                {
                    isPressed            = keyboard.escapeKey.isPressed,
                    wasPressedThisFrame  = keyboard.escapeKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.escapeKey.wasReleasedThisFrame
                };
                keyboardQuery.equalsKey = new ButtonQuery
                {
                    isPressed            = keyboard.equalsKey.isPressed,
                    wasPressedThisFrame  = keyboard.equalsKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.equalsKey.wasReleasedThisFrame
                };
                keyboardQuery.minusKey = new ButtonQuery
                {
                    isPressed            = keyboard.minusKey.isPressed,
                    wasPressedThisFrame  = keyboard.minusKey.wasPressedThisFrame,
                    wasReleasedThisFrame = keyboard.minusKey.wasReleasedThisFrame
                };
                
                SystemAPI.SetSingleton(keyboardQuery);
            }
        }
    }
}