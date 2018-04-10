using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAvatarController2D : BaseKinectManager {

	[Tooltip("Array of models used for avatar instatiation.")]
	public GameObject[] avatarModels;

	[Tooltip("Game object prefab for testing user position.")]
	public GameObject UserLocatorPrefab;

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



	// dictionary containing user avatars
	private Dictionary<long, GameObject> allUserAvatars = new Dictionary<long, GameObject>();

	// dictionary containing locators
	private Dictionary<long, GameObject> UserLocators = new Dictionary<long, GameObject>();

	// Update is called once per frame
	public override void Update () {
		base.Update();
		checkForUserUpdates();
	}

	//-----------------------------------------------------------------------------
	void checkForUserUpdates()
	{
		long checksum = GetUserChecksum(out userCount);

		if (userChecksum != checksum)
		{
			userChecksum = checksum;
			List<long> avatarsToRemove = new List<long>(allUserAvatars.Keys);

			List<long> allCurrentlyUsedIds = mKinectManager.GetAllUserIds();

			//for (int i = 0; i < userCount; i++)
			foreach( long userId in allCurrentlyUsedIds )
			{
				//long userId = mKinectManager.GetUserIdByIndex(i);
				//if( userId == 0)
				//{
				//	continue;
				//}
				// clear out all avatars that exist in the avatar dictionary
				if (allUserAvatars.ContainsKey(userId))
				{
					avatarsToRemove.Remove(userId);
				}

				if (!allUserAvatars.ContainsKey(userId))
				{
					// create avatar for the user not yet in the dictionary
					int userIndex = mKinectManager.GetUserIndexById(userId);
					Debug.Log("Creating avatar for userId: " + userId + " user index: " + userIndex);
					GameObject avatarObj = CreateUserAvatar(userId, userIndex);
					allUserAvatars[userId] = avatarObj;
				}
			}

			// remove the missing users from the list
			foreach (long userId in avatarsToRemove)
			{
				if (allUserAvatars.ContainsKey(userId))
				{
					Debug.Log("Destroying avatar for userId: " + userId);
					GameObject avatarObj = allUserAvatars[userId];
					allUserAvatars.Remove(userId);

					if (UserLocators.ContainsKey(userId))
					{
						// remove the locator
						GameObject locator = UserLocators[userId];
						UserLocators.Remove(userId);
						Destroy(locator);
					}
					// destroy the user's avatar
					DestroyUserAvatar(avatarObj);
				}
			}
		}
	}

	//-----------------------------------------------------------------------------
	// creates avatar for the given user
	private GameObject CreateUserAvatar(long userId, int userIndex)
	{
		GameObject avatarObj = null;
		GameObject userLocatorObject = null;

		if (avatarModels.Length > 0)
		{
			Vector3 userPos = new Vector3(0, 1, 0);
			//Quaternion userRot = Quaternion.Euler(!mirroredMovement ? Vector3.zero : new Vector3(0, 180, 0));
			Quaternion userRot = Quaternion.Euler(new Vector3(0, 180, 0));

			// create random avatar
			int randomModelIndex = Random.Range(0, avatarModels.Length);
			GameObject avatarModel = avatarModels[randomModelIndex];

			// create the locator object

			Debug.Log("Setting avatar index to: " + randomModelIndex);

			//userLocatorObject = addUserLocator(userId, Vector3.zero);
			avatarObj = Instantiate(avatarModel, userPos, userRot);
			avatarObj.name = "User-" + userId;

			AvatarController2D ac = avatarObj.GetComponent<AvatarController2D>();
			if (ac != null)
			{
				//ac = avatarObj.AddComponent<AvatarController>();
				//ac = avatarObj.AddComponent<AvatarController2D>();
				ac.playerIndex = userIndex;

				ac.smoothFactor = smoothFactor;
				ac.posRelativeToCamera = posRelativeToCamera;

				ac.mirroredMovement = mirroredMovement;
				ac.verticalMovement = verticalMovement;

				ac.groundedFeet = groundedFeet;
				ac.applyMuscleLimits = applyMuscleLimits;

				//set offset node
				//ac.offsetNode = userLocatorObject;

				// start the avatar controller
				ac.SuccessfulCalibration(userId, false);

			}

			

			// refresh the KM-list of available avatar controllers
			MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
			mKinectManager.avatarControllers.Clear();

			foreach (MonoBehaviour monoScript in monoScripts)
			{
				if ((monoScript is AvatarController2D) && monoScript.enabled)
				{
					AvatarController2D avatar = (AvatarController2D)monoScript;
					mKinectManager.avatarControllers.Add(avatar);
				}
			}
		}

		return avatarObj;
	}


	//-----------------------------------------------------------------------------
	// destroys the avatar and refreshes the list of avatar controllers
	private void DestroyUserAvatar(GameObject avatarObj)
	{
		if (avatarObj)
		{
			Destroy(avatarObj);
			if (mKinectManager)
			{
				// refresh the KM-list of available avatar controllers
				MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
				mKinectManager.avatarControllers.Clear();

				foreach (MonoBehaviour monoScript in monoScripts)
				{
					if ((monoScript is AvatarController2D) && monoScript.enabled)
					{
						AvatarController2D avatar = (AvatarController2D)monoScript;
						mKinectManager.avatarControllers.Add(avatar);
					}
				}
			}

		}
	}

}
