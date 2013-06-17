using UnityEngine;

public class GunAimController : MonoBehaviour
{
    // Objects to drag in
    public Transform armRotatePoint;

    public Transform bodySprite;
    public Transform bulletSpawn;
    private Vector3 cameraOffset = Vector3.zero;

    // Settings
    public float cameraPreview = 2.0f;
    public float cameraSmoothing = 0.01f;
    private Vector3 cameraVelocity = Vector3.zero;
    public Transform character;

    // Cursor settings
    public float cursorFacingCamera;
    private Transform cursorObject;
    public float cursorPlaneHeight;
    public GameObject cursorPrefab;
    private Vector3 cursorScreenPosition;
    public float cursorSmallerWhenClose = 1;
    public float cursorSmallerWithDistance;
    public Transform gunSprite;
    private Vector3 initOffsetToPlayer;
    public GameObject joystickPrefab;
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

    private void Awake()
    {
        // Set main camera
        mainCamera = Camera.main;
        mainCameraTransform = mainCamera.transform;

        bodySprite = transform.Find("RastaBody");
        gunSprite = transform.Find("WeaponSlot");
        bulletSpawn = gunSprite.Find("BulletSpawn");

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
            cursorObject.localScale = Vector3.one*cursorSmallerWhenClose;
        }
#else
        if (cursorPrefab)
        {
            cursorObject = (Instantiate(cursorPrefab) as GameObject).transform;
            cursorObject.localScale = Vector3.one*cursorSmallerWhenClose;
        }
#endif

        // Save camera offset so we can use it in the first frame
        cameraOffset = mainCameraTransform.position - character.position;

        // Set the initial cursor position to the center of the screen
        cursorScreenPosition = new Vector3(0.5f*Screen.width, 0.5f*Screen.height, 0);

        // caching movement plane
        playerMovementPlane = new Plane(Vector3.forward*-1, character.position);
    }

    private void Start()
    {
#if UNITY_IPHONE || UNITY_ANDROID
		//new edit
    // Move to right side of screen
		//GUITexture guiTex = joystickRightGO.GetComponent<GUITexture> ();
		//guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;
		
#endif

        // it's fine to calculate this on Start () as the camera is static in rotation

        screenMovementSpace = Quaternion.Euler(0, mainCameraTransform.eulerAngles.z, 0);
        screenMovementForward = screenMovementSpace*Vector3.up;
        screenMovementRight = screenMovementSpace*Vector3.right;
    }

    private void OnDisable()
    {
        //if (joystickLeft)
        //    joystickLeft.enabled = false;

        //if (joystickRight)
        //    joystickRight.enabled = false;
    }

    private void OnEnable()
    {
        //if (joystickLeft)
        //    joystickLeft.enabled = true;

        //if (joystickRight)
        //    joystickRight.enabled = true;
    }

    private void Update()
    {
#if UNITY_IPHONE || UNITY_ANDROID

        // On mobiles, use the thumb stick and convert it into screen movement space
        //motor.facingDirection = joystickRight.position.x * screenMovementRight + joystickRight.position.y * screenMovementForward;

        //cameraAdjustmentVector = motor.facingDirection;		
		
		 // On PC, the cursor point is the mouse position
        Vector3 cursorScreenPosition = Input.mousePosition;

        // Find out where the mouse ray intersects with the movement plane of the player
        Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane,
                                                                     mainCamera);

        Vector3 characterScreenPoint = mainCamera.WorldToScreenPoint(character.position);
        Vector3 mouseScreenPoint = Input.mousePosition;

        mouseScreenPoint.y = Mathf.Clamp(mouseScreenPoint.y, characterScreenPoint.y + 1, float.MaxValue);

        Vector3 cursorScreenspaceDirection = mouseScreenPoint - characterScreenPoint;
        cursorScreenspaceDirection.z = 0;

        if (cursorScreenspaceDirection.x > 0)
        {
            bodySprite.transform.localScale = new Vector3(-1, 1, 1);
            gunSprite.transform.localScale = new Vector3(-1, 1, 1);
            bulletSpawn.localRotation = Quaternion.Euler(new Vector3(0, 90, -90));
            // Set cursor rotation
            character.rotation = Quaternion.FromToRotation(Vector3.right, cursorScreenspaceDirection);
        }
        else
        {
            bodySprite.transform.localScale = new Vector3(1, 1, 1);
            gunSprite.transform.localScale = new Vector3(1, 1, 1);
            bulletSpawn.localRotation = Quaternion.Euler(new Vector3(0, 270, -270));

            // Set cursor rotation
            character.rotation = Quaternion.FromToRotation(-Vector3.right, cursorScreenspaceDirection);
        }

        // Draw the cursor nicely
        HandleCursorAlignment(cursorWorldPosition);
