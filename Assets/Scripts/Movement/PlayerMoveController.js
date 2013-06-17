#pragma strict

// Objects to drag in
public var character : Transform;
public var cursorPrefab : GameObject;
public var joystickPrefab : GameObject;
public var armRotatePoint : Transform;

// Settings
public var cameraSmoothing : float = 0.01;
public var cameraPreview : float = 2.0f;

// Cursor settings
public var cursorPlaneHeight : float = 0;
public var cursorFacingCamera : float = 0;
public var cursorSmallerWithDistance : float = 0;
public var cursorSmallerWhenClose : float = 1;

// Private memeber data
private var mainCamera : Camera;

private var cursorObject : Transform;
private var joystickLeft : Joystick;
private var joystickRight : Joystick;

private var mainCameraTransform : Transform;
private var cameraVelocity : Vector3 = Vector3.zero;
private var cameraOffset : Vector3 = Vector3.zero;
private var initOffsetToPlayer : Vector3;

// Prepare a cursor point varibale. This is the mouse position on PC and controlled by the thumbstick on mobiles.
private var cursorScreenPosition : Vector3;

private var playerMovementPlane : Plane;

private var joystickRightGO : GameObject;

private var screenMovementSpace : Quaternion;
private var screenMovementForward : Vector3;
private var screenMovementRight : Vector3;

function Awake () {			
	
	// Set main camera
	mainCamera = Camera.main;
	mainCameraTransform = mainCamera.transform;
	
	// Ensure we have character set
	// Default to using the transform this component is on
	if (!character)
		character = transform;
	
	initOffsetToPlayer = mainCameraTransform.position - character.position;
	
	#if UNITY_IPHONE || UNITY_ANDROID
		if (joystickPrefab) {
			// Create left joystick
			var joystickLeftGO : GameObject = Instantiate (joystickPrefab) as GameObject;
			joystickLeftGO.name = "Joystick Left";
			joystickLeft = joystickLeftGO.GetComponent.<Joystick> ();
			
			// Create right joystick
			joystickRightGO = Instantiate (joystickPrefab) as GameObject;
			joystickRightGO.name = "Joystick Right";
			joystickRight = joystickRightGO.GetComponent.<Joystick> ();			
		}
	#else
		if (cursorPrefab) {
			cursorObject = (Instantiate (cursorPrefab) as GameObject).transform;
			cursorObject.localScale = Vector3.one * cursorSmallerWhenClose;
		}
	#endif
	
	// Save camera offset so we can use it in the first frame
	cameraOffset = mainCameraTransform.position - character.position;
	
	// Set the initial cursor position to the center of the screen
	cursorScreenPosition = Vector3 (0.5 * Screen.width, 0.5 * Screen.height, 0);
	
	// caching movement plane
	playerMovementPlane = new Plane (Vector3.forward * -1, character.position);
}

function Start () {
	#if UNITY_IPHONE || UNITY_ANDROID
		// Move to right side of screen
		var guiTex : GUITexture = joystickRightGO.GetComponent.<GUITexture> ();
		guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;			
	#endif	
	
	// it's fine to calculate this on Start () as the camera is static in rotation
	
	screenMovementSpace = Quaternion.Euler (0, mainCameraTransform.eulerAngles.z, 0);
	screenMovementForward = screenMovementSpace * Vector3.up;
	screenMovementRight = screenMovementSpace * Vector3.right;	
}

function OnDisable () {
	if (joystickLeft) 
		joystickLeft.enabled = false;
	
	if (joystickRight)
		joystickRight.enabled = false;
}

function OnEnable () {
	if (joystickLeft) 
		joystickLeft.enabled = true;
	
	if (joystickRight)
		joystickRight.enabled = true;
}

