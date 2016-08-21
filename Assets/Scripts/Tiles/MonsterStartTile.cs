using UnityEngine;
using System.Collections;

namespace MapEditor
{
	public class MonsterStartTile : Deployable 
	{
		public override DeployableType GetDeployableType()
		{
			return DeployableType._Monster;
		}
	}
}