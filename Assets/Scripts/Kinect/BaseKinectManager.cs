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

	[Tooltip("If enabled, makes the avatar position relative to this camera to be the same as the player's position to the sensor.")]
	public Camera posRelativeToCamera;

	protected KinectManager mKinectManager = null;
	protected int userCount = 0;
	protected long userChecksum = 0;


	// dictionary containing user gameObjects
	protected Dictionary<long, GameObject> allUsersGameObjects = new Dictionary<long, GameObject>();

	//-----------------------------------------------------------------------------
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
	protected virtual void checkForUserUpdates()
	{
		long checksum = GetUserChecksum(out userCount);

		if (userChecksum != checksum)
		{
			userChecksum = checksum;
			List<long> usersToRemove = new List<long>(allUsersGameObjects.Keys);
			List<long> allCurrentlyUsedIds = mKinectManager.GetAllUserIds();

			foreach (long userId in allCurrentlyUsedIds)
			{
				// clear out all avatars that exist in the avatar dictionary
				if (allUsersGameObjects.ContainsKey(userId))
				{
					usersToRemove.Remove(userId);
				}

				if (!allUsersGameObjects.ContainsKey(userId))
				{
					// create avatar for the user not yet in the dictionary
					int userIndex = mKinectManager.GetUserIndexById(userId);
					Debug.Log("Creating gameObject for userId: " + userId + " user index: " + userIndex);
					GameObject userGameObj = CreateUserGameObject(userId, userIndex);
					allUsersGameObjects[userId] = userGameObj;
				}
			}

			// remove the missing users from the list
			foreach (long userId in usersToRemove)
			{
				if (allUsersGameObjects.ContainsKey(userId))
				{
					Debug.Log("Destroying user gameObject for userId: " + userId);
					GameObject userGameObj = allUsersGameObjects[userId];
					allUsersGameObjects.Remove(userId);
					// destroy the user's avatar
					DestroyUserGameObject(userGameObj);
				}
			}
		}
	}


	//-----------------------------------------------------------------------------
	// creates gameObject for the given user to be extended in derrived class
	protected virtual GameObject CreateUserGameObject(long userId, int userIndex)
	{
		return null;
	}


	//-----------------------------------------------------------------------------
	// destroys the gameObjects assosiated with the user, be extended in derrived class
	protected virtual void DestroyUserGameObject(GameObject gameObj)
	{
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
