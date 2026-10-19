
using UnityEngine;

public class BehaviorScript : MonoBehaviour
{
    float fullnessVal = 10f;
    float cleanlessVal = 10f;
    float hp = 10f;

    float needsTime;
    float cleanTime;

    GameObject closestFood;
    Rigidbody2D rb;
    Collider2D bodyCollider;
    Camera cam;

    public float speed = 2f;
    public float needsTimeReset = 5f;
    public float needsTimeStep = 1f;
    public float cleanTimeReset = 5f;
    public float cleanTimeStep = 1f;

    [SerializeField] GameManager myManager;
    [SerializeField] GameObject Pond;
    [SerializeField] Collider2D colA, colB, colC;

    Vector2 targetPos, pondPos;

    bool movingToTarget, movingIdle, movingPond;
    bool needsClean, touchingPond;

    string state = "Idle";
    
    float debugTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        cam = Camera.main;

        needsTime = needsTimeReset;
        cleanTime = cleanTimeReset;

        if (colA != null && colB != null)
            Physics2D.IgnoreCollision(colA, colB, true);
        if (colA != null && colC != null)
            Physics2D.IgnoreCollision(colA, colC, true);
    }

    void Update()
    {
        needsTime -= needsTimeStep * Time.deltaTime;

        if (needsTime <= 0)
            IncrementNeeds();

        if (touchingPond && needsClean)
        {
            state = "Cleaning";
            cleanTime -= cleanTimeStep * Time.deltaTime;

            if (cleanTime <= 0)
            {
                cleanlessVal = 10;
                needsClean = false;
                cleanTime = cleanTimeReset;
                movingPond = false;
                state = "Idle";
            }
        }
        
        debugTimer += Time.deltaTime;

        if (debugTimer >= 1f)
        {
            Debug.Log("Name: " + transform.name +
                      ", Fullness: " + fullnessVal +
                      ", Cleanliness: " + cleanlessVal +
                      ", HP: " + hp +
                      ", State: " + state);

            debugTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (movingPond && !touchingPond)
        {
            state = "Going Pond";
            Move(pondPos);
        }
        else if (movingToTarget)
        {
            state = "Finding Food";
            Move(targetPos);

            if (Vector2.Distance(rb.position, targetPos) < 0.1f)
            {
                movingToTarget = false;
                EatFood();
            }
        }
        else if (movingIdle && !movingPond)
        {
            Move(targetPos);

            if (Vector2.Distance(rb.position, targetPos) < 0.1f)
                movingIdle = false;
        }
    }

    void Move(Vector2 target)
    {
        Vector2 nextPos = Vector2.MoveTowards(
            rb.position, target, speed * Time.fixedDeltaTime);

        rb.MovePosition(ClampToScreen(nextPos));
    }

    Vector2 ClampToScreen(Vector2 pos)
    {
        if (cam == null || !cam.orthographic)
            return pos;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;

        float x = bodyCollider != null ? bodyCollider.bounds.extents.x : 0;
        float y = bodyCollider != null ? bodyCollider.bounds.extents.y : 0;

        return new Vector2(
            Mathf.Clamp(pos.x, c.x - halfW + x, c.x + halfW - x),
            Mathf.Clamp(pos.y, c.y - halfH + y, c.y + halfH - y)
        );
    }

    void IncrementNeeds()
    {
        fullnessVal--;

        if (!touchingPond)
            cleanlessVal -= 0.5f;

        needsTime = needsTimeReset;

        if (cleanlessVal < 0 && !touchingPond)
            GoPond();

        if (fullnessVal <= 0 && !movingPond && !needsClean)
            FindFood();

        if (fullnessVal <= -5)
            hp--;

        if (hp <= 0)
            Destroy(gameObject);

        if (fullnessVal > 0 && cleanlessVal >= 0 &&
            !movingPond && !movingToTarget && !movingIdle)
            Idling();
    }

    void FindFood()
    {
        if (myManager == null || myManager.allFood == null)
            return;

        closestFood = null;
        float dist = float.MaxValue;

        foreach (GameObject food in myManager.allFood)
        {
            if (food == null) continue;

            float d = Vector2.Distance(transform.position, food.transform.position);

            if (d < dist)
            {
                dist = d;
                closestFood = food;
            }
        }

        if (closestFood != null)
        {
            targetPos = closestFood.transform.position;
            movingToTarget = true;
            movingIdle = false;
        }
    }

    void EatFood()
    {
        if (closestFood != null && myManager != null)
        {
            myManager.DestroyFood(closestFood);
            fullnessVal = 10;
        }

        closestFood = null;
    }

    void Idling()
    {
        if (movingIdle || movingToTarget || movingPond)
            return;

        Vector2 offset = Random.insideUnitCircle * (speed / 2f);
        targetPos = ClampToScreen((Vector2)transform.position + offset);
        movingIdle = true;
    }

    void GoPond()
    {
        needsClean = true;
        movingIdle = false;
        movingToTarget = false;

        if (Pond != null && !touchingPond)
        {
            pondPos = ClampToScreen(Pond.transform.position);
            movingPond = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pond"))
        {
            touchingPond = true;
            movingPond = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Pond"))
            touchingPond = false;
    }
}
