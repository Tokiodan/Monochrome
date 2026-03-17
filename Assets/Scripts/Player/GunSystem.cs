using System.Collections;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    public enum GunColor
    {
        Red,
        Yellow,
        Blue,
        Purple,
        Orange,
        White
    }

    [System.Serializable]
    public class GunData
    {
        public string gunName;
        public GunColor gunColor;

        [Header("References")]
        public GameObject gunObject;
        public Transform firePoint;
        public Projectile projectilePrefab;

        [Header("Shooting")]
        public bool automatic;
        public int bulletsPerShot = 1;
        public float spreadAngle = 0f;
        public float fireRate = 0.25f;
        public float projectileSpeed = 40f;
        public int damage = 1;

        [Header("Ammo")]
        public int magazineSize = 10;
        public float reloadTime = 1.2f;

        [HideInInspector] public int currentAmmo;
        [HideInInspector] public bool isReloading;
        [HideInInspector] public float nextFireTime;
    }

    [Header("Guns")]
    [SerializeField] private GunData[] guns = new GunData[6];

    [Header("Current Gun")]
    [SerializeField] private int currentGunIndex = 0;

    private void Start()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].currentAmmo = guns[i].magazineSize;
            guns[i].isReloading = false;
            guns[i].nextFireTime = 0f;
        }

        EquipGun(currentGunIndex);
    }

    private void Update()
    {
        HandleGunSwitch();
        HandleShoot();
        HandleReload();
    }

    private void HandleGunSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipGun(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipGun(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipGun(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipGun(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) EquipGun(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) EquipGun(5);
    }

    private void HandleShoot()
    {
        GunData gun = guns[currentGunIndex];

        if (gun.isReloading)
            return;

        bool shootPressed = gun.automatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (!shootPressed)
            return;

        if (Time.time < gun.nextFireTime)
            return;

        if (gun.currentAmmo <= 0)
        {
            StartCoroutine(ReloadGun(currentGunIndex));
            return;
        }

        FireGun(gun);
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GunData gun = guns[currentGunIndex];

            if (!gun.isReloading && gun.currentAmmo < gun.magazineSize)
                StartCoroutine(ReloadGun(currentGunIndex));
        }
    }

    private void FireGun(GunData gun)
    {
        gun.nextFireTime = Time.time + gun.fireRate;
        gun.currentAmmo--;

        for (int i = 0; i < gun.bulletsPerShot; i++)
        {
            Quaternion shotRotation = gun.firePoint.rotation;

            if (gun.spreadAngle > 0f)
            {
                float yawOffset = Random.Range(-gun.spreadAngle, gun.spreadAngle);
                float pitchOffset = Random.Range(-gun.spreadAngle, gun.spreadAngle);
                shotRotation *= Quaternion.Euler(pitchOffset, yawOffset, 0f);
            }

            Projectile projectile = Instantiate(
                gun.projectilePrefab,
                gun.firePoint.position,
                shotRotation
            );

            projectile.SetSpeed(gun.projectileSpeed);
            projectile.SetDamage(gun.damage);
            projectile.SetColor(ConvertGunColor(gun.gunColor));
        }
    }

    private IEnumerator ReloadGun(int gunIndex)
    {
        GunData gun = guns[gunIndex];

        if (gun.isReloading)
            yield break;

        gun.isReloading = true;
        yield return new WaitForSeconds(gun.reloadTime);
        gun.currentAmmo = gun.magazineSize;
        gun.isReloading = false;
    }

    private void EquipGun(int index)
    {
        if (index < 0 || index >= guns.Length)
            return;

        currentGunIndex = index;

        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i].gunObject != null)
                guns[i].gunObject.SetActive(i == currentGunIndex);
        }
    }

    private Projectile.ProjectileColor ConvertGunColor(GunColor gunColor)
    {
        switch (gunColor)
        {
            case GunColor.Red:
                return Projectile.ProjectileColor.Red;
            case GunColor.Yellow:
                return Projectile.ProjectileColor.Yellow;
            case GunColor.Blue:
                return Projectile.ProjectileColor.Blue;
            case GunColor.Purple:
                return Projectile.ProjectileColor.Purple;
            case GunColor.Orange:
                return Projectile.ProjectileColor.Orange;
            case GunColor.White:
                return Projectile.ProjectileColor.White;
            default:
                return Projectile.ProjectileColor.Red;
        }
    }

    public GunData GetCurrentGun()
    {
        return guns[currentGunIndex];
    }

    public int GetCurrentGunIndex()
    {
        return currentGunIndex;
    }
}