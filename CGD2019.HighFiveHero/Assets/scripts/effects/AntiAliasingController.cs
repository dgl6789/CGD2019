using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiAliasingController : MonoBehaviour
{
    static List<float> frameTimes = new List<float>();
    static bool undecided;
    int FXAANUM = 8;
    public static void CheckFramerate()
    {
        frameTimes = new List<float>();
        undecided = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        undecided = true;
        QualitySettings.antiAliasing = FXAANUM;
    }

    // Update is called once per frame
    void Update()
    {
        if (frameTimes.Count < 60&&undecided)//gather data
        {
            frameTimes.Add(Time.time);
        }
        else if(undecided)
        {
            int lateframes = 0;
            for (int i = 0; i < frameTimes.Count-1; i++)
            {
                if ((double)(frameTimes[i + 1] - frameTimes[i]) > 0.01666666666)
                {
                    lateframes++;//evaluate
                }
            }
            if (lateframes > 10)//are we late too often
            {
                if (FXAANUM == 2)
                {
                    FXAANUM = 0;
                    undecided = false;//give up phone is to slow
                }
                else
                {
                    frameTimes = new List<float>();
                    FXAANUM /= 2;//turn doen anti-aliasing
                }
                QualitySettings.antiAliasing = FXAANUM;
            }
            else
            {
                frameTimes = new List<float>();
            }
        }
    }
}
