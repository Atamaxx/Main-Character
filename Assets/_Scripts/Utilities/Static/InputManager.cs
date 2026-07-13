using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static bool LeftMouseButtonDown => Input.GetMouseButtonDown(0);
    public static bool LeftMouseButtonUp => Input.GetMouseButtonUp(0);
    public static bool LeftMouseButton => Input.GetMouseButton(0);

    public static bool RightMouseButtonDown => Input.GetMouseButtonDown(1);
    public static bool RightMouseButtonUp => Input.GetMouseButtonUp(1);
    public static bool RightMouseButton => Input.GetMouseButton(1);
    public static bool Space => Input.GetKeyDown(KeyCode.Space);
}