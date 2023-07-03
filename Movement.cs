using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement
{
    private CarAgent carAgent;

    public Movement(CarAgent carAgent)
    {
        this.carAgent = carAgent;
    }

    public void MoveCar(float horizontal, float vertical, float dt)
    {
        float distance = carAgent.speed * vertical;
        carAgent.transform.Translate(distance * dt * Vector3.forward);

        float rotation = horizontal * carAgent.torque * 90f;
        carAgent.transform.Rotate(0f, rotation * dt, 0f);
        carAgent.prevHorizontal = horizontal;
        carAgent.prevVertical = vertical;
    }
}