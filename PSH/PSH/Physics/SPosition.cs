using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monofoxe.Engine.ECS;
using Monofoxe.Engine.SceneSystem;
using Microsoft.Xna.Framework;

namespace PSH.Physics
{
	public class SPosition : BaseSystem
	{
		public override Type ComponentType => _componentType;
		private Type _componentType = typeof(CPosition);

		public override int Priority => 1000000;
		

		public override void FixedUpdate(List<Component> components)
		{
			for(var i = 0; i < components.Count; i += 1)
			{
				var position = (CPosition)components[i];
				position.OldPosition = position.Position;
			}
		}
		
	}
}
