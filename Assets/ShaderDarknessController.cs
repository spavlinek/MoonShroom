using UnityEngine;

public class ShaderDarknessController : MonoBehaviour
{
    [Header("Darkness Settings")]
    public bool isDarkMode = false;
    public Color darknessColor = Color.black;
    
    [Header("Light Settings")]
    public float lightRadius = 5f;
    public float edgeSoftness = 0.5f;
    public Transform player;
    
    [Header("References")]
    public Camera mainCamera;
    public Shader darknessShader; // Assign in Inspector
    
    private GameObject darknessQuad;
    private Material darknessMaterial;

    [Header("Gradual Darkness")]
    public bool useGradualDarkness = true;
    public Transform caveEntrance; // Position of cave entrance
    public float darknessTransitionDistance = 10f; // How far into cave before fully dark
    private float currentDarknessIntensity = 0f; // 0 = no darkness, 1 = full darkness
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Find shader if not assigned in inspector
        if (darknessShader == null)
        {
            darknessShader = Shader.Find("Custom/DarknessOverlay");
            if (darknessShader == null)
            {
                Debug.LogError("DarknessOverlay shader not found! Please assign it in the Inspector or add it to Graphics Settings > Always Included Shaders");
                return;
            }
        }
        
        CreateDarknessQuad();
    }
    
    void CreateDarknessQuad()
    {
        // Create a quad that covers the screen
        darknessQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        darknessQuad.name = "DarknessOverlay";
        
        // Remove collider
        Destroy(darknessQuad.GetComponent<Collider>());
        
        // Create material with custom shader
        darknessMaterial = new Material(darknessShader);
        darknessMaterial.SetColor("_DarknessColor", darknessColor);
        darknessMaterial.SetFloat("_LightRadius", lightRadius);
        darknessMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
        
        // Apply material
        MeshRenderer renderer = darknessQuad.GetComponent<MeshRenderer>();
        renderer.material = darknessMaterial;
        renderer.sortingOrder = 1000;
        
        // Position quad in front of camera
        UpdateQuadTransform();
        
        darknessQuad.SetActive(isDarkMode);
    }
    
    void UpdateQuadTransform()
    {
        if (mainCamera == null || darknessQuad == null) return;
        
        // Calculate size to cover camera view
        float distance = 0.01f; // Distance from camera
        //float height = 2f * mainCamera.orthographicSize;
        float height = 10000;
        //float width = height * mainCamera.aspect;
        float width = 10000;
        
        // Position and scale the quad
        darknessQuad.transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            distance
        );
        darknessQuad.transform.localScale = new Vector3(width * 2f, height * 2f, 0.01f);
        darknessQuad.transform.rotation = Quaternion.identity;
    }
    
    void Update()
    {
        if (!isDarkMode || player == null || darknessMaterial == null) return;
        
        // Calculate darkness intensity based on distance from entrance
        if (useGradualDarkness && caveEntrance != null)
        {
            float distanceFromEntrance = Vector3.Distance(player.position, caveEntrance.position);
            
            // Calculate intensity: 0 at entrance, 1 at transitionDistance
            currentDarknessIntensity = Mathf.Clamp01(distanceFromEntrance / darknessTransitionDistance);
            
            // Apply intensity to shader
            Color adjustedColor = darknessColor;
            adjustedColor.a = darknessColor.a * currentDarknessIntensity;
            darknessMaterial.SetColor("_DarknessColor", adjustedColor);
        }
        
        // Update player position in shader
        darknessMaterial.SetVector("_PlayerPos", new Vector4(
            player.position.x, 
            player.position.y, 
            player.position.z, 
            0
        ));
        
        // Update quad to follow camera
        UpdateQuadTransform();
    }
    
    // Public methods
    public void EnableDarkness()
    {
        isDarkMode = true;
        if (darknessQuad != null) 
            darknessQuad.SetActive(true);
    }
    
    public void DisableDarkness()
    {
        isDarkMode = false;
        if (darknessQuad != null) 
            darknessQuad.SetActive(false);
    }
    
    public void SetLightRadius(float radius)
    {
        lightRadius = radius;
        if (darknessMaterial != null)
            darknessMaterial.SetFloat("_LightRadius", radius);
    }
    
    public void SetEdgeSoftness(float softness)
    {
        edgeSoftness = softness;
        if (darknessMaterial != null)
            darknessMaterial.SetFloat("_EdgeSoftness", softness);
    }
    
    void OnDestroy()
    {
        if (darknessMaterial != null)
            Destroy(darknessMaterial);
        if (darknessQuad != null)
            Destroy(darknessQuad);
    }
}