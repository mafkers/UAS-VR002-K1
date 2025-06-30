// Health.cs (Versi Final & Paling Stabil dengan Sistem Skor)
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    // Pastikan ini dicentang di Inspector untuk Player
    public bool canDie = true;
    public float startingHealth = 100.0f;
    public float maxHealth = 100.0f;
    public int scoreValue = 10;
    public float currentHealth;
    public bool replaceWhenDead = false;
    public GameObject deadReplacement;
    public bool makeExplosion = false;
    public GameObject explosion;
    public bool isPlayer = false;
    public GameObject deathCam;
    private bool dead = false;

    void Start()
    {
        currentHealth = startingHealth;
    }

    public void ChangeHealth(float amount)
    {
        // Jika sudah mati, jangan proses damage lagi.
        if (dead) return;

        currentHealth += amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Pastikan darah tidak minus di UI
            
            // Cek apakah diizinkan untuk mati
            if(canDie)
            {
                Die();
            }
        }

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void Die()
    {
        // Pastikan fungsi ini hanya berjalan sekali
        if (dead) return;
        dead = true;

        if (isPlayer)
        {
            Debug.Log("Player Mati! Memuat scene Game Over...");
            SceneManager.LoadScene("GameOver");
        }
        else // Jika yang mati bukan player (zombie)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                // --- BARIS BARU DITAMBAHKAN ---
                // Kirim nilai skor dari zombie ini ke GameManager
                gm.AddScore(scoreValue);
                // -----------------------------
                
                // Laporkan kematian untuk logika wave
                gm.ZombieKilled();
            }
            
            ZombieAI ai = GetComponent<ZombieAI>();
            if (ai != null)
            {
                ai.OnDeath();
            }
            
            if (replaceWhenDead)
                Instantiate(deadReplacement, transform.position, transform.rotation);
            if (makeExplosion)
                Instantiate(explosion, transform.position, transform.rotation);
            
            Destroy(gameObject, 5f);
        }
    }
}