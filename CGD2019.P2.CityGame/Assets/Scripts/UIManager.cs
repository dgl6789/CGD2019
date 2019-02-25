using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager Instance;

    [SerializeField] Image moralityBar; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void UpdateMoralityBar()
    {
        moralityBar.fillAmount = ScoreManager.Instance.Score;
    }
    
}
