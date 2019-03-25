using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiAliasing : MonoBehaviour
{
    List<float> frameTimes = new List<float>();
    bool undecided;
    public static void CheckFramerate()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        undecided = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (frameTimes.Count < 120)
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
                    lateframes++;
                }
            }
            if(lateframes>18)
            {

            }
        }
    }
}
