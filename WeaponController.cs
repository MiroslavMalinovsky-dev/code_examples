using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
    Blaster,
    Laser,
    GrenadesLauncher,
    MachineGun,
    SpawnBomb,
    Enemies,
    Melee,
}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image that will be used for this weapon's crosshair")]
    public Sprite crosshairSprite;
    [Tooltip("The size of the crosshair image")]
    public int crosshairSize;
    [Tooltip("The color of the crosshair image")]
    public Color crosshairColor;
}

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("The name that will be displayed in the UI for this weapon")]
    public string weaponName;
    [Tooltip("The image that will be displayed in the UI for this weapon")]
    public Sprite weaponIcon;

    [Tooltip("Default data for the crosshair")]
    public CrosshairData crosshairDataDefault;
    [Tooltip("Data for the crosshair when targeting an enemy")]
    public CrosshairData crosshairDataTargetInSight;

    [Header("Internal References")]
    [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
    public GameObject weaponRoot;
    [Tooltip("Tip of the weapon, where the projectiles are shot")]
    public Transform weaponMuzzle;

    [Header("Shoot Parameters")]
    [Tooltip("The type of weapon wil affect how it shoots")]
    public WeaponShootType shootType;
    [Tooltip("The projectile prefab")]
    public ProjectileBase projectilePrefab;
    public ProjectileBase projectilePrefabBuff;
    public bool isBuffed;
    [Tooltip("Minimum duration between two shots")]
    public float delayBetweenShots = 0.5f;
    [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    public float bulletSpreadAngle = 0f;
    [Tooltip("Amount of bullets per shot")]
    public int bulletsPerShot = 1;
    [Tooltip("Force that will push back the weapon after each shot")]
    [Range(0f, 2f)]
    public float recoilForce = 1;
    [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
    [Range(0f, 1f)]
    public float aimZoomRatio = 1f;
    [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
    public Vector3 aimOffset;

    [Header("Ammo Parameters (Common)")]
    [Tooltip("Speed of reloaded or cooling per second")]
    public float reloadRate = 1f;
    [Tooltip("Delay after the last shot before starting to reload or to cool")]
    public float reloadDelay = 2f;
    
    [Header("Ammo Parameters for Blasters")]
    [Tooltip("Amount of ammo in one magazine")]
    public float ammoInMagazine;
    [Tooltip("Amount of magazines")]
    public float magazinesAmount;
    [Tooltip("Manual or Automatic")]
    public bool automatic;

    [Header("Ammo Parameters for MachineGun")]
    [Tooltip("Amount of ammo")]
    public float ammoTotal;
    [Tooltip("Max cooling level")]
    public float coolingTotal;

    [Header("Ammo Parameters for GrenadesLauncher")]
    [Tooltip("Max amount of grenades")]
    public float grenadesTotal;
    [Tooltip("amount of grenades in weapon")]
    public float grenadesInWeapon;

    [Header("Ammo Parameters for SpawnBomb")]
    [Tooltip("Max amount of SpawnBomb")]
    public float spawnBombsTotal;

    [Header("Charging parameters for charging weapons only")]
    [Tooltip("Trigger a shot when maximum charge is reached")]
    public bool automaticReleaseOnCharged;
    [Tooltip("Duration to reach maximum charge")]
    public float maxChargeDuration = 2f;
    [Tooltip("Initial ammo used when starting to charge")]
    public float ammoUsedOnStartCharge = 1f;
    [Tooltip("Additional ammo used when charge reaches its maximum")]
    public float ammoUsageRateWhileCharging = 1f;

    [Header("Audio & Visual")]
    [Tooltip("Optional weapon animator for OnShoot animations")]
    public Animator weaponAnimator;
    [Tooltip("Prefab of the muzzle flash")]
    public GameObject muzzleFlashPrefab;
    [Tooltip("Unparent the muzzle flash instance on spawn")]
    public bool unparentMuzzleFlash;
    [Tooltip("sound played when shooting")]
    public AudioClip shootSFX;
    [Tooltip("Sound played when changing to this weapon")]
    public AudioClip changeWeaponSFX;

    [Tooltip("Continuous Shooting Sound")]
    public bool useContinuousShootSound = false;
    public AudioClip continuousShootStartSFX;
    public AudioClip continuousShootLoopSFX;
    public AudioClip continuousShootEndSFX;
    private AudioSource m_continuousShootAudioSource = null;
    private bool m_wantsToShoot = false;

    public UnityAction onShoot;

    float m_CurrentAmmo;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    public float LastChargeTriggerTimestamp { get; private set; }
    Vector3 m_LastMuzzlePosition;

    public GameObject owner { get; set; }
    public GameObject sourcePrefab { get; set; }
    public bool isCharging { get; private set; }
    public float currentAmmoRatio { get; private set; }
    public float coolingRatio { get; private set; }
    public bool isWeaponActive { get; private set; }
    public bool isCooling { get; private set; }
    public bool isRecharged { get; private set; }        
    public float currentCharge { get; private set; }
    public Vector3 muzzleWorldVelocity { get; private set; }
    public float GetAmmoNeededToShoot() => (shootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, ammoUsedOnStartCharge)) / (bulletsPerShot);

    AudioSource m_ShootAudioSource;

    //MachineGun parameters
    float currentAmmo;
    float currentCooling;

    //Blaster parameters
    float ammoInCurrentMagazine;
    float currentMagazinesAmount;
    bool isReloaded;

    //GrenadesLauncher
    float grenadesAmount;
    float currentGrenadesInWeapon;

    //SpawnBombs
    float spawnBombsAmount;
    bool isInitAmmo = false;

    SkillReceiver[] m_skillRecArr;
    const string k_AnimAttackParameter = "Attack";

    void Awake()
    {
        m_CurrentAmmo = 0f;

        //Common
        currentAmmoRatio = 1f;
        coolingRatio = 1f;
        isRecharged = false;
        isCooling = false;

        //MachineGun
        currentAmmo = ammoTotal;
        currentCooling = coolingTotal;

        //Blaster
        isReloaded = false;
        ammoInCurrentMagazine = ammoInMagazine;
        currentMagazinesAmount = magazinesAmount - 1;

        //GrenadesLauncher
        currentGrenadesInWeapon = grenadesInWeapon;
        grenadesAmount = grenadesTotal;

        //SpawnBombs
        spawnBombsAmount = spawnBombsTotal;

        m_LastMuzzlePosition = weaponMuzzle.position;

        m_ShootAudioSource = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this, gameObject);

        if (useContinuousShootSound)
        {
            m_continuousShootAudioSource = gameObject.AddComponent<AudioSource>();
            m_continuousShootAudioSource.playOnAwake = false;
            m_continuousShootAudioSource.clip = continuousShootLoopSFX;
            m_continuousShootAudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
            m_continuousShootAudioSource.loop = true;
        }
    }

    void Start()
    {
        m_skillRecArr = GetComponents<SkillReceiver>();
        if (m_skillRecArr != null)
        {
            for (int i = 0; i < m_skillRecArr.Length; i++)
            {
                OnSelectThisSkill(m_skillRecArr[i].endIndex);
            }
        }
    }

    void Update()
    {
        if (!isInitAmmo)
        {
            InitializeAmmo();
            isInitAmmo = true;
        }

        UpdateBlaster();
        UpdateMachineGun();
        UpdateGrenadesLauncher();
        UpdateContinuousShootSound();

        if (Time.deltaTime > 0)
        {
            muzzleWorldVelocity = (weaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = weaponMuzzle.position;
        }
    }

    void InitializeAmmo()
    {
        PlayerWeaponsManager m_weaponManager = GetComponentInParent<PlayerWeaponsManager>();
        if (m_weaponManager)
        {
            if (shootType == WeaponShootType.GrenadesLauncher)
            {
                grenadesTotal += m_weaponManager.addGrenadesAmount;
                grenadesAmount = grenadesTotal;

            }

            if (shootType == WeaponShootType.Blaster)
            {
                magazinesAmount += m_weaponManager.addMagazinesAmount;
                currentMagazinesAmount = magazinesAmount - 1;
            }
        }
    }

    void UpdateMachineGun()
    {
        if (coolingRatio >= 0.2f)
        {
            isCooling = false;
        }

        if (m_LastTimeShot + reloadDelay < Time.time &&
            currentCooling < coolingTotal            &&
            shootType == WeaponShootType.MachineGun)
        {
            currentCooling += reloadRate * Time.deltaTime;
            currentCooling = Mathf.Clamp(currentCooling, 0, coolingTotal);
            coolingRatio = currentCooling / coolingTotal;
        }
    }

    void UpdateBlaster()
    {
        if (m_LastTimeShot + reloadDelay < Time.time &&
            (isRecharged || isReloaded)              &&
            ammoInCurrentMagazine < ammoInMagazine   &&
            currentMagazinesAmount >= 1f             &&
            shootType == WeaponShootType.Blaster)
        {
            ammoInCurrentMagazine += reloadRate * Time.deltaTime;
            ammoInCurrentMagazine = Mathf.Clamp(ammoInCurrentMagazine, 0, ammoInMagazine);

            if (ammoInCurrentMagazine == ammoInMagazine)
            {
                isRecharged = false;
                currentMagazinesAmount -= 1f;
            }
            else
            {
                isRecharged = true;
            }

            currentAmmoRatio = ammoInCurrentMagazine / ammoInMagazine;
        }
    }

    void UpdateGrenadesLauncher()
    {
        if (m_LastTimeShot + reloadDelay < Time.time &&
            isRecharged                              &&
            shootType == WeaponShootType.GrenadesLauncher)
        {
            currentGrenadesInWeapon += reloadRate * Time.deltaTime;
            currentGrenadesInWeapon = Mathf.Clamp(currentGrenadesInWeapon, 0, grenadesInWeapon);

            if (currentGrenadesInWeapon == grenadesInWeapon)
            {
                isRecharged = false;
            }

            currentAmmoRatio =  currentGrenadesInWeapon / grenadesInWeapon;
        }
    }

    void ShotWithMachineGun()
    {
        if (currentAmmo >= 0 &&
            shootType == WeaponShootType.MachineGun)
        {
            if (currentCooling >= 1f)
            {
                currentAmmo -= 1f;
                currentAmmoRatio = currentAmmo / ammoTotal;
                currentCooling -= 1f;
                coolingRatio = currentCooling / coolingTotal;
                if (currentCooling < 1f)
                {
                    isCooling = true;
                }
            }
        }
    }

    void ShotWithBlaster()
    {
        if ((ammoInCurrentMagazine + ammoInMagazine * currentMagazinesAmount) != 0 &&
            shootType == WeaponShootType.Blaster)
        {
            if (ammoInCurrentMagazine >= 1f)
            {
                ammoInCurrentMagazine -= 1f;
                if (ammoInCurrentMagazine == 0f)
                {
                    isRecharged = true;
                    m_LastTimeShot += 1f;
                }
                currentAmmoRatio = ammoInCurrentMagazine / ammoInMagazine;
            }   
        }
    }

    void ShotWithGrenadesLauncher()
    {
        if (grenadesAmount >= 1f &&
            shootType == WeaponShootType.GrenadesLauncher)
        {  
            grenadesAmount -= 1f;
            currentGrenadesInWeapon -= 1f;
            if (currentGrenadesInWeapon == 0f && grenadesAmount > 1f)
            {
                isRecharged = true;
            }
            currentAmmoRatio = currentGrenadesInWeapon / grenadesInWeapon;  
        }
    }

    void ShotWithSpawnBomb()
    {
        if (spawnBombsAmount >= 1f &&
            shootType == WeaponShootType.SpawnBomb)
        {
            spawnBombsAmount -= 1f;
            currentAmmoRatio = spawnBombsAmount / spawnBombsTotal;
        }
    }

    void UpdateCharge()
    {
        if (isCharging)
        {
            if (currentCharge < 1f)
            {
                float chargeLeft = 1f - currentCharge;

                // Calculate how much charge ratio to add this frame
                float chargeAdded = 0f;
                if (maxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }
                else
                {
                    chargeAdded = (1f / maxChargeDuration) * Time.deltaTime;
                }

                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // See if we can actually add this charge
                float ammoThisChargeWouldRequire = chargeAdded * ammoUsageRateWhileCharging;
                if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo based on charge added
                    UseAmmo(ammoThisChargeWouldRequire);

                    // set current charge ratio
                    currentCharge = Mathf.Clamp01(currentCharge + chargeAdded);
                }
            }
        }
    }

    void UpdateContinuousShootSound()
    {
        if (useContinuousShootSound)
        {
            if (m_wantsToShoot && m_CurrentAmmo >= 1f)
            {
                if (!m_continuousShootAudioSource.isPlaying)
                {
                    m_ShootAudioSource.PlayOneShot(shootSFX);
                    m_ShootAudioSource.PlayOneShot(continuousShootStartSFX);
                    m_continuousShootAudioSource.Play();
                }
            }
            else if (m_continuousShootAudioSource.isPlaying)
            {
                m_ShootAudioSource.PlayOneShot(continuousShootEndSFX);
                m_continuousShootAudioSource.Stop();
            }
        }
    }

    bool TryShootWithMachineGun()
    {
        if (currentAmmo >= 1f                              &&
            m_LastTimeShot + delayBetweenShots < Time.time &&
            currentCooling >= 1f)
        {
            HandleShoot();
            ShotWithMachineGun();
            return true;
        }
        return false;
    }

    bool TryShootWithBlaster()
    {
        if (ammoInCurrentMagazine >= 1f                    &&
            m_LastTimeShot + delayBetweenShots < Time.time &&
            !isRecharged)
        {
            HandleShoot();
            ShotWithBlaster();
            return true;
        }
        return false;
    }

    bool TryShootWithGrenadesLauncher()
    {
        if (grenadesAmount >= 1f                           &&
            m_LastTimeShot + delayBetweenShots < Time.time &&
            !isRecharged)
        {
            HandleShoot();
            ShotWithGrenadesLauncher();
            return true;
        }
        return false;
    }

    bool TryShootWithSpawnBomb()
    {
        if (spawnBombsAmount >= 1f                         &&
            m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot();
            ShotWithSpawnBomb();
            return true;
        }
        return false;
    }

    bool TryShootWithEnemiesWeapon()
    {
        if (m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot();
            
            return true;
        }

        return false;
    }

    bool TryMelee()
    {
        if (m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot();
        }
            
        return true;
    }

    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f 
            && m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1f;

            return true;
        }

        return false;
    }

    bool TryBeginCharge()
    {
        if (!isCharging
            && m_CurrentAmmo >= ammoUsedOnStartCharge
            && Mathf.FloorToInt((m_CurrentAmmo - ammoUsedOnStartCharge) * bulletsPerShot) > 0
            && m_LastTimeShot + delayBetweenShots < Time.time)
        {
            UseAmmo(ammoUsedOnStartCharge);

            LastChargeTriggerTimestamp = Time.time;
            isCharging = true;

            return true;
        }

        return false;
    }

    bool TryReleaseCharge()
    {
        if (isCharging)
        {
            HandleShoot();

            currentCharge = 0f;
            isCharging = false;

            return true;
        }
        return false;
    }

    void HandleShoot()
    {
        int bulletsPerShotFinal = shootType == WeaponShootType.Charge ? Mathf.CeilToInt(currentCharge * bulletsPerShot) : bulletsPerShot;
        
        // spawn all bullets with random direction
        for (int i = 0; i < bulletsPerShotFinal; i++)
        {
            Vector3 shotDirection = GetShotDirectionWithinSpread(weaponMuzzle);

            if (!isBuffed)
            {
                ProjectileBase newProjectile = Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }
            else
            {
                ProjectileBase newProjectile = Instantiate(projectilePrefabBuff, weaponMuzzle.position, Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }
            
        }

        // muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlashInstance = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle.transform);
            // Unparent the muzzleFlashInstance
            if (unparentMuzzleFlash)
            {
                muzzleFlashInstance.transform.SetParent(null);
            }

            Destroy(muzzleFlashInstance, 2f);
        }

        m_LastTimeShot = Time.time;

        // play shoot SFX
        if (shootSFX && !useContinuousShootSound)
        {
            m_ShootAudioSource.PlayOneShot(shootSFX);
        }

        // Trigger attack animation if there is any
        if (weaponAnimator)
        {
            weaponAnimator.SetTrigger(k_AnimAttackParameter);
        }

        // Callback on shoot
        if (onShoot != null)
        {
            onShoot();
        }
    }


    public void ShowWeapon(bool show)
    {
        weaponRoot.SetActive(show);

        if (show && changeWeaponSFX)
        {
            m_ShootAudioSource.PlayOneShot(changeWeaponSFX);
        }

        isWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, 1f);
        m_LastTimeShot = Time.time;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp, bool inputReload, bool isMelee)
    {
        m_wantsToShoot = inputDown || inputHeld;
        isReloaded = inputReload;

        switch (shootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.SpawnBomb:
                if (inputDown)
                {
                    return TryShootWithSpawnBomb();
                }
                return false;

            case WeaponShootType.MachineGun:
                if (inputHeld)
                {
                    return TryShootWithMachineGun();
                }
                return false;

            case WeaponShootType.Blaster:
                if (automatic ? inputHeld : inputDown)
                {
                    return TryShootWithBlaster();
                }
                return false;

            case WeaponShootType.GrenadesLauncher:
                if (inputDown)
                {
                    return TryShootWithGrenadesLauncher();
                }
                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Enemies:
                if (inputHeld)
                {
                    return TryShootWithEnemiesWeapon();
                }
                return false;

            case WeaponShootType.Melee:
                if (isMelee)
                {
                    return TryMelee();
                }
                return false;

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    TryBeginCharge();
                }
                // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                if (inputUp || (automaticReleaseOnCharged && currentCharge >= 1f))
                {
                    return TryReleaseCharge();
                }
                return false;

            default:
                return false;
        }
    }

    public void ChangeAmount(float addGrenadesAmount, float addMagazinesAmount)
    {
        if (shootType == WeaponShootType.GrenadesLauncher)
        {
            grenadesAmount += addGrenadesAmount;
        }

        if (shootType == WeaponShootType.Blaster)
        {
            currentMagazinesAmount += addMagazinesAmount;
        }
    }

    public void OnSelectThisSkill(int ui_Index)
    {
        if (m_skillRecArr != null)
        {
            for (int i = 0; i < m_skillRecArr.Length; i++)
            {
                if (m_skillRecArr[i].endIndex == 10 && m_skillRecArr[i].isOpened &&
                    (m_skillRecArr[i].endIndex == ui_Index || ui_Index == 0))
                {
                    if ((shootType == WeaponShootType.Blaster ||
                           shootType == WeaponShootType.MachineGun))
                    {
                        delayBetweenShots *= (1f - m_skillRecArr[i].receivedValues[0]);
                        if (ui_Index != 0)
                            break;
                    }
                }

                if (m_skillRecArr[i].endIndex == 13 && m_skillRecArr[i].isOpened &&
                    (m_skillRecArr[i].endIndex == ui_Index || ui_Index == 0))
                {
                    if ((shootType == WeaponShootType.GrenadesLauncher))
                    {
                        isBuffed = true;
                        if (ui_Index != 0)
                            break;
                    }
                }

                if (m_skillRecArr[i].endIndex == 14 && m_skillRecArr[i].isOpened &&
                    (m_skillRecArr[i].endIndex == ui_Index || ui_Index == 0))
                {
                    if ((shootType == WeaponShootType.Blaster ||
                           shootType == WeaponShootType.MachineGun))
                    {
                        isBuffed = true;
                        if (ui_Index != 0)
                            break;
                    }
                }
            }
        }
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = bulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);

        return spreadWorldDirection;
    }

    public int GetCurrentAmmo()
    {
        switch (shootType)
        {
            case WeaponShootType.MachineGun:
                return (int)currentAmmo;

            case WeaponShootType.GrenadesLauncher:
                return (int)grenadesAmount;

            case WeaponShootType.Blaster:
                return (int)ammoInCurrentMagazine;

            case WeaponShootType.SpawnBomb:
                return (int)spawnBombsAmount;

            default:
                return 0;
        }
    }

    public int GetCurrentMagazines()
    {
        switch (shootType)
        {
            case WeaponShootType.MachineGun:
                return 0;

            case WeaponShootType.GrenadesLauncher:
                return 0;

            case WeaponShootType.Blaster:
                return (int)currentMagazinesAmount;

            case WeaponShootType.SpawnBomb:
                return 0;

            default:
                return 0;
        }
    }
}
