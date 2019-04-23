using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using App.UI;

namespace App {
    /// <summary>
    /// Manages the saving and loading of the player's data.
    /// </summary>
    public class SaveManager : MonoBehaviour {

        /// Singleton instance.
        public static SaveManager Instance;

        // Saving controls
        [SerializeField] bool load;
        [HideInInspector] public SaveData LoadedData;

        [Space(10)]

        // Filesystem controls.
        [SerializeField] string folderName;
        [SerializeField] string fileName;
        [SerializeField] string fileExtension;

        /// <summary>
        /// Singleton initialization and startup loading.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
            
            if (this) LoadedData = LoadData();
            ApplyLoadedData();
        }

        /// <summary>
        /// Write the selected theme to the loaded data.
        /// </summary>
        /// <param name="mat">Material to write.</param>
        public void SetTheme(Material mat) {
            LoadedData.Material = mat;
        }

        /// <summary>
        /// Add a new score to the saved list of high scores.
        /// </summary>
        public void SetHighScore() {
            LoadedData.HighScore = RunManager.Instance.HighScore;
        }
        /// <summary>
        /// sets the themes we've purchased to a new bool[]
        /// </summary>
        /// <param name="b"></param>
        public void SetThemes(bool[] b)
        {
            LoadedData.Bought = b;
        }
        /// <summary>
        /// unlocks a theme
        /// </summary>
        /// <param name="T"></param>
        public void UnlockTheme(RingManager.Theme T)
        {
            LoadedData.Bought[(int)T] = true;
        }
        /// <summary>
        /// Save the currency value to the loaded data.
        /// </summary>
        /// <param name="value">Value of the currency.</param>
        public void SaveCurrency(int value) {
            LoadedData.Currency = value;
        }

        /// <summary>
        /// Save the volume settings.
        /// </summary>
        /// <param name="sfxVolume">Sound effect volume value.</param>
        /// <param name="musicVolume">Music volume value.</param>
        public void SaveVolumes(float sfxVolume, float musicVolume) {
            LoadedData.MusicVolume = musicVolume;
            LoadedData.SfxVolume = sfxVolume;
        }

        /// <summary>
        /// save unlocked themes
        /// </summary>
        public void SaveThemes()
        {
            if (LoadedData.Bought == null)
            {
                LoadedData.Bought = new bool[7];
            }
            for (int i = 0; i < RingManager.Instance.bought.Count; i++)
            {
                LoadedData.Bought[(int)RingManager.Instance.bought[i]] = true;
            }
        }
        /// <summary>
        /// Save or load data when the app loses or gains focus, respectively.
        /// </summary>
        /// <param name="focus">Whether the app gained focus.</param>
        private void OnApplicationFocus(bool focus) {
            if (!focus) SaveData(LoadedData);
            else ApplyLoadedData();
        }

        /// <summary>
        /// Save data on quit.
        /// </summary>
        private void OnApplicationQuit() {
            SaveData(LoadedData);
        }

        /// <summary>
        /// Load the game data into a manipulable, non-serializable object.
        /// </summary>
        /// <returns>Deserialized save data object.</returns>
        public SaveData LoadData() {
            if (load) {
                try {
                    string folderPath = Path.Combine(Application.persistentDataPath, folderName);

                    if (Directory.GetFiles(folderPath).Length > 0) {

                        BinaryFormatter formatter = new BinaryFormatter();

                        using (FileStream fs = File.Open(Path.Combine(Application.persistentDataPath, Path.Combine(folderName, fileName + "." + fileExtension)), FileMode.Open))
                            return (SaveData)formatter.Deserialize(fs);
                    } else {
                        return new SaveData(0, RunManager.Instance.Currency, 1, 1, UIManager.Instance.GetMaterialByIndex(0)); // Default data (if none is present).
                    }
                } catch (IOException) {
                    return new SaveData(0, RunManager.Instance.Currency, 1, 1, UIManager.Instance.GetMaterialByIndex(0)); // Default data (if none is present).
                }
            } else {
                return new SaveData(0, RunManager.Instance.Currency, 1, 1, UIManager.Instance.GetMaterialByIndex(0)); // Default data (if none is present).
            }
        }

        /// <summary>
        /// Save the game data into a serialized object.
        /// </summary>
        /// <param name="data">Non-serialized data to save.</param>
        public void SaveData(SaveData data) {
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string dataPath = Path.Combine(folderPath, fileName + "." + fileExtension);

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = File.Open(dataPath, FileMode.OpenOrCreate))
                formatter.Serialize(fs, data);
        }

        /// <summary>
        /// Apply the loaded data to the game.
        /// </summary>
        private void ApplyLoadedData() {
            RunManager.Instance.HighScore = LoadedData.HighScore;
            RunManager.Instance.Currency = LoadedData.Currency;

            

            UIManager.Instance.SetBestScoreValue(LoadedData.HighScore);

            UIManager.Instance.ThemeMaterial = LoadedData.Material;
            UIManager.Instance.SetThemeCheckmarkStates();

            UIManager.Instance.SetSoundVolumes(LoadedData.SfxVolume, LoadedData.MusicVolume);
            SoundManager.Instance.AdjustMusicVolume();
            SoundManager.Instance.AdjustSfxVolume();
        }
    }

    /// <summary>
    /// Serializable save data object. Contains methods for serializing and deserializing data.
    /// </summary>
    [System.Serializable]
    public class SaveData {
        // High score (serialized as int).
        int highScore;
        public int HighScore {
            get { return highScore; }
            set { highScore = value; }
        }
        bool[] masks;
        public bool[] Masks
        {
            get { return masks; }
            set { masks = value; }
        }
        bool[] bought;
        public bool[] Bought
        {
            get { return bought; }
            set { bought = value; }
        }
        // Game theme material. Serialized by index (see UIManager).
        int material;
        public Material Material {
            get { return UIManager.Instance.GetMaterialByIndex(material); }
            set { material = UIManager.Instance.GetMaterialIndex(value); }
        }

        int currency;
        public int Currency {
            get { return currency; }
            set { currency = value; }
        }
        
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }

        public SaveData(int highScore, int currency, float musicVolume, float sfxVolume, Material theme) {
            this.highScore = highScore;
            this.currency = currency;
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
            this.bought = new bool[] { false, false, false, false, false, false, true };
            Material = theme;
        }
    }
}
