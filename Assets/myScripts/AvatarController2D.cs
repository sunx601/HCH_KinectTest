using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController2D : AvatarControllerClassic {

	private void LateUpdate()
	{
		foreach( var bone in bones)
		{
			// make sure the avatar is flat
			if (bone != null)
			{
				Vector3 boneRotation = bone.transform.rotation.eulerAngles;
				boneRotation.x = 0;
				boneRotation.y = 180;
				bone.transform.rotation = Quaternion.Euler(boneRotation);
			}
		}
	}
}
