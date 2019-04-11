using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingManager : MonoBehaviour
{
    int Rings
    {
        get { return App.RunManager.Instance.Currency; }
        set { App.RunManager.Instance.Currency = value; }
    }
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
        b[1] = true;
        for (int i = 0; i < b.Length; i++)
        {
            if (b[i])
            {
                bought.Add((Theme)i);
            }
        }
    }
    public bool unlockTheme(Theme T)
    {
        
        if (App.SaveManager.Instance.LoadedData.Bought[(int)T])
        {
            return true;
        }
        else if (Rings >= 20)
        {
            
            bought.Add(T);//thx diana
            Rings -= 20;
            App.SaveManager.Instance.SaveThemes();
            return true;
        }
        return false;
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        setThemes(App.SaveManager.Instance.LoadedData.Bought);
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
