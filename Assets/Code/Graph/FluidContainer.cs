using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidContainer : MonoBehaviour {
    public float volume = -1f;      // in liters
    public int count = 0;
    public FluidType[] types = new FluidType[MAX_COUNT];
    public float[] mass = new float[MAX_COUNT];        // in moles
    public float[] temp = new float[MAX_COUNT];        // in kelvin
    public float[] pressure = new float[MAX_COUNT];
    private float[] ratio = new float[MAX_COUNT];
    public float totalMass;
    public float totalPressure;
    // public float totalTemp;
    public float fullPressure = -1f;
    public float massAvailable;

    private const float MOVEMENT_DIVISOR = 0.05f;
    private const int MAX_COUNT = 8;
    private const float ZERO_EPSILON = 0.00001f;

    public void Add(FluidType fluid, float mols, float kelvin) {
        for (int i = 0; i < count; i++) {
            if (types[i] == fluid) {
                mass[i] += mols;
                // TODO temp
                return;
            }
        }
        if (count == MAX_COUNT) {
            throw new System.Exception("tried to add {mols}mol of {fluid} to container with no empty slots");
        }
        types[count] = fluid;
        mass[count] = mols;
        temp[count] = kelvin;
        count += 1;
    }

    public void Remove(FluidType fluid, float mols) {
        for (int i = 0; i < count; i++) {
            if (types[i] == fluid) {
                if (mass[i] < mols) {
                    throw new System.Exception($"tried to remove {mols}mol of {fluid}, current mass is {mass[i]}mol");
                }
                mass[i] -= mols;
                if (mass[i] < ZERO_EPSILON) {
                    count -= 1;
                    for (int j = i; j < count; j++) {
                        mass[j] = mass[j + 1];
                        temp[j] = temp[j + 1];
                        types[j] = types[j + 1];
                    }
                }
                return;
            }
        }
        throw new System.Exception($"tried to remove {mols}mol of {fluid}, no fluid of {fluid} found");
    }

    public void MarkDirty() {
        /* don't mark it dirty, just update all values */
        totalMass = 0f;
        for (int i = 0; i < count; i++) {
            totalMass += mass[i];
        }
        totalPressure = 0f;
        // totalTemp = 0f;
        for (int i = 0; i < count; i++) {
            ratio[i] = mass[i] / totalMass;
            float vol = volume * ratio[i];
            pressure[i] = (mass[i] * Fluids.IDEAL_GAS_CONSTANT * temp[i]) / vol;
            totalPressure = Mathf.Max(totalPressure, pressure[i]);
        }
        if (totalPressure >= fullPressure) {
            massAvailable = 0f;
            // Debug.Log($"{totalPressure} >= {fullPressure}");
        } else {
            // PV = nRT
            massAvailable = ((fullPressure - totalPressure) * volume)
                / (Fluids.IDEAL_GAS_CONSTANT * 280f); // XXX
            // Debug.Log($"{massAvailable} = (({fullPressure} - {totalPressure}) * volume) / ({Fluids.IDEAL_GAS_CONSTANT} * 280f)");
        }
    }

    public void Equalize(FluidContainer other, bool bidirectional) {
        bool dirty = false;
        // should we move to other?
        if (other.massAvailable > 0f && totalPressure > other.totalPressure) {
            DoTransfer(this, other);
            dirty = true;
        }
        // should other move to us?
        else if (bidirectional && massAvailable > 0 && other.totalPressure > totalPressure) {
            DoTransfer(other, this);
            dirty = true;
        }
        if (dirty) {
            this.MarkDirty();
            other.MarkDirty();
        }
    }

    public void Equalize(FluidContainer other) {
        Equalize(other, true);
    }

    private static void DoTransfer(FluidContainer src, FluidContainer dest) {
        for (int i = 0; i < src.count; i++) {
            float mass = Mathf.Min(dest.massAvailable * MOVEMENT_DIVISOR * src.ratio[i],
                                   src.mass[i]);
            FluidType fluid = src.types[i];
            float temp = src.temp[i];
            src.Remove(fluid, mass);
            dest.Add(fluid, mass, temp);
        }
    }
}
