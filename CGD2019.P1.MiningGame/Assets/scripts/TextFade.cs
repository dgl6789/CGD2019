using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace App {
    public class TextFade : MonoBehaviour {

        [SerializeField] float lifetime;
        [SerializeField] float growthFactor;
        [SerializeField] float speed;
        float time;

        TextMeshPro textMesh;
        Camera cam;

        // Use this for initialization
        void Awake() {
            textMesh = GetComponent<TextMeshPro>();
            cam = Camera.main;

            time = lifetime;

            StartCoroutine(Fade());
        }

        public void Initialize(MineralItem item) {
            textMesh.color = item.Color;
            textMesh.text = string.Format("${0:n0}", item.Value);
        }

        private void LateUpdate() {
            // billboard to the camera
            transform.LookAt(cam.transform, cam.transform.up);
        }

        public IEnumerator Fade() {

            while (time > 0) {
                time -= Time.deltaTime;

                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, CurrentTime);

                transform.localScale *= 1 + (growthFactor * (1 - CurrentTime));

                transform.position += cam.transform.up * CurrentTime * speed;

                yield return null;
            }

            Destroy(gameObject);
        }

        float CurrentTime { get { return time / lifetime; } }
    }
}
