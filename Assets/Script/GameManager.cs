// GameManager.cs (Versi Final dengan Spawn Hadiah)
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Wave { public string waveName; public EnemyGroup[] enemyGroups; public float spawnInterval; }
[System.Serializable]
public class EnemyGroup { public GameObject enemyPrefab; public int count; }


public class GameManager : MonoBehaviour
{
    [Header("Skor & UI")]
    public int currentScore = 0;
    public TMP_Text scoreText;
    public TMP_Text waveAnnouncerText;
    public float waveAnnouncerDisplayTime = 3f;

    [Header("Wave & Spawn Settings")]
    public Wave[] waves;
    public Transform[] spawnPoints;

    [Header("Drop Settings")]
    public GameObject healthPackPrefab;
    public GameObject ammoPackPrefab;

    private int currentWaveIndex = 0;
    private int zombiesAliveInScene;

    void Start()
    {
        if (waveAnnouncerText != null)
        {
            waveAnnouncerText.gameObject.SetActive(false);
        }
        
        UpdateScoreUI();
        StartNextWave();
    }

    void StartNextWave()
    {
        if (currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        else
        {
            WinGame();
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        if (waveAnnouncerText != null)
        {
            waveAnnouncerText.text = wave.waveName;
            waveAnnouncerText.gameObject.SetActive(true);
            yield return new WaitForSeconds(waveAnnouncerDisplayTime);
            waveAnnouncerText.gameObject.SetActive(false);
        }
        
        Debug.Log("Memulai " + wave.waveName);
        
        zombiesAliveInScene = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            zombiesAliveInScene += group.count;
        }

        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                if (spawnPoints.Length == 0)
                {
                    Debug.LogError("Spawn Points belum diatur di GameManager!");
                    yield break;
                }
                Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Instantiate(group.enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        UpdateScoreUI();
    }

    public void ZombieKilled()
    {
        zombiesAliveInScene--;
        Debug.Log("Satu zombie mati. Sisa zombie hidup di wave ini: " + zombiesAliveInScene);

        if (zombiesAliveInScene <= 0)
        {
            Debug.Log(waves[currentWaveIndex].waveName + " Selesai!");
            
            // --- LOGIKA BARU UNTUK SPAWN HADIAH ---
            if (healthPackPrefab != null && ammoPackPrefab != null && spawnPoints.Length > 0)
            {
                // Pilih titik spawn acak untuk meletakkan hadiah
                Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                
                // Munculkan hadiah di posisi titik spawn tersebut
                Instantiate(healthPackPrefab, randomSpawnPoint.position, Quaternion.identity);
                // Beri sedikit offset agar tidak tumpang tindih
                Instantiate(ammoPackPrefab, randomSpawnPoint.position + new Vector3(1,0,1), Quaternion.identity);
            }
            // ------------------------------------

            currentWaveIndex++;
            // Beri jeda 5 detik agar pemain sempat ambil hadiah sebelum wave berikutnya
            Invoke("StartNextWave", 5f); 
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void WinGame()
    {
        Debug.Log("SEMUA WAVE SELESAI! ANDA MENANG!");
        SceneManager.LoadScene("YouWin");
    }
}