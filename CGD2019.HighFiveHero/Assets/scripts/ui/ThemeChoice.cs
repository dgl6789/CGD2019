using UnityEngine;
using UnityEngine.UI;

namespace App.UI {
    /// <summary>
    /// Functionality for a button that swaps the color theme of the app.
    /// </summary>
    public class ThemeChoice : MonoBehaviour {

        // Component references
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
            checkmark.SetActive(chosen);
        }

        /// <summary>
        /// Set the new theme and update relevant UI.
        /// </summary>
        public void OnTap() {
            UIManager.Instance.SetThemeFromSwatch(Material);
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
