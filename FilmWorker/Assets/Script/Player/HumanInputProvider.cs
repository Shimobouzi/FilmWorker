using UnityEngine;
using UnityEngine.InputSystem;

public class HumanInputProvider : MonoBehaviour, IInputProvider
{
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference actionAction;

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        actionAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        actionAction.action.Disable();
    }

    public float GetHorizontal()
    {
        return moveAction.action.ReadValue<Vector2>().x;
    }

    public bool GetJumpDown()
    {
        return jumpAction.action.WasPressedThisFrame();
    }

    public bool GetActionDown()
    {
        return actionAction.action.WasPressedThisFrame();
    }
}
