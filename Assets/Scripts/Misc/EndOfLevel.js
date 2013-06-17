
#pragma strict

@script RequireComponent (BoxCollider)

public var timeToTriggerLevelEnd : float = 2.0f;
public var endSceneName : String = "3-4_Pain";


function OnTriggerEnter (other : Collider) {
	if (other.tag == "Player") {
		
		
	}
}

function FadeOutAudio () {
		
}