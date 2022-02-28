using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FluidType {
    Air,
    CarbonDioxide,
    Deuterium,
    Helium,
    Lithium,
    Nitrogen,
    Oxygen,
    Tritium,
}


public static class Fluids {
    public const float IDEAL_GAS_CONSTANT = 0.0821f;
}

public static class FluidTypeExtensions {
    /* density in g/cm3 at STP */
    public static float[] FluidDensity = new float[] {
        0.0012f,
        0.001977f,
        0.00008988f,
        0.0001785f,
        0.534f,
        0.0012506f,
        0.001429f,
        0.00008988f,
    };

    public static string[] ShortChar = new string[] {
        "Air",
        "CO2",
        "D",
        "He",
        "Li",
        "N",
        "O",
        "T",
    };

    public static float Density(this FluidType fluid) {
        // return FluidDensity[(int)fluid];
        return -1f;
    }

    public static string ShortName(this FluidType fluid) {
        return ShortChar[(int)fluid];
    }
}
