﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class MobileObject : ScriptableObject
    {
        [SerializeField] protected string spriteName;
        public string SpriteName { get { return spriteName; } }

        [SerializeField] protected string mobileName;
        public string MobileName { get { return mobileName; } }

        [SerializeField] protected int speed;
        public int Speed { get { return speed; } }

        [SerializeField] protected Vector3 worldBounds;
        public Vector3 WorldBounds { get { return worldBounds; } }

        public void SetValues(string spriteName, string mobileName, int speed, Vector3 worldBounds)
        {
            this.spriteName = spriteName;
            this.mobileName = mobileName;
            this.speed = speed;
            this.worldBounds = worldBounds;
        }
    }
}
