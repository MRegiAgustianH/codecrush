using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    public static ParticleEffectManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Memunculkan efek partikel burst di posisi tertentu dengan warna yang ditentukan.
    /// </summary>
    public void PlayMatchEffect(Vector3 position, Color color)
    {
        GameObject particleObj = new GameObject("MatchParticle");
        particleObj.transform.position = position;

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main module
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = color;
        main.gravityModifier = 1.5f;
        main.maxParticles = 20;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission module — burst sekali
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 12, 18)
        });

        // Shape module — lingkaran kecil
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        // Size over lifetime — mengecil perlahan
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime — fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Renderer — gunakan default sprite
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = color;

        ps.Play();

        // Auto-destroy setelah selesai
        Destroy(particleObj, main.duration + main.startLifetime.constantMax + 0.1f);
    }

    /// <summary>
    /// Warna berdasarkan tipe ubin (index logo).
    /// </summary>
    public static Color GetColorForType(int type)
    {
        switch (type)
        {
            case 0: return new Color(0.56f, 0.27f, 0.68f); // C# — ungu
            case 1: return new Color(0.0f, 0.47f, 0.72f);  // C++ — biru
            case 2: return new Color(1.0f, 0.32f, 0.30f);  // Laravel — merah
            case 3: return new Color(0.40f, 0.73f, 0.24f);  // Node.js — hijau
            case 4: return new Color(1.0f, 0.84f, 0.24f);  // Python — kuning
            case 5: return new Color(0.38f, 0.85f, 0.95f);  // React — cyan
            default: return Color.white;
        }
    }
}
