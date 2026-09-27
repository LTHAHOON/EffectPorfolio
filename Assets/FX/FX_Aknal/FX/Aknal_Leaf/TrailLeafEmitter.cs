using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public sealed class TrailLeafEmitter : MonoBehaviour
{
    [SerializeField] private TrailRenderer sourceTrail;
    [SerializeField, Min(0f)] private float particlesPerSecond = 30f;
    [SerializeField, Range(0f, 1f)] private float widthSpread = 0.35f;

    private ParticleSystem leafParticles;
    private float emissionRemainder;

#if UNITY_EDITOR
    private double lastEditorTime;
#endif

    private void OnEnable()
    {
        leafParticles = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            lastEditorTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += EditorUpdate;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
        emissionRemainder = 0f;
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
            EmitAlongTrail(Time.deltaTime);
    }

    private void ConfigureParticleSystem()
    {
        if (leafParticles == null)
            return;

        var main = leafParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = leafParticles.emission;
        emission.enabled = false;

        if (!leafParticles.isPlaying)
            leafParticles.Play();
    }

    private void EmitAlongTrail(float deltaTime)
    {
        if (sourceTrail == null || leafParticles == null || particlesPerSecond <= 0f)
            return;

        int count = sourceTrail.positionCount;
        if (count < 2)
        {
            emissionRemainder = 0f;
            return;
        }

        Vector3[] points = new Vector3[count];
        count = sourceTrail.GetPositions(points);
        if (count < 2)
            return;

        float totalLength = 0f;
        for (int i = 1; i < count; i++)
            totalLength += Vector3.Distance(points[i - 1], points[i]);

        if (totalLength <= Mathf.Epsilon)
            return;

        emissionRemainder += particlesPerSecond * Mathf.Max(0f, deltaTime);
        int toEmit = Mathf.Min(Mathf.FloorToInt(emissionRemainder), 32);
        emissionRemainder -= toEmit;

        for (int particle = 0; particle < toEmit; particle++)
        {
            float distance = Random.value * totalLength;
            for (int i = 1; i < count; i++)
            {
                Vector3 start = points[i - 1];
                Vector3 end = points[i];
                float segmentLength = Vector3.Distance(start, end);
                if (distance > segmentLength && i < count - 1)
                {
                    distance -= segmentLength;
                    continue;
                }

                Vector3 position = Vector3.Lerp(start, end, distance / Mathf.Max(segmentLength, Mathf.Epsilon));
                position += Random.insideUnitSphere * (sourceTrail.widthMultiplier * widthSpread * 0.5f);
                leafParticles.Emit(new ParticleSystem.EmitParams { position = position }, 1);
                break;
            }
        }
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (this == null || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;

        double now = EditorApplication.timeSinceStartup;
        float deltaTime = Mathf.Clamp((float)(now - lastEditorTime), 0f, 0.1f);
        if (deltaTime < 1f / 30f)
            return;

        lastEditorTime = now;
        if (leafParticles == null || sourceTrail == null)
            return;

        if (sourceTrail.positionCount >= 2 || leafParticles.particleCount > 0)
        {
            leafParticles.Simulate(deltaTime, false, false, false);
            EmitAlongTrail(deltaTime);
            SceneView.RepaintAll();
        }
    }
#endif
}
