using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleGestureEventListener : MonoBehaviour {

	// Use this for initialization
	void OnEnable()
	{
		// register to gesture events
		BasicGestureListener.OnGestureComplete += OnGestureCompleteEvent;
		BasicGestureListener.OnGestureInProgress += OnGestureInProgresEvent;
	}

	void OnDisable()
	{
		// unregister from gesture events
		BasicGestureListener.OnGestureComplete -= OnGestureCompleteEvent;
		BasicGestureListener.OnGestureInProgress -= OnGestureInProgresEvent;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	

	// Update is called once per frame
		void OnGestureCompleteEvent( long userId, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPosition)
	{
		Debug.Log("recieving gesture complete event " + gesture);
	}

	// Update is called once per frame
	void OnGestureInProgresEvent(long userId, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPosition)
	{
		Debug.Log("recieving gesture progress event " + gesture);
	}


}

