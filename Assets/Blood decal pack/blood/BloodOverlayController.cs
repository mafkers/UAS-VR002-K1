// BloodOverlayController.cs (Versi Final Hibrida - Text & TMP)
using UnityEngine;
using UnityEngine.UI; // <-- Digunakan untuk HealthText
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // <-- Digunakan untuk NotificationText

public class BloodOverlayController : MonoBehaviour
{
    [Header("Referensi Utama (WAJIB DIISI)")]
    public Health playerHealth;
    public Image healthBarFill;
    public Text healthText;         // <-- Menggunakan Text biasa
    public GameObject gameOverPanel;
    public TMP_Text notificationText;   // <-- Tetap menggunakan TextMeshPro

    [Header("Blood Effect Images")]
    public Image bloodLight;
    public Image bloodMedium;
    public Image bloodHeavy1;
    public Image bloodHeavy2;

    private Coroutine notificationCoroutine;
    private bool isGameOver = false;

    void Start()
    {
        // Cari Health Player
        Health[] allHealths = FindObjectsOfType<Health>();
        foreach (Health h in allHealths)
        {
            if (h.isPlayer)
            {
                playerHealth = h;
                break;
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("Referensi 'playerHealth' di BloodOverlayController belum diatur di Inspector!", this.gameObject);
        }

        // Nonaktifkan panel dan teks di awal
        if(gameOverPanel) gameOverPanel.SetActive(false);
        if(notificationText) notificationText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowNotification("Tidak bisa keluar saat permainan berlangsung!", 3f);
        }
        
        if (playerHealth == null || isGameOver) return;

        UpdateBloodEffect();
        UpdateHealthBar();

        if (playerHealth.currentHealth <= 0)
        {
            ShowGameOverScreen();
        }
    }

    public void ShowNotification(string message, float duration)
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, duration));
    }

    private IEnumerator ShowNotificationCoroutine(string message, float duration)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            notificationText.gameObject.SetActive(false);
        }
    }
    
    void ShowGameOverScreen()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateBloodEffect()
    {
        if (playerHealth == null) return;
        
        bloodLight.enabled = false;
        bloodMedium.enabled = false;
        bloodHeavy1.enabled = false;
        bloodHeavy2.enabled = false;

        if (playerHealth.currentHealth <= 15f) { bloodHeavy1.enabled = true; bloodHeavy2.enabled = true; }
        else if (playerHealth.currentHealth <= 30f) { bloodMedium.enabled = true; }
        else if (playerHealth.currentHealth <= 50f) { bloodLight.enabled = true; }
    }

    void UpdateHealthBar()
    {
        if (playerHealth == null) return;
        
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = playerHealth.currentHealth / playerHealth.maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(playerHealth.currentHealth) + " / " + Mathf.RoundToInt(playerHealth.maxHealth);
        }
    }
}