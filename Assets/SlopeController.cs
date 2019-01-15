using UnityEngine;
using System.Collections;

public class SlopeController : MonoBehaviour {

    GameObject spriteObj;
    
    BoxCollider2D col;
    public LayerMask mask;
    public float walkSpeed = 6f;

    public float gravitation = 4f;
    private bool grounded = false;

    private float pushFromCellingAmount = 0.01f;

    private float raycastSkin = 0.0005f;
    
    Vector2 velocity = Vector2.zero;

    float horRayDistance;
    float verRayDistance;

    public float jumpAmount = 5f;
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

        SetInitialVelocity();
        VerticalVelocityCalculation();
        HorizontalVelocityCalculation();

        // Apply final position
        transform.position = new Vector3(transform.position.x + velocity.x, transform.position.y + velocity.y, transform.position.z);
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        
    }
    
    void SetInitialVelocity()
    {
        // If grounded then reset it's current y velocity -> this also resets any y velocity from slope movement
        if (grounded)
            velocity.y = 0;
        // Apply gravity
        velocity.y -= gravitation * Time.deltaTime;

        // Set initial horizontal movement    
        if (Input.GetKey(KeyCode.LeftArrow))
            velocity.x = -walkSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.RightArrow))
            velocity.x = walkSpeed * Time.deltaTime;
        else
            velocity.x = 0;

        // Set initial vertical movement
        if (Input.GetKey(KeyCode.UpArrow) && grounded)
        {
            velocity.y = jumpAmount;
            grounded = false;
        }

    }


    void VerticalVelocityCalculation()
    {
        // Set grounded to false and if at least one vertical ray hit then set it to true
        grounded = false;

        // set initial 'slopeDirVector' that will change if character is not standing on flat surface
        slopeDirVector = Vector2.zero;

        // Applies to first and last vertical ray. It removes posibility of vert. ray hitting vertical walls(90 degree walls).
        float startPosXOffset = 0.0005f;
        // Direction of vertical velocity
        float rayDir = Mathf.Sign(velocity.y);
        // Length of vertical velocity y component
        float rayLength = Mathf.Abs(velocity.y);
        Vector2 firstRayStartPos = new Vector2( transform.position.x - col.bounds.size.x / 2 + raycastSkin + startPosXOffset, transform.position.y + (rayDir * (col.bounds.size.y / 2 - raycastSkin)) );
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
                // Set grounded state
                grounded = (rayDir < 0) ? true : grounded;
                // Apply calculations above only if this is the shortest vertical ray
                if (rayHit.distance < shortestRayHitDistance)
                {
                    shortestRayHitDistance = rayHit.distance;
                    velocity.y = rayDir * (shortestRayHitDistance - raycastSkin);
                    // angle to rotate by
                    float horVelDir = Mathf.Sign(velocity.x);
                    float up_floor_angle = 0;
                    up_floor_angle = (horVelDir == -1) ? -Vector2.Angle(rayHit.normal, Vector2.right) : Vector2.Angle(rayHit.normal, Vector2.left);
                    float sin = Mathf.Sin(up_floor_angle * Mathf.Deg2Rad);
                    float cos = Mathf.Cos(up_floor_angle * Mathf.Deg2Rad);
                    // rotate vector in direction of a slope (calculate its x and y)
                    slopeDirVector = Vector2.down;
                    float tx = slopeDirVector.x;
                    float ty = slopeDirVector.y;
                    slopeDirVector.x = (cos * tx) - (sin * ty);
                    slopeDirVector.y = ((sin * tx) + (cos * ty)) * -1;
                }
                // If vertical collision was with the top then give player negative velocity.y so it doesnt get stuck into the celling for couple frames
                if (rayDir == 1)
                    velocity.y = -pushFromCellingAmount;
            }
        }
    }


    void HorizontalVelocityCalculation()
    {
        // Direction of horizontal velocity
        float rayDir = Mathf.Sign(velocity.x);
        // Length of horizontal velocity x component
        float rayLength = Mathf.Abs(velocity.x);
        // start casting horizontal ray but take in calculation previously calculated vertical velocity 'velocity.y'
        Vector2 firstRayStartPos = new Vector2( transform.position.x + (rayDir * (col.bounds.size.x / 2 - raycastSkin)), transform.position.y - (col.bounds.size.y / 2 - raycastSkin) + velocity.y );
        // Shorest ray hit distance
        float shortestRayHitDistance = 99999.99f;
        // vertical surface hit
        bool slopeSurfaceHit = false;
        // Cast horizontal rays
        for (int i = 0; i < horRayNum; ++i)
        {
            // Calculate start position of current ray
            Vector2 curRayStartPos = new Vector2(firstRayStartPos.x, firstRayStartPos.y + horRayDistance * i);
            // Cast ray
            RaycastHit2D rayHit = Physics2D.Raycast(curRayStartPos, new Vector2(rayDir, 0), rayLength, mask);
            // Check if ray hit something
            if (rayHit)
            {
                // Apply calculations above only if this is the shortest vertical ray #### and if surface hit is under 90 deg angle -> ray--->|
                if (rayHit.distance < shortestRayHitDistance /*&& rayHit.normal.y == 0*/)
                {
                    shortestRayHitDistance = rayHit.distance;
                    velocity.x = rayDir * (shortestRayHitDistance - raycastSkin);
                    slopeSurfaceHit = (rayHit.normal != Vector2.left && rayHit.normal != Vector2.right) ? true : false;
                }
            }
        }

        //apply slope velocity
        if (grounded && velocity.x != 0 && slopeDirVector != Vector2.left && slopeDirVector != Vector2.right)
        {
            // Set possible slope velocity
            velocity.x = slopeDirVector.x * walkSpeed * Time.deltaTime;
            velocity.y = velocity.y + slopeDirVector.y * walkSpeed * Time.deltaTime;
            // Check if collision occures in the direction of slope movement vector
            Vector2 curRayStartPos = new Vector2(firstRayStartPos.x, firstRayStartPos.y + raycastSkin);
            RaycastHit2D rayHit = Physics2D.Raycast(curRayStartPos, velocity.normalized, velocity.magnitude, mask);
            if (rayHit)
            {
                // If surface was hit while moving down/up the slope then don't just move it down/up the slope by velocity but limit that velocity by distance of surface that was hit
                // This removes possibility of character moving through flat surface while moving down the slope 
                float rayLengthReduceFactor = rayHit.distance / velocity.magnitude;
                velocity.x *= rayLengthReduceFactor;
                velocity.y *= rayLengthReduceFactor;
            }
        }

        // If character is currently on flat surface but next frame will be on up slope then push character up so it doesnt get stuck between flat surface and slope
        if ((slopeDirVector == Vector2.right || slopeDirVector == Vector2.left) && slopeSurfaceHit)
        {
            velocity.y += 0.025f;
        }

    }



}