function Update () {
		
	#if UNITY_IPHONE || UNITY_ANDROID
	
		// On mobiles, use the thumb stick and convert it into screen movement space
		//motor.facingDirection = joystickRight.position.x * screenMovementRight + joystickRight.position.y * screenMovementForward;
				
		//cameraAdjustmentVector = motor.facingDirection;		
	
	#else
	
		#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)

			// On consoles use the analog sticks
			//var axisX : float = Input.GetAxis("LookHorizontal");
			//var axisY : float = Input.GetAxis("LookVertical");
			//motor.facingDirection = axisX * screenMovementRight + axisY * screenMovementForward;
	
			//cameraAdjustmentVector = motor.facingDirection;		
		
		#else
	
			// On PC, the cursor point is the mouse position
			var cursorScreenPosition : Vector3 = Input.mousePosition;
						
			// Find out where the mouse ray intersects with the movement plane of the player
			var cursorWorldPosition : Vector3 = ScreenPointToWorldPointOnPlane (cursorScreenPosition, playerMovementPlane, mainCamera);
			
		    var characterScreenPoint = mainCamera.WorldToScreenPoint (character.position);
		    var mouseScreenPoint = Input.mousePosition;
		    		    
		    mouseScreenPoint.y = Mathf.Clamp(mouseScreenPoint.y, characterScreenPoint.y + 1, float.MaxValue);
		    		    
		    var cursorScreenspaceDirection : Vector3 = mouseScreenPoint - characterScreenPoint;
			cursorScreenspaceDirection.z = 0;
			
			// Set cursor rotation
			character.rotation = Quaternion.FromToRotation(-Vector3.right, cursorScreenspaceDirection);
            
			// Draw the cursor nicely
			HandleCursorAlignment (cursorWorldPosition);
			
		#endif
		
	#endif		
}

// The angle between dirA and dirB around axis
static function AngleAroundAxis (dirA : Vector3, dirB : Vector3, axis : Vector3) {
    // Project A and B onto the plane orthogonal target axis
    dirA = dirA - Vector3.Project (dirA, axis);
    dirB = dirB - Vector3.Project (dirB, axis);
   
    // Find (positive) angle between A and B
    var angle : float = Vector3.Angle (dirA, dirB);
   
    // Return angle multiplied with 1 or -1
    return angle * (Vector3.Dot (axis, Vector3.Cross (dirA, dirB)) < 0 ? -1 : 1);
}

public static function PlaneRayIntersection (plane : Plane, ray : Ray) : Vector3 {
	var dist : float;
	plane.Raycast (ray, dist);
	return ray.GetPoint (dist);
}

public static function ScreenPointToWorldPointOnPlane (screenPoint : Vector3, plane : Plane, camera : Camera) : Vector3 {
	// Set up a ray corresponding to the screen position
	var ray : Ray = camera.ScreenPointToRay (screenPoint);
	
	// Find out where the ray intersects with the plane
	return PlaneRayIntersection (plane, ray);
}

function HandleCursorAlignment (cursorWorldPosition : Vector3) {
	if (!cursorObject)
		return;
	
	// HANDLE CURSOR POSITION
	
	// Set the position of the cursor object
	cursorObject.position = cursorWorldPosition;
	
	// Hide mouse cursor when within screen area, since we're showing game cursor instead
	Screen.showCursor = (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height);	
	
	// HANDLE CURSOR ROTATION	
	var cursorWorldRotation : Quaternion = cursorObject.rotation;
		
	// Calculate cursor billboard rotation
	var cursorScreenspaceDirection : Vector3 = Input.mousePosition - mainCamera.WorldToScreenPoint (character.position);
	cursorScreenspaceDirection.z = 0;
	var cursorBillboardRotation : Quaternion = mainCameraTransform.rotation * Quaternion.LookRotation (cursorScreenspaceDirection, -Vector3.forward);
	
	// Set cursor rotation
	cursorObject.rotation = Quaternion.Slerp (cursorWorldRotation, cursorBillboardRotation, cursorFacingCamera);
	
	// HANDLE CURSOR SCALING
	
	// The cursor is placed in the world so it gets smaller with perspective.
	// Scale it by the inverse of the distance to the camera plane to compensate for that.
	var compensatedScale : float = 0.1 * Vector3.Dot (cursorWorldPosition - mainCameraTransform.position, mainCameraTransform.forward);
	
		
	// DEBUG - REMOVE LATER
	if (Input.GetKey(KeyCode.O)) cursorFacingCamera += Time.deltaTime * 0.5;
	if (Input.GetKey(KeyCode.P)) cursorFacingCamera -= Time.deltaTime * 0.5;
	cursorFacingCamera = Mathf.Clamp01(cursorFacingCamera);
	
	if (Input.GetKey(KeyCode.K)) cursorSmallerWithDistance += Time.deltaTime * 0.5;
	if (Input.GetKey(KeyCode.L)) cursorSmallerWithDistance -= Time.deltaTime * 0.5;
	cursorSmallerWithDistance = Mathf.Clamp01(cursorSmallerWithDistance);
}
