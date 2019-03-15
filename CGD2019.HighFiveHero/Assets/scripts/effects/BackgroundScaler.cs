using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    /// <summary>
    /// Scales background elements to cover the camera's frustum.
    /// </summary>
    [ExecuteInEditMode]
    public class BackgroundScaler : MonoBehaviour {
        // Space between the camera bounds and edge of the background elements.
        [SerializeField] float padding;

        // Update checking.
        float lastWidth;
        float lastHeight;
        float lastOrthoSize;

        /// <summary>
        /// Set the initial scale.
        /// </summary>
        private void Start() {
            Scale();
        }

        /// <summary>
        /// Update the scale if the screen dimensions have changed.
        /// </summary>
        public void UpdateScaling() {
            if (lastWidth != Screen.width || 
                lastHeight != Screen.height || 
                lastOrthoSize != Camera.main.orthographicSize)
                Scale();
        }

        /// <summary>
        /// Scale the background elements to fit the screen.
        /// </summary>
        private void Scale() {
            SpriteRenderer r = GetComponent<SpriteRenderer>();

            float spriteWidth = r.sprite.bounds.size.x;
            float spriteHeight = r.sprite.bounds.size.y;

            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);

            transform.localScale = new Vector3((horzExtent + padding) / spriteWidth, (vertExtent + padding) / spriteHeight, 1);

            // Set the update checking parameters.
            lastOrthoSize = Camera.main.orthographicSize;
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
}
