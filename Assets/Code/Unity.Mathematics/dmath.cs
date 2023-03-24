using System;

using Unity.Burst;

namespace Icarus.Mathematics {
    public static partial class dmath {
        public const double G = 6.674e-11; // m^3 * kg^-1 * s^-2
        public const double PI = System.Math.PI;

        // find orbital period from semi-major axis and (total) mass
        [BurstCompile]
        public static double Period(double sma, double mass1, double mass2) {
            // see: https://en.wikipedia.org/wiki/Kepler%27s_laws_of_planetary_motion#Third_law
            // T^2 = r^3 * (4PI^2 / GM)
            return Math.Sqrt(Math.Pow(sma * 1000, 3) * ((4 * dmath.PI * dmath.PI) / (G * (mass1 + mass2))));
        }
    }
}
