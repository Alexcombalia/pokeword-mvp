using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public static bool InteractPressed  { get; private set; }
    public static bool PokeWorldPressed { get; private set; }
    public static bool ClosePressed     { get; private set; }

    private void LateUpdate()
    {
        InteractPressed  = false;
        PokeWorldPressed = false;
        ClosePressed     = false;
    }

    public static void PressInteract()  { InteractPressed  = true; }
    public static void PressPokeWorld() { PokeWorldPressed = true; }
    public static void PressClose()     { ClosePressed     = true; }
}
