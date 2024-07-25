using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutterBullet : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 100f;
    public float explosionRadius = 5f;  // Bán kính của vùng AOE
    public float explosionDuration = 0.3f;  // Thời gian tồn tại của vùng AOE
    public LayerMask enemyLayer;  // Lớp của enemy để kiểm tra va chạm

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ThienThach")) // Kiểm tra tag của đối tượng va chạm
        {
            ThienThach thienThach = collision.gameObject.GetComponent<ThienThach>();
            if (thienThach != null)
            {
                thienThach.TakeDamage(damage);
            }

            // Gọi hàm tạo vùng AOE
            StartCoroutine(CreateAOE(transform.position));

            // Hủy đạn sau khi va chạm
            Destroy(gameObject);
        }
    }

    IEnumerator CreateAOE(Vector3 explosionPosition)
    {
        // Tạo ra một Circle Collider tạm thời để mô phỏng vùng AOE
        GameObject aoeObject = new GameObject("AOE");
        aoeObject.transform.position = explosionPosition;
        CircleCollider2D aoeCollider = aoeObject.AddComponent<CircleCollider2D>();
        aoeCollider.radius = explosionRadius;
        aoeCollider.isTrigger = true;

        // Tìm tất cả các enemy trong vùng AOE
        Collider2D[] enemies = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius, enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            // Gây sát thương hoặc bất kỳ hiệu ứng gì bạn muốn ở đây
            Debug.Log("Enemy trong vùng AOE: " + enemy.name);
        }

        // Chờ 0,3 giây
        yield return new WaitForSeconds(explosionDuration);

        // Hủy đối tượng AOE sau khi hết thời gian tồn tại của vùng AOE
        Destroy(aoeObject);
    }
}
