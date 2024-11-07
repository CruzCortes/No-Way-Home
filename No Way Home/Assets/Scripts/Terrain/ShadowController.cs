using UnityEngine;

public class ShadowController : MonoBehaviour
{
    private DayNightController dayNightController;
    private WeatherSystem weatherSystem;
    private MaterialPropertyBlock propertyBlock;

    [Header("Shadow Properties")]
    public float maxShadowLength = 5f;
    public Color shadowBaseColor = new Color(0.514f, 0.529f, 0.710f, 0.2f);
    public float shadowNoiseScale = 0.5f;
    public float shadowMovementSpeed = 1f;

    private float timeOffset = 0f;

    void Start()
    {
        dayNightController = FindObjectOfType<DayNightController>();
        weatherSystem = WeatherSystem.Instance;
        propertyBlock = new MaterialPropertyBlock();

        if (weatherSystem == null)
        {
            GameObject weatherObj = new GameObject("WeatherSystem");
            weatherSystem = weatherObj.AddComponent<WeatherSystem>();
        }
    }

    void Update()
    {
        if (dayNightController != null && weatherSystem != null)
        {
            UpdateShadows();
        }
    }

    void UpdateShadows()
    {
        timeOffset += Time.deltaTime * weatherSystem.GetWindSpeed() * shadowMovementSpeed;

        float timeOfDay = dayNightController.timeOfDay;
        float sunAngle = timeOfDay * 2 * Mathf.PI;

        Vector2 windDir = weatherSystem.GetCurrentWindDirection();
        float windSpeed = weatherSystem.GetWindSpeed();

        Vector2 baseShadowDir = new Vector2(
            Mathf.Cos(sunAngle),
            Mathf.Sin(sunAngle)
        ).normalized;

        float sunHeight = Mathf.Sin(timeOfDay * Mathf.PI);
        float shadowIntensity = maxShadowLength * (1f - sunHeight);

        // Use GroundTile tag instead of Ground
        var tiles = GameObject.FindGameObjectsWithTag("GroundTile");
        foreach (var tile in tiles)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                UpdateTileShadow(renderer, baseShadowDir, windDir, windSpeed, shadowIntensity, sunHeight);
            }
        }
    }

    void UpdateTileShadow(Renderer renderer, Vector2 baseShadowDir, Vector2 windDir, float windSpeed, float shadowIntensity, float sunHeight)
    {
        renderer.GetPropertyBlock(propertyBlock);

        // Calculate actual movement direction based on wind
        Vector2 movementDir = windDir * windSpeed;

        // Continuously moving seed value based on time
        float movingSeed = timeOffset * windSpeed;

        // Apply properties
        propertyBlock.SetVector("_ShadowDirection", movementDir);
        propertyBlock.SetFloat("_ShadowIntensity", shadowIntensity);
        propertyBlock.SetColor("_ShadowColor", shadowBaseColor);
        propertyBlock.SetFloat("_Seed", movingSeed);

        renderer.SetPropertyBlock(propertyBlock);
    }
}