using UnityEngine;
using System.Collections;

public class Kocka : MonoBehaviour {

    public GameObject debugObj;
    public GameObject debugObj2;


    GameObject spriteObj;
    
    BoxCollider2D col;
    public LayerMask mask;
    public float walkSpeed = 6f;

    public float gravitation = 4f;
    private bool grounded = false;

    private float pushFromCellingAmount = 0.01f;

    private float raycastSkin = 0.0005f;

    Vector2 initialVelocity = Vector2.zero;
    //Vector2 velocityToApply = Vector2.zero;

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

    int count = 0;
    // Update is called once per frame
    void Update() {

        SetInitialVelocity();
        //Debug.Log("1# " + initialVelocity.x + ", " + initialVelocity.y);
        VerticalVelocityCalculation();
        //Debug.Log("2# " + initialVelocity.x + ", " + initialVelocity.y);
        HorizontalVelocityCalculation();
        //Debug.Log("3# " + initialVelocity.x + ", " + initialVelocity.y);

        // velocityToApply = initialVelocity;

        // Apply final position
        transform.position = new Vector3(transform.position.x + initialVelocity.x, transform.position.y + initialVelocity.y, transform.position.z);
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        // If movement was done on slope on this frame then consider character grounded and current initialVelocity.y 
        if (slopeDirVector != Vector2.left && slopeDirVector != Vector2.zero && slopeDirVector != Vector2.right) {
            grounded = true;
        }
    }
    
    void SetInitialVelocity()
    {
        // If grounded then reset it's current y velocity -> this needs to be done so y slope velocity can be reseted to 0
        if (grounded)
            initialVelocity.y = 0;
        // Apply gravity
        initialVelocity.y -= gravitation * Time.deltaTime;

        // Set initial horizontal movement    
        if (Input.GetKey(KeyCode.LeftArrow))
            initialVelocity.x = -walkSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.RightArrow))
            initialVelocity.x = walkSpeed * Time.deltaTime;
        else
            initialVelocity.x = 0;
        


        /*
        if (count++ > 4 && count < 8)
            initialVelocity.x = 0;
        else
            initialVelocity.x = walkSpeed * Time.deltaTime;
        */

        // Set initial vertical movement
        if (Input.GetKey(KeyCode.UpArrow) && grounded)
        {
            initialVelocity.y = jumpAmount;
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
        float rayDir = Mathf.Sign(initialVelocity.y);
        // Length of vertical velocity y component
        float rayLength = Mathf.Abs(initialVelocity.y);
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
                //Debug.Log("VERT HIT");
                // Set grounded state
                grounded = (rayDir < 0) ? true : grounded;
                // Apply calculations above only if this is the shortest vertical ray
                if (rayHit.distance < shortestRayHitDistance)
                {
                    shortestRayHitDistance = rayHit.distance;
                    initialVelocity.y = rayDir * (shortestRayHitDistance - raycastSkin);
                    // angle to rotate by
                    float horVelDir = Mathf.Sign(initialVelocity.x);
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
                // If vertical collision was with the top then give player negative velocity.y so it doesnt get stuck into the celling
                if (rayDir == 1)
                    initialVelocity.y = -pushFromCellingAmount;
            }
            // Debug rays
            // Debug.DrawRay(curRayStartPos, Vector2.down * rayLength, Color.green);
            // Debug.DrawRay(new Vector2(curRayStartPos.x + 0.005f, curRayStartPos.y), new Vector2(0, initialVelocity.y), Color.red);
        }

        /*
        Debug.Log(grounded);
        if (grounded)
            initialVelocity.y = 0;
        */
    }


    void HorizontalVelocityCalculation()
    {
        // Direction of horizontal velocity
        float rayDir = Mathf.Sign(initialVelocity.x);
        // Length of horizontal velocity x component
        float rayLength = Mathf.Abs(initialVelocity.x);
        // start casting horizontal ray but take in calculation previously calculated vertical velocity 'initialVelocity.y'
        Vector2 firstRayStartPos = new Vector2( transform.position.x + (rayDir * (col.bounds.size.x / 2 - raycastSkin)), transform.position.y - (col.bounds.size.y / 2 - raycastSkin) + initialVelocity.y );
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
                //Debug.Log("HOR HIT");
                // Apply calculations above only if this is the shortest vertical ray #### and if surface hit is under 90 deg angle -> ray--->|
                if (rayHit.distance < shortestRayHitDistance /*&& rayHit.normal.y == 0*/)
                {
                    shortestRayHitDistance = rayHit.distance;
                    initialVelocity.x = rayDir * (shortestRayHitDistance - raycastSkin);
                    slopeSurfaceHit = (rayHit.normal != Vector2.left && rayHit.normal != Vector2.right) ? true : false;
                }
            }
            //Instantiate(debugObj, new Vector2(curRayStartPos.x, curRayStartPos.y), Quaternion.identity);
            //Instantiate(debugObj2, new Vector2(curRayStartPos.x + initialVelocity.x, curRayStartPos.y), Quaternion.identity);
            // Debug rays
            // Debug.DrawRay(new Vector2(curRayStartPos.x + 0.005f, curRayStartPos.y), new Vector2(0, initialVelocity.y), Color.red);
        }

        // Debug.Log(grounded + ", " + slopeDirVector);
        //apply slope velocity
        //Debug.Log(slopeDirVector)
        if (grounded && initialVelocity.x != 0 && slopeDirVector != Vector2.left && slopeDirVector != Vector2.right)
        {
            initialVelocity.x = slopeDirVector.x * walkSpeed * Time.deltaTime;
            initialVelocity.y = initialVelocity.y + slopeDirVector.y * walkSpeed * Time.deltaTime;
            //Check if collision occures in the direction of slope movement vector
            Vector2 curRayStartPos = new Vector2(firstRayStartPos.x, firstRayStartPos.y + raycastSkin);
            RaycastHit2D rayHit = Physics2D.Raycast(curRayStartPos, initialVelocity.normalized, initialVelocity.magnitude, mask);
            Debug.Log(initialVelocity.x + ", " + initialVelocity.y);
            if (rayHit)
            {
                Debug.Log("HIT!");
                if (slopeDirVector.y < 0)
                {
                    float rayLengthReduceFactor = rayHit.distance / initialVelocity.magnitude;
                    initialVelocity.x *= rayLengthReduceFactor;
                    initialVelocity.y *= rayLengthReduceFactor;
                }
            }
        }

        if ((slopeDirVector == Vector2.right || slopeDirVector == Vector2.left) && slopeSurfaceHit)
        {
            initialVelocity.y += 0.025f;
        }

    }



}
