using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float lifetime = 0.8f;
    private float elapsed = 0f;
    private Color startColor;
    private Vector3 startPos;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }
    }

    /// <summary>
    /// Inisialisasi floating text dengan teks, warna, dan posisi.
    /// </summary>
    public void Init(string text, Color color, Vector3 position)
    {
        transform.position = position;
        startPos = position;

        textMesh.text = text;
        textMesh.fontSize = 5f;
        textMesh.color = color;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100;
        startColor = color;

        elapsed = 0f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / lifetime;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Naik perlahan
        transform.position = startPos + Vector3.up * (progress * 1.2f);

        // Membesar di awal lalu mengecil
        float scale;
        if (progress < 0.2f)
        {
            scale = Mathf.Lerp(0.5f, 1.2f, progress / 0.2f);
        }
        else
        {
            scale = Mathf.Lerp(1.2f, 0.8f, (progress - 0.2f) / 0.8f);
        }
        transform.localScale = Vector3.one * scale;

        // Fade out
        float alpha = 1f - Mathf.Clamp01((progress - 0.4f) / 0.6f);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    }
}
