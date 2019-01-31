﻿using System.Collections;
using UnityEngine;

namespace App.Util {

    /// <summary>
    /// Chooses a random mesh from an array on start.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class DebrisParticle : MonoBehaviour {

        [SerializeField] Mesh[] Meshes;

        [SerializeField] Vector2 startScale;
        [SerializeField] float shrinkRate;
        [SerializeField] float sizeMinimum;

        [SerializeField] Vector2 angularVelocity;
        [SerializeField] float velocityUpwardBias;
        [SerializeField] Vector2 velocity;

        [SerializeField] float gravityScale;

        Rigidbody rb;

        // Choose a mesh and starting conditions
        void Start() {
            GetComponent<MeshFilter>().mesh = Meshes[Random.Range(0, Meshes.Length)];

            float r = Random.Range(startScale.x, startScale.y);
            transform.localScale = new Vector3(r, r, r);

            rb = GetComponent<Rigidbody>();

            rb.angularVelocity = new Vector3(
                Random.Range(angularVelocity.x, angularVelocity.y),
                Random.Range(angularVelocity.x, angularVelocity.y),
                Random.Range(angularVelocity.x, angularVelocity.y));

            rb.velocity = new Vector3(
                Random.Range(velocity.x, velocity.y),
                Random.Range(velocity.x, velocity.y),
                Random.Range(velocity.x, velocity.y)) 
                + (Camera.main.transform.up * velocityUpwardBias);

            StartCoroutine(Shrink());
        }

        private void FixedUpdate() {
            rb.AddForce(-Camera.main.transform.up * gravityScale);
        }

        public IEnumerator Shrink() {
            while(transform.localScale.sqrMagnitude > sizeMinimum) {
                transform.localScale *= shrinkRate;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
