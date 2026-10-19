using UnityEngine;
using UnityEngine.UI;

public class BerryCounterUI : MonoBehaviour
{
    public GameManager myManager;
    public Image[] berryIcons;

    public Color availableColor = Color.white;
    public Color unavailableColor = Color.gray;

    void Update()
    {
        if (myManager == null || berryIcons == null)
            return;

        for (int i = 0; i < berryIcons.Length; i++)
        {
            if (berryIcons[i] == null)
                continue;

            if (i < myManager.FoodCount)
            {
                berryIcons[i].color = availableColor;
            }
            else
            {
                berryIcons[i].color = unavailableColor;
            }
        }
    }
}