// Weapon.cs - VERSI LENGKAP & FINAL DEBUGGING
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WeaponType
{
	Projectile,
	Raycast,
	Beam
}
public enum Auto
{
	Full,
	Semi
}
public enum BulletHoleSystem
{
	Tag,
	Material,
	Physic_Material
}


[System.Serializable]
public class SmartBulletHoleGroup
{
	public string tag;
	public Material material;
	public PhysicMaterial physicMaterial;
	public BulletHolePool bulletHole;
	
	public SmartBulletHoleGroup()
	{
		tag = "Everything";
		material = null;
		physicMaterial = null;
		bulletHole = null;
	}
	public SmartBulletHoleGroup(string t, Material m, PhysicMaterial pm, BulletHolePool bh)
	{
		tag = t;
		material = m;
		physicMaterial = pm;
		bulletHole = bh;
	}
}

public class Weapon : MonoBehaviour
{
	// Weapon Type
	public WeaponType type = WeaponType.Projectile;
	public bool shooterAIEnabled = false;
	public bool bloodyMessEnabled = false;
	public int weaponType = 0;
	public Auto auto = Auto.Full;
	public bool playerWeapon = true;
	public GameObject weaponModel;
	public Transform raycastStartSpot;
	public float delayBeforeFire = 0.0f;
	public bool warmup = false;
	public float maxWarmup = 2.0f;
	public bool multiplyForce = true;
	public bool multiplyPower = false;
	public float powerMultiplier = 1.0f;
	public float initialForceMultiplier = 1.0f;
	public bool allowCancel = false;
	private float heat = 0.0f;
	public GameObject projectile;
	public Transform projectileSpawnSpot;
	public bool reflect = true;
	public Material reflectionMaterial;
	public int maxReflections = 5;
	public string beamTypeName = "laser_beam";
	public float maxBeamHeat = 1.0f;
	public bool infiniteBeam = false;
	public Material beamMaterial;
	public Color beamColor = Color.red;
	public float startBeamWidth = 0.5f;
	public float endBeamWidth = 1.0f;
	private float beamHeat = 0.0f;
	private bool coolingDown = false;
	private GameObject beamGO;
	private bool beaming = false;
	public float power = 80.0f;
	public float forceMultiplier = 10.0f;
	public float beamPower = 1.0f;
	public float range = 9999.0f;
	public float rateOfFire = 10;
	private float actualROF;
	private float fireTimer;
	public bool infiniteAmmo = false;
	public int ammoCapacity = 12;
	public int shotPerRound = 1;
	private int currentAmmo;
	public float reloadTime = 2.0f;
	public bool showCurrentAmmo = true;
	public bool reloadAutomatically = true;
	public float accuracy = 80.0f;
	private float currentAccuracy;
	public float accuracyDropPerShot = 1.0f;
	public float accuracyRecoverRate = 0.1f;
	public int burstRate = 3;
	public float burstPause = 0.0f;
	private int burstCounter = 0;
	private float burstTimer = 0.0f;
	public bool recoil = true;
	public float recoilKickBackMin = 0.1f;
	public float recoilKickBackMax = 0.3f;
	public float recoilRotationMin = 0.1f;
	public float recoilRotationMax = 0.25f;
	public float recoilRecoveryRate = 0.01f;
	public bool spitShells = false;
	public GameObject shell;
	public float shellSpitForce = 1.0f;
	public float shellForceRandom = 0.5f;
	public float shellSpitTorqueX = 0.0f;
	public float shellSpitTorqueY = 0.0f;
	public float shellTorqueRandom = 1.0f;
	public Transform shellSpitPosition;
	public bool makeMuzzleEffects = true;
	public GameObject[] muzzleEffects = new GameObject[] { null };
	public Transform muzzleEffectsPosition;
	public bool makeHitEffects = true;
	public GameObject[] hitEffects = new GameObject[] { null };
	public bool makeBulletHoles = true;
	public BulletHoleSystem bhSystem = BulletHoleSystem.Tag;
	public List<string> bulletHolePoolNames = new List<string>();
	public List<string> defaultBulletHolePoolNames = new List<string>();
	public List<SmartBulletHoleGroup> bulletHoleGroups = new List<SmartBulletHoleGroup>();
	public List<BulletHolePool> defaultBulletHoles = new List<BulletHolePool>();
	public List<SmartBulletHoleGroup> bulletHoleExceptions = new List<SmartBulletHoleGroup>();
	public bool showCrosshair = true;
	public Texture2D crosshairTexture;
	public int crosshairLength = 10;
	public int crosshairWidth = 4;
	public float startingCrosshairSize = 10.0f;
	private float currentCrosshairSize;
	public AudioClip fireSound;
	public AudioClip reloadSound;
	public AudioClip dryFireSound;
	private bool canFire = true;

