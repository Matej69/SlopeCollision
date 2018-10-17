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
    private float speed = 5f;
    private float gravitation = 0.38f;
    private float baseRayLength = 0.1f;
    Vector2 velocityToApply = Vector2.zero;

    bool grounded = false;

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
	void Update () {

        if (Input.GetKey(KeyCode.LeftArrow))
            hor_movement = HOR_MOVEMENT.LEFT;
        else if (Input.GetKey(KeyCode.RightArrow))
            hor_movement = HOR_MOVEMENT.RIGHT;
        else
            hor_movement = HOR_MOVEMENT.NONE;

        if (grounded && Input.GetKeyDown(KeyCode.UpArrow))
        {
            velocityToApply.y = 0.15f;
            grounded = false;
        }

        // rotate sprite hor_movement
        Vector3 spriteScale = spriteObj.transform.localScale;
        if (hor_movement == HOR_MOVEMENT.LEFT)
            spriteScale.x = -Mathf.Abs(spriteScale.x);
        if (hor_movement == HOR_MOVEMENT.RIGHT)
            spriteScale.x = Mathf.Abs(spriteScale.x);
        spriteObj.transform.localScale = spriteScale;


        velocityToApply.y -= gravitation * Time.deltaTime;

        // ray hit info
        bool didRayHitSurface = false;
        float shortestRayHitDist = 0;
        Vector2 normal = Vector2.zero;
        Vector2 rayPos = Vector2.zero;

        // find shortest vertical ray hit
        for (int i = 0; i < horRayNum; ++i)
        {
            Vector2 rayDir = (velocityToApply.y > 0) ? Vector2.up : Vector2.down;
            Vector2 startRaycastPos = (velocityToApply.y > 0) ? GetTopLeftRayBegin() : GetBottomLeftRayBegin();
            startRaycastPos.x += horRayDistance * i;
            // cast ray
            RaycastHit2D hit = Physics2D.Raycast(startRaycastPos, rayDir, Mathf.Abs(velocityToApply.y) + skin, mask);
            if(hit.collider)
            {                
                if (!didRayHitSurface)
                {
                    shortestRayHitDist = hit.distance;
                    normal = hit.normal;
                    rayPos = startRaycastPos;
                    didRayHitSurface = true;
                }
                else
                {
                    shortestRayHitDist = (hit.distance < shortestRayHitDist) ? hit.distance : shortestRayHitDist;
                    normal = (hit.distance < shortestRayHitDist) ? hit.normal : normal;
                    rayPos = (hit.distance < shortestRayHitDist) ? startRaycastPos : rayPos;
                }               
            }            
        }
        Debug.DrawRay(rayPos, ((velocityToApply.y > 0) ? Vector2.up : Vector2.down) * shortestRayHitDist, Color.red);

        // If collided with ground
        if (didRayHitSurface)
        {
            // if ray hit surface on bottom
            if (velocityToApply.y < 0)
            {
                grounded = true;

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

                    velocityToApply = new Vector2(floorDir.x * speed * Time.deltaTime, -(shortestRayHitDist - skin) + floorDir.y * speed * Time.deltaTime);
                }
                // if there is no horizontal movement -> move to the position of collision (place on ground)
                else
                {
                    velocityToApply = new Vector2(0, -(shortestRayHitDist - skin));
                }
            }
            // if ray hit surface on top
            else
            {
                velocityToApply = new Vector2(velocityToApply.x, 0);
            }

        }
        // If there was no collision with ground -> apply gravitation force
        else
        {
            velocityToApply = new Vector2(0, velocityToApply.y);
        }

        // Apply final position
        transform.position = new Vector3(transform.position.x + velocityToApply.x, transform.position.y + velocityToApply.y, transform.position.z);

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }





    Vector2 GetBottomLeftRayBegin()
    {
        return new Vector2( transform.position.x - col.bounds.size.x / 2, 
                            transform.position.y - col.bounds.size.y / 2 + skin);
    }

    Vector2 GetTopLeftRayBegin()
    {
        return new Vector2(transform.position.x - col.bounds.size.x / 2,
                            transform.position.y + col.bounds.size.y / 2 - skin);
    }



}
