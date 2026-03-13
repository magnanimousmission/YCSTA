using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Aura Skinned Setup")]
public class AuraSkinnedSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer sourceRenderer;
    [SerializeField] private Material auraMaterial;

    [Header("Aura Object")]
    [SerializeField] private string auraObjectName = "AuraShell";
    [SerializeField] private bool createOnStart = true;

    [Header("Cleanup")]
    [Tooltip("If enabled, an existing aura object with the same name is replaced.")]
    [SerializeField] private bool replaceExisting = true;

    private void Start()
    {
        if (createOnStart)
            CreateOrRefreshAura();
    }

    [ContextMenu("Create Or Refresh Aura")]
    public void CreateOrRefreshAura()
    {
        if (sourceRenderer == null)
            sourceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (sourceRenderer == null)
        {
            Debug.LogError("AuraSkinnedSetup: No source SkinnedMeshRenderer found.", this);
            return;
        }

        if (auraMaterial == null)
        {
            Debug.LogError("AuraSkinnedSetup: Aura material is not assigned.", this);
            return;
        }

        var existing = transform.Find(auraObjectName);
        if (existing != null && replaceExisting)
            Destroy(existing.gameObject);

        if (existing != null && !replaceExisting)
        {
            ApplyRendererSettings(existing.GetComponent<SkinnedMeshRenderer>());
            return;
        }

        var auraObject = new GameObject(auraObjectName);
        auraObject.transform.SetParent(transform, false);

        var auraRenderer = auraObject.AddComponent<SkinnedMeshRenderer>();
        auraRenderer.sharedMesh = sourceRenderer.sharedMesh;
        auraRenderer.rootBone = sourceRenderer.rootBone;
        auraRenderer.bones = sourceRenderer.bones;
        auraRenderer.updateWhenOffscreen = sourceRenderer.updateWhenOffscreen;
        auraRenderer.localBounds = sourceRenderer.localBounds;

        ApplyRendererSettings(auraRenderer);
    }

    private void ApplyRendererSettings(SkinnedMeshRenderer auraRenderer)
    {
        if (auraRenderer == null)
            return;

        auraRenderer.sharedMaterial = auraMaterial;
        auraRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        auraRenderer.receiveShadows = false;
        auraRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        auraRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        auraRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }
}
