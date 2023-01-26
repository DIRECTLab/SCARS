using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}

public class NoiseMap : MonoBehaviour
{
    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ, Wave[] waves, float seed)
    {
        float[,] noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                float sampleX = (xIndex + offsetX) / scale;
                float sampleZ = (zIndex + offsetZ) / scale;

                float noise = 0f;
                float normalization = 0f;
                foreach (Wave wave in waves)
                {
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed + seed, sampleZ * wave.frequency + wave.seed + seed);
                    normalization += wave.amplitude;
                }

                noise /= normalization;

                noiseMap[zIndex, xIndex] = noise;
            }
        }

        return noiseMap;
    }
}
