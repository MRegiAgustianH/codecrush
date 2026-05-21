using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButtonUI : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Tentukan level berapa tombol ini (1 sampai 10).")]
    public int levelNumber = 1;

    [Header("UI Components")]
    public TextMeshProUGUI levelText;      // Teks angka nomor level
    public GameObject lockIcon;            // Gambar ikon gembok
    public GameObject checkmarkIcon;       // Gambar ikon centang (selesai)

    [Header("Visual States")]
    [Tooltip("Warna tombol saat level terbuka.")]
    public Color unlockedColor = new Color(0.6f, 0.4f, 0.9f, 1f); // Ungu khas Duolingo
    [Tooltip("Warna tombol saat level terkunci.")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);   // Abu-abu gelap

    private Button button;
    private Board board;

    void Awake()
    {
        button = GetComponent<Button>();
        board = FindObjectOfType<Board>();
    }

    void OnEnable()
    {
        UpdateUIState();
    }

    public void UpdateUIState()
    {
        if (board == null)
        {
            board = FindObjectOfType<Board>();
            if (board == null) return;
        }

        bool isUnlocked = board.IsLevelUnlocked(levelNumber);
        bool isCompleted = board.IsLevelCompleted(levelNumber);

        // 1. Atur apakah tombol bisa diklik
        button.interactable = isUnlocked;

        // 2. Atur warna visual tombol
        Image btnImage = GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // 3. Tampilkan angka level hanya jika sudah terbuka
        if (levelText != null)
        {
            levelText.text = levelNumber.ToString();
            levelText.gameObject.SetActive(isUnlocked && !isCompleted); // Hilangkan angka jika sudah selesai/terkunci
        }

        // 4. Atur visibilitas ikon gembok
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // 5. Atur visibilitas ikon centang
        if (checkmarkIcon != null)
        {
            checkmarkIcon.SetActive(isCompleted);
        }
    }

    // Fungsi klik tombol untuk memulai level ini
    public void OnClickStartLevel()
    {
        if (board != null)
        {
            if (board.IsLevelCompleted(levelNumber))
            {
                board.ShowReplayConfirmation(levelNumber);
            }
            else
            {
                board.StartLevel(levelNumber);
            }
        }
    }
}
