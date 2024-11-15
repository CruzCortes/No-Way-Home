using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SnowStormController : MonoBehaviour
{
    public enum SnowState
    {
        None,
        LightSnow,
        Blizzard
    }

    [Header("Weather States")]
    public SnowState currentState = SnowState.None;
    public float stateCheckInterval = 10f;

    [Header("Noise Settings")]
    private int textureSize = 256;
    private Texture2D snowNoiseTexture;
    private Texture2D noiseGradient;

    [Header("Snow Parameters")]
    [Range(0, 1)]
    public float snowIntensity = 0.5f;
    public float scrollSpeed = 1f;

    private Material snowMaterial;
    private float stateTimer;
    private Vector2 scrollOffset;

    void Start()
    {
        // Generate noise textures
        GenerateNoiseTextures();

        // Create material from shader
        Shader shader = Shader.Find("Custom/SnowStormShader");
        if (shader != null)
        {
            snowMaterial = new Material(shader);
            snowMaterial.SetTexture("_SnowTexture", snowNoiseTexture);
            snowMaterial.SetTexture("_NoiseMask", noiseGradient);
        }
        else
        {
            Debug.LogError("SnowStormShader not found!");
        }

        stateTimer = stateCheckInterval;
    }

    void GenerateNoiseTextures()
    {
        // Generate main noise texture
        snowNoiseTexture = new Texture2D(textureSize, textureSize);
        snowNoiseTexture.wrapMode = TextureWrapMode.Repeat;

        // Generate gradient noise texture
        noiseGradient = new Texture2D(textureSize, textureSize);
        noiseGradient.wrapMode = TextureWrapMode.Repeat;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Generate main noise (snow particles)
                float xCoord = (float)x / textureSize * 20f;
                float yCoord = (float)y / textureSize * 20f;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                float sharpNoise = sample > 0.7f ? 1f : 0f; // Create distinct snow particles
                snowNoiseTexture.SetPixel(x, y, new Color(sharpNoise, sharpNoise, sharpNoise, sharpNoise));

                // Generate gradient noise (for movement variation)
                float gradientSample = Mathf.PerlinNoise(xCoord * 0.5f, yCoord * 0.5f);
                noiseGradient.SetPixel(x, y, new Color(gradientSample, gradientSample, gradientSample, 1));
            }
        }

        snowNoiseTexture.Apply();
        noiseGradient.Apply();
    }

    void Update()
    {
        if (WeatherSystem.Instance != null)
        {
            float temp = WeatherSystem.Instance.GetCurrentTemperature();
            Vector2 windDir = WeatherSystem.Instance.GetCurrentWindDirection();
            float windSpeed = WeatherSystem.Instance.GetWindSpeed();

            // Update state timer
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                stateTimer = stateCheckInterval;

                // Update weather state based on temperature
                if (temp < 0f)
                {
                    currentState = Random.value < 0.5f ? SnowState.Blizzard : SnowState.LightSnow;
                }
                else
                {
                    currentState = SnowState.None;
                }
            }

            // Update scroll offset based on wind
            scrollOffset += new Vector2(
                windDir.x * windSpeed * Time.deltaTime * scrollSpeed,
                windDir.y * windSpeed * Time.deltaTime * scrollSpeed
            );

            UpdateShaderProperties(windDir, windSpeed);
        }
    }

    void UpdateShaderProperties(Vector2 windDir, float windSpeed)
    {
        if (snowMaterial != null)
        {
            float baseIntensity = 0f;
            float baseSpeed = 1f;

            switch (currentState)
            {
                case SnowState.None:
                    baseIntensity = 0f;
                    break;
                case SnowState.LightSnow:
                    baseIntensity = 0.3f;
                    baseSpeed = 0.5f;
                    break;
                case SnowState.Blizzard:
                    baseIntensity = 1f;
                    baseSpeed = 2f;
                    break;
            }

            snowMaterial.SetFloat("_SnowIntensity", baseIntensity * snowIntensity);
            snowMaterial.SetFloat("_Speed", baseSpeed * windSpeed);
            snowMaterial.SetVector("_OffsetDirection", new Vector4(windDir.x, windDir.y, 0, 0));
            snowMaterial.SetVector("_ScrollOffset", scrollOffset);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (snowMaterial != null && currentState != SnowState.None)
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
        if (snowNoiseTexture != null)
            Destroy(snowNoiseTexture);
        if (noiseGradient != null)
            Destroy(noiseGradient);
        if (snowMaterial != null)
            Destroy(snowMaterial);
    }
}