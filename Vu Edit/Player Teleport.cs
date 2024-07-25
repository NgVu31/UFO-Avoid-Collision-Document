using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeleport : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    public Slider healthBar; // Tham chiếu đến UI thanh máu

    public float maxHealth = 100f; // Máu tối đa của player
    private float currentHealth; // Máu hiện tại của player

    private bool isPlayerAlive = true; // Biến cờ để kiểm tra xem người chơi còn sống hay không

    public MeteorSpawner meteorSpawner; // Tham chiếu đến MeteorSpawner
    public GameManager gameManager;

    public GameObject bulletPrefab; // Prefab của đạn
    public float fireRate = 0.2f; // Tốc độ bắn đạn (số giây giữa mỗi lần bắn)
    private float nextFireTime; // Thời gian kế tiếp được phép bắn đạn

    public float dodgeDistance = 5f;  // Khoảng cách dịch chuyển
    public float cooldownTime = 15f;  // Thời gian hồi chiêu
    public int maxDodgeCount = 3;  // Số lần dịch chuyển tối đa

    private int currentDodgeCount;
    private bool isCooldown;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        // Khởi tạo thời gian bắn đạn ban đầu
        nextFireTime = Time.time;

        // Gán GameManager bằng cách tìm đối tượng GameManager trong Scene
        gameManager = FindObjectOfType<GameManager>();

        // Khởi tạo số lần dịch chuyển và trạng thái hồi chiêu
        currentDodgeCount = maxDodgeCount;
        isCooldown = false;
    }

    void Update()
    {
        // Kiểm tra xem người chơi còn sống hay không
        if (!isPlayerAlive)
        {
            return; // Nếu không còn sống, không cần xử lý các input nữa
        }

        // Nhận input từ bàn phím
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Bắn đạn khi nhấn phím cách và đến thời điểm được phép bắn
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate; // Cập nhật thời gian kế tiếp được phép bắn đạn
        }

        // Dịch chuyển khi nhấn phím E và đủ điều kiện
        if (Input.GetKeyDown(KeyCode.E) && !isCooldown && currentDodgeCount > 0)
        {
            Dodge();
        }
    }

    void FixedUpdate()
    {
        // Kiểm tra xem người chơi còn sống hay không
        if (!isPlayerAlive)
        {
            return; // Nếu không còn sống, không cần di chuyển nữa
        }

        // Di chuyển player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Dodge()
    {
        // Dịch chuyển player theo hướng ngẫu nhiên
        Vector3 dodgeDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        rb.MovePosition(rb.position + (Vector2)dodgeDirection * dodgeDistance);

        currentDodgeCount--;

        if (currentDodgeCount <= 0)
        {
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        currentDodgeCount = maxDodgeCount;
        isCooldown = false;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isPlayerAlive = false; // Đặt cờ là không còn sống
            rb.simulated = false;
            this.enabled = false;
            if (meteorSpawner != null)
            {
                meteorSpawner.enabled = false;
            }
            gameManager.ShowGameOver();
        }
        healthBar.value = currentHealth;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthBar.value = currentHealth;
    }

    void FireBullet()
    {
        // Tìm thiên thạch gần nhất
        GameObject[] meteors = GameObject.FindGameObjectsWithTag("ThienThach");
        GameObject nearestMeteor = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject meteor in meteors)
        {
            float distance = Vector2.Distance(transform.position, meteor.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestMeteor = meteor;
            }
        }

        // Nếu tìm thấy thiên thạch gần nhất, bắn đạn về hướng đó
        if (nearestMeteor != null)
        {
            Vector2 direction = (nearestMeteor.transform.position - transform.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity); // Bắn từ vị trí của player
            bullet.GetComponent<Bullet>().Initialize(direction);
        }
    }
}
