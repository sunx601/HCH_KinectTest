using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HandIconController : BaseKinectManager
{
	[Tooltip("Object representing the cursor")]
	public GameObject cursorPrefab;

	[Tooltip("Smooth factor for cursor movement.")]
	public float smoothFactor = 10f;

	[Tooltip("Limit number of users")]
	public int maxUsers = 6;

	/// Dictionary of possible users and their cursors
	protected Dictionary<int,List<GameObject>> availableCursorsPerUser = new Dictionary<int, List<GameObject>>();

	// store the joints tracked
	private List<int> jointsTypeTracked = new List<int>();

	public Quaternion initialRotation = Quaternion.identity;

	// is motion flipped
	private bool objFlipped = false;

	//--------------------------------------------------------------------------------------------------
	public override void Start()
	{
		base.Start();

		// initialize the cursors
		for( int i = 0; i< maxUsers; i++ )
		{
			List<GameObject> possibleUsers = new List<GameObject>();
			for (int n = 0; n < 2; n++) {
				GameObject cursor = Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity);
				cursor.name = "Cursor_" + i;
				possibleUsers.Add(cursor);
			}
			availableCursorsPerUser[i] = possibleUsers;
		}

		// track both hands
		jointsTypeTracked.Add((int)KinectInterop.JointType.HandRight);
		jointsTypeTracked.Add((int)KinectInterop.JointType.HandLeft);
	}

	//--------------------------------------------------------------------------------------------------
	// Update is called once per frame
	public override void Update () {
		base.Update();
		if (mKinectManager != null && mKinectManager.IsInitialized())
		{
			userCount = mKinectManager.GetUsersCount();
			for (int i = 0; i < maxUsers; i++ )
			{
				long userID = mKinectManager.GetUserIdByIndex(i);
				updateCursorForUserIndex(i, userID);
			}
		}
	}

	//--------------------------------------------------------------------------------------------------
	void updateCursorForUserIndex( int index, long userID )
	{
		for( int i=0; i< jointsTypeTracked.Count; i++ )
		{
			GameObject cursor = availableCursorsPerUser[index][i];

			if( !cursor )
			{
				return;
			}

			if (!mKinectManager.IsJointTracked(userID, jointsTypeTracked[i]))
			{
				cursor.SetActive(false);
			} else {
				cursor.SetActive(true);
				GameObject CursorWithName = GameObject.Find("Cursor_" + index + "/Canvas/Text");
				Text cursorText = null;
				if( CursorWithName )
				{
					cursorText = CursorWithName.GetComponent<Text>();
				}

				if (cursorText)
				{
					//Debug.Log("setting text "+ userID );
					cursorText.text = ""+userID;
				}

				Rect backgroundRect = foregroundCamera.pixelRect;
				Vector3 posJointRaw = mKinectManager.GetJointPosColorOverlay(userID, jointsTypeTracked[i], foregroundCamera, backgroundRect);

				if (posJointRaw != Vector3.zero)
				{
					//cursor.transform.position = posJointRaw;
					Quaternion rotJoint = mKinectManager.GetJointOrientation(userID, jointsTypeTracked[i], !objFlipped);
					rotJoint = initialRotation * rotJoint;
					cursor.transform.rotation = Quaternion.Slerp(cursor.transform.rotation, rotJoint, smoothFactor * Time.deltaTime);

					//overlayObject.position = posJoint;
					cursor.transform.position = Vector3.Slerp(posJointRaw, cursor.transform.position, smoothFactor * Time.deltaTime);
				}
			} 
		}
	}
}
