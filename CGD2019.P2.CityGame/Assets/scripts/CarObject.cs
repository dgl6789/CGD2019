using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    [CreateAssetMenu(fileName = "Car", menuName = "Mobiles/Car", order = 1)]
    public class CarObject : MobileObject
    {
        public void SetValues(string spriteName, string mobileName, int speed)
        {
            base.SetValues(spriteName, mobileName, speed);
        }
    }
}