using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.UI {
    /// <summary>
    /// UIManager manages the app's UI elements. Not all the functionality therein has to be
    /// in this class necessarily, but it should all be routed through here for ease of 
    /// compartmentalization, especially in UI methods that need updating, or references to 
    /// one object that need to be made in multiple classes.
    /// </summary>
    [ExecuteInEditMode]
    public class UIManager : MonoBehaviour {

        /// Singleton instance.
        public static UIManager Instance;

        // State objects and swapping variables
        [Header("States")]
        [SerializeField] GameObject[] UIStateObjects;
        [SerializeField] float stateSwapDuration;

        [HideInInspector] public bool InputLock;
        
        // UI transform references
        [Header("Common")]
        [SerializeField] RectTransform canvasContent;
        [SerializeField] RectTransform title;

        // Background controls
        [SerializeField] ZigzagScroll zigzagObject;
        [SerializeField] BackgroundScaler backgroundScaler;
        [SerializeField] float zigzagScrollSpeed;

        [Header("Customization")]
        [SerializeField] RectTransform themeContainer;
        [SerializeField] Material[] themeMaterials;

        [SerializeField] Slider sfxSlider;
        [SerializeField] Slider musicSlider;

        [Header("Menu")]
        [SerializeField] TextMeshProUGUI currencyValue;
        [SerializeField] TextMeshProUGUI bestScoreValue;

        [Header("Ingame")]
        [SerializeField] TextMeshProUGUI gameScoreValue;
        [SerializeField] TextMeshProUGUI gameTimerValue;
        [SerializeField] TextMeshProUGUI gameCurrencyValue;

        [Header("Game Over")]
        [SerializeField] TextMeshProUGUI finalScoreValue;
        [SerializeField] TextMeshProUGUI newBestText;
        [SerializeField] TextMeshProUGUI endBestScoreText;

        // Juice
        [Header("Misc")]
        [SerializeField] float[] buttonShakeAmounts;
        [SerializeField] float[] buttonShakeDurations;

        [Header("Custom Colored Objects")]
        public Material ThemeMaterial;

        [SerializeField] SpriteRenderer background;

        [Space(10)]
        [SerializeField] Image titleBackground;
        [SerializeField] TextMeshProUGUI titleTextShadow;

        Material lastThemeMaterial;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if(this) Initialize();
        }

        /// <summary>
        /// Initialize basic manager.
        /// </summary>
        private void Initialize() {
            UpdateGameColors();

            SetThemeCheckmarkStates();
        }

        /// <summary>
        /// Update App UI.
        /// </summary>
        private void Update() {
            if (lastThemeMaterial != ThemeMaterial) UpdateGameColors();

            zigzagObject.UpdateScrollOffset(zigzagScrollSpeed);
            backgroundScaler.UpdateScaling();
            
        }

        /// <summary>
        /// Asynchronously swap game states via a procedural animation.
        /// </summary>
        /// <param name="from">State to swap from.</param>
        /// <param name="to">State to swap to.</param>
        /// <returns>Couroutine.</returns>
        public IEnumerator SwapState(GameState from, GameState to) {
            // Do nothing if the input is locked.
            if (InputLock) yield break;

            // Input remains locked for the duration of the coroutine.
            InputLock = true;

            float screenWidth = canvasContent.rect.width * canvasContent.localScale.x;
            float screenHeight = canvasContent.rect.height * canvasContent.localScale.y;

            Vector2 desiredContentPos = new Vector2(canvasContent.localPosition.x - screenWidth * 1.5f, canvasContent.localPosition.y);

            // If the title needs to move, make it so.
            if (to == GameState.Game) StartCoroutine(MoveTitle(new Vector2(title.localPosition.x, title.localPosition.y + screenHeight / 2)));
            else if (from == GameState.Game) StartCoroutine(MoveTitle(Vector2.zero));

            // Lerp the canvas' content one screen-width to the left of its original position.
            while (Vector2.Distance(canvasContent.localPosition, desiredContentPos) > 0.1f) {
                canvasContent.localPosition = Vector2.Lerp(canvasContent.localPosition, desiredContentPos, Time.deltaTime * stateSwapDuration);

                yield return null;
            }
            
            // Enable the proper state
            for (int i = 0; i < UIStateObjects.Length; i++) UIStateObjects[i].SetActive(i == (int)to);

            // Teleport the canvas' content one screen-width to the right of its original position.
            canvasContent.localPosition = new Vector2(screenWidth, 0);
            desiredContentPos = Vector2.zero;
            
            // Lerp the canvas' content back to its original position.
            while (Vector2.Distance(canvasContent.localPosition, desiredContentPos) > 0.1f) {
                canvasContent.localPosition = Vector2.Lerp(canvasContent.localPosition, desiredContentPos, Time.deltaTime * stateSwapDuration);

                yield return null;
            }

            // Re-enable input.
            InputLock = false;
            yield return null;
        }

        /// <summary>
        /// Move the title rect asynchronously.
        /// </summary>
        /// <param name="desiredPosition">Position to move to.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator MoveTitle(Vector2 desiredPosition) {
            while (Vector2.Distance(title.localPosition, desiredPosition) > 0.1f) {
                title.localPosition = Vector2.Lerp(title.localPosition, desiredPosition, Time.deltaTime * stateSwapDuration);

                yield return null;
            }
        }

        /// <summary>
        /// Set the theme of the game from one of the customization buttons.
        /// </summary>
        /// <param name="material">Material of the swatch.</param>
        public void SetThemeFromSwatch(Material material) {
            ThemeMaterial = material;

            SaveManager.Instance.SetTheme(material);

            SetThemeCheckmarkStates();
        }

        /// <summary>
        /// Update the checkmarks on the theme swatches to reflect which one's selected.
        /// </summary>
        public void SetThemeCheckmarkStates() {
            foreach(ThemeChoice c in themeContainer.GetComponentsInChildren<ThemeChoice>()) {
                if (c.Material == ThemeMaterial) c.ToggleOn();
                else c.ToggleOff();
            }
        }

        /// <summary>
        /// Set the value displayed in the currency box on the main menu.
        /// </summary>
        /// <param name="value">Value to set the indicator to.</param>
        public void SetCurrencyValue(int value) {
            currencyValue.text = string.Format("{0:n0}", value);
            gameCurrencyValue.text = string.Format("{0:n0}", value);
        }

        /// <summary>
        /// Set the score value of the indicator on the main menu.
        /// </summary>
        /// <param name="value">Value to set the indicator to.</param>
        public void SetBestScoreValue(int value) {
            bestScoreValue.text = "BEST " + string.Format("{0:n0}", value) + "!";
        }

        /// <summary>
        /// Update the ingame score text.
        /// </summary>
        /// <param name="value">Value to reflect.</param>
        public void UpdateGameScoreText(int value) {
            gameScoreValue.text = string.Format("{0:n0}", value);
        }

        /// <summary>
        /// Updates the ingame timer text.
        /// </summary>
        /// <param name="value">Value to reflect.</param>
        public void UpdateGameTimerText(float value) {
            gameTimerValue.text = string.Format("{0:n0}", value);
        }

        /// <summary>
        /// Set the values and active states of the game over screen text elements.
        /// </summary>
        /// <param name="finalScore">The final score from the run.</param>
        /// <param name="isHighscore">Was the final score the highest score?</param>
        /// <param name="highScore">The player's high score.</param>
        public void SetGameOverTexts(int finalScore, bool isHighscore, int highScore) {
            finalScoreValue.text = "FINAL\n" + string.Format("{0:n0}", finalScore) + "!";

            if(!isHighscore) {
                endBestScoreText.text = "BEST " + string.Format("{0:n0}", highScore) + "...";
            }

            endBestScoreText.gameObject.SetActive(!isHighscore);
            newBestText.gameObject.SetActive(isHighscore);
        }

        /// <summary>
        /// Get the value of the sfx slider.
        /// </summary>
        /// <returns>Value (0-1) of the sfx slider.</returns>
        public float GetSfxVolume() { return sfxSlider.value; }

        /// <summary>
        /// Get the value of the music slider.
        /// </summary>
        /// <returns>Value (0-1) of the music slider.</returns>
        public float GetMusicVolume() { return musicSlider.value; }

        /// <summary>
        /// Set the volumes of the game's sounds.
        /// </summary>
        /// <param name="sfxVolume">SFX volume.</param>
        /// <param name="musicVolume">Music volume.</param>
        public void SetSoundVolumes(float sfxVolume, float musicVolume) {
            sfxSlider.value = sfxVolume;
            musicSlider.value = musicVolume;
        }

        /// <summary>
        /// Shake the screen from a UI button.
        /// </summary>
        /// <param name="strength">Strength of the shake (0, 1, or 2).</param>
        public void DoUIScreenShake(int strength) {
            strength = Mathf.Min(strength, buttonShakeAmounts.Length);
            StartCoroutine(CameraEffects.Instance.ShakeCamera(buttonShakeAmounts[strength], buttonShakeDurations[strength]));
        }

        /// <summary>
        /// Update the game colors from the selected theme material.
        /// </summary>
        public void UpdateGameColors() {
            background.material = ThemeMaterial;

            titleBackground.color = ThemeMaterial.GetColor("_Color");
            titleTextShadow.color = ThemeMaterial.GetColor("_Color");

            lastThemeMaterial = ThemeMaterial;
        }

        /// <summary>
        /// Get the index of a material from the manifest (for saving non-serializable data).
        /// </summary>
        /// <param name="m">Material to get the index of.</param>
        /// <returns>Index of the given material.</returns>
        public int GetMaterialIndex(Material m) {
            for(int i = 0; i < themeMaterials.Length; i++) {
                if (m == themeMaterials[i]) return i;
            }

            return -1;
        }

        /// <summary>
        /// Get the material from the array by index (for loading non-serializable data).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Material GetMaterialByIndex(int index) {
            return themeMaterials[Mathf.Clamp(index, 0, themeMaterials.Length)];
        }
    }
}
