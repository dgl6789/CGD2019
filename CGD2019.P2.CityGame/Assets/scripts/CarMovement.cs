using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{

    public class CarMovement : MonoBehaviour
    {

        float speed;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        Vector2 direction;
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        [SerializeField] float killRadius;

        [SerializeField] GameObject bloodSplatter;
        [SerializeField] GameObject trail;

        Dictionary<CivilianMovement, bool> endangeredCivilians;

        Bounds worldBounds;

        // Use this for initialization
        void Start()
        {
            endangeredCivilians = new Dictionary<CivilianMovement, bool>();

            Rotate();

            worldBounds = OrthographicBounds();
        }

        // Update is called once per frame
        void Update()
        {
            worldBounds = OrthographicBounds();

            if (OutOfBounds()) {

                foreach(KeyValuePair<CivilianMovement, bool> kvp in endangeredCivilians) {
                    // for each civilian in the list that isnt dead and was moved since the car entered, add a good thing.
                    
                    if(kvp.Value) {
                        if (CivilianManager.Instance.CivilianList.Contains(kvp.Key)) ScoreManager.Instance.DoGoodThing(); // for a living civilian, add a good thing.
                        else ScoreManager.Instance.DoBadThing(); // for a dead one, add a bad thing.
                    }
                }

                CarManager.Instance.Cars.Remove(this);
                Destroy(gameObject);
            }

            Move();

            CheckCivilianCollision();
            UpdatePotentialCivilianCollisions();
        }

        //method to move the car
        void Move()
        {
            Vector3 currentPos = transform.position;

            currentPos.x += direction.x * speed;
            currentPos.y += direction.y * speed;

            transform.position = currentPos;
        }

        //method to rotate sprite
        void Rotate()
        {
            if (direction.x == 1)
                transform.Rotate(Vector3.forward * 180);
            else if (direction.y == 1)
                transform.Rotate(Vector3.forward * -90);
            else if (direction.y == -1)
                transform.Rotate(Vector3.forward * 90);
        }

        void CheckCivilianCollision() {
            List<CivilianMovement> toDestroy = new List<CivilianMovement>();

            foreach(CivilianMovement c in CivilianManager.Instance.CivilianList) {
                if(Vector2.Distance(c.transform.position, transform.position) < killRadius) {

                    // TODO: replace the civilian with a blood splatter
                    Destroy(Instantiate(bloodSplatter, c.transform.position, Quaternion.identity), 5);

                    toDestroy.Add(c);

                    // TODO: enable the blood streak renderer for this car
                    BloodTrail t = Instantiate(trail, transform.position, Quaternion.identity).GetComponent<BloodTrail>();
                    t.SetEmitter(transform);

                }
            }

            CivilianMovement[] d = toDestroy.ToArray();
            for(int i = 0; i < d.Length; i++) {
                d[i].Die();
            }
        }

        void UpdatePotentialCivilianCollisions() {
            // Make a list of all the civilians in this car's path.
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, killRadius, direction * 200f);
            
            foreach(RaycastHit2D hit in hits) {
                CivilianMovement c = hit.collider.GetComponent<CivilianMovement>();

                if (c == null) continue;

                if(!endangeredCivilians.ContainsKey(c)) {
                    endangeredCivilians.Add(c, false);
                }
            }
        }

        public void CivilianWasMoved(CivilianMovement c) {
            if (endangeredCivilians.ContainsKey(c)) endangeredCivilians[c] = true;
        }

        //method to determine if the car is out of bounds
        bool OutOfBounds()
        {
            return (transform.position.x > worldBounds.max.x || transform.position.x < worldBounds.min.x || transform.position.y > worldBounds.max.y || transform.position.y < worldBounds.min.y);
        }

        //method to determine bounds of screen
        public static Bounds OrthographicBounds() {
            SpriteRenderer r = CivilianManager.Instance.worldRenderer;

            return new Bounds(r.transform.position, new Vector3(r.sprite.bounds.extents.x * 2.5f * r.transform.localScale.x, r.sprite.bounds.extents.y * 2.5f * r.transform.localScale.y, 1));
        }
    }
}
