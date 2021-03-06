﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    /// <summary>
    /// Functionality for a button that swaps the color theme of the app.
    /// </summary>
    public class MaskChoice : MonoBehaviour
    {
        float Vibrating = 0;
        Vector3 startingPos;
        // Component references
        [SerializeField] int Mask;
        [SerializeField] Image swatch;
        [SerializeField] GameObject checkmark;

        // Get the material associated with this button from the image component.
        public Material Material
        {
            get { return swatch.material; }
        }

        bool chosen;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            startingPos = transform.position;
            checkmark.SetActive(chosen);
        }
        private void Update()
        {
            if (App.SaveManager.Instance.LoadedData.Masks[Mask] || Mask == 0)
            {
                transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(2).gameObject.SetActive(true);
            }
            if (Vibrating > 0)
            {
                float xOffset = Mathf.Lerp(-10f, 10f, Mathf.Sin((Time.realtimeSinceStartup * 100f) + 1 / 2));

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
        public void OnTap()
        {
            if (!RingManager.Instance.unlockMask(Mask))
            {
                Vibrating = .5f;
            }
            else
            {
                App.SaveManager.Instance.LoadedData.currentMask = Mask;
            }
        }

        /// <summary>
        /// Toggle the active state of the swatch.
        /// </summary>
        public void ToggleOn()
        {
            chosen = true;
            checkmark.SetActive(true);
        }

        /// <summary>
        /// Toggle the active state of the swatch.
        /// </summary>
        public void ToggleOff()
        {
            chosen = false;
            checkmark.SetActive(false);
        }
    }
}
