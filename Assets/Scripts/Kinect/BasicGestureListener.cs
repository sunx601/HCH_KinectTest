using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
//using Windows.Kinect;

public class UserGestureData : MonoBehaviour
{
	// constructor
	public UserGestureData()
	{
	}

	// getters setters
	public void setUserId( long iD ) { mUserID = iD; }
	public long getUserId() { return mUserID; }

	// whether the needed gesture has been detected or not
	public bool isLeanRight { get; set; }
	public bool isLeanLeft { get; set; }
	public bool isRaiseRightHand { get; set; }
	public bool isRaisedLeftHand { get; set; }

	long mUserID = -1;
	int mUserIndex = -1;

	//private bool progressDisplayed;
	//private float progressGestureTime;

}

public class BasicGestureListener : BaseKinectManager, KinectGestures.GestureListenerInterface
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("UI-Text to display gesture-listener messages and gesture information.")]
	public UnityEngine.UI.Text gestureInfo;

	[Tooltip("GameObject prefab for userListenerGameObject.")]
	public GameObject userListenerPrefab;

	// singleton instance of the class
	private static BasicGestureListener instance = null;

	// internal variables to track if progress message has been displayed
	private bool progressDisplayed;
	private float progressGestureTime;

	/// <summary>
	/// Gesture Events
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="gesture"></param>
	/// <param name="progress"></param>
	/// <param name="joint"></param>
	/// <param name="screenPosition"></param>
	// event handlers to send messages to gesture subscribers
	public delegate void GestureEventHandler(long userId, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPosition);
	public static event GestureEventHandler OnGestureInProgress;
	public static event GestureEventHandler OnGestureComplete;
	public static event GestureEventHandler OnGestureCanceled;

	// dictionary to store users
	private Dictionary<long, UserGestureData> mUserDictionary = new Dictionary<long, UserGestureData>();

	/// <summary>
	/// Gets the singleton CubeGestureListener instance.
	/// </summary>
	/// <value>The CubeGestureListener instance.</value>
	public static BasicGestureListener Instance
	{
		get
		{
			return instance;
		}
	}

	/// <summary>
	/// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	public void UserDetected(long userId, int userIndex)
	{
		// add new user to the dictionary
		if (!mUserDictionary.ContainsKey(userId))
		{
			KinectManager manager = KinectManager.Instance;

			if (!manager)
				return;

			// detect these user specific gestures
			manager.DetectGesture(userId, KinectGestures.Gestures.LeanLeft);
			manager.DetectGesture(userId, KinectGestures.Gestures.LeanRight);
			manager.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
			manager.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);

			Debug.Log("Adding new user " + userId);
			UserGestureData newUser = gameObject.AddComponent<UserGestureData>(); ;
			newUser.setUserId(userId);
			mUserDictionary[userId] = newUser;

			if (gestureInfo != null)
			{
				gestureInfo.text = "lean left, right or raise your hands nevigate.";
			}
		}
	}

	/// <summary>
	/// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	public void UserLost(long userId, int userIndex)
	{
		// the gestures are allowed for the primary user only
		//if(userIndex != playerIndex)
		//	return;

		if (gestureInfo != null)
		{
			gestureInfo.text = string.Empty;
		}

		if (mUserDictionary.ContainsKey(userId))
		{
			UserGestureData lostUser = mUserDictionary[userId];
			mUserDictionary.Remove(userId);
			Destroy(lostUser);
			Debug.Log("Destroying user: " + userId);
		}

	}

	/// <summary>
	/// Invoked when a gesture is in progress.
	/// </summary>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="progress">Gesture progress [0..1]</param>
	/// <param name="joint">Joint type</param>
	/// <param name="screenPos">Normalized viewport position</param>
	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
								  float progress, KinectInterop.JointType joint, Vector3 screenPos)
	{
		if ((gesture == KinectGestures.Gestures.LeanLeft || gesture == KinectGestures.Gestures.LeanRight) && progress > 0.5f)
		{
			//Debug.Log("!!!!!Detecting leaning  " + gesture + " for user userId " + userId );

			if (gestureInfo != null)
			{
				string sGestureText = string.Format("{0} - {1:F0}%", gesture, screenPos.z * 100f);
				gestureInfo.text = sGestureText;
			}


			if (OnGestureInProgress != null)
			{
				OnGestureInProgress(userId, gesture, 1.0f, joint, screenPos);
			}
		}

	}

	/// <summary>
	/// Invoked if a gesture is completed.
	/// </summary>
	/// <returns>true</returns>
	/// <c>false</c>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="joint">Joint type</param>
	/// <param name="screenPos">Normalized viewport position</param>
	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
								  KinectInterop.JointType joint, Vector3 screenPos)
	{
		// only listen to users we have contained in our dictionary 
		if (!mUserDictionary.ContainsKey(userId))
			return false;

		UserGestureData userData = mUserDictionary[userId];
		Debug.Log("Gesture completed " + gesture);

		if (gestureInfo != null)
		{
			string sGestureText = gesture + " detected";
			gestureInfo.text = sGestureText;
		}

		if (OnGestureComplete != null)
		{
			// send the event to listeners
			Debug.Log("Sending Gesture complete " + userId + " gesture: " + gesture);
			OnGestureComplete(userId, gesture, 1.0f, joint, screenPos);
		}

		return true;
	}

	/// <summary>
	/// Invoked if a gesture is cancelled.
	/// </summary>
	/// <returns>true</returns>
	/// <c>false</c>
	/// <param name="userId">User ID</param>
	/// <param name="userIndex">User index</param>
	/// <param name="gesture">Gesture type</param>
	/// <param name="joint">Joint type</param>
	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
								  KinectInterop.JointType joint)
	{
		// only listen to users we have contained in our dictionary 
		if (!mUserDictionary.ContainsKey(userId))
			return false;

		if (progressDisplayed)
		{
			progressDisplayed = false;

			if (gestureInfo != null)
			{
				gestureInfo.text = String.Empty;
			}
		}

		return true;
	}

	void Awake()
	{
		instance = this;
	}

	/// <summary>
	/// extend base kinect manager
	/// </summary>
	public override void Update()
	{
		base.Update();
		checkForUserUpdates();
		/*if(progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
		{
			progressDisplayed = false;
			gestureInfo.text = String.Empty;
			Debug.Log("Forced progress to end.");
		}*/

		/// debug information
		/*
		Debug.Log("mUserDictionary " + mUserDictionary.Count);
		foreach ( long key in mUserDictionary.Keys )
		{
			UserGestureData user = mUserDictionary[key];
			Debug.Log("user: " + user.getUserId()  );
		}
		*/
	}

	//-----------------------------------------------------------------------------
	// creates gameObject for the given user
	protected override GameObject CreateUserGameObject(long userId, int userIndex)
	{
		GameObject listenerObj = null;
		if (userListenerPrefab != null)
		{
			listenerObj = Instantiate( userListenerPrefab, Vector3.zero, Quaternion.identity );
			listenerObj.name = "Listener_User-" + userId;
		}
		return listenerObj;
	}


	//-----------------------------------------------------------------------------
	// destroys the gameObjects and refreshes the list of avatar controllers
	protected override void DestroyUserGameObject(GameObject gameObj)
	{
		if (gameObj)
		{
			Destroy(gameObj);
		}
	}
}
