using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingManager : MonoBehaviour
{
    public static RingManager Instance;
    public enum Theme
    {
        arid,
        candyshop,
        flora,
        lagoon,
        peacock,
        professional,
        tigres,
    };
    public List<Theme> bought;
    public void setThemes(bool[] b)
    {
        
        for (int i = 0; i < b.Length; i++)
        {
            if (b[i])
            {
                bought.Add((Theme)i);
            }
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        bought = new List<Theme>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
