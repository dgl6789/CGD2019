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

        
        b[6] = true;
        for (int i = 0; i < b.Length; i++)
        {
            if (b[i])
            {
                bought.Add((Theme)i);
            }
        }
        App.SaveManager.Instance.SaveThemes();
    }
    public bool unlockMask(int masknum)
    {
        if (App.SaveManager.Instance.LoadedData.Masks == null)
        {
            App.SaveManager.Instance.LoadedData.Masks = new bool[3];
        }
        
        switch (masknum)
        {
            case 0:
                App.SaveManager.Instance.LoadedData.Masks[0] = true;
                App.UI.UIManager.Instance.maskIndex = 0;
                return true;
                break;
            case 1:
                if (App.SaveManager.Instance.LoadedData.Masks[1])
                {
                    App.UI.UIManager.Instance.maskIndex = 1;
                    return true;
                }
                else if (Rings >= 10)
                {
                    Rings -= 10;
                    App.SaveManager.Instance.LoadedData.Masks[1] = true;
                    App.UI.UIManager.Instance.maskIndex = 1;
                    return true;
                }
                break;
            case 2:
                if (App.SaveManager.Instance.LoadedData.Masks[2])
                {
                    App.UI.UIManager.Instance.maskIndex = 2;
                    return true;
                }
                else if (Rings >= 10)
                {
                    Rings -= 10;
                    App.SaveManager.Instance.LoadedData.Masks[2] = true;
                    App.UI.UIManager.Instance.maskIndex = 2;
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }
    public bool unlockTheme(Theme T)
    {
       
        if (App.SaveManager.Instance.LoadedData.Bought == null)
        {
            App.SaveManager.Instance.LoadedData.Bought = new bool[7];
        }
        
        if (App.SaveManager.Instance.LoadedData.Bought[(int)T]||bought.Contains(T))
        {
            return true;
        }
        else if (Rings >= 20)
        {
            App.SaveManager.Instance.LoadedData.Bought[(int)T] = true;
            bought.Add(T);//thx diana
            Rings -= 20;
            App.SaveManager.Instance.SaveThemes();
            return true;
        }
        App.SaveManager.Instance.SaveThemes();
        return false;
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
        bought.Add(Theme.tigres);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
