using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidContainer : MonoBehaviour {
    public float volume;      // in liters
    public int count = 0;
    public FluidType[] types = new FluidType[MAX_COUNT];
    public float[] mass = new float[MAX_COUNT];        // in moles
    public float[] temp = new float[MAX_COUNT];        // in kelvin
    public float[] pressure = new float[MAX_COUNT];
    public float totalMass;
    public float totalPressure;

    private const int MAX_COUNT = 8;
    private const float IDEAL_GAS_CONSTANT = 0.0821f;

    public void MarkDirty() {
        /* don't mark it dirty, just update all values */
        totalMass = 0f;
        for (int i = 0; i < count; i++) {
            totalMass += mass[i];
        }
        totalPressure = 0f;
        for (int i = 0; i < count; i++) {
            float vol = volume * (mass[i] / totalMass);
            pressure[i] = (mass[i] * IDEAL_GAS_CONSTANT * temp[i]) / vol;
            totalPressure = Mathf.Max(totalPressure, pressure[i]);
        }
    }
}
