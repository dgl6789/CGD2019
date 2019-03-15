using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.UI {
    /// <summary>
    /// Scrolls the background zig-zags.
    /// </summary>
    [ExecuteInEditMode]
    public class ZigzagScroll : MonoBehaviour {

        // Scrolling directions.
        [SerializeField] bool x;
        [SerializeField] bool y;

        Material material;

        /// <summary>
        /// Set reference to the material to scroll.
        /// </summary>
        private void Start() {
            material = GetComponent<Renderer>().sharedMaterial;
        }

        /// <summary>
        /// Scroll the material by setting its offset.
        /// </summary>
        /// <param name="speed">Speed at which to scroll.</param>
        public void UpdateScrollOffset(float speed) {
            float offset = Time.time * speed;
            material.SetTextureOffset("_MainTex", new Vector2(x ? offset : 0, y ? offset : 0));
        }
    }
}
