using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;

namespace App {
    /// <summary>
    /// Plays sound and handles common sound-making functionality. All the sound effects, music, and other
    /// noise the app makes should be handles from here.
    /// </summary>
    public class SoundManager : MonoBehaviour {

        /// Singleton instance.
        public static SoundManager Instance;

        AudioSource source;

        [SerializeField] GameObject noiseMakerObject;

        // Lists of named audio clips that can be referenced by string.
        [SerializeField] NamedClip[] ClipManifest;

        [SerializeField] NamedClip[] TrackManifest;

        private float sfxVolumeMultiplier;
        private float musicVolumeMultiplier;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if(this) source = GetComponent<AudioSource>();
        }
        
        /// <summary>
        /// Change the soundtrack.
        /// </summary>
        /// <param name="track">Track to play by name.</param>
        /// <param name="volume">Volume to play at.</param>
        public void SwapTrack(string track, float volume = 1f) {
            source.clip = GetTrackFromManifest(track);
        }

        /// <summary>
        /// Play a sound clip by reference.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="volume">Volume at which to play the clip.</param>
        public void PlaySoundClip(AudioClip clip, float volume = 1f) {
            source.PlayOneShot(clip, volume * sfxVolumeMultiplier);
        }

        /// <summary>
        /// Play a sound clip by name.
        /// </summary>
        /// <param name="name">String based name of the clip to play.</param>
        /// <param name="volume">Volume at which to play the clip.</param>
        public void PlaySoundByName(string name, float volume = 1f) {
            source.PlayOneShot(GetClipFromManifest(name), volume * sfxVolumeMultiplier);
        }

        /// <summary>
        /// Play a sound clip with a random pitch adjustment.
        /// </summary>
        /// <param name="name">String based name of the clip to play.</param>
        /// <param name="min">Minimum random pitch.</param>
        /// <param name="max">Maximum random pitch.</param>
        /// <param name="volume">Volume at which to play the clip.</param>
        public void PlaySoundWithRandomPitch(string name, float min, float max, float volume = 1f) {
            // Instatiate a noise maker object.
            AudioSource s = Instantiate(noiseMakerObject, transform).GetComponent<AudioSource>();
            AudioClip clip = GetClipFromManifest(name);

            // Set the object's pitch.
            s.pitch = Random.Range(min, max);
            s.PlayOneShot(clip, volume * sfxVolumeMultiplier);

            // Destroy the object after the clip is done.
            Destroy(s.gameObject, clip.length);
        }

        /// <summary>
        /// Play a slap sound. For UI buttons.
        /// </summary>
        public void PlayUIButtonClickSound() {
            if (!UIManager.Instance.InputLock) PlaySoundWithRandomPitch("slap medium", 0.8f, 1.2f, 0.5f * sfxVolumeMultiplier);
        }

        /// <summary>
        /// Play a slap sound. For UI buttons.
        /// </summary>
        public void PlayThemeButtonClickSound() {
            if(!UIManager.Instance.InputLock) PlaySoundWithRandomPitch("slap soft", 0.8f, 1.2f, 0.5f * sfxVolumeMultiplier);
        }

        /// <summary>
        /// Get an audio clip reference by name.
        /// </summary>
        /// <param name="name">Name of the refrence to find.</param>
        /// <returns>Reference to the named clip.</returns>
        private AudioClip GetClipFromManifest(string name) {
            foreach(NamedClip c in ClipManifest) {
                if (c.Name == name) return c.Clip;
            }

            return null;
        }

        /// <summary>
        /// Get an audio track reference by name.
        /// </summary>
        /// <param name="name">Name of the refrence to find.</param>
        /// <returns>Reference to the named track.</returns>
        private AudioClip GetTrackFromManifest(string name) {
            foreach (NamedClip c in TrackManifest) {
                if (c.Name == name) return c.Clip;
            }

            return null;
        }

        /// <summary>
        /// Adjust the volume of the sound effects.
        /// </summary>
        public void AdjustSfxVolume() {
            sfxVolumeMultiplier = UIManager.Instance.GetSfxVolume();
            SaveManager.Instance.SaveVolumes(sfxVolumeMultiplier, UIManager.Instance.GetMusicVolume());
        }

        /// <summary>
        /// Adjust the volume of the music.
        /// </summary>
        public void AdjustMusicVolume() {
            musicVolumeMultiplier = UIManager.Instance.GetMusicVolume();
            SaveManager.Instance.SaveVolumes(UIManager.Instance.GetSfxVolume(), musicVolumeMultiplier);

            source.volume = musicVolumeMultiplier;
        }
    }

    /// <summary>
    /// Associates an audio clip with a string name for ease of reference.
    /// </summary>
    [System.Serializable]
    public class NamedClip {
        public string Name;
        public AudioClip Clip;

        public NamedClip(string name, AudioClip clip) {
            Name = name;
            Clip = clip;
        }
    }
}
