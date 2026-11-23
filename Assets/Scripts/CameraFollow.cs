using UnityEngine;

/// <summary>
/// Kamera plynule sleduje hráče. Přidej na Main Camera.
/// Pokud target není nastavený, najde si hráče sám podle tagu "Player".
/// Funguje v obou režimech (shora i z boku).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Cíl")]
    [SerializeField] private Transform target;

    [Header("Sledování")]
    [SerializeField] private float smoothTime = 0.15f;      // menší = kamera tužší, 0 = bez zpoždění
    [SerializeField] private Vector2 offset = Vector2.zero; // posun kamery vůči hráči
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;           // v jeskyni můžeš vypnout, pokud nechceš svislé sledování

    [Header("Hranice mapy (volitelné)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minPosition = new Vector2(-50f, -50f);   // levý dolní roh, kam může střed kamery
    [SerializeField] private Vector2 maxPosition = new Vector2(50f, 50f);     // pravý horní roh

    private Vector3 velocity;
    private float fixedZ;

    void Awake()
    {
        fixedZ = transform.position.z;
    }

    void Start()
    {
        FindTarget();

        // kamera se na začátku okamžitě postaví k hráči, ať "nevyletí" z jiného místa
        if (target != null)
            transform.position = ClampToBounds(GetDesiredPosition());
    }

    // LateUpdate: kamera se pohne až po pohybu hráče, takže se netřese
    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        Vector3 desired = ClampToBounds(GetDesiredPosition());
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    Vector3 GetDesiredPosition()
    {
        Vector3 pos = transform.position;

        if (followX) pos.x = target.position.x + offset.x;
        if (followY) pos.y = target.position.y + offset.y;
        pos.z = fixedZ;   // 2D hra: kamera zůstává ve stejné hloubce

        return pos;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        if (!useBounds) return pos;

        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        return pos;
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minPosition.x + maxPosition.x) / 2f, (minPosition.y + maxPosition.y) / 2f, 0f);
        Vector3 size = new Vector3(maxPosition.x - minPosition.x, maxPosition.y - minPosition.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
