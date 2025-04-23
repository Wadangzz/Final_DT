using UnityEngine;

public class ObjectReset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeleteAllConveyorItems()
    {
        ConveyorItem[] items = FindObjectsByType<ConveyorItem>(FindObjectsSortMode.None);

        foreach (var item in items)
        {
            if (item.name.Contains("Clone"))
            {
                Destroy(item.gameObject);
            }
        }
    }
}
