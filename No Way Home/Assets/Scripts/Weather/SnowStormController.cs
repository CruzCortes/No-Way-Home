using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SnowStormController : MonoBehaviour
{
    [Header("Noise Settings")]
    public int textureSize = 256;
    public float noiseScale = 20f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Snow Settings")]
    [Range(0, 1)]
    public float snowIntensity = 0.5f;
    public float horizontalSpeed = 1f;
    public float verticalSpeed = 1f;
    public float tiling = 1f;

    private Material snowMaterial;
    private Vector2 scrollOffset;
    private Texture2D noiseTexture;

    void Start()
    {
        // Generate noise texture
        GenerateNoiseTexture();

        // Create material from shader
        Shader shader = Shader.Find("Custom/SnowStormShader");
        if (shader != null)
        {
            snowMaterial = new Material(shader);
            snowMaterial.SetTexture("_NoiseTex", noiseTexture);
        }
        else
        {
            Debug.LogError("SnowStormShader not found!");
        }
    }

    void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(textureSize, textureSize);
        noiseTexture.wrapMode = TextureWrapMode.Repeat;

        float[,] noiseMap = new float[textureSize, textureSize];

        // Generate multiple octaves of noise
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Generate octaves
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / noiseScale * frequency;
                    float sampleY = y / noiseScale * frequency;

                    // Generate snow-like pattern
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize and create snow-like pattern
        Color[] colorMap = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedHeight = noiseMap[x, y];

                // Create snow-like effect by thresholding
                float snowValue = normalizedHeight > 0.7f ? 1f : 0f;

                colorMap[y * textureSize + x] = new Color(snowValue, snowValue, snowValue, snowValue);
            }
        }

        noiseTexture.SetPixels(colorMap);
        noiseTexture.Apply();
    }

    void Update()
    {
        if (WeatherSystem.Instance != null)
        {
            Vector2 windDir = WeatherSystem.Instance.GetCurrentWindDirection();
            float windSpeed = WeatherSystem.Instance.GetWindSpeed();

            // Update scroll speed based on wind
            horizontalSpeed = windDir.x * windSpeed;
            verticalSpeed = windDir.y * windSpeed;
        }

        // Update scroll offset
        scrollOffset.x += horizontalSpeed * Time.deltaTime;
        scrollOffset.y += verticalSpeed * Time.deltaTime;

        UpdateShaderProperties();
    }

    void UpdateShaderProperties()
    {
        if (snowMaterial != null)
        {
            snowMaterial.SetFloat("_Intensity", snowIntensity);
            snowMaterial.SetFloat("_Tiling", tiling);
            snowMaterial.SetVector("_ScrollOffset", scrollOffset);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (snowMaterial != null)
        {
            Graphics.Blit(source, destination, snowMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void OnDestroy()
    {
        if (noiseTexture != null)
        {
            Destroy(noiseTexture);
        }

        if (snowMaterial != null)
        {
            Destroy(snowMaterial);
        }
    }
}