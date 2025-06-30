using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistenceTest : MonoBehaviour
{
    void Awake()
    {
        // Pesan ini muncul saat objek pertama kali dibuat
        Debug.Log("AWAKE: Objek '" + gameObject.name + "' telah bangun.");
        
        // Perintah untuk jangan hancur saat pindah scene
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("AWAKE: Perintah DontDestroyOnLoad sudah diberikan pada '" + gameObject.name + "'.");
    }

    void OnEnable()
    {
        // Pesan ini muncul saat objek menjadi aktif
        Debug.Log("ON-ENABLE: Objek '" + gameObject.name + "' sekarang aktif.");
        SceneManager.sceneLoaded += OnSceneLoaded; // Langganan event pindah scene
    }

    void OnDisable()
    {
        // Pesan ini akan muncul jika objek dinonaktifkan
        Debug.LogWarning("ON-DISABLE: Objek '" + gameObject.name + "' sedang dinonaktifkan.");
        SceneManager.sceneLoaded -= OnSceneLoaded; // Berhenti langganan
    }

    void OnDestroy()
    {
        // Pesan ini HANYA akan muncul jika objek benar-benar dihancurkan
        Debug.LogError("ON-DESTROY: Objek '" + gameObject.name + "' SEDANG DIHANCURKAN! Ini seharusnya TIDAK terjadi jika DontDestroyOnLoad berfungsi.");
    }

    // Fungsi ini akan dipanggil secara otomatis setelah scene baru berhasil dimuat
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("SCENE LOADED: Scene baru '" + scene.name + "' telah dimuat. Saya, '" + gameObject.name + "', MASIH HIDUP!");
    }
}