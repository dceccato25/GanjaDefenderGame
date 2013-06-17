#pragma strict

@script RequireComponent (PerFrameRaycast)

var bulletPrefab : GameObject;
var spawnPoint : Transform;
var frequency : float = 10;
var coneAngle : float = 1.5;
var firing : boolean = false;
var damagePerSecond : float = 20.0;
var forcePerSecond : float = 20.0;
var hitSoundVolume : float = 0.5;

var muzzleFlashFront : GameObject;

private var lastFireTime : float = -1;
private var raycast : PerFrameRaycast;

function Awake () {
	muzzleFlashFront.active = false;
	
	raycast = GetComponent.<PerFrameRaycast> ();
	if (spawnPoint == null)
		spawnPoint = transform;
}

function Update () 
{
    if(Input.GetButton("Fire1"))
    {
	    if (!firing) 
		{
			OnStartFire();
	    }
    }
    else
    {
    	if (firing) 
		{
			OnStopFire();
	    }
	    return;
    }

	if (firing) 
	{		
	    if (Time.time > lastFireTime + 1 / frequency) 
		{
			// Spawn visual bullet
			var coneRandomRotation = Quaternion.Euler (Random.Range (-coneAngle, coneAngle), Random.Range (-coneAngle, coneAngle), 0);
			var go : GameObject = Spawner.Spawn (bulletPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
			go.tag = "Bullet";
			
			var bullet : SimpleBullet = go.GetComponent.<SimpleBullet> ();
			
			lastFireTime = Time.time;
			
			muzzleFlashFront.active = true;
			
			bullet.dist = 1000;			
		}
		else if(Time.time > lastFireTime + 1 / (frequency * 2))
		{
			muzzleFlashFront.active = false;
		}
	}
}

function OnStartFire () {
	if (Time.timeScale == 0)
		return;
	
	firing = true;
	
	//muzzleFlashFront.active = true;
	
	if (audio)
		audio.Play ();
}

function OnStopFire () {
	firing = false;
	
	lastFireTime = 0;
	
	muzzleFlashFront.active = false;
	
	if (audio)
		audio.Stop ();
}