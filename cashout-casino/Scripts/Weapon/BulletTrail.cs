using Godot;

namespace CashoutCasino.Weapon
{
	public partial class BulletTrail : Node3D
	{
		private MeshInstance3D meshInstance;
		private StandardMaterial3D material;

		private float lifetime = 0.1f;
		private float elapsed = 0f;

		public Color TrailColor = new Color(1f, 0.95f, 0.6f, 1f);

		private Vector3 worldFrom;
		private Vector3 worldTo;

		public void Init(Vector3 startWorld, Vector3 endWorld)
		{
			worldFrom = startWorld;
			worldTo = endWorld;
		}

		public override void _Ready()
		{
			float length = worldFrom.DistanceTo(worldTo);
			Vector3 mid = (worldFrom + worldTo) * 0.5f;

			// Y axis of the cylinder must point along the shot direction
			Vector3 axisY = (worldTo - worldFrom).Normalized();

			// Pick any vector not parallel to axisY to derive X and Z
			Vector3 arbitrary = Mathf.Abs(axisY.Dot(Vector3.Forward)) < 0.99f
				? Vector3.Forward
				: Vector3.Right;

			Vector3 axisX = axisY.Cross(arbitrary).Normalized();
			Vector3 axisZ = axisX.Cross(axisY).Normalized();

			// Construct basis with Y along the shot, then position at midpoint
			var basis = new Basis(axisX, axisY, axisZ);
			GlobalTransform = new Transform3D(basis, mid);

			material = new StandardMaterial3D();
			material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			material.AlbedoColor = TrailColor;
			material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

			var cylinder = new CylinderMesh();
			cylinder.TopRadius = 0.012f;
			cylinder.BottomRadius = 0.012f;
			cylinder.Height = length;
			cylinder.RadialSegments = 5;
			cylinder.Material = material;

			meshInstance = new MeshInstance3D();
			meshInstance.Mesh = cylinder;
			AddChild(meshInstance);
		}

		public override void _Process(double delta)
		{
			elapsed += (float)delta;

			float alpha = Mathf.Clamp(1f - (elapsed / lifetime), 0f, 1f);
			material.AlbedoColor = new Color(TrailColor.R, TrailColor.G, TrailColor.B, alpha);

			if (elapsed >= lifetime)
				QueueFree();
		}
	}
}