#else

#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)

        // On consoles use the analog sticks
        //float axisX = Input.GetAxis("LookHorizontal");
        //float axisY = Input.GetAxis("LookVertical");
        //motor.facingDirection = axisX * screenMovementRight + axisY * screenMovementForward;

        //cameraAdjustmentVector = motor.facingDirection;		

#else

        // On PC, the cursor point is the mouse position
        Vector3 cursorScreenPosition = Input.mousePosition;

        // Find out where the mouse ray intersects with the movement plane of the player
        Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane,
                                                                     mainCamera);

        Vector3 characterScreenPoint = mainCamera.WorldToScreenPoint(character.position);
        Vector3 mouseScreenPoint = Input.mousePosition;

        mouseScreenPoint.y = Mathf.Clamp(mouseScreenPoint.y, characterScreenPoint.y + 1, float.MaxValue);

        Vector3 cursorScreenspaceDirection = mouseScreenPoint - characterScreenPoint;
        cursorScreenspaceDirection.z = 0;

        if (cursorScreenspaceDirection.x > 0)
        {
            bodySprite.transform.localScale = new Vector3(-1, 1, 1);
            gunSprite.transform.localScale = new Vector3(-1, 1, 1);
            bulletSpawn.localRotation = Quaternion.Euler(new Vector3(0, 90, -90));
            // Set cursor rotation
            character.rotation = Quaternion.FromToRotation(Vector3.right, cursorScreenspaceDirection);
        }
        else
        {
            bodySprite.transform.localScale = new Vector3(1, 1, 1);
            gunSprite.transform.localScale = new Vector3(1, 1, 1);
            bulletSpawn.localRotation = Quaternion.Euler(new Vector3(0, 270, -270));

            // Set cursor rotation
            character.rotation = Quaternion.FromToRotation(-Vector3.right, cursorScreenspaceDirection);
        }

        // Draw the cursor nicely
        HandleCursorAlignment(cursorWorldPosition);

#endif

#endif
    }

    //// The angle between dirA and dirB around axis
    //static void AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    //{
    //    // Project A and B onto the plane orthogonal target axis
    //    dirA = dirA - Vector3.Project(dirA, axis);
    //    dirB = dirB - Vector3.Project(dirB, axis);

    //    // Find (positive) angle between A and B
    //    float angle = Vector3.Angle(dirA, dirB);

    //    // Return angle multiplied with 1 or -1
    //    return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0f ? -1f : 1f);
    //}

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

        // Calculate cursor billboard rotation
        Vector3 cursorScreenspaceDirection = Input.mousePosition - mainCamera.WorldToScreenPoint(character.position);
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


        // DEBUG - REMOVE LATER
        if (Input.GetKey(KeyCode.O)) cursorFacingCamera += Time.deltaTime*0.5f;
        if (Input.GetKey(KeyCode.P)) cursorFacingCamera -= Time.deltaTime*0.5f;
        cursorFacingCamera = Mathf.Clamp01(cursorFacingCamera);

        if (Input.GetKey(KeyCode.K)) cursorSmallerWithDistance += Time.deltaTime*0.5f;
        if (Input.GetKey(KeyCode.L)) cursorSmallerWithDistance -= Time.deltaTime*0.5f;
        cursorSmallerWithDistance = Mathf.Clamp01(cursorSmallerWithDistance);
    }
}