using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResponsiveBackground : MonoBehaviour
{
    [Header("Scaling Mode")]
    [Tooltip("Jika true, gambar akan ditarik pas (stretch) sesuai layar. Jika false, aspect ratio gambar dijaga (crop) agar tidak gepeng.")]
    public bool stretchToFit = true;

    private SpriteRenderer spriteRenderer;
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;
    private float lastOrthoSize = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ScaleToFitScreen();
    }

    void Update()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Deteksi jika ukuran layar ATAU ukuran orthographic kamera berubah
        if (Screen.width != lastScreenWidth || 
            Screen.height != lastScreenHeight || 
            !Mathf.Approximately(mainCam.orthographicSize, lastOrthoSize))
        {
            ScaleToFitScreen();
        }
    }

    public void ScaleToFitScreen()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || spriteRenderer == null || spriteRenderer.sprite == null) return;

        // Reset skala sementara untuk mengukur ukuran aslinya
        transform.localScale = Vector3.one;

        // Dapatkan ukuran asli Sprite dalam world unit
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        if (spriteWidth == 0 || spriteHeight == 0) return;

        // Dapatkan tinggi dan lebar layar dalam world unit dari Kamera Orthographic
        float worldScreenHeight = mainCam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight * mainCam.aspect;

        // Hitung skala rasio untuk X dan Y
        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        if (stretchToFit)
        {
            // Menarik gambar pas mengikuti resolusi layar
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        else
        {
            // Menjaga aspect ratio gambar agar tidak gepeng (melakukan crop jika rasio berbeda)
            float maxScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(maxScale, maxScale, 1f);
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastOrthoSize = mainCam.orthographicSize;
    }
}
