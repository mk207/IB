using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyChangeEventArgs : EventArgs
{
    public float Energy { get; set; }
    public float InitEnergy { get; set; }

    public EnergyChangeEventArgs(float energyAmount, float initEnergy)
    {
        Energy = energyAmount;
        InitEnergy = initEnergy;
    }
}
