using UnityEngine;
using System.Collections;

namespace MapEditor
{
	public class BuildableTile : Deployable 
	{
		public override DeployableType GetDeployableType()
		{
			return DeployableType._Buildable;
		}
	}
}