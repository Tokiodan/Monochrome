using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileColor
    {
        Red,
        Yellow,
        Blue,
        Purple,
        Orange,
        White
    }

    [Header("Stats")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 1;

    [Header("Info")]
    [SerializeField] private ProjectileColor projectileColor;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetColor(ProjectileColor newColor)
    {
        projectileColor = newColor;
    }

    public int GetDamage()
    {
        return damage;
    }

    public ProjectileColor GetProjectileColor()
    {
        return projectileColor;
    }
}