using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    // Objects to drag in
    //public MovementMotor motor;
    private Vector3 cameraOffset = Vector3.zero;
    public float cameraPreview = 2.0f;
    public float cameraSmoothing = 0.01f;
    private Vector3 cameraVelocity = Vector3.zero;
    public Transform character;
    public float cursorFacingCamera;
    private Transform cursorObject;
    public float cursorPlaneHeight;
    public GameObject cursorPrefab;
    private Vector3 cursorScreenPosition;
    public float cursorSmallerWhenClose = 1;
    public float cursorSmallerWithDistance;
    private Vector3 facingDirection;
    private Vector3 initOffsetToPlayer;
    private GameObject joystickRightGO;

    // Private memeber data
    private Camera mainCamera;

    //private Joystick joystickLeft;
    //private Joystick joystickRight;

    private Transform mainCameraTransform;

    private Plane playerMovementPlane;

    private Vector3 screenMovementForward;
    private Vector3 screenMovementRight;
    private Quaternion screenMovementSpace;
    public float turningSmoothing = 0.3f;

    private void Awake()
    {
        //motor.movementDirection = Vector2.zero;
        //motor.facingDirection = Vector2.zero;

        // Set main camera
        mainCamera = Camera.main;
        mainCameraTransform = mainCamera.transform;

        // Ensure we have character set
        // Default to using the transform this component is on
        if (!character)
            character = transform;

        initOffsetToPlayer = mainCameraTransform.position - character.position;

#if UNITY_IPHONE || UNITY_ANDROID
		//new edit
		
		//if (joystickPrefab) {
			// Create left joystick
		//	GameObject joystickLeftGO = Instantiate (joystickPrefab) as GameObject;
		//	joystickLeftGO.name = "Joystick Left";
		//	joystickLeft = joystickLeftGO.GetComponent<Joystick> ();
			
			// Create right joystick
		//	joystickRightGO = Instantiate (joystickPrefab) as GameObject;
		//	joystickRightGO.name = "Joystick Right";
		//	joystickRight = joystickRightGO.GetComponent<Joystick> ();			
		//}
		if (cursorPrefab)
        {
            cursorObject = (Instantiate(cursorPrefab) as GameObject).transform;
        }
#else
        if (cursorPrefab)
        {
            cursorObject = (Instantiate(cursorPrefab) as GameObject).transform;
        }
#endif

        // Save camera offset so we can use it in the first frame
        cameraOffset = mainCameraTransform.position - character.position;

        // Set the initial cursor position to the center of the screen
        cursorScreenPosition = new Vector3(0.5f*Screen.width, 0.5f*Screen.height, 0);

        // caching movement plane
        playerMovementPlane = new Plane(character.up, character.position + character.up*cursorPlaneHeight);
    }

    private void Start()
    {
#if UNITY_IPHONE || UNITY_ANDROID
    // Move to right side of screen
		//new edit
		//GUITexture guiTex = joystickRightGO.GetComponent<GUITexture>();
		//guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;
		screenMovementSpace = Quaternion.Euler(0, mainCameraTransform.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace*Vector3.forward;
        screenMovementRight = screenMovementSpace*Vector3.right;
#endif

        // it's fine to calculate this on Start () as the camera is static in rotation

        screenMovementSpace = Quaternion.Euler(0, mainCameraTransform.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace*Vector3.forward;
        screenMovementRight = screenMovementSpace*Vector3.right;
    }

    private void OnDisable()
    {
        //if (joystickLeft) 
        //	joystickLeft.enabled = false;

        //if (joystickRight)
        //	joystickRight.enabled = false;
    }

    private void OnEnable()
    {
        //if (joystickLeft) 
        //	joystickLeft.enabled = true;

        //if (joystickRight)
        //	joystickRight.enabled = true;
    }

    private void FixedUpdate()
    {
        // Handle the movement of the character
        //Vector3 targetVelocity = movementDirection * walkingSpeed;
        //Vector3 deltaVelocity = targetVelocity - rigidbody.velocity;
        //if (rigidbody.useGravity)
        //	deltaVelocity.y = 0;
        //rigidbody.AddForce (deltaVelocity * walkingSnappyness, ForceMode.Acceleration);

        // Setup player to face facingDirection, or if that is zero, then the movementDirection
        Vector3 faceDir = facingDirection;

        //if (faceDir == Vector3.zero)
        //	faceDir = movementDirection;

        // Make the character rotate towards the target rotation
        //if (faceDir == Vector3.zero) {
        //	rigidbody.angularVelocity = Vector3.zero;
        //}
        //else {
        float rotationAngle = AngleAroundAxis(transform.up, faceDir, Vector3.forward);
        rigidbody.angularVelocity = (Vector3.forward*rotationAngle*turningSmoothing);
        //}
    }

    private void Update()
    {
        // HANDLE CHARACTER MOVEMENT DIRECTION
#if UNITY_IPHONE || UNITY_ANDROID
        //motor.movementDirection = joystickLeft.position.x * screenMovementRight + joystickLeft.position.y * screenMovementForward;
#else
        //motor.movementDirection = Input.GetAxis ("Horizontal") * screenMovementRight + Input.GetAxis ("Vertical") * screenMovementForward;
#endif

        // Make sure the direction vector doesn't exceed a length of 1
        // so the character can't move faster diagonally than horizontally or vertically
        //if (motor.movementDirection.sqrMagnitude > 1)
        //	motor.movementDirection.Normalize();


        // HANDLE CHARACTER FACING DIRECTION AND SCREEN FOCUS POINT

        // First update the camera position to take into account how much the character moved since last frame
        //mainCameraTransform.position = Vector3.Lerp (mainCameraTransform.position, character.position + cameraOffset, Time.deltaTime * 45.0ff * deathSmoothoutMultiplier);

        // Set up the movement plane of the character, so screenpositions
        // can be converted into world positions on this plane
        //playerMovementPlane = new Plane (Vector3.up, character.position + character.up * cursorPlaneHeight);

        // optimization (instead of newing Plane):

        playerMovementPlane.normal = character.up;
        playerMovementPlane.distance = -character.position.y + cursorPlaneHeight;

        // used to adjust the camera based on cursor or joystick position

        Vector3 cameraAdjustmentVector = Vector3.zero;

#if UNITY_IPHONE || UNITY_ANDROID

        // On mobiles, use the thumb stick and convert it into screen movement space
        //motor.facingDirection = joystickRight.position.x * screenMovementRight + joystickRight.position.y * screenMovementForward;

        //cameraAdjustmentVector = motor.facingDirection;	
		
		// On PC, the cursor point is the mouse position
        Vector3 cursorScreenPosition = Input.mousePosition;

        // Find out where the mouse ray intersects with the movement plane of the player
        Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane,
                                                                     mainCamera);

        float halfWidth = Screen.width/2.0f;
        float halfHeight = Screen.height/2.0f;
        float maxHalf = Mathf.Max(halfWidth, halfHeight);

        // Acquire the relative screen position			
        Vector3 posRel = cursorScreenPosition - new Vector3(halfWidth, halfHeight, cursorScreenPosition.z);
        posRel.x /= maxHalf;
        posRel.y /= maxHalf;

        facingDirection = posRel;

        cameraAdjustmentVector = posRel.x*screenMovementRight + posRel.y*screenMovementForward;
        cameraAdjustmentVector.y = 0.0f;

        // The facing direction is the direction from the character to the cursor world position
        //motor.facingDirection = (cursorWorldPosition - character.position);
        //motor.facingDirection.y = 0;			

        // Draw the cursor nicely
        HandleCursorAlignment(cursorWorldPosition);

#else

#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)

    // On consoles use the analog sticks
			float axisX = Input.GetAxis("LookHorizontal");
			float axisY = Input.GetAxis("LookVertical");
        //motor.facingDirection = axisX * screenMovementRight + axisY * screenMovementForward;

        //cameraAdjustmentVector = motor.facingDirection;		

#else

        // On PC, the cursor point is the mouse position
        Vector3 cursorScreenPosition = Input.mousePosition;

        // Find out where the mouse ray intersects with the movement plane of the player
        Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane,
                                                                     mainCamera);

        float halfWidth = Screen.width/2.0f;
        float halfHeight = Screen.height/2.0f;
        float maxHalf = Mathf.Max(halfWidth, halfHeight);

        // Acquire the relative screen position			
        Vector3 posRel = cursorScreenPosition - new Vector3(halfWidth, halfHeight, cursorScreenPosition.z);
        posRel.x /= maxHalf;
        posRel.y /= maxHalf;

        facingDirection = posRel;

        cameraAdjustmentVector = posRel.x*screenMovementRight + posRel.y*screenMovementForward;
        cameraAdjustmentVector.y = 0.0f;

        // The facing direction is the direction from the character to the cursor world position
        //motor.facingDirection = (cursorWorldPosition - character.position);
        //motor.facingDirection.y = 0;			

        // Draw the cursor nicely
        HandleCursorAlignment(cursorWorldPosition);

#endif

#endif

        // HANDLE CAMERA POSITION

        // Set the target position of the camera to point at the focus point
        Vector3 cameraTargetPosition = character.position + initOffsetToPlayer + cameraAdjustmentVector*cameraPreview;

        // Apply some smoothing to the camera movement
        mainCameraTransform.position = Vector3.SmoothDamp(mainCameraTransform.position, cameraTargetPosition,
                                                          ref cameraVelocity, cameraSmoothing);

        // Save camera offset so we can use it in the next frame
        cameraOffset = mainCameraTransform.position - character.position;
    }

    public static Vector3 PlaneRayIntersection(Plane plane, Ray ray)
    {
        float dist;
        plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }

    public static Vector3 ScreenPointToWorldPointOnPlane(Vector3 screenPoint, Plane plane, Camera camera)
    {
        // Set up a ray corresponding to the screen position
        Ray ray = camera.ScreenPointToRay(screenPoint);

        // Find out where the ray intersects with the plane
        return PlaneRayIntersection(plane, ray);
    }

    private void HandleCursorAlignment(Vector3 cursorWorldPosition)
    {
        if (!cursorObject)
            return;

        // HANDLE CURSOR POSITION

        // Set the position of the cursor object
        cursorObject.position = cursorWorldPosition;

        // Hide mouse cursor when within screen area, since we're showing game cursor instead
        Screen.showCursor = (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
                             Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height);


        // HANDLE CURSOR ROTATION

        Quaternion cursorWorldRotation = cursorObject.rotation;
        //if (motor.facingDirection != Vector3.zero)
        //	cursorWorldRotation = Quaternion.LookRotation (motor.facingDirection);

        // Calculate cursor billboard rotation
        Vector3 cursorScreenspaceDirection = Input.mousePosition -
                                             mainCamera.WorldToScreenPoint(transform.position +
                                                                           character.up*cursorPlaneHeight);
        cursorScreenspaceDirection.z = 0;
        Quaternion cursorBillboardRotation = mainCameraTransform.rotation*
                                             Quaternion.LookRotation(cursorScreenspaceDirection, -Vector3.forward);

        // Set cursor rotation
        cursorObject.rotation = Quaternion.Slerp(cursorWorldRotation, cursorBillboardRotation, cursorFacingCamera);


        // HANDLE CURSOR SCALING

        // The cursor is placed in the world so it gets smaller with perspective.
        // Scale it by the inverse of the distance to the camera plane to compensate for that.
        float compensatedScale = 0.1f*
                                 Vector3.Dot(cursorWorldPosition - mainCameraTransform.position,
                                             mainCameraTransform.forward);

        // Make the cursor smaller when close to character
        //float cursorScaleMultiplier = Mathf.Lerp (0.7f, 1.0f, Mathf.InverseLerp (0.5f, 4.0f, motor.facingDirection.magnitude));

        // Set the scale of the cursor
        //cursorObject.localScale = Vector3.one * Mathf.Lerp (compensatedScale, 1, cursorSmallerWithDistance) * cursorScaleMultiplier;

        // DEBUG - REMOVE LATER
        if (Input.GetKey(KeyCode.O)) cursorFacingCamera += Time.deltaTime*0.5f;
        if (Input.GetKey(KeyCode.P)) cursorFacingCamera -= Time.deltaTime*0.5f;
        cursorFacingCamera = Mathf.Clamp01(cursorFacingCamera);

        if (Input.GetKey(KeyCode.K)) cursorSmallerWithDistance += Time.deltaTime*0.5f;
        if (Input.GetKey(KeyCode.L)) cursorSmallerWithDistance -= Time.deltaTime*0.5f;
        cursorSmallerWithDistance = Mathf.Clamp01(cursorSmallerWithDistance);
    }

    // The angle between dirA and dirB around axis
    private static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle*(Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }
}