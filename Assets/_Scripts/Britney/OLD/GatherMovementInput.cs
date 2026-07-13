using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GatherMovementInput : MonoBehaviour
{
    PlayerControls playerControls;
    public float XInput;
    public bool JumpRightPressing;
    public bool JumpLeftPressing;
    public bool JumpUpPressed;
    public bool JumpUpPressing;
    public bool JumpUpReleased;
    public bool SpeedUpPressing;
    public bool SpeedDownPressing;


    void OnMove(InputValue inputValue)
    {
        XInput = inputValue.Get<float>();
    }

    void OnJumpRight(InputValue inputValue)
    {
        JumpRightPressing = inputValue.isPressed;
    }

    void OnJumpLeft(InputValue inputValue)
    {
        JumpLeftPressing = inputValue.isPressed;
    }

    private int _numOfJumpPresses;
    void OnJumpUp(InputValue inputValue)
    {
        _numOfJumpPresses++;
        if (_numOfJumpPresses % 2 == 0)
        {
            JumpUpReleased = true;
            JumpUpPressed = false;
            JumpUpPressing = false;
        }
        else
        {
            JumpUpPressed = true;
            JumpUpReleased = false;
            JumpUpPressing = true;
            StartCoroutine(PerformActionOnNextFrame());
        }
    }

    private IEnumerator PerformActionOnNextFrame()
    {
        // Wait for the next frame
        yield return null;

        // Perform the action on the next frame
        JumpUpPressed = false;
    }

    void OnSpeedUp(InputValue inputValue)
    {
        SpeedUpPressing = inputValue.isPressed;
    }
    
    void OnSpeedDown(InputValue inputValue)
    {
        SpeedDownPressing = inputValue.isPressed;
        
    }

    private void Start()
    {
        playerControls = new();
    }
}
