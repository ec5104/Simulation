using Unity.VisualScripting;
using UnityEngine;

public class MawileBehavior : MonoBehaviour
{
    float fullnessVal = 5f;

    float needsTime;
    GameObject closestFood;
    float hp = 10;
    float cleanlessVal = 10;
    
    public float speed;
    public float needsTimeReset;
    public float needsTimeStep;

    [SerializeField] GameManager myManager;
    [SerializeField] GameObject Pond;

    Vector3 targetPos;
    Vector3 pondPos;
    bool movingToTarget;
    bool movingIdle;
    bool movingPond;
    
    bool touchingPond;

    string state = "idle";
    
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        needsTime = needsTimeReset;
        closestFood = null;
    }

    void Update()
    {
        needsTime -= needsTimeStep * Time.deltaTime;
        if (needsTime <= 0)
        {
            IncrementNeeds();
        }
        
        if (touchingPond)
        {
            state = "Cleaning in Pond";
            if (cleanlessVal < 10)
            {
                cleanlessVal += Time.deltaTime * 4; // Clean up over time
            }
            else
            {
                movingPond = false;
            }
        }
        
        Debug.Log("Fullness: " + fullnessVal + ", Cleaniless: " + cleanlessVal + ", HP: " + hp + " State: "  + state);
    }

    void FixedUpdate()
    {
        // Prioritize actions: Eating/Pond cleaning should override idle wandering
        if (movingToTarget)
        {
            state = "Finding Food";
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
            
            if (Vector3.Distance(transform.position, targetPos) <= 0.05f)
            {
                movingToTarget = false;
                EatFood();
            }
        }
        else if (movingPond && !touchingPond)
        {
            state = "Going Pond";
            Vector3 nextPos = Vector3.MoveTowards(transform.position, pondPos, speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
        }
        else if (movingIdle && !movingPond) 
        {
            state = "Idle";
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
            
            if (Vector3.Distance(transform.position, targetPos) <= 0.05f)
            {
                movingIdle = false;
            }
        }
    }

    void IncrementNeeds()
    {
        fullnessVal--;
        if(!touchingPond)
        cleanlessVal -= 2;
        
        // ONLY pick a random idle spot if we aren't currently busy seeking food or cleaning
        if (cleanlessVal >= 0 && fullnessVal >= 0)
        {
            Idling();
        }
        
        needsTime = needsTimeReset;
        
        if (fullnessVal <= 0)
        {
            FindFood();
            hp--;
        }

        if (cleanlessVal < 5 && !touchingPond)
        {
            goPondToClean();
        }

        if (hp <= 0)
        {
            Dies();
        }
    }

    void FindFood()
    {
        float dist = 2000f;
        
        foreach (GameObject food in myManager.allFood)
        {
            if (food != null && Vector3.Distance(transform.position, food.transform.position) < dist)
            {
                closestFood = food;
                dist = Vector3.Distance(transform.position, food.transform.position);
            }
        }

        if (closestFood != null)
        {
            targetPos = closestFood.transform.position;
            movingToTarget = true;
            movingIdle = false;
            movingPond = false;
        }
    }

    void EatFood()
    {
        if (closestFood != null)
        {
            myManager.DestroyFood(closestFood);
            isFull();
            Debug.Log("Ate Food");
        }
    }

    void isFull()
    {
        fullnessVal = 5f;
    }

    void Dies()
    {
        Destroy(this.gameObject);
    }

    void Idling()
    {
        if (!movingIdle && !movingToTarget && !movingPond)
        {
            float RandomX = Random.Range(transform.position.x - speed / 2, transform.position.x + speed / 2);
            float RandomY = Random.Range(transform.position.y - speed / 2, transform.position.y + speed / 2);
            targetPos = new Vector3(RandomX, RandomY, 0);
            movingIdle = true;
        }
    }

    void goPondToClean()
    {
        if (!touchingPond && Pond != null)
        {
            pondPos = Pond.transform.position;
            movingPond = true;
            movingIdle = false; // Stop wandering
        }
    }

    // --- TRIGGER DETECTION SYSTEM ---
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered trigger: " + other.gameObject.name);
        
        if (other.gameObject.CompareTag("Pond"))
        {
            Debug.Log("Touching Pond Trigger");
            touchingPond = true;
            movingPond = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pond"))
        {
            Debug.Log("Left Pond Trigger");
            touchingPond = false;
        }
    }
}
