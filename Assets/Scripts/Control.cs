using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlState
{
    Pressed,
    JustPressed,
    Released,
    JustReleased
}

public class Control
{
    private float value;
    private ControlState state;

    public Control()
    {
        value = 0.0f;
        state = ControlState.Released;
    }

    public ControlState computeState()
    {
        if (value == 0.0)
        {
            if (state == ControlState.Pressed || state == ControlState.JustPressed)
            {
                state = ControlState.JustReleased;
            }
            else
            {
                state = ControlState.Released;
            }
        }
        else
        {
            if (state == ControlState.Released || state == ControlState.JustReleased)
            {
                state = ControlState.JustPressed;
            }
            else
            {
                state = ControlState.Pressed;
            }
        }
        return state;
    }

    public bool pressing()
    {
        return state == ControlState.Pressed || state == ControlState.JustPressed;
    }

    public void setValue(float ivalue)
    {
        value = ivalue;
    }

    public float getValue()
    {
        return value;
    }
}