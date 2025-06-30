// ZombieAI.cs (Versi Final dengan Auto-Positioning & Bersih)
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Zombie Stats")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    private Transform playerTarget;
    private NavMeshAgent navAgent;
    private Health playerHealth;
    private float lastAttackTime;
    private Animator animator;

    void Start()
    {
        // Ambil komponen dasar
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // ==================================================================================
        // ### KODE KUNCI UNTUK MEMPERBAIKI POSISI ZOMBIE SECARA OTOMATIS ###
        // ==================================================================================
        // Secara paksa mencari posisi valid terdekat di NavMesh dan 'teleport' ke sana.
        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(transform.position, out closestHit, 500f, NavMesh.AllAreas))
        {
            // Pindahkan posisi zombie ke titik terdekat yang valid di NavMesh
            transform.position = closestHit.position;
            // Aktifkan NavMeshAgent setelah posisinya benar
            navAgent.enabled = true;
        }
        else
        {
            // Jika tidak ada NavMesh sama sekali di dekatnya, hancurkan zombie ini agar tidak error
            Debug.LogError("FATAL: Tidak ada NavMesh yang ditemukan di dekat posisi awal zombie '" + gameObject.name + "'!");
            Destroy(gameObject);
            return; // Hentikan sisa fungsi Start()
        }
        // ==================================================================================
    }

    void Update()
    {
        // Jika target player belum ditemukan, terus cari setiap frame
        if (playerTarget == null)
        {
            FindPlayer();
            return; 
        }
        
        if (!navAgent.enabled) return;
        
        // Atur tujuan dan animasi berdasarkan kecepatan
        navAgent.SetDestination(playerTarget.position);
        animator.SetFloat("Speed", navAgent.velocity.magnitude);

        // Logika untuk menyerang
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            playerHealth = playerObject.GetComponent<Health>();
            Debug.Log("Player DITEMUKAN oleh " + gameObject.name);
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(-attackDamage);
        }
    }
    
    public void OnDeath()
    {
        // Pengecekan untuk memastikan fungsi ini tidak dipanggil berulang kali
        if (!this.enabled) return;

        // Beritahu Animator untuk memainkan animasi kematian
        animator.SetBool("IsDead", true);

        // Matikan komponen AI dan navigasi
        navAgent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        
        // --- BAGIAN INI DIPERBAIKI URUTANNYA ---
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 1. Hentikan dulu semua sisa kecepatan SAAT MASIH NON-KINEMATIC
            rb.velocity = Vector3.zero;
            
            // 2. BARU buat dia menjadi kinematic agar tidak terpengaruh gravitasi
            rb.isKinematic = true;
        }
        // ------------------------------------

        // Matikan skrip AI ini sebagai langkah terakhir
        this.enabled = false;
    }
}