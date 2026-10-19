using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    InputAction leftMouse;
    public GameObject foodObj;

    public List<GameObject> allFood = new List<GameObject>();

    [SerializeField] float maxFoodCount;
    float foodCount;
    
    public int FoodCount => Mathf.RoundToInt(foodCount);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftMouse = InputSystem.actions.FindAction("MouseClick");
        foodCount = maxFoodCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (leftMouse.WasPressedThisFrame())
        {
            if (foodCount > 0)
            {
                MakeFood();
                
            }
        }   
    }

    void MakeFood()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        newPos.z = 0;
        allFood.Add(Instantiate(foodObj, newPos, Quaternion.identity));
        foodCount--;
    }

    public void DestroyFood(GameObject food)
    {
        allFood.Remove(food);
        Destroy(food);
        foodCount++;
    }
}