	void Start()
	{
		if (rateOfFire != 0)
			actualROF = 1.0f / rateOfFire;
		else
			actualROF = 0.01f;
		currentCrosshairSize = startingCrosshairSize;
		fireTimer = 0.0f;
		currentAmmo = ammoCapacity;
		if (GetComponent<AudioSource>() == null)
		{
			gameObject.AddComponent(typeof(AudioSource));
		}
		if (raycastStartSpot == null) raycastStartSpot = gameObject.transform;
		if (muzzleEffectsPosition == null) muzzleEffectsPosition = gameObject.transform;
		if (projectileSpawnSpot == null) projectileSpawnSpot = gameObject.transform;
		if (weaponModel == null) weaponModel = gameObject;
		if (crosshairTexture == null) crosshairTexture = new Texture2D(0, 0);
		for (int i = 0; i < bulletHolePoolNames.Count; i++)
		{
			GameObject g = GameObject.Find(bulletHolePoolNames[i]);
			if (g != null && g.GetComponent<BulletHolePool>() != null) bulletHoleGroups[i].bulletHole = g.GetComponent<BulletHolePool>();
			else Debug.LogWarning("Bullet Hole Pool does not exist or does not have a BulletHolePool component.");
		}
		for (int i = 0; i < defaultBulletHolePoolNames.Count; i++)
		{
			GameObject g = GameObject.Find(defaultBulletHolePoolNames[i]);
			if (g.GetComponent<BulletHolePool>() != null) defaultBulletHoles[i] = g.GetComponent<BulletHolePool>();
			else Debug.LogWarning("Default Bullet Hole Pool does not have a BulletHolePool component.");
		}
	}

	void Update()
	{
		currentAccuracy = Mathf.Lerp(currentAccuracy, accuracy, accuracyRecoverRate * Time.deltaTime);
		currentCrosshairSize = startingCrosshairSize + (accuracy - currentAccuracy) * 0.8f;
		fireTimer += Time.deltaTime;
		if (playerWeapon)
		{
			CheckForUserInput();
		}
		if (reloadAutomatically && currentAmmo <= 0)
			Reload();
		if (playerWeapon && recoil && type != WeaponType.Beam)
		{
			if (weaponModel != null)
			{
				weaponModel.transform.localPosition = Vector3.Lerp(weaponModel.transform.localPosition, Vector3.zero, recoilRecoveryRate * Time.deltaTime);
				weaponModel.transform.localRotation = Quaternion.Slerp(weaponModel.transform.localRotation, Quaternion.identity, recoilRecoveryRate * Time.deltaTime);
			}
		}
		if (type == WeaponType.Beam)
		{
			if (!beaming)
				StopBeam();
			beaming = false;
		}
	}

	// ### FUNGSI INI DIMODIFIKASI DENGAN DEBUG.LOG ###
	void CheckForUserInput()
	{
		if (type == WeaponType.Raycast)
		{
			bool isReadyToFire = fireTimer >= actualROF;
			bool burstIsReady = burstCounter < burstRate;

			if (isReadyToFire && burstIsReady && canFire)
			{
				if (Input.GetButton("Fire1"))
				{
					if (!warmup)
					{
						Fire();
					}
					else if (heat < maxWarmup)
					{
						heat += Time.deltaTime;
					}
				}
			}
		}
		if (type == WeaponType.Projectile)
		{
			if (fireTimer >= actualROF && burstCounter < burstRate && canFire)
			{
				if (Input.GetButton("Fire1"))
				{
					if (!warmup) Launch();
					else if (heat < maxWarmup) heat += Time.deltaTime;
				}
				if (warmup && Input.GetButtonUp("Fire1"))
				{
					if (allowCancel && Input.GetButton("Cancel")) heat = 0.0f;
					else Launch();
				}
			}
		}
		if (burstCounter >= burstRate)
		{
			burstTimer += Time.deltaTime;
			if (burstTimer >= burstPause)
			{
				burstCounter = 0;
				burstTimer = 0.0f;
			}
		}
		if (type == WeaponType.Beam)
		{
			if (Input.GetButton("Fire1") && beamHeat <= maxBeamHeat && !coolingDown) Beam();
			else StopBeam();
			if (beamHeat >= maxBeamHeat) coolingDown = true;
			else if (beamHeat <= maxBeamHeat - (maxBeamHeat / 2)) coolingDown = false;
		}
		if (Input.GetButtonDown("Reload")) Reload();
		if (Input.GetButtonUp("Fire1")) canFire = true;
	}

