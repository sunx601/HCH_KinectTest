using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseKinectManager : MonoBehaviour {


	[Tooltip("Printout debug info")]
	public bool bPrintDebug;

	[Tooltip("Camera that may be used to overlay the mesh over the color background.")]
	public Camera foregroundCamera;

	[Tooltip("UI-Text to display current tracked users.")]
	public UnityEngine.UI.Text userDebugText;

	/*
	[Tooltip("Whether the avatar is facing the player or not.")]
	public bool mirroredMovement = true;

	[Tooltip("Whether the avatar is allowed to move vertically or not.")]
	public bool verticalMovement = true;

	[Tooltip("Whether the avatar's feet must stick to the ground.")]
	public bool groundedFeet = false;

	[Tooltip("Whether to apply the humanoid model's muscle limits or not.")]
	public bool applyMuscleLimits = false;

	[Tooltip("Smooth factor used by the avatar controller.")]
	public float smoothFactor = 10f;
	*/

	[Tooltip("If enabled, makes the avatar position relative to this camera to be the same as the player's position to the sensor.")]
	public Camera posRelativeToCamera;

	protected KinectManager mKinectManager = null;
	protected int userCount = 0;
	protected long userChecksum = 0;

	void Awake()
	{
		Debug.Log("Awake called.");
	}

	// Use this for initialization
	public virtual void Start () {
		SetupKinectManager();
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if( bPrintDebug && mKinectManager )
		{
			PrintoutUserIDs();
		}
	}

	//-----------------------------------------------------------------------------
	void PrintoutUserIDs()
	{
		if (mKinectManager && mKinectManager.IsInitialized())
		{
			userCount = mKinectManager.GetUsersCount();
			Debug.Log("\r\nBaseKinectManager User count: " + userCount);
			for (int i = 0; i < userCount; i++)
			{
				Debug.Log("User ID: " + mKinectManager.GetUserIdByIndex(i));
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Initialize kinect manager
	protected virtual void SetupKinectManager()
	{
		Debug.Log("setting up kinect.");
		mKinectManager = KinectManager.Instance;
	}


	//-----------------------------------------------------------------------------
	// returns the checksum of current users
	protected long GetUserChecksum(out int userCount)
	{
		userCount = 0;
		long checksum = 0;
		//bool foundUserZero = false;
		if (mKinectManager && mKinectManager.IsInitialized())
		{
			
			userCount = mKinectManager.GetUsersCount();
			//for (int i = 0; i < userCount; i++)
			//{
				// a trick to avoid dismissing userID 0
				//long userID = mKinectManager.GetUserIdByIndex(i);

				//if (userID == 0)
				//{
				//	foundUserZero = true;
				//	Debug.Log("Found user zero! ");
				//}

				//checksum ^= userID;
			//}
			


			////Debug.Log("\r\n==== foundUserZero ======");
			//checksum = 0;

			string debubgTextString = "";
			List<long> ids = mKinectManager.GetAllUserIds();

			////Debug.Log("\r\n==== start of list ======");
			foreach (long id in ids)
			{
				//checksum ^= id;
				checksum += id;
				debubgTextString += "id-> " + id + "\n\r";
				//Debug.Log("id-> " + id);
			}
			//Debug.Log("\r\n==== end of list ======");

			if (userDebugText)
			{
				userDebugText.text = debubgTextString + " CHECKSUM: " +checksum;
			}

		}

		


		return checksum;
	}

	//-----------------------------------------------------------------------------
	// returns the world- or camera-overlay joint position 
	public Vector3 GetJointPosition(KinectManager manager, long userID, int iJoint)
	{
		if (foregroundCamera)
		{
			Rect backgroundRect = foregroundCamera.pixelRect;
			PortraitBackground portraitBack = PortraitBackground.Instance;

			if (portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}

			return manager.GetJointPosColorOverlay(userID, iJoint, foregroundCamera, backgroundRect);
		}
		else
		{
			return manager.GetJointPosition(userID, iJoint);
		}
	}
}
