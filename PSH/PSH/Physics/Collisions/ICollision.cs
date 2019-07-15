namespace PSH.Physics.Collisions
{
	public interface ICollision
	{
		bool Collided {get; set;}
		
		ICollider A {get; set;}

		ICollider B {get; set;}
	}
}
