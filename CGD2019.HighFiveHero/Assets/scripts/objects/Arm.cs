using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {

    /// <summary>
    /// Arm class controls a line renderer that draws a bezier curve between two points,
    /// the shoulder and wrist of a dude.
    /// </summary>
    [ExecuteInEditMode]
    public class Arm : MonoBehaviour {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] bool drawGizmos;

        // 'Joint' transforms
        [SerializeField] Transform shoulder;
        [SerializeField] Transform elbow;
        [SerializeField] Transform wrist;

        [SerializeField] int detail;
        [SerializeField] float wristWidthModifier;

        /// <summary>
        /// Adjust the size of the wrist according to the size of the attached hand.
        /// </summary>
        /// <param name="handSize">Size of the hand.</param>
        public void AdjustWidthForHand(float handSize) { lineRenderer.endWidth = handSize * wristWidthModifier; }

        /// <summary>
        /// Adjust the joints in the arm to change its look.
        /// </summary>
        /// <param name="shoulderPosition">Position of the shoulder.</param>
        /// <param name="elbowPosition">Position of the elbow (Bezier point).</param>
        /// <param name="wristPosition">Position of the wrist (Hand).</param>
        public void AdjustJointPositions(Vector3 elbowPosition, Vector3 wristPosition, Vector3 shoulderPosition) {
            elbow.position = elbowPosition;
            wristPosition = wrist.position;
            
            shoulder.position = shoulderPosition;

            RenderArm();
        }

        /// <summary>
        /// Adjust the joints in the arm to change its look, keeping the shoulder position constant.
        /// </summary>
        /// <param name="elbowPosition">Position of the elbow (Bezier point).</param>
        /// <param name="wristPosition">Position of the wrist (Hand).</param>
        public void AdjustJointPositions(Vector3 elbowPosition, Vector3 wristPosition) {
            elbow.position = elbowPosition;
            wristPosition = wrist.position;

            RenderArm();
        }

#if UNITY_EDITOR
        /// <summary>
        /// In editor only, update the positions of the vertices in the line rendered corresponding
        /// to the positions of the joint transforms.
        /// </summary>
        private void Update() { RenderArm(); }
#endif

        private void RenderArm() {
            List<Vector3> points = new List<Vector3>();

            // Construct the list of points.
            for (float ratio = 0; ratio <= 1; ratio += 1.0f / detail) {
                points.Add(Vector2.Lerp(
                    Vector2.Lerp(shoulder.position, elbow.position, ratio),
                    Vector2.Lerp(elbow.position, wrist.position, ratio),
                    ratio));
            }

            // Apply it.
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        /// <summary>
        /// Optionally draw lines in the editor to visualize the logic of the curve.
        /// </summary>
        void OnDrawGizmos() {
            if (drawGizmos) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(shoulder.position, elbow.position);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(elbow.position, wrist.position);

                Gizmos.color = Color.red;
                for (float ratio = 0.5f / detail; ratio < 1; ratio += 1.0f / detail) {
                    Gizmos.DrawLine(Vector2.Lerp(shoulder.position, elbow.position, ratio),
                        Vector2.Lerp(elbow.position, wrist.position, ratio));
                }
            }
        }
    }
}
