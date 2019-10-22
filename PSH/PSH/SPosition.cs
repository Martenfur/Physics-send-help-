using Monofoxe.Engine.ECS;
using System;
using System.Collections.Generic;

namespace PSH
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