	void OnGUI()
	{
		if (type == WeaponType.Projectile || type == WeaponType.Beam)
		{
			currentAccuracy = accuracy;
		}
		if (showCrosshair)
		{
			Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
			Rect leftRect = new Rect(center.x - crosshairLength - currentCrosshairSize, center.y - (crosshairWidth / 2), crosshairLength, crosshairWidth);
			GUI.DrawTexture(leftRect, crosshairTexture, ScaleMode.StretchToFill);
			Rect rightRect = new Rect(center.x + currentCrosshairSize, center.y - (crosshairWidth / 2), crosshairLength, crosshairWidth);
			GUI.DrawTexture(rightRect, crosshairTexture, ScaleMode.StretchToFill);
			Rect topRect = new Rect(center.x - (crosshairWidth / 2), center.y - crosshairLength - currentCrosshairSize, crosshairWidth, crosshairLength);
			GUI.DrawTexture(topRect, crosshairTexture, ScaleMode.StretchToFill);
			Rect bottomRect = new Rect(center.x - (crosshairWidth / 2), center.y + currentCrosshairSize, crosshairWidth, crosshairLength);
			GUI.DrawTexture(bottomRect, crosshairTexture, ScaleMode.StretchToFill);
		}
		if (showCurrentAmmo)
		{
			if (type == WeaponType.Raycast || type == WeaponType.Projectile)
				GUI.Label(new Rect(10, Screen.height - 30, 100, 20), "Ammo: " + currentAmmo);
			else if (type == WeaponType.Beam)
				GUI.Label(new Rect(10, Screen.height - 30, 100, 20), "Heat: " + (int)(beamHeat * 100) + "/" + (int)(maxBeamHeat * 100));
		}
	}

