using System;
using UnityEngine;

namespace App.Util {
    public class AssetManager : MonoBehaviour {

        public static AssetManager Instance;

        [SerializeField] NamedSprite[] SpriteManifest;
        [SerializeField] NamedModel[] ModelManifest;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public Sprite GetSpriteFromManifest(string name) {
            foreach(NamedSprite ns in SpriteManifest) {
                if (ns.Name.Equals(name)) return ns.Sprite;
            }

            return null;
        }

        public Mesh GetModelFromManifest(string name) {
            foreach (NamedModel ns in ModelManifest) {
                if (ns.Name.Equals(name)) return ns.Model;
            }

            return null;
        }
    }

    [Serializable]
    public struct NamedSprite {
        public string Name;
        public Sprite Sprite;
    }

    [Serializable]
    public struct NamedModel {
        public string Name;
        public Mesh Model;
    }
}