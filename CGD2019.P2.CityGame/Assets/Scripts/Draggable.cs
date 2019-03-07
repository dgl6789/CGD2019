using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// put on any object to make it draggable
/// </summary>
/// 
namespace App {
    public class Draggable : MonoBehaviour {
        RaycastHit2D touch;
        public static Draggable dragable;
        Vector3 lastPos;
        Vector3 lastMousePos;
        bool dragging = false;
        public SpriteRenderer renderer;
        Vector3 originalLscale;
        int phase = 0;
        int delta = 1;
        // Use this for initialization
        void Start() {
            dragable = null;
            lastPos = transform.position;
            lastMousePos = Input.mousePosition;
            originalLscale = transform.localScale;
        }
        private void Update() {
            if (Input.GetButtonDown("LeftMouse") && dragable == null) {
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {

                    if (((transform.position + new Vector3(0, 0, -10)) - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0))).magnitude < 1) {
                        transform.localScale = originalLscale * 1.4f;
                        dragable = this;
                    }


                    lastPos = transform.position;
                } else if (Application.platform == RuntimePlatform.Android) {


                    if (((transform.position + new Vector3(0, 0, -10)) - Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0))).magnitude < 1) {

                        dragable = this;
                        transform.localScale = originalLscale * 1.4f;


                    }

                    lastPos = transform.position;

                }
            }
            if (dragable == this || dragging == true) {
                dragging = true;
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
                    transform.position = new Vector3(0, 0, 10) + Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                    lastPos = transform.position;
                } else if (Application.platform == RuntimePlatform.Android) {
                    transform.position = new Vector3(0, 0, 10) + Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0));
                    lastPos = transform.position;
                }
                if (!Input.GetButton("LeftMouse")) {
                    dragable = null;
                    dragging = false;
                }
            } else {
                transform.localScale = originalLscale;
            }

            if (dragable != null && dragable.GetComponent<CivilianMovement>() != null) {
                CarManager.Instance.UpdateEndangeredCiviliansDragStates(dragable.GetComponent<CivilianMovement>());
            }
    }
        private void OnMouseOver() {

            /*if (renderer != null&&draggables.Count == 0||draggables.Contains(this))
            {
                renderer.color = Color.blue;
            }
            transform.localScale = originalLscale * 1.1f;
            if (Input.GetButton("LeftMouse"))
            {
            if (draggables.Count == 0 || draggables.Contains(this))
            {
                dragging = true;
                //transform.position = Input.mousePosition;
                transform.localScale = originalLscale * 1.4f;
                if(!draggables.Contains(this))
                draggables.Add(this);
            }
            }
            else
            {
                dragging = false;
            }*/

        }
        private void OnMouseExit() {



        }
    }
}