using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    //The size of the generated noise in px
    public Vector2Int pixSize;

    //The origin of the noise
    public Vector2 origin;

    public float scale;
    public uint harmonics = 1;

    private Texture2D noiseTex;
    private List<float> map = new List<float>();
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        noiseTex = new Texture2D(pixSize.x, pixSize.y);
        rend.material.mainTexture = noiseTex;
    }

    public static float sampleNoise(float x, float y, uint harmonics = 1, float persistence = 0.5f, float lacunarity = 2f)
    {
        float sample = 0;

        float frequency = 1;
        float amplitude = 1;

        for (int h = 0; h < harmonics; h++, frequency *= lacunarity, amplitude *= persistence)
        {
            sample += (Mathf.PerlinNoise(x * frequency, y * frequency) * 2 - 1) * amplitude;
        }

        return sample;
    }

    public static void populateMap(List<float> map, uint width, uint height, Vector2 origin, float scale = 1.0f, uint harmonics = 1)
    {
        float minVal = 1f;
        float maxVal = -1f;

        for (float y = 0.0f; y < height; y++)
        {
            for (float x = 0.0f; x < width; x++)
            {
                float X = origin.x + x / width * scale;
                float Y = origin.y + y / height * scale;

                float sample = NoiseGenerator.sampleNoise(X, Y, harmonics);

                map.Add(sample);

                minVal = Mathf.Min(minVal, sample);
                maxVal = Mathf.Max(maxVal, sample);
            }
        }

        for (int i = 0; i < map.Count; i++)
        {
            //Inverse lerping between minVal and maxVal for map[i]
            map[i] = (map[i] - minVal) / (maxVal - minVal);
        }
    }

    void UpdateTex(List<float> map, Texture2D tex)
    {
        Color[] pixels = new Color[tex.width * tex.height];

        for (int i = 0; i < map.Count; i++)
        {
            float value = map[i];
            pixels[i] = new Color(value, value, value);
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    void Update()
    {
        map = new List<float>();

        noiseTex = new Texture2D(pixSize.x, pixSize.y);
        rend.material.mainTexture = noiseTex;

        populateMap(map, (uint)pixSize.x, (uint)pixSize.y, origin, scale, harmonics);
        UpdateTex(map, noiseTex);
    }
}
