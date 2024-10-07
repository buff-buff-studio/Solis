using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

[RequireComponent(typeof(PlayerInput))]
public class SolisInput : MonoBehaviour
{
    public enum InputType
    {
        Keyboard,
        Gamepad
    }

    [SerializeField] private PlayerInput _input;
    private Coroutine _rumbleCoroutine;

    public static SolisInput Instance { get; private set; }
    public static InputType CurrentInputType;
    public static UnityAction<string, object> OnInputAction;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);
    }

    private void OnEnable()
    {
        _input.ActivateInput();
        _input.onActionTriggered += OnActionTriggered;
        _input.onControlsChanged += OnControlsChanged;
    }

    private void OnDisable()
    {
        _input.DeactivateInput();
        _input.onActionTriggered -= OnActionTriggered;
    }

    private void OnActionTriggered(InputAction.CallbackContext obj)
    {
        OnInputAction?.Invoke(obj.action.name, obj.ReadValueAsObject());
    }

    private void OnControlsChanged(PlayerInput obj)
    {
        CurrentInputType = obj.currentControlScheme switch
        {
            "Keyboard" => InputType.Keyboard,
            "Gamepad" => InputType.Gamepad,
            _ => InputType.Keyboard
        };
        ;
        Debug.Log("Controls changed to: " + obj.currentControlScheme);
    }

    public static Vector2 GetVector2(string actionName)
    {
        return Instance._input.actions[actionName].ReadValue<Vector2>();
    }

    public static bool GetKey(string actionName)
    {
        return Instance._input.actions[actionName].IsPressed();
    }

    public static bool GetKeyDown(string actionName)
    {
        return Instance._input.actions[actionName].WasPressedThisFrame();
    }

    public static bool GetKeyUp(string actionName)
    {
        return Instance._input.actions[actionName].WasReleasedThisFrame();
    }

    public static void GamepadLight(Color col)
    {
        if (CurrentInputType == InputType.Gamepad)
        {
            var gamepad = (DualShockGamepad)Gamepad.all.ToList().Find(x => x is DualShockGamepad);
            if (gamepad == null) return;

            gamepad.SetLightBarColor(col);
        }
    }

    public void RumblePulse(float low, float high, float duration)
    {
        if (CurrentInputType == InputType.Gamepad)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            gamepad.SetMotorSpeeds(low, high);
            _rumbleCoroutine = StartCoroutine(StopRumble(gamepad, duration));
        }
    }

    private IEnumerator StopRumble(Gamepad gamepad, float duration)
    {
        yield return new WaitForSeconds(duration);
        gamepad.SetMotorSpeeds(0,0);
    }
}
