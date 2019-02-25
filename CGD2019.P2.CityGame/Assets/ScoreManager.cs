using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager Instance;

    float score = 0.5f;

    /// <summary>
    /// Use this property to adjust the player's score by an amount between 0 and 1.
    /// 
    /// e.g. ScoreManager.Instance.Score += [amount];
    /// </summary>
    public float Score
    {
        get { return score; }
        set {
            AdjustScore(Mathf.Clamp01(value) - score);
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    
    private void AdjustScore(float adj) {
        score = Mathf.Clamp01(score + adj);

        UIManager.Instance.UpdateMoralityBar();
    }
}
