using UnityEngine;
using System.Collections;

public class Kocka : MonoBehaviour {

    enum HOR_MOVEMENT
    {
        LEFT,
        RIGHT,
        NONE
    }
    HOR_MOVEMENT hor_movement = HOR_MOVEMENT.NONE;

    BoxCollider2D col;
    public LayerMask mask;
    private float speed = 2f;
    private float gravitation = 8f;
    private float baseRayLength = 0.1f;

    float horRayDistance;
    int horRayNum = 10;

    private float skin = 0.1f;

    void Awake()
    {
        col = transform.GetComponent<BoxCollider2D>();
    }

	// Use this for initialization
	void Start () {
        horRayDistance = col.bounds.size.x / (horRayNum - 1);
    }
	
	// Update is called once per frame
	void Update () {        

        Vector3 finalPos = transform.position;

        if (Input.GetKey(KeyCode.LeftArrow))
            hor_movement = HOR_MOVEMENT.LEFT;
        else if (Input.GetKey(KeyCode.RightArrow))
            hor_movement = HOR_MOVEMENT.RIGHT;
        else
            hor_movement = HOR_MOVEMENT.NONE;



        float possibleYGravAmount = gravitation * Time.deltaTime;

        // ray hit info
        bool didRayHitGround = false;
        float longestRayHitDist = 0;
        Vector2 normal = Vector2.zero;
        Vector2 rayPos = Vector2.zero;

        // find shortest vertical ray hit
        for (int i = 0; i < horRayNum; ++i)
        {
            Vector2 startRaycastPos = GetBottomLeftRayBegin();
            startRaycastPos.x += horRayDistance * i;
            // cast ray
            RaycastHit2D hit = Physics2D.Raycast(startRaycastPos, Vector2.down, possibleYGravAmount + skin, mask);
            if(hit.collider)
            {                
                if (!didRayHitGround)
                {
                    longestRayHitDist = hit.distance;
                    normal = hit.normal;
                    rayPos = startRaycastPos;
                    didRayHitGround = true;
                }
                else
                {
                    longestRayHitDist = (hit.distance > longestRayHitDist) ? hit.distance : longestRayHitDist;
                    normal = (hit.distance > longestRayHitDist) ? hit.normal : normal;
                    rayPos = (hit.distance > longestRayHitDist) ? startRaycastPos : rayPos;
                }               
            }            
        }
        Debug.DrawRay(rayPos, Vector2.down * longestRayHitDist, Color.blue);

        // If collided with vertical bottom then move to that y intersection position
        if (didRayHitGround)
        {
            finalPos = new Vector3(finalPos.x, finalPos.y - (longestRayHitDist - skin), 0);
            /*
            if (hor_movement != HOR_MOVEMENT.NONE)
            {
                // angle to rotate by
                float up_floor_angle = 0;
                if (hor_movement == HOR_MOVEMENT.LEFT)
                    up_floor_angle = Vector2.Angle(normal, Vector2.right);
                else if (hor_movement == HOR_MOVEMENT.RIGHT)
                    up_floor_angle = Vector2.Angle(normal, Vector2.left);
                float sin = Mathf.Sin(up_floor_angle * Mathf.Deg2Rad);
                float cos = Mathf.Cos(up_floor_angle * Mathf.Deg2Rad);

                // rotate vector in direction of a slope (calculate its x and y)
                Vector2 floorDir = Vector2.up;
                float tx = floorDir.x;
                float ty = floorDir.y;
                floorDir.x = (cos * tx) - (sin * ty);
                if (hor_movement == HOR_MOVEMENT.LEFT)
                    floorDir.x *= 1;
                else if (hor_movement == HOR_MOVEMENT.RIGHT)
                    floorDir.x *= -1;
                floorDir.y = (sin * tx) + (cos * ty);
                // Debug.DrawRay(hit.point, floorDir * speed * Time.deltaTime, Color.blue);

                // calculate final position
                finalPos = new Vector3(finalPos.x + floorDir.x * speed * Time.deltaTime, finalPos.y - (longestRayHitDist - skin) + floorDir.y * speed * Time.deltaTime, 0);
            }
            else
            {
                finalPos = new Vector3(finalPos.x, finalPos.y - (longestRayHitDist - skin), 0);
            } 
            */
        }
        // Apply gravitation
        else
        {
            finalPos = new Vector3(finalPos.x, finalPos.y - possibleYGravAmount, 0);
        }


        // Apply final position
        transform.position = finalPos;

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }





    Vector2 GetBottomLeftRayBegin()
    {
        return new Vector2( transform.position.x - col.size.x / 2, 
                            transform.position.y - col.size.y / 2 + skin);
    }



}
