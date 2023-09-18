using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct UnmanagedMouse : IComponentData
    {
        public float2      delta;
        public float2      position;
        public float2      scroll;
        public ButtonQuery leftButton;
        public ButtonQuery rightButton;
        public ButtonQuery middleButton;
        public bool        IsDisconnected;
    }

    public struct ButtonQuery
    {
        public bool isPressed;
        public bool wasPressedThisFrame;
        public bool wasReleasedThisFrame;
    }

    public struct UnmanagedKeyboard : IComponentData
    {
        public bool        IsDisconnected;
        public ButtonQuery aKey;
        public ButtonQuery bKey;
        public ButtonQuery cKey;
        public ButtonQuery dKey;
        public ButtonQuery eKey;
        public ButtonQuery fKey;
        public ButtonQuery gKey;
        public ButtonQuery hKey;
        public ButtonQuery iKey;
        public ButtonQuery jKey;
        public ButtonQuery kKey;
        public ButtonQuery lKey;
        public ButtonQuery mKey;
        public ButtonQuery nKey;
        public ButtonQuery oKey;
        public ButtonQuery pKey;
        public ButtonQuery qKey;
        public ButtonQuery rKey;
        public ButtonQuery sKey;
        public ButtonQuery tKey;
        public ButtonQuery uKey;
        public ButtonQuery vKey;
        public ButtonQuery wKey;
        public ButtonQuery xKey;
        public ButtonQuery yKey;
        public ButtonQuery zKey;
        public ButtonQuery spaceKey;
        public ButtonQuery Backspace;
        public ButtonQuery deleteKey;
        public ButtonQuery enterKey;
        public ButtonQuery escapeKey;
        public ButtonQuery upArrowKey;
        public ButtonQuery downArrowKey;
        public ButtonQuery leftArrowKey;
        public ButtonQuery rightArrowKey;
        public ButtonQuery leftShiftKey;
        public ButtonQuery rightShiftKey;
        public ButtonQuery leftCtrlKey;
        public ButtonQuery rightCtrlKey;
        public ButtonQuery leftAltKey;
        public ButtonQuery rightAltKey;
        public ButtonQuery leftWindowsKey;
        public ButtonQuery rightWindowsKey;
        public ButtonQuery numLockKey;
        public ButtonQuery CapsLock;
        public ButtonQuery scrollLockKey;
        public ButtonQuery insertKey;
        public ButtonQuery homeKey;
        public ButtonQuery endKey;
        public ButtonQuery pageUpKey;
        public ButtonQuery pageDownKey;

        public ButtonQuery tabKey;
        public ButtonQuery f1Key;
        public ButtonQuery f2Key;
        public ButtonQuery f3Key;
        public ButtonQuery f4Key;
        public ButtonQuery f5Key;
        public ButtonQuery f6Key;
        public ButtonQuery f7Key;
        public ButtonQuery f8Key;
        public ButtonQuery f9Key;
        public ButtonQuery f10Key;
        public ButtonQuery f11Key;
        public ButtonQuery f12Key;

        public ButtonQuery equalsKey;
        public ButtonQuery minusKey;
    }
}