using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch : MonoBehaviour
{
	public OVRInput.Controller controle;

	//Public Buttons
	public bool buttonOnePress = false;
	public bool buttonTwoPress = false;
	public bool buttonStartPress = false;
	public bool buttonStickPress = false;

	//Public Capacitive Touch
	public bool thumbRest = false;
	public bool buttonOneTouch = false;
	public bool buttonTwoTouch = false;
	public bool buttonThreeTouch = false;
	public bool buttonFourTouch = false;
	public bool buttonTrigger = false;
	public bool buttonStick = false;

	//Public Near Touch
	public bool nearTouchIndexTrigger = false;
	public bool nearTouchThumbButtons = false;

	//Public Trigger & Grip
	public float trigger = 0.0f;
	public float grip = 0.0f;

	//Public Stick Axis
	private Vector2 stickXYPos;
	public float stickXPos = 0.0f;
	public float stickYPos = 0.0f;

	private void Update()
	{
		//Controller Position & Rotation
		transform.localPosition = OVRInput.GetLocalControllerPosition(controle);
		transform.localRotation = OVRInput.GetLocalControllerRotation(controle);

		//Controller Button State
		buttonOnePress = OVRInput.Get(OVRInput.Button.One, controle);
		buttonTwoPress = OVRInput.Get(OVRInput.Button.Two, controle);
		buttonStartPress = OVRInput.Get(OVRInput.Button.Start, controle);
		buttonStickPress = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controle);

		//Controller Capacitive Sensors State
		thumbRest = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, controle);
		buttonOneTouch = OVRInput.Get(OVRInput.Touch.One, controle);
		buttonTwoTouch = OVRInput.Get(OVRInput.Touch.Two, controle);
		buttonThreeTouch = OVRInput.Get(OVRInput.Touch.Three, controle);
		buttonFourTouch = OVRInput.Get(OVRInput.Touch.Four, controle);
		buttonTrigger = OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, controle);
		buttonStick = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, controle);

		//Controller NearTouch State
		nearTouchIndexTrigger = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, controle);
		nearTouchThumbButtons = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controle);

		//Controller Trigger State
		grip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controle);
		trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controle);

		//Controller Analogue Stick State
		stickXYPos = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controle);
		stickXPos = stickXYPos.x;
		stickYPos = stickXYPos.y;
	}
}
