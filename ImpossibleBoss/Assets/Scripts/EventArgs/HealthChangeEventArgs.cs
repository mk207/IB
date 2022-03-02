using System;



public class HealthChangeEventArgs : EventArgs
{
    public float Health { get; set; }    
    public float InitHealth { get; set; }

    public HealthChangeEventArgs(float healthAmount, float initHealth)
    {
        Health = healthAmount;
        InitHealth = initHealth;
    }
}
