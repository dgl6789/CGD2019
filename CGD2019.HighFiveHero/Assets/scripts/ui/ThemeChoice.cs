using UnityEngine;
using UnityEngine.UI;

namespace App.UI {
    /// <summary>
    /// Functionality for a button that swaps the color theme of the app.
    /// </summary>
    public class ThemeChoice : MonoBehaviour {
        float Vibrating = 0;
        Vector3 startingPos;
        // Component references
        [SerializeField] int Theme;
        [SerializeField] Image swatch;
        [SerializeField] GameObject checkmark;

        // Get the material associated with this button from the image component.
        public Material Material {
            get { return swatch.material; }
        }

        bool chosen;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start() {
            startingPos = transform.position;
            checkmark.SetActive(chosen);
        }
        private void Update()
        {
            if (Vibrating > 0)
            {
                float xOffset = Mathf.Lerp(-10f, 10f, Mathf.Sin((Time.realtimeSinceStartup * 100f)+1/2 ));
                
                Vibrating -= Time.deltaTime;
                transform.position = startingPos + new Vector3(xOffset, 0, 0);
                if (Vibrating <= 0)
                {
                    transform.position = startingPos;
                }
            }
            else
            {
                startingPos = transform.position;
            }
        }
        /// <summary>
        /// Set the new theme and update relevant UI.
        /// </summary>
        public void OnTap() {
            if (RingManager.Instance.unlockTheme((RingManager.Theme)Theme))
            {
                UIManager.Instance.SetThemeFromSwatch(Material);
            }
            else
            {
                Vibrating = .5f;
            }
        }

        /// <summary>
        /// Toggle the active state of the swatch.
        /// </summary>
        public void ToggleOn() {
            chosen = true;
            checkmark.SetActive(true);
        }

        /// <summary>
        /// Toggle the active state of the swatch.
        /// </summary>
        public void ToggleOff() {
            chosen = false;
            checkmark.SetActive(false);
        }
    }
}
