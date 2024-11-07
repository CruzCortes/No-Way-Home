using System;
using UnityEngine;

public class FastNoiseLite
{
    private const int PERM_SIZE = 256;
    private const int PERM_MASK = PERM_SIZE - 1;
    private int[] perm = new int[PERM_SIZE * 2];
    private float frequency = 0.01f;
    private NoiseType noiseType = NoiseType.Perlin;
    private int seed;

    public enum NoiseType
    {
        Perlin,
        Simplex,
        Value
    }

    public FastNoiseLite(int seed = 1337)
    {
        SetSeed(seed);
    }

    public void SetSeed(int seed)
    {
        this.seed = seed;

        System.Random rand = new System.Random(seed);
        for (int i = 0; i < PERM_SIZE; i++)
        {
            perm[i] = i;
        }

        // Fisher-Yates shuffle
        for (int i = PERM_SIZE - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            int temp = perm[i];
            perm[i] = perm[j];
            perm[j] = temp;
        }

        // Duplicate permutation table to avoid overflow
        for (int i = 0; i < PERM_SIZE; i++)
        {
            perm[PERM_SIZE + i] = perm[i];
        }
    }

    public void SetFrequency(float frequency)
    {
        this.frequency = frequency;
    }

    public void SetNoiseType(NoiseType type)
    {
        this.noiseType = type;
    }

    public float GetNoise(float x, float y)
    {
        switch (noiseType)
        {
            case NoiseType.Perlin:
                return GetPerlinNoise(x * frequency, y * frequency);
            case NoiseType.Value:
                return GetValueNoise(x * frequency, y * frequency);
            default:
                return GetPerlinNoise(x * frequency, y * frequency);
        }
    }

    private float GetPerlinNoise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float sx = x - x0;
        float sy = y - y0;

        float n0 = DotGridGradient(x0, y0, x, y);
        float n1 = DotGridGradient(x1, y0, x, y);
        float ix0 = Mathf.Lerp(n0, n1, SmoothStep(sx));

        n0 = DotGridGradient(x0, y1, x, y);
        n1 = DotGridGradient(x1, y1, x, y);
        float ix1 = Mathf.Lerp(n0, n1, SmoothStep(sx));

        return Mathf.Lerp(ix0, ix1, SmoothStep(sy)) * 0.5f + 0.5f;
    }

    private float GetValueNoise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float sx = x - x0;
        float sy = y - y0;

        float n0 = GetPseudoRandom(x0, y0);
        float n1 = GetPseudoRandom(x1, y0);
        float ix0 = Mathf.Lerp(n0, n1, SmoothStep(sx));

        n0 = GetPseudoRandom(x0, y1);
        n1 = GetPseudoRandom(x1, y1);
        float ix1 = Mathf.Lerp(n0, n1, SmoothStep(sx));

        return Mathf.Lerp(ix0, ix1, SmoothStep(sy));
    }

    private float DotGridGradient(int ix, int iy, float x, float y)
    {
        int hash = perm[(perm[ix & PERM_MASK] + iy) & PERM_MASK];
        hash = hash & 3;

        float gx = hash == 0 ? 1 : hash == 1 ? -1 : hash == 2 ? 1 : -1;
        float gy = hash == 0 ? 1 : hash == 1 ? 1 : hash == 2 ? -1 : -1;

        float dx = x - ix;
        float dy = y - iy;

        return dx * gx + dy * gy;
    }

    private float GetPseudoRandom(int x, int y)
    {
        int hash = perm[(perm[x & PERM_MASK] + y) & PERM_MASK];
        return hash / (float)PERM_SIZE;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3 - 2 * t);
    }
}