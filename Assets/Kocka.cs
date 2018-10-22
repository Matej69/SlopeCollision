using UnityEngine;
using System.Collections;

public class Kocka : MonoBehaviour {

    GameObject spriteObj;

    enum HOR_MOVEMENT
    {
        LEFT,
        RIGHT,
        NONE
    }
    HOR_MOVEMENT hor_movement = HOR_MOVEMENT.NONE;

    BoxCollider2D col;
    public LayerMask mask;
    private float speed = 6f;
    public float gravitation = 15f;
    private float gravitationToApply = 0;
    private bool grounded = false;
    Vector2 velocityToApply = Vector2.zero;

    public float jumpDesc = 1f;
    private float curJumpForce = 0;
    public float maxJump = 0.2f;

    float horRayDistance;
    int horRayNum = 30;

    private float skin = 0.1f;

    void Awake()
    {
        col = transform.GetComponent<BoxCollider2D>();
        spriteObj = transform.Find("Sprite").gameObject;
    }

	// Use this for initialization
	void Start () {
        horRayDistance = col.bounds.size.x / (horRayNum - 1);
    }

    // Update is called once per frame
    void Update() {
        
        // handle jumping
        velocityToApply.y += curJumpForce * Time.deltaTime;
        curJumpForce -= jumpDesc * Time.deltaTime;
        if (curJumpForce <= 0)
            curJumpForce = 0;

            if (Input.GetKey(KeyCode.LeftArrow))
        {
            hor_movement = HOR_MOVEMENT.LEFT;
            velocityToApply.x = -speed * Time.deltaTime;
        }            
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            hor_movement = HOR_MOVEMENT.RIGHT;
            velocityToApply.x = speed * Time.deltaTime;
        }
        else
        {
            hor_movement = HOR_MOVEMENT.NONE;
            velocityToApply.x = 0;
        }

        // rotate spritehor_movement
        Vector3 spriteScale = spriteObj.transform.localScale;
        if (hor_movement == HOR_MOVEMENT.LEFT)
            spriteScale.x = -Mathf.Abs(spriteScale.x);
        if (hor_movement == HOR_MOVEMENT.RIGHT)
            spriteScale.x = Mathf.Abs(spriteScale.x);
        spriteObj.transform.localScale = spriteScale;

        gravitationToApply += gravitation * Time.deltaTime;
        float possibleYGravAmount = gravitationToApply * Time.deltaTime;










        /**
         * HANDLE BOTTOM COLLISION
         */

        // ray hit info
        bool didRayHitGround = false;
        float shortestRayHitDist = 0;
        Vector2 normal = Vector2.zero;
        Vector2 rayPos = Vector2.zero;

        // find shortest vertical ray hit ground
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
                    shortestRayHitDist = hit.distance;
                    normal = hit.normal;
                    rayPos = startRaycastPos;
                    didRayHitGround = true;
                }
                else
                {
                    shortestRayHitDist = (hit.distance < shortestRayHitDist) ? hit.distance : shortestRayHitDist;
                    normal = (hit.distance < shortestRayHitDist) ? hit.normal : normal;
                    rayPos = (hit.distance < shortestRayHitDist) ? startRaycastPos : rayPos;
                }               
            }            
        }
        Debug.DrawRay(rayPos, Vector2.down * shortestRayHitDist, Color.red);


        Vector2 holdVelocityForSlope;
        // If collided with ground
        if (didRayHitGround)
        {
            gravitationToApply = 0;
            grounded = true;
            curJumpForce = 0;
            // If character vertical velocity is down
            if (Input.GetKey(KeyCode.UpArrow) && grounded)
            {
                curJumpForce = maxJump;
                grounded = false;
            }
            

            // move to that y intersection position
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

                // set velocity depending on slope
                holdVelocityForSlope = new Vector2(floorDir.x * speed * Time.deltaTime, curJumpForce + - (shortestRayHitDist - skin) + floorDir.y * speed * Time.deltaTime); 
            }
            // if there is no horizontal movement -> move to the position of collision (place on ground)
            else
            {
                holdVelocityForSlope = new Vector2(0, curJumpForce + - (shortestRayHitDist - skin));
            }
        }
        // If there was no collision with ground -> apply gravitation force
        else
        {
            holdVelocityForSlope = new Vector2(velocityToApply.x, curJumpForce - possibleYGravAmount);
        }






        bool didRayHitTop = false;
        // Handle top collision
        if (velocityToApply.y > 0)
        {
            // find shortest vertical ray hit ground
            for (int i = 0; i < horRayNum; ++i)
            {
                Vector2 startRaycastPos = new Vector2(transform.position.x - col.bounds.size.x / 2, transform.position.y + col.bounds.size.y / 2 - skin);
                startRaycastPos.x += horRayDistance * i;
                // cast ray
                RaycastHit2D hit = Physics2D.Raycast(startRaycastPos, Vector2.up, velocityToApply.y + skin, mask);
                if (hit.collider)
                {
                    velocityToApply.y = -Mathf.Abs(velocityToApply.y / 2);
                    curJumpForce = 0;
                    didRayHitTop = true;
                }
            }
        }

        if (!didRayHitTop)
            velocityToApply = holdVelocityForSlope;








        // Apply final position
        transform.position = new Vector3(transform.position.x + velocityToApply.x, transform.position.y + velocityToApply.y, transform.position.z);

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }





    Vector2 GetBottomLeftRayBegin()
    {
        return new Vector2( transform.position.x - col.bounds.size.x / 2, 
                            transform.position.y - col.bounds.size.y / 2 + skin);
    }



}
