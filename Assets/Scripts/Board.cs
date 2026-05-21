using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public GameObject tilePrefab;
    public Sprite[] logoSprites;

    [Header("UI Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI levelInfoText;
    public TextMeshProUGUI comboText;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject gameUIPanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    [Header("Game Settings")]
    public int score = 0;
    public int moves = 20;
    public int targetScore = 500;
    public float timeLeft = 60f;

    private Tile[,] tiles;
    private Tile selectedTile;
    private bool isProcessing = false;
    private bool isGameStarted = false;
    private bool isEndlessMode = false;
    private int currentLevel = 1;
    private int comboCount = 0;
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;
    private Dictionary<string, Vector2> originalButtonPositions = new Dictionary<string, Vector2>();

    void Awake()
    {
        ConfigureCanvasScalers();
        MakePanelResponsive(mainMenuPanel);
        MakePanelResponsive(levelSelectPanel);
        MakePanelResponsive(gameUIPanel);
        MakePanelResponsive(gameOverPanel);
        MakePanelResponsive(levelCompletePanel);
    }

    void Start()
    {
        // Paksa Board berada di (0,0,0) sejajar dengan posisi kamera agar posisi click 100% pas
        transform.position = Vector3.zero;

        ConfigureCamera();
        ShowMainMenu();
        CreateResetButton();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ConfigureCamera();
            ConfigureCanvasScalers();
        }

        // Hotkey reset progress untuk testing di Unity Editor
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllProgress();
        }
        #endif

        if (!isGameStarted) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            UpdateUI();
            GameOver();
            return;
        }

        UpdateUI();
    }

    public void ShowMainMenu()
    {
        isGameStarted = false;

        ClearBoard();

        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        gameUIPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    public void StartEndlessMode()
    {
        isEndlessMode = true;
        currentLevel = 0;

        score = 0;
        moves = 999;
        timeLeft = 30f;
        targetScore = 0;

        StartGame();
    }

    public void StartLevelFromUI(int level)
    {
        StartLevel(level);
    }

    // Fungsi pembantu untuk memanggil Level 1 sampai 10 secara terpusat
    public void StartLevel(int level)
    {
        isEndlessMode = false;
        currentLevel = level;

        // Setelan spesifik untuk 10 Level dengan progresi tingkat kesulitan
        switch (level)
        {
            case 1:
                moves = 20;
                timeLeft = 60f;
                targetScore = 300;
                break;
            case 2:
                moves = 18;
                timeLeft = 55f;
                targetScore = 450;
                break;
            case 3:
                moves = 16;
                timeLeft = 50f;
                targetScore = 600;
                break;
            case 4:
                moves = 15;
                timeLeft = 45f;
                targetScore = 750;
                break;
            case 5:
                moves = 14;
                timeLeft = 45f;
                targetScore = 900;
                break;
            case 6:
                moves = 13;
                timeLeft = 40f;
                targetScore = 1100;
                break;
            case 7:
                moves = 12;
                timeLeft = 40f;
                targetScore = 1300;
                break;
            case 8:
                moves = 12;
                timeLeft = 35f;
                targetScore = 1500;
                break;
            case 9:
                moves = 10;
                timeLeft = 30f;
                targetScore = 1800;
                break;
            case 10:
                moves = 10;
                timeLeft = 30f;
                targetScore = 2200;
                break;
            default:
                // Fallback default jika di luar 1-10
                moves = 15;
                timeLeft = 45f;
                targetScore = 1000;
                break;
        }

        score = 0;
        StartGame();
    }

    // Menampilkan popup konfirmasi ulangi jika level sudah selesai secara responsive & dynamic
    public void ShowReplayConfirmation(int level)
    {
        // Cegah munculnya popup ganda jika popup sudah terbuka!
        if (GameObject.Find("ReplayConfirmOverlay") != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            StartLevel(level);
            return;
        }

        TMP_FontAsset customFont = null;
        Material customMaterial = null;
        if (scoreText != null)
        {
            customFont = scoreText.font;
            customMaterial = scoreText.fontSharedMaterial;
        }

        // Deteksi rasio layar untuk skala ukuran popup yang pas
        float aspect = (float)Screen.width / Screen.height;
        float cardWidth = aspect < 1.0f ? 700f : 600f;
        float cardHeight = aspect < 1.0f ? 400f : 350f;
        float titleFontSize = aspect < 1.0f ? 48f : 36f;
        float descFontSize = aspect < 1.0f ? 34f : 24f;
        float buttonFontSize = aspect < 1.0f ? 34f : 24f;

        // 1. Overlay Background (Blokir input di belakang)
        GameObject overlayObj = new GameObject("ReplayConfirmOverlay");
        overlayObj.transform.SetParent(canvas.transform, false);
        
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f); // Hitam transparan premium

        CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
        overlayGroup.blocksRaycasts = true;

        // 2. Pop-up Card Box
        GameObject cardObj = new GameObject("ConfirmCard");
        cardObj.transform.SetParent(overlayObj.transform, false);
        
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.16f, 1f); // Dark theme premium
        
        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.6f, 0.4f, 0.9f, 0.6f); // Glow ungu Duolingo
        cardOutline.effectDistance = new Vector2(4f, 4f);

        // 3. Judul Pop-up ("Level Selesai")
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.65f);
        titleRect.anchorMax = new Vector2(1f, 0.9f);
        titleRect.offsetMin = new Vector2(20f, 0f);
        titleRect.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            titleText.font = customFont;
            titleText.fontSharedMaterial = customMaterial;
        }
        titleText.text = "Level Selesai";
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        // 4. Deskripsi Pertanyaan ("Ingin Ulangi?")
        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(cardObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0.38f);
        descRect.anchorMax = new Vector2(1f, 0.62f);
        descRect.offsetMin = new Vector2(20f, 0f);
        descRect.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            descText.font = customFont;
            descText.fontSharedMaterial = customMaterial;
        }
        descText.text = "Ingin mainkan kembali level " + level + "?";
        descText.fontSize = descFontSize;
        descText.color = new Color(0.85f, 0.85f, 0.9f, 1f);
        descText.alignment = TextAlignmentOptions.Center;

        // 5. Tombol Ulangi (Play Again)
        GameObject playBtnObj = new GameObject("ReplayPlayButton");
        playBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform playBtnRect = playBtnObj.AddComponent<RectTransform>();
        playBtnRect.anchorMin = new Vector2(0.08f, 0.08f);
        playBtnRect.anchorMax = new Vector2(0.48f, 0.3f);
        playBtnRect.offsetMin = Vector2.zero;
        playBtnRect.offsetMax = Vector2.zero;

        Image playBtnImg = playBtnObj.AddComponent<Image>();
        playBtnImg.color = new Color(0.6f, 0.4f, 0.9f, 1f); // Ungu Duolingo
        
        Button playBtn = playBtnObj.AddComponent<Button>();
        
        GameObject playTextObj = new GameObject("PlayText");
        playTextObj.transform.SetParent(playBtnObj.transform, false);
        RectTransform playTextRect = playTextObj.AddComponent<RectTransform>();
        playTextRect.anchorMin = Vector2.zero;
        playTextRect.anchorMax = Vector2.one;
        playTextRect.offsetMin = Vector2.zero;
        playTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI playText = playTextObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            playText.font = customFont;
            playText.fontSharedMaterial = customMaterial;
        }
        playText.text = "Ulangi";
        playText.fontSize = buttonFontSize;
        playText.fontStyle = FontStyles.Bold;
        playText.color = Color.white;
        playText.alignment = TextAlignmentOptions.Center;

        playBtn.onClick.AddListener(() => {
            Destroy(overlayObj);
            StartLevel(level);
        });

        // 6. Tombol Batal (Cancel)
        GameObject cancelBtnObj = new GameObject("ReplayCancelButton");
        cancelBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform cancelBtnRect = cancelBtnObj.AddComponent<RectTransform>();
        cancelBtnRect.anchorMin = new Vector2(0.52f, 0.08f);
        cancelBtnRect.anchorMax = new Vector2(0.92f, 0.3f);
        cancelBtnRect.offsetMin = Vector2.zero;
        cancelBtnRect.offsetMax = Vector2.zero;

        Image cancelBtnImg = cancelBtnObj.AddComponent<Image>();
        cancelBtnImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        
        Button cancelBtn = cancelBtnObj.AddComponent<Button>();
        
        GameObject cancelTextObj = new GameObject("CancelText");
        cancelTextObj.transform.SetParent(cancelBtnObj.transform, false);
        RectTransform cancelTextRect = cancelTextObj.AddComponent<RectTransform>();
        cancelTextRect.anchorMin = Vector2.zero;
        cancelTextRect.anchorMax = Vector2.one;
        cancelTextRect.offsetMin = Vector2.zero;
        cancelTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            cancelText.font = customFont;
            cancelText.fontSharedMaterial = customMaterial;
        }
        cancelText.text = "Batal";
        cancelText.fontSize = buttonFontSize;
        cancelText.fontStyle = FontStyles.Bold;
        cancelText.color = Color.white;
        cancelText.alignment = TextAlignmentOptions.Center;

        cancelBtn.onClick.AddListener(() => {
            Destroy(overlayObj);
        });

        // Animasi masuk scale yang halus (Pop-up Effect)
        cardObj.transform.localScale = Vector3.zero;
        StartCoroutine(AnimateCardScaleUp(cardObj.transform));
    }

    IEnumerator AnimateCardScaleUp(Transform cardTransform)
    {
        float elapsed = 0f;
        float duration = 0.18f;
        while (elapsed < duration)
        {
            if (cardTransform == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(0f, 1f, t * t * (3f - 2f * t)); // Elastic Ease Out
            cardTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (cardTransform != null)
            cardTransform.localScale = Vector3.one;
    }

    // Fungsi pembantu untuk mengecek apakah suatu level sudah diselesaikan
    public bool IsLevelCompleted(int levelNum)
    {
        return PlayerPrefs.GetInt("LevelCompleted_" + levelNum, 0) == 1;
    }

    // Fungsi pembantu untuk mengecek apakah suatu level terbuka (unlocked)
    public bool IsLevelUnlocked(int levelNum)
    {
        // Level 1 selalu terbuka
        if (levelNum == 1) return true;
        
        // Level lainnya terbuka jika level sebelumnya sudah selesai
        return IsLevelCompleted(levelNum - 1);
    }

    void StartGame()
    {
        ClearBoard();

        tiles = new Tile[width, height];
        GenerateBoard();

        isGameStarted = true;
        selectedTile = null;
        isProcessing = false;

        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);

        UpdateUI();
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        isProcessing = false;

        if (isEndlessMode)
        {
            StartEndlessMode();
        }
        else
        {
            StartLevel(currentLevel);
        }
    }

    public void BackToHome()
    {
        StopAllCoroutines();
        isProcessing = false;
        ShowMainMenu();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;

        if (isEndlessMode)
            moveText.text = "Moves: ∞";
        else
            moveText.text = "Moves: " + moves;

        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);

        if (levelInfoText != null)
        {
            if (isEndlessMode)
                levelInfoText.text = "Endless Mode";
            else
                levelInfoText.text = "Level " + currentLevel + " / Target: " + targetScore;
        }

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + highScore;
        }
    }

    void GameOver()
    {
        if (!isGameStarted) return;

        isGameStarted = false;
        isProcessing = false;

        SaveHighScore();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayGameOver();

        gameOverPanel.SetActive(true);
        gameUIPanel.SetActive(true);

        // Otomatis kembali ke main menu setelah 3 detik
        StartCoroutine(AutoReturnToMenu(3f));
    }

    IEnumerator AutoReturnToMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowMainMenu();
    }

    void LevelComplete()
    {
        if (!isGameStarted) return;

        isGameStarted = false;
        isProcessing = false;

        SaveHighScore();

        // Simpan progress level yang berhasil diselesaikan di PlayerPrefs
        PlayerPrefs.SetInt("LevelCompleted_" + currentLevel, 1);
        PlayerPrefs.Save();

        // Buat dan tampilkan tombol Next Level secara dinamis dan responsif
        CreateNextLevelButton();

        levelCompletePanel.SetActive(true);
        gameUIPanel.SetActive(true);
    }

    void SaveHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }
    }

    void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y, true);
            }
        }
    }

    void CreateTile(int x, int y, bool avoidMatches = false)
    {
        int type;

        if (avoidMatches)
        {
            List<int> availableTypes = new List<int>();
            for (int i = 0; i < logoSprites.Length; i++)
                availableTypes.Add(i);

            // Hindari match horizontal (cek 2 tile di sebelah kiri)
            if (x >= 2 && tiles[x - 1, y] != null && tiles[x - 2, y] != null
                && tiles[x - 1, y].type == tiles[x - 2, y].type)
            {
                availableTypes.Remove(tiles[x - 1, y].type);
            }

            // Hindari match vertikal (cek 2 tile di bawah)
            if (y >= 2 && tiles[x, y - 1] != null && tiles[x, y - 2] != null
                && tiles[x, y - 1].type == tiles[x, y - 2].type)
            {
                availableTypes.Remove(tiles[x, y - 1].type);
            }

            if (availableTypes.Count == 0)
                type = Random.Range(0, logoSprites.Length);
            else
                type = availableTypes[Random.Range(0, availableTypes.Count)];
        }
        else
        {
            type = Random.Range(0, logoSprites.Length);
        }

        // Hitung posisi agar grid berada di tengah Board
        float offsetX = (width - 1) / 2f;
        float offsetY = (height - 1) / 2f;

        Vector2 localPos = new Vector2(x - offsetX, y - offsetY);

        // Instantiate sebagai child dari Board
        GameObject obj = Instantiate(tilePrefab, transform);

        // Set local position (bukan world position)
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * 0.8f;

        Tile tile = obj.GetComponent<Tile>();
        tile.Init(x, y, type, logoSprites[type], this);

        tiles[x, y] = tile;
    }

    public void SelectTile(Tile tile)
    {
        if (!isGameStarted) return;
        if (isProcessing) return;
        if (!isEndlessMode && moves <= 0) return;

        if (selectedTile == null)
        {
            selectedTile = tile;
            tile.transform.localScale = Vector3.one * 1.15f;
            return;
        }

        if (selectedTile == tile)
        {
            selectedTile.transform.localScale = Vector3.one * 0.8f;
            selectedTile = null;
            return;
        }

        if (AreNeighbors(selectedTile, tile))
        {
            StartCoroutine(TrySwap(selectedTile, tile));
            selectedTile.transform.localScale = Vector3.one * 0.8f;
            selectedTile = null;
        }
        else
        {
            selectedTile.transform.localScale = Vector3.one * 0.8f;
            selectedTile = tile;
            selectedTile.transform.localScale = Vector3.one * 1.15f;
        }
    }

    bool AreNeighbors(Tile a, Tile b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    IEnumerator TrySwap(Tile a, Tile b)
    {
        isProcessing = true;

        // SFX swap
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySwap();

        yield return StartCoroutine(SwapTilesAnimated(a, b));

        List<Tile> matches = FindMatches();

        if (matches.Count == 0)
        {
            // Swap gagal, kembalikan posisi dengan animasi
            yield return StartCoroutine(SwapTilesAnimated(a, b));
        }
        else
        {
            if (!isEndlessMode)
            {
                moves--;
            }

            UpdateUI();

            yield return StartCoroutine(ClearAndRefillBoard());

            // Sembunyikan combo text setelah cascade selesai
            if (comboText != null)
                comboText.gameObject.SetActive(false);

            if (!isEndlessMode && score >= targetScore)
            {
                LevelComplete();
            }
            else if (!isEndlessMode && moves <= 0)
            {
                GameOver();
            }
        }

        isProcessing = false;
    }

    IEnumerator SwapTilesAnimated(Tile a, Tile b)
    {
        // Tukar data di array
        tiles[a.x, a.y] = b;
        tiles[b.x, b.y] = a;

        int tempX = a.x;
        int tempY = a.y;

        a.x = b.x;
        a.y = b.y;

        b.x = tempX;
        b.y = tempY;

        // Animasikan perpindahan posisi secara mulus
        Vector3 posA = a.transform.localPosition;
        Vector3 posB = b.transform.localPosition;

        StartCoroutine(AnimateMoveTo(a.transform, posB, 0.2f));
        StartCoroutine(AnimateMoveTo(b.transform, posA, 0.2f));

        yield return new WaitForSeconds(0.2f);
    }

    List<Tile> FindMatches()
    {
        List<Tile> matchedTiles = new List<Tile>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Tile a = tiles[x, y];
                Tile b = tiles[x + 1, y];
                Tile c = tiles[x + 2, y];

                if (a != null && b != null && c != null)
                {
                    if (a.type == b.type && b.type == c.type)
                    {
                        AddMatch(matchedTiles, a);
                        AddMatch(matchedTiles, b);
                        AddMatch(matchedTiles, c);
                    }
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Tile a = tiles[x, y];
                Tile b = tiles[x, y + 1];
                Tile c = tiles[x, y + 2];

                if (a != null && b != null && c != null)
                {
                    if (a.type == b.type && b.type == c.type)
                    {
                        AddMatch(matchedTiles, a);
                        AddMatch(matchedTiles, b);
                        AddMatch(matchedTiles, c);
                    }
                }
            }
        }

        return matchedTiles;
    }

    void AddMatch(List<Tile> list, Tile tile)
    {
        if (!list.Contains(tile))
        {
            list.Add(tile);
        }
    }

    IEnumerator ClearAndRefillBoard()
    {
        bool needsCheck = true;
        int safetyCounter = 0;
        comboCount = 0;

        while (needsCheck && safetyCounter < 100)
        {
            safetyCounter++;

            // Bersihkan semua match secara beruntun (cascade)
            while (true)
            {
                List<Tile> matches = FindMatches();

                if (matches.Count == 0)
                    break;

                comboCount++;
                int scoreGained = matches.Count * 10 * comboCount;
                score += scoreGained;

                // Bonus waktu +2 detik per match di Endless Mode
                if (isEndlessMode)
                {
                    timeLeft += 2f;
                }

                // Tampilkan combo text jika combo >= 2
                if (comboCount >= 2 && comboText != null)
                {
                    comboText.gameObject.SetActive(true);
                    comboText.text = "COMBO x" + comboCount + "!";

                    if (SFXManager.Instance != null)
                        SFXManager.Instance.PlayCombo(comboCount);
                }
                else
                {
                    if (SFXManager.Instance != null)
                        SFXManager.Instance.PlayMatch();
                }

                UpdateUI();

                // Hitung posisi tengah dari semua match untuk floating text
                Vector3 centerPos = Vector3.zero;
                foreach (Tile tile in matches)
                {
                    centerPos += tile.transform.position;
                }
                centerPos /= matches.Count;

                // Spawn floating text
                SpawnFloatingText("+" + scoreGained, centerPos, matches[0].type);

                // Hancurkan tile dengan efek partikel
                foreach (Tile tile in matches)
                {
                    // Efek partikel
                    if (ParticleEffectManager.Instance != null)
                    {
                        Color particleColor = ParticleEffectManager.GetColorForType(tile.type);
                        ParticleEffectManager.Instance.PlayMatchEffect(tile.transform.position, particleColor);
                    }

                    tiles[tile.x, tile.y] = null;
                    Destroy(tile.gameObject);
                }

                yield return new WaitForSeconds(0.2f);

                yield return StartCoroutine(CollapseTiles());

                RefillBoard();

                yield return new WaitForSeconds(0.2f);
            }

            // Deteksi kebuntuan (deadlock)
            if (!HasValidMoves())
            {
                Debug.Log("Tidak ada langkah valid! Mengacak ulang papan...");
                ShuffleBoard();
                yield return new WaitForSeconds(0.3f);
                // Loop kembali untuk memeriksa match dari pengacakan
            }
            else
            {
                needsCheck = false;
            }
        }
    }

    IEnumerator CollapseTiles()
    {
        float offsetX = (width - 1) / 2f;
        float offsetY = (height - 1) / 2f;
        bool hasMoved = false;

        for (int x = 0; x < width; x++)
        {
            int emptyY = 0;

            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    Tile tile = tiles[x, y];

                    tiles[x, emptyY] = tile;

                    if (emptyY != y)
                    {
                        tiles[x, y] = null;
                        tile.y = emptyY;

                        Vector2 targetPos = new Vector2(x - offsetX, emptyY - offsetY);
                        StartCoroutine(AnimateMoveTo(tile.transform, targetPos, 0.15f));
                        hasMoved = true;
                    }

                    emptyY++;
                }
            }
        }

        if (hasMoved)
            yield return new WaitForSeconds(0.15f);
    }

    void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                {
                    CreateTile(x, y);
                }
            }
        }
    }

    IEnumerator AnimateMoveTo(Transform target, Vector3 destination, float duration)
    {
        Vector3 startPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // SmoothStep untuk easing yang mulus
            t = t * t * (3f - 2f * t);
            target.localPosition = Vector3.Lerp(startPos, destination, t);
            yield return null;
        }

        if (target != null)
            target.localPosition = destination;
    }

    void SwapTilesData(int x1, int y1, int x2, int y2)
    {
        Tile temp = tiles[x1, y1];
        tiles[x1, y1] = tiles[x2, y2];
        tiles[x2, y2] = temp;

        if (tiles[x1, y1] != null) { tiles[x1, y1].x = x1; tiles[x1, y1].y = y1; }
        if (tiles[x2, y2] != null) { tiles[x2, y2].x = x2; tiles[x2, y2].y = y2; }
    }

    bool HasValidMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null) continue;

                // Cek swap ke kanan
                if (x < width - 1 && tiles[x + 1, y] != null)
                {
                    SwapTilesData(x, y, x + 1, y);
                    bool hasMatch = FindMatches().Count > 0;
                    SwapTilesData(x, y, x + 1, y);
                    if (hasMatch) return true;
                }

                // Cek swap ke atas
                if (y < height - 1 && tiles[x, y + 1] != null)
                {
                    SwapTilesData(x, y, x, y + 1);
                    bool hasMatch = FindMatches().Count > 0;
                    SwapTilesData(x, y, x, y + 1);
                    if (hasMatch) return true;
                }
            }
        }
        return false;
    }

    void ShuffleBoard()
    {
        List<Tile> allTiles = new List<Tile>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (tiles[x, y] != null)
                    allTiles.Add(tiles[x, y]);

        // Fisher-Yates shuffle
        for (int i = allTiles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Tile temp = allTiles[i];
            allTiles[i] = allTiles[j];
            allTiles[j] = temp;
        }

        float offsetX = (width - 1) / 2f;
        float offsetY = (height - 1) / 2f;

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = allTiles[index];
                tiles[x, y] = tile;
                tile.x = x;
                tile.y = y;
                tile.transform.localPosition = new Vector2(x - offsetX, y - offsetY);
                index++;
            }
        }
    }

    void SpawnFloatingText(string text, Vector3 position, int tileType)
    {
        GameObject floatingObj = new GameObject("FloatingText");
        TextMeshPro tmp = floatingObj.AddComponent<TextMeshPro>();
        FloatingText ft = floatingObj.AddComponent<FloatingText>();

        Color textColor = ParticleEffectManager.GetColorForType(tileType);
        ft.Init(text, textColor, position);
    }

    void ClearBoard()
    {
        if (tiles == null) return;

        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        tiles = null;
    }

    void ConfigureCanvasScalers()
    {
        // Temukan semua Canvas di dalam scene dan paksa menggunakan ScreenSpaceOverlay
        // agar koordinat klik UI 100% presisi dengan kursor mouse/touch
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        // Temukan semua CanvasScaler di dalam scene
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (var scaler in scalers)
        {
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                // Deteksi rasio layar untuk mengatur orientasi UI dinamis
                float aspect = (float)Screen.width / Screen.height;
                if (aspect < 1.0f) // Portrait (HP/Mobile)
                {
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0f; // Match width agar UI tidak terpotong secara horizontal
                }
                else // Landscape (Desktop/WebGL)
                {
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 1f; // Match height agar UI terisi vertikal penuh
                }
            }
        }
    }

    void MakePanelResponsive(GameObject panel)
    {
        if (panel == null) return;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        // Nonaktifkan raycastTarget pada background Image gameUIPanel
        // agar tidak memblokir input klik pada tile permainan di belakangnya
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg != null && panel == gameUIPanel)
        {
            panelImg.raycastTarget = false;
        }

        // Enforce stretch-stretch juga pada ScrollRect (Scroll View) di bawah panel ini
        ScrollRect scrollRect = panel.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            if (scrollRectTransform != null)
            {
                scrollRectTransform.anchorMin = new Vector2(0f, 0f);
                scrollRectTransform.anchorMax = new Vector2(1f, 1f);
                scrollRectTransform.offsetMin = new Vector2(0f, 0f);
                scrollRectTransform.offsetMax = new Vector2(0f, 0f);
                scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }
    }

    void ConfigureCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Pastikan kamera bernilai orthographic
        mainCam.orthographic = true;

        float aspect = (float)Screen.width / Screen.height;

        // Tambahkan ruang horizontal & vertikal ekstra untuk padding dan UI
        float targetWidth = width + 2f; 
        float targetHeight = height + 4.5f; // Ruang ekstra vertikal untuk UI (skor, timer, level info)

        float sizeForHeight = targetHeight / 2f;
        float sizeForWidth = targetWidth / (2f * aspect);

        // Pilih ukuran terbesar agar seluruh board dan padding UI tetap masuk layar
        mainCam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

        // Pastikan posisi kamera berada di tengah board (0, 0)
        mainCam.transform.position = new Vector3(0, 0, -10);

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    // --- FITUR RESET PROGRESS DYNAMIC & RESPONSIVE ---

    private void CreateResetButton()
    {
        if (levelSelectPanel == null) return;

        // Cari apakah sudah ada tombol Reset di levelSelectPanel
        if (levelSelectPanel.transform.Find("ResetProgressButton") != null) return;

        // Ambil font & material dari scoreText agar seragam
        TMP_FontAsset customFont = null;
        Material customMaterial = null;
        if (scoreText != null)
        {
            customFont = scoreText.font;
            customMaterial = scoreText.fontSharedMaterial;
        }

        // Deteksi rasio layar untuk skala ukuran tombol yang pas
        float aspect = (float)Screen.width / Screen.height;
        float btnWidth = aspect < 1.0f ? 280f : 240f;
        float btnHeight = aspect < 1.0f ? 80f : 70f;
        float fontSize = aspect < 1.0f ? 30f : 22f;

        // 1. Buat Game Object Tombol
        GameObject btnObj = new GameObject("ResetProgressButton");
        btnObj.transform.SetParent(levelSelectPanel.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        // Posisikan di pojok kanan atas dengan anchor
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-40f, -40f); // margin dari pojok kanan atas
        rect.sizeDelta = new Vector2(btnWidth, btnHeight);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.18f, 0.22f, 0.9f); // Dark background premium

        // Outline merah halus untuk tombol reset
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.3f, 0.3f, 0.4f);
        outline.effectDistance = new Vector2(2f, 2f);

        Button button = btnObj.AddComponent<Button>();
        
        // Atur efek transisi warna tombol
        ColorBlock cb = button.colors;
        cb.normalColor = new Color(0.18f, 0.18f, 0.22f, 0.9f);
        cb.highlightedColor = new Color(0.25f, 0.2f, 0.25f, 1f);
        cb.pressedColor = new Color(0.12f, 0.12f, 0.15f, 1f);
        button.colors = cb;

        // 2. Buat Teks Tombol
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            textMesh.font = customFont;
            textMesh.fontSharedMaterial = customMaterial;
        }
        textMesh.text = "Reset Progress";
        textMesh.fontSize = fontSize;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.color = new Color(0.95f, 0.4f, 0.4f, 1f); // Warna teks merah soft premium
        textMesh.alignment = TextAlignmentOptions.Center;

        // 3. Tambahkan Event Listener
        button.onClick.AddListener(ShowResetConfirmationPopup);
    }

    public void ShowResetConfirmationPopup()
    {
        // Cegah munculnya popup ganda jika popup sudah terbuka!
        if (GameObject.Find("ResetConfirmOverlay") != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            ResetAllProgress();
            return;
        }

        TMP_FontAsset customFont = null;
        Material customMaterial = null;
        if (scoreText != null)
        {
            customFont = scoreText.font;
            customMaterial = scoreText.fontSharedMaterial;
        }

        // Deteksi rasio layar untuk skala ukuran popup yang pas
        float aspect = (float)Screen.width / Screen.height;
        float cardWidth = aspect < 1.0f ? 750f : 650f;
        float cardHeight = aspect < 1.0f ? 450f : 380f;
        float titleFontSize = aspect < 1.0f ? 46f : 36f;
        float descFontSize = aspect < 1.0f ? 32f : 22f;
        float buttonFontSize = aspect < 1.0f ? 32f : 22f;

        // 1. Overlay Background (Blokir input di belakang)
        GameObject overlayObj = new GameObject("ResetConfirmOverlay");
        overlayObj.transform.SetParent(canvas.transform, false);
        
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.8f); // Hitam transparan premium pekat

        CanvasGroup overlayGroup = overlayObj.AddComponent<CanvasGroup>();
        overlayGroup.blocksRaycasts = true;

        // 2. Pop-up Card Box
        GameObject cardObj = new GameObject("ResetConfirmCard");
        cardObj.transform.SetParent(overlayObj.transform, false);
        
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.16f, 1f); // Dark theme premium
        
        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.9f, 0.3f, 0.3f, 0.6f); // Glow merah soft (peringatan)
        cardOutline.effectDistance = new Vector2(4f, 4f);

        // 3. Judul Pop-up ("Reset Progress?")
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.7f);
        titleRect.anchorMax = new Vector2(1f, 0.95f);
        titleRect.offsetMin = new Vector2(20f, 0f);
        titleRect.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            titleText.font = customFont;
            titleText.fontSharedMaterial = customMaterial;
        }
        titleText.text = "Reset Progress?";
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.95f, 0.35f, 0.35f, 1f); // Merah warning
        titleText.alignment = TextAlignmentOptions.Center;

        // 4. Deskripsi Pertanyaan
        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(cardObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0.35f);
        descRect.anchorMax = new Vector2(1f, 0.65f);
        descRect.offsetMin = new Vector2(30f, 0f);
        descRect.offsetMax = new Vector2(-30f, 0f);

        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            descText.font = customFont;
            descText.fontSharedMaterial = customMaterial;
        }
        descText.text = "Semua data level dan High Score akan dihapus secara permanen. Anda harus mengulang dari Level 1. Lanjutkan?";
        descText.fontSize = descFontSize;
        descText.color = new Color(0.85f, 0.85f, 0.9f, 1f);
        descText.alignment = TextAlignmentOptions.Center;

        // 5. Tombol Reset (Konfirmasi)
        GameObject resetBtnObj = new GameObject("ConfirmResetButton");
        resetBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform resetBtnRect = resetBtnObj.AddComponent<RectTransform>();
        resetBtnRect.anchorMin = new Vector2(0.08f, 0.08f);
        resetBtnRect.anchorMax = new Vector2(0.48f, 0.28f);
        resetBtnRect.offsetMin = Vector2.zero;
        resetBtnRect.offsetMax = Vector2.zero;

        Image resetBtnImg = resetBtnObj.AddComponent<Image>();
        resetBtnImg.color = new Color(0.85f, 0.3f, 0.3f, 1f); // Merah terang premium
        
        Button resetBtn = resetBtnObj.AddComponent<Button>();
        
        GameObject resetTextObj = new GameObject("ResetText");
        resetTextObj.transform.SetParent(resetBtnObj.transform, false);
        RectTransform resetTextRect = resetTextObj.AddComponent<RectTransform>();
        resetTextRect.anchorMin = Vector2.zero;
        resetTextRect.anchorMax = Vector2.one;
        resetTextRect.offsetMin = Vector2.zero;
        resetTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI resetText = resetTextObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            resetText.font = customFont;
            resetText.fontSharedMaterial = customMaterial;
        }
        resetText.text = "Reset";
        resetText.fontSize = buttonFontSize;
        resetText.fontStyle = FontStyles.Bold;
        resetText.color = Color.white;
        resetText.alignment = TextAlignmentOptions.Center;

        resetBtn.onClick.AddListener(() => {
            Destroy(overlayObj);
            ResetAllProgress();
        });

        // 6. Tombol Batal (Cancel)
        GameObject cancelBtnObj = new GameObject("CancelResetButton");
        cancelBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform cancelBtnRect = cancelBtnObj.AddComponent<RectTransform>();
        cancelBtnRect.anchorMin = new Vector2(0.52f, 0.08f);
        cancelBtnRect.anchorMax = new Vector2(0.92f, 0.28f);
        cancelBtnRect.offsetMin = Vector2.zero;
        cancelBtnRect.offsetMax = Vector2.zero;

        Image cancelBtnImg = cancelBtnObj.AddComponent<Image>();
        cancelBtnImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        
        Button cancelBtn = cancelBtnObj.AddComponent<Button>();
        
        GameObject cancelTextObj = new GameObject("CancelText");
        cancelTextObj.transform.SetParent(cancelBtnObj.transform, false);
        RectTransform cancelTextRect = cancelTextObj.AddComponent<RectTransform>();
        cancelTextRect.anchorMin = Vector2.zero;
        cancelTextRect.anchorMax = Vector2.one;
        cancelTextRect.offsetMin = Vector2.zero;
        cancelTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
        if (customFont != null)
        {
            cancelText.font = customFont;
            cancelText.fontSharedMaterial = customMaterial;
        }
        cancelText.text = "Batal";
        cancelText.fontSize = buttonFontSize;
        cancelText.fontStyle = FontStyles.Bold;
        cancelText.color = Color.white;
        cancelText.alignment = TextAlignmentOptions.Center;

        cancelBtn.onClick.AddListener(() => {
            Destroy(overlayObj);
        });

        // Animasi masuk scale yang halus (Pop-up Effect)
        cardObj.transform.localScale = Vector3.zero;
        StartCoroutine(AnimateCardScaleUp(cardObj.transform));
    }

    public void ResetAllProgress()
    {
        // Hapus semua data progress level 1 sampai 10
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);
        }
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();

        // Refresh status visual semua Level Button UI
        LevelButtonUI[] levelButtons = FindObjectsOfType<LevelButtonUI>(true);
        foreach (LevelButtonUI btn in levelButtons)
        {
            if (btn != null)
            {
                btn.UpdateUIState();
            }
        }

        // Refresh UI skor
        UpdateUI();

        Debug.Log("Semua progress level berhasil direset!");
    }

    // --- FITUR TOMBOL NEXT LEVEL PROGRAMMATIC & DYNAMIC ---

    private void CreateNextLevelButton()
    {
        if (levelCompletePanel == null) return;
        
        // Hanya buat jika level saat ini kurang dari 10 (karena level 10 adalah level terakhir)
        if (currentLevel >= 10)
        {
            Transform existing = levelCompletePanel.transform.Find("NextLevelButton");
            if (existing != null) existing.gameObject.SetActive(false);
            return;
        }

        // Cari atau buat tombol NextLevelButton
        Transform btnTransform = levelCompletePanel.transform.Find("NextLevelButton");
        GameObject btnObj;
        Button nextBtn;

        if (btnTransform == null)
        {
            btnObj = new GameObject("NextLevelButton");
            btnObj.transform.SetParent(levelCompletePanel.transform, false);
            nextBtn = btnObj.AddComponent<Button>();
        }
        else
        {
            btnObj = btnTransform.gameObject;
            btnObj.SetActive(true);
            nextBtn = btnObj.GetComponent<Button>();
        }

        // Ambil font & material dari scoreText agar seragam
        TMP_FontAsset customFont = null;
        Material customMaterial = null;
        if (scoreText != null)
        {
            customFont = scoreText.font;
            customMaterial = scoreText.fontSharedMaterial;
        }

        // Atur posisi & ukuran tombol Next Level secara dinamis & premium
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        if (rect == null) rect = btnObj.AddComponent<RectTransform>();

        // Ambil info rasio layar
        float aspect = (float)Screen.width / Screen.height;
        float btnWidth = aspect < 1.0f ? 360f : 300f;
        float btnHeight = aspect < 1.0f ? 100f : 80f;
        float fontSize = aspect < 1.0f ? 36f : 26f;

        // Posisikan tombol Next Level sebagai aksi utama yang menonjol di tengah-bawah panel
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -100f); // Geser sedikit ke bawah (-100f) agar ada jarak dari teks LEVEL COMPLETE
        rect.sizeDelta = new Vector2(btnWidth, btnHeight);

        // Visual: Hijau Duolingo premium yang kontras & menarik perhatian pemain untuk lanjut
        Image btnImg = btnObj.GetComponent<Image>();
        if (btnImg == null) btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.35f, 0.75f, 0.35f, 1f); // Hijau cerah premium

        Outline outline = btnObj.GetComponent<Outline>();
        if (outline == null) outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.5f, 0.2f, 0.5f);
        outline.effectDistance = new Vector2(3f, -3f);

        // Atur transisi warna tombol
        ColorBlock cb = nextBtn.colors;
        cb.normalColor = new Color(0.35f, 0.75f, 0.35f, 1f);
        cb.highlightedColor = new Color(0.4f, 0.85f, 0.4f, 1f);
        cb.pressedColor = new Color(0.28f, 0.65f, 0.28f, 1f);
        nextBtn.colors = cb;

        // Buat atau update Teks Tombol
        Transform textTrans = btnObj.transform.Find("ButtonText");
        GameObject textObj;
        TextMeshProUGUI textMesh;

        if (textTrans == null)
        {
            textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(btnObj.transform, false);
            textMesh = textObj.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            textObj = textTrans.gameObject;
            textMesh = textObj.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null) textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        if (customFont != null)
        {
            textMesh.font = customFont;
            textMesh.fontSharedMaterial = customMaterial;
        }
        textMesh.text = "Level Berikutnya";
        textMesh.fontSize = fontSize;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;

        // Tambahkan Event Listener
        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(() => {
            // Sembunyikan panel selesai
            levelCompletePanel.SetActive(false);
            // Mulai level berikutnya!
            StartLevel(currentLevel + 1);
        });

        // Reposisi otomatis tombol Restart & Home agar layout terlihat rapi & seimbang!
        RepositionCompletePanelButtons(rect.anchoredPosition.y, btnHeight);
    }

    private void RepositionCompletePanelButtons(float nextBtnY, float nextBtnHeight)
    {
        // Temukan semua Button anak dari levelCompletePanel yang bukan NextLevelButton
        Button[] buttons = levelCompletePanel.GetComponentsInChildren<Button>(true);

        // Simpan posisi asli tombol jika belum ada di dictionary cache
        foreach (Button btn in buttons)
        {
            if (btn.name != "NextLevelButton")
            {
                RectTransform r = btn.GetComponent<RectTransform>();
                if (r != null && !originalButtonPositions.ContainsKey(btn.name))
                {
                    originalButtonPositions[btn.name] = r.anchoredPosition;
                }
            }
        }

        // Kembalikan ke posisi asli terlebih dahulu, lalu geser ke bawah secara vertikal (sumbu Y)
        // Hal ini menjaga posisi horizontal (sumbu X), lebar, dan tinggi tombol tetap seperti semula di editor
        foreach (Button btn in buttons)
        {
            if (btn.name != "NextLevelButton")
            {
                RectTransform r = btn.GetComponent<RectTransform>();
                if (r != null && originalButtonPositions.ContainsKey(btn.name))
                {
                    Vector2 originalPos = originalButtonPositions[btn.name];
                    // Geser ke bawah sejauh 140 unit dari posisi aslinya agar memberikan ruang
                    // yang pas dan rapi tanpa menimpa atau menempel tombol Next Level
                    r.anchoredPosition = new Vector2(originalPos.x, originalPos.y - 140f);
                }
            }
        }
    }
}