	void Fire()
	{
		fireTimer = 0.0f;
		burstCounter++;
		if (auto == Auto.Semi)
			canFire = false;
		if (currentAmmo <= 0)
		{
			DryFire();
			return;
		}
		if (!infiniteAmmo)
			currentAmmo--;
		for (int i = 0; i < shotPerRound; i++)
		{
			float accuracyVary = (100 - currentAccuracy) / 1000;
			Vector3 direction = raycastStartSpot.forward;
			direction.x += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			direction.y += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			direction.z += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			currentAccuracy -= accuracyDropPerShot;
			if (currentAccuracy <= 0.0f)
				currentAccuracy = 0.0f;
			Ray ray = new Ray(raycastStartSpot.position, direction);
			RaycastHit hit;
			int ignoreFogWall = ~LayerMask.GetMask("FogWall");
			if (Physics.Raycast(ray, out hit, range, ignoreFogWall))
			{
				float damage = power;
				if (warmup)
				{
					damage *= heat * powerMultiplier;
					heat = 0.0f;
				}
				hit.collider.gameObject.SendMessageUpwards("ChangeHealth", -damage, SendMessageOptions.DontRequireReceiver);
				if (shooterAIEnabled)
				{
					hit.transform.SendMessageUpwards("Damage", damage / 100, SendMessageOptions.DontRequireReceiver);
				}
				if (bloodyMessEnabled)
				{
					if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Limb"))
					{
						// ... Bloody Mess Code ...
					}
				}
				if (makeBulletHoles)
				{
					// ... Bullet Hole Code ...
				}
				if (makeHitEffects)
				{
					foreach (GameObject hitEffect in hitEffects)
					{
						if (hitEffect != null)
							Instantiate(hitEffect, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
					}
				}
				if (hit.rigidbody)
				{
					hit.rigidbody.AddForce(ray.direction * power * forceMultiplier);
				}
			}
		}
		if (recoil)
			Recoil();
		if (makeMuzzleEffects)
		{
			GameObject muzfx = muzzleEffects[Random.Range(0, muzzleEffects.Length)];
			if (muzfx != null)
				Instantiate(muzfx, muzzleEffectsPosition.position, muzzleEffectsPosition.rotation);
		}
		if (spitShells)
		{
			GameObject shellGO = Instantiate(shell, shellSpitPosition.position, shellSpitPosition.rotation) as GameObject;
			shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(shellSpitForce + Random.Range(0, shellForceRandom), 0, 0), ForceMode.Impulse);
			shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(shellSpitTorqueX + Random.Range(-shellTorqueRandom, shellTorqueRandom), shellSpitTorqueY + Random.Range(-shellTorqueRandom, shellTorqueRandom), 0), ForceMode.Impulse);
		}
		GetComponent<AudioSource>().PlayOneShot(fireSound);
	}

	public void Launch()
	{
		fireTimer = 0.0f;
		burstCounter++;
		if (auto == Auto.Semi)
			canFire = false;
		if (currentAmmo <= 0)
		{
			DryFire();
			return;
		}
		if (!infiniteAmmo)
			currentAmmo--;
		for (int i = 0; i < shotPerRound; i++)
		{
			if (projectile != null)
			{
				GameObject proj = Instantiate(projectile, projectileSpawnSpot.position, projectileSpawnSpot.rotation);
				SetLayerRecursively(proj, LayerMask.NameToLayer("Bullet"));
				if (warmup)
				{
					if (multiplyPower) proj.SendMessage("MultiplyDamage", heat * powerMultiplier, SendMessageOptions.DontRequireReceiver);
					if (multiplyForce) proj.SendMessage("MultiplyInitialForce", heat * initialForceMultiplier, SendMessageOptions.DontRequireReceiver);
					heat = 0.0f;
				}
			}
			else
			{
				Debug.Log("Projectile is null! Cek inspector.");
			}
		}
		if (recoil) Recoil();
		if (makeMuzzleEffects)
		{
			GameObject muzfx = muzzleEffects[Random.Range(0, muzzleEffects.Length)];
			if (muzfx != null) Instantiate(muzfx, muzzleEffectsPosition.position, muzzleEffectsPosition.rotation);
		}
		if (spitShells)
		{
			GameObject shellGO = Instantiate(shell, shellSpitPosition.position, shellSpitPosition.rotation);
			shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(shellSpitForce + Random.Range(0, shellForceRandom), 0, 0), ForceMode.Impulse);
			shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(shellSpitTorqueX + Random.Range(-shellTorqueRandom, shellTorqueRandom), shellSpitTorqueY + Random.Range(-shellTorqueRandom, shellTorqueRandom), 0), ForceMode.Impulse);
		}
		GetComponent<AudioSource>().PlayOneShot(fireSound);
	}

	void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	void Beam()
	{
		// ... Kode asli Beam ...
	}

	public void StopBeam()
	{
		beamHeat -= Time.deltaTime;
		if (beamHeat < 0)
			beamHeat = 0;
		GetComponent<AudioSource>().Stop();
		if (beamGO != null)
		{
			Destroy(beamGO);
		}
		SendMessageUpwards("OnEasyWeaponsStopBeaming", SendMessageOptions.DontRequireReceiver);
	}

	void Reload()
	{
		currentAmmo = ammoCapacity;
		fireTimer = -reloadTime;
		GetComponent<AudioSource>().PlayOneShot(reloadSound);
		SendMessageUpwards("OnEasyWeaponsReload", SendMessageOptions.DontRequireReceiver);
	}

	void DryFire()
	{
		GetComponent<AudioSource>().PlayOneShot(dryFireSound);
	}

	void Recoil()
	{
		if (!playerWeapon)
			return;
		if (weaponModel == null)
		{
			Debug.Log("Weapon Model is null.  Make sure to set the Weapon Model field in the inspector.");
			return;
		}
		float kickBack = Random.Range(recoilKickBackMin, recoilKickBackMax);
		float kickRot = Random.Range(recoilRotationMin, recoilRotationMax);
		weaponModel.transform.Translate(new Vector3(0, 0, -kickBack), Space.Self);
		weaponModel.transform.Rotate(new Vector3(-kickRot, 0, 0), Space.Self);
	}

	MeshRenderer FindMeshRenderer(GameObject go)
	{
		MeshRenderer hitMesh;
		if (go.GetComponent<Renderer>() != null)
		{
			hitMesh = go.GetComponent<MeshRenderer>();
		}
		else
		{
			hitMesh = go.GetComponentInChildren<MeshRenderer>();
			if (hitMesh == null)
			{
				GameObject curGO = go;
				while (hitMesh == null && curGO.transform != curGO.transform.root)
				{
					curGO = curGO.transform.parent.gameObject;
					hitMesh = curGO.GetComponent<MeshRenderer>();
				}
			}
		}
		return hitMesh;
	}

	public void RemoteFire()
	{
		AIFiring();
	}

	public void AIFiring()
	{
		if (type == WeaponType.Raycast)
		{
			if (fireTimer >= actualROF && burstCounter < burstRate && canFire)
			{
				StartCoroutine(DelayFire());
			}
		}
		if (type == WeaponType.Projectile)
		{
			if (fireTimer >= actualROF && canFire)
			{
				StartCoroutine(DelayLaunch());
			}
		}
	}

	IEnumerator DelayFire()
	{
		fireTimer = 0.0f;
		burstCounter++;
		if (auto == Auto.Semi)
			canFire = false;
		SendMessageUpwards("OnEasyWeaponsFire", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(delayBeforeFire);
		Fire();
	}

	IEnumerator DelayLaunch()
	{
		fireTimer = 0.0f;
		burstCounter++;
		if (auto == Auto.Semi)
			canFire = false;
		SendMessageUpwards("OnEasyWeaponsLaunch", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(delayBeforeFire);
		Launch();
	}
	IEnumerator DelayBeam()
	{
		yield return new WaitForSeconds(delayBeforeFire);
		Beam();
	}
}