using UnityEngine;
using System.Collections;

public class Kocka : MonoBehaviour {

    GameObject spriteObj;
    
    BoxCollider2D col;
    public LayerMask mask;
    public float walkSpeed = 6f;

    public float gravitation = 10f;
    private bool grounded = false;

    private float raycastSkin = 0.0005f;

    Vector2 initialVelocity = Vector2.zero;
    Vector2 velocityToApply = Vector2.zero;

    float horRayDistance;
    float verRayDistance;
    int horRayNum = 12;
    int vertRayNum = 12;
    

    Vector2 slopeDirVector = Vector2.zero;

    void Awake()
    {
        col = transform.GetComponent<BoxCollider2D>();
        spriteObj = transform.Find("Sprite").gameObject;
    }

	// Use this for initialization
	void Start () {
        horRayDistance = (col.bounds.size.y - (raycastSkin * 2)) / (horRayNum - 1);
        verRayDistance = (col.bounds.size.x - (raycastSkin * 2)) / (vertRayNum - 1);
    }

    // Update is called once per frame
    void Update() {

        initialVelocity.y -= gravitation * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            initialVelocity.x = -walkSpeed;
        else if (Input.GetKey(KeyCode.RightArrow))
            initialVelocity.x = walkSpeed;

        VerticalVelocityCalculation();





        velocityToApply = initialVelocity;
        Debug.Log(slopeDirVector);

        // Apply final position
        transform.position = new Vector3(transform.position.x + velocityToApply.x, transform.position.y + velocityToApply.y, transform.position.z);

        // Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }



    void VerticalVelocityCalculation()
    {
        // Applies to first and last vertical ray. It removes posibility of vert. ray hitting vertical walls(90 degree walls).
        float startPosXOffset = 0.0005f;
        // Direction of vertical velocity
        float rayDir = Mathf.Sign(initialVelocity.y);
        // Length of vertical velocity y component
        float rayLength = Mathf.Abs(initialVelocity.y);
        Vector2 firstRayStartPos = new Vector2( transform.position.x - (col.bounds.size.x / 2 - raycastSkin - startPosXOffset), transform.position.y + (rayDir * (col.bounds.size.y / 2 - raycastSkin)) );
        // Shorest ray hit distance
        float shortestRayHitDistance = 99999.99f;
        // Cast vertical rays
        for (int i = 0; i < vertRayNum; ++i)
        {
            // Calculate start position of current ray
            Vector2 curRayStartPos = new Vector2(firstRayStartPos.x + verRayDistance * i, firstRayStartPos.y);
            curRayStartPos.x = (i == 1)                 ? curRayStartPos.x + startPosXOffset : curRayStartPos.x;
            curRayStartPos.x = (i == vertRayNum - 1)    ? curRayStartPos.x - startPosXOffset : curRayStartPos.x;
            // Cast ray
            RaycastHit2D rayHit = Physics2D.Raycast(curRayStartPos, new Vector2(0, rayDir), rayLength, mask);
            // Check if ray hit something
            if(rayHit)
            {
                // Apply calculations above only if this is the shortest vertical ray
                if (rayHit.distance < shortestRayHitDistance)
                {
                    shortestRayHitDistance = rayHit.distance;
                    initialVelocity.y = rayDir * (shortestRayHitDistance - raycastSkin);
                    // angle to rotate by
                    float horVelDir = Mathf.Abs(initialVelocity.x);
                    float up_floor_angle = 0;
                    up_floor_angle = (horVelDir == -1) ? Vector2.Angle(rayHit.normal, Vector2.right) : Vector2.Angle(rayHit.normal, Vector2.left);
                    float sin = Mathf.Sin(up_floor_angle * Mathf.Deg2Rad);
                    float cos = Mathf.Cos(up_floor_angle * Mathf.Deg2Rad);
                    // rotate vector in direction of a slope (calculate its x and y)
                    slopeDirVector = Vector2.up;
                    float tx = slopeDirVector.x;
                    float ty = slopeDirVector.y;
                    slopeDirVector.x = (cos * tx) - (sin * ty);
                    slopeDirVector.x = (horVelDir == -1) ? 1 : -1;
                    slopeDirVector.y = (sin * tx) + (cos * ty);
                }
            }
            Debug.DrawRay(curRayStartPos, Vector2.down * rayLength, Color.green);
            // Debug.DrawRay(new Vector2(curRayStartPos.x + 0.005f, curRayStartPos.y), new Vector2(0, initialVelocity.y), Color.red);
        }

    }



    /*
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
        */


}
