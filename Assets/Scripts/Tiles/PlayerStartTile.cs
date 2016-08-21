using UnityEngine;
using System.Collections;

namespace MapEditor
{
	public class PlayerStartTile : Deployable 
	{
		public override DeployableType GetDeployableType()
		{
			return DeployableType._Player;
		}
	}
}