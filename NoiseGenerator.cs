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
    private Color[] pix;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        noiseTex = new Texture2D(pixSize.x, pixSize.y);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.material.mainTexture = noiseTex;
    }

    void InitiatePixs(Color[] pix)
    {
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = new Color(0, 0, 0);
        }
    }

    void ComputeNoise(Color[] pix, float scale = 1.0f, uint harmonics = 1)
    {
        float minVal = 1f;
        float maxVal = -1f;
        for (float y = 0.0f; y < noiseTex.height; y++)
        {
            for (float x = 0.0f; x < noiseTex.width; x++)
            {
                float X = origin.x + x / noiseTex.width * scale;
                float Y = origin.y + y / noiseTex.height * scale;

                float sample = 0;

                float frequency = 1;
                float amplitude = 1;

                for (int h = 0; h < harmonics; h++)
                {
                    sample += (Mathf.PerlinNoise(X * frequency, Y * frequency) * 2 - 1) * amplitude;
                    frequency *= 2;
                    amplitude /= 2;
                }

                pix[(int)(x + y * noiseTex.height)] += new Color(sample, sample, sample);
                minVal = Mathf.Min(minVal, sample);
                maxVal = Mathf.Max(maxVal, sample);
            }
        }

        for (int i = 0; i < pix.Length; i++)
        {
            float val = (pix[i].r - minVal) / (maxVal - minVal);
            pix[i] = new Color(val, val, val);
        }
    }

    void UpdateTex(Color[] pix, Texture2D tex)
    {
        tex.SetPixels(pix);
        tex.Apply();
    }

    void Update()
    {
        InitiatePixs(pix);

        ComputeNoise(pix, scale, harmonics);

        //RescalePix(pix);
        UpdateTex(pix, noiseTex);
    }
}
