using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateFuelDispersion
{
    private CarAgent carAgent;
    private CarInformation carInformation;
    private RewardCalculation rewardCalculation;

    public float currentFuel = 0;

    public CalculateFuelDispersion(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
        this.rewardCalculation = carAgent.rewardCalculation;
    }

    public void CarFuelConsumption()
    {
        if (carAgent.speed >= 10)
        {
            carAgent.fuelConsumption += carAgent.fuelPenaltyRateSpeed10;
        }
        else if (carAgent.speed >= 9)
        {
            carAgent.fuelConsumption += carAgent.fuelPenaltyRateSpeed9;
        }
        else if (carAgent.speed >= 8)
        {
            carAgent.fuelConsumption += carAgent.fuelPenaltyRateSpeed8;
        }
        else if (carAgent.speed >= 7)
        {
            carAgent.fuelConsumption += carAgent.fuelPenaltyRateSpeed7;
        }
        else
        {
            carAgent.fuelConsumption += 0;
        }
        Debug.Log(carAgent.id + ": Fuel Consumption = " + carAgent.fuelConsumption);
    }

    public void CollectFuelConsumption(float currentFuel)
    {
        currentFuel = carAgent.fuelConsumption;
        if(carAgent.maxFuelConsumption < currentFuel) 
        {
            carAgent.maxFuelConsumption = currentFuel;
        }
        if(carAgent.minFuelConsumption > currentFuel) 
        {
            carAgent.minFuelConsumption = currentFuel;
        }
    }

    public void ResetCarFuelConsumption()
    {
        carAgent.fuelConsumption = 0;
        Debug.Log(carAgent.id + ": Fuel Consumption has been reset.");
    }
    public void SlipStreamReward()
    {
        
    }
}
