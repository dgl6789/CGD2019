using System.Collections;
using UnityEngine;
using TMPro;

namespace App.UI {
    /// <summary>
    /// An indicator which spawns when the player is rewarded with something.
    /// </summary>
    public class Indicator : MonoBehaviour {

        [SerializeField] float lifetime;
        [SerializeField] float upwardSpeed;
        [SerializeField] float perfectDescriptionShakeAmount;

        bool perfect;

        [SerializeField] TextMeshPro countText;
        [SerializeField] TextMeshPro descriptionText;
        [SerializeField] SpriteRenderer sprite;

        /// <summary>
        /// Initializes text fields.
        /// </summary>
        /// <param name="count">Number of the stuff to indicate, like points or seconds.</param>
        /// <param name="description">Description of the indicator, like 'Perfect' or 'Bad.'</param>
        /// <param name="perfect">Whether the high five was 'Perfect.'</param>
        public void Initialize(int count, string description = "", bool perfect = false, float delay = 0) {
            countText.text = (count > 0 ? "+" : "") + count;
            descriptionText.text = perfect ? "PERFECT!" : description;

            this.perfect = perfect;

            StartCoroutine(RiseAndFade(delay));
        }

        /// <summary>
        /// Causes the timer to rise upward and fade to 0% opacity over the lifetime.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator RiseAndFade(float delay = 0) {
            yield return new WaitForSecondsRealtime(delay);

            float timer = lifetime;
            Vector2 descriptionTextPosition = descriptionText.transform.localPosition;

            while(timer > 0) {
                float p = timer / lifetime;

                countText.color = descriptionText.color = new Color(1f, 1f, 1f, p);
                if (sprite) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, p);

                // Shake the text if it was a perfect five.
                descriptionText.transform.localPosition = descriptionTextPosition + (perfect ? Random.insideUnitCircle * perfectDescriptionShakeAmount : Vector2.zero);

                // Move the indicator upward.
                transform.Translate(new Vector2(0, upwardSpeed) * Time.deltaTime);

                timer -= Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}