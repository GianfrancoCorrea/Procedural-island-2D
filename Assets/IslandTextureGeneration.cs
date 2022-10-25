using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class IslandTextureGeneration : MonoBehaviour
{
    public int TextureSize;
    public float NoiseScale, IslandSize;
    [Range(1, 20)] public int NoiseOctaves;
    [Range(0, 99999999)] public int Seed;
    public Sprite _spite;

    // Privates

    private Color[] colors;
    private Texture2D texture;

    public Gradient ColorGradient;

    private void Start()
    {
         GenerateTexture();
        //GenerateTextureFromSprite(_spite);
    }

     private void OnValidate()
    {
        if (texture != null)
        {
            Debug.Log("Texture Updated");
            GenerateTexture();
            //GenerateTextureFromSprite(_spite);
        }
    }

    public void GenerateTexture()
    {
        // Create a new texture
        texture = new Texture2D(TextureSize, TextureSize);

        // Create a new array of colors
        colors = new Color[TextureSize * TextureSize];

        // Generate the colors
        GenerateColors();

        // Apply the colors to the texture
        texture.SetPixels(colors);
        texture.Apply();
        //texture.wrapMode = TextureWrapMode.Clamp;

        // Set the texture to the material
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    public void GenerateTextureFromSprite(Sprite sprite)
    {
        // Create a new texture
        texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        // make the texture readable
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Create a new array of colors
        colors = new Color[(int)sprite.rect.width * (int)sprite.rect.height];

        // Generate the colors
        GenerateColorsFromSprite(sprite);

        // Apply the colors to the texture
        texture.SetPixels(colors);
        texture.Apply();
        //texture.wrapMode = TextureWrapMode.Clamp;

        // Set the texture to the material
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void GenerateColors()
    {
        // Loop through the texture
        for (int x = 0; x < TextureSize; x++)
        {
            for (int y = 0; y < TextureSize; y++)
            {
                // Get the noise value
                float noise = Noisefunction(x, y, GetSeed());

                // Get the color from the gradient
                Color color = ColorGradient.Evaluate(noise);

                // Set the color
                colors[x + y * TextureSize] = color;
            }
        }
    }

    private void GenerateColorsFromSprite(Sprite sprite)
    {
        // Loop through the texture
        for (int x = 0; x < sprite.rect.width; x++)
        {
            for (int y = 0; y < sprite.rect.height; y++)
            {
                // Get the noise value
                float noise = Noisefunction(x, y, GetSeed());

                // Get the color from the gradient
                Color color = ColorGradient.Evaluate(noise);
                // noice from sprite
                Color spriteColor = sprite.texture.GetPixel(x, y);


                // Set the color
                colors[x + y * (int)sprite.rect.width] = spriteColor;
            }
        }
    }

    private Vector2 GetSeed()
    {
        return new Vector2(Mathf.Sqrt(Seed), Mathf.Sqrt(Seed));
    }

   /*  private void GenerateTexture()
    {
        texture = new Texture2D(TextureSize, TextureSize);
        colors = new Color[texture.height * texture.height];
        GetComponent<MeshRenderer>().material.mainTexture = texture;

        

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        // 

    } */

    private float Noisefunction(float x, float y, Vector2 Origin)
    {

        float a = 0, noisesize = NoiseScale, opacity = 1;

        for (int octaves = 0; octaves < NoiseOctaves; octaves++)
        {
            float xVal = (x / (noisesize * TextureSize)) + Origin.x;
            float yVal = (y / (noisesize * TextureSize)) - Origin.y;
            float z = noise.snoise(new float2(xVal, yVal));
            a += Mathf.InverseLerp(0, 1, z) / opacity;

            noisesize /= 2f;
            opacity *= 2f;
        }

        return a -= FallOffMap(x, y, TextureSize, IslandSize);
    }

    private float FallOffMap(float x, float y, int size, float islandSize)
    {
        float gradient = 1;

        gradient /= (x * y) / (size * size) * (1 - (x / size)) * (1 - (y / size));
        gradient -= 16;
        gradient /= islandSize;


        return gradient;
    }

   
}


/*   
Original Generate texture function:

        texture = new Texture2D(TextureSize, TextureSize);
        col = new Color[texture.height * texture.width];

        Renderer rend = GetComponent<MeshRenderer>();
        rend.sharedMaterial.mainTexture = texture;

        Vector2 Org = new Vector2(Mathf.Sqrt(Seed), Mathf.Sqrt(Seed));

        for (int x = 0, i = 0; x < TextureSize; x++){
            for (int y = 0; y < TextureSize; y++, i++){
                float a = Noisefunction(x, y, Org);
                col[i] = ColorGradient.Evaluate(a);
            }
        }
        texture.SetPixels(col);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp; 
*/