using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAvatarController2D : BaseKinectManager {

	[Tooltip("Array of models used for avatar instatiation.")]
	public GameObject[] costumeAvatarModels;

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

	// Update is called once per frame
	public override void Update () {
		base.Update();
		checkForUserUpdates();
	}
	/*
	//-----------------------------------------------------------------------------
	protected virtual void checkForUserUpdates()
	{
		long checksum = GetUserChecksum(out userCount);

		if (userChecksum != checksum)
		{
			userChecksum = checksum;
			List<long> usersToRemove = new List<long>(allUsersGameObjects.Keys);
			List<long> allCurrentlyUsedIds = mKinectManager.GetAllUserIds();

			foreach( long userId in allCurrentlyUsedIds )
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
	*/
	//-----------------------------------------------------------------------------
	// creates gameObject for the given user
	protected override GameObject CreateUserGameObject(long userId, int userIndex)
	{
		GameObject avatarObj = null;

		if (costumeAvatarModels.Length > 0)
		{
			Vector3 userPos = new Vector3(0, 0, 0);
			//Quaternion userRot = Quaternion.Euler(!mirroredMovement ? Vector3.zero : new Vector3(0, 180, 0));
			Quaternion userRot = Quaternion.Euler(new Vector3(0, 180, 0));

			// create random avatar
			int randomModelIndex = Random.Range(0, costumeAvatarModels.Length);
			GameObject avatarModel = costumeAvatarModels[randomModelIndex];

			Debug.Log("Setting gameObject index to: " + randomModelIndex);

			avatarObj = Instantiate(avatarModel, userPos, userRot);
			avatarObj.name = "User-" + userId;

			AvatarController2D ac = avatarObj.GetComponent<AvatarController2D>();
			if (ac != null)
			{
				ac.userID = userId;
				ac.playerIndex = userIndex;

				ac.smoothFactor = smoothFactor;
				ac.posRelativeToCamera = posRelativeToCamera;

				ac.mirroredMovement = mirroredMovement;
				ac.verticalMovement = verticalMovement;

				ac.groundedFeet = groundedFeet;
				ac.applyMuscleLimits = applyMuscleLimits;

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
	// destroys the gameObjects and refreshes the list of avatar controllers
	protected override void DestroyUserGameObject(GameObject gameObj)
	{
		if (gameObj)
		{
			Destroy(gameObj);
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
