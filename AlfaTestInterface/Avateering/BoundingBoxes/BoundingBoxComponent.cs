using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.Avateering.BoundingBoxes
{
	public class BoundingBoxComponent : DrawableGameComponent
	{
		private readonly string _modelAssetName;
	    private Matrix _translateMatrix;
		private BoundingBoxBuffers _buffers;
		private BasicEffect _effect;

        public BoundingBoxComponent(Game game, string modelAssetName, Matrix translateMatrix)
			: base(game)
		{
			_modelAssetName = modelAssetName;
            _translateMatrix = translateMatrix;
		}

		protected override void LoadContent()
		{
			Model model = Game.Content.Load<Model>(_modelAssetName);
			BoundingBox boundingBox = CreateBoundingBox(model, _translateMatrix);
			_buffers = CreateBoundingBoxBuffers(boundingBox, GraphicsDevice);

			_effect = new BasicEffect(GraphicsDevice);
			_effect.LightingEnabled = false;
			_effect.TextureEnabled = false;
			_effect.VertexColorEnabled = true;

			base.LoadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice.SetVertexBuffer(_buffers.Vertices);
			GraphicsDevice.Indices = _buffers.Indices;

			//ICameraService cameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));
			_effect.World = Matrix.Identity;
			_effect.View = AvateeringXNA.view;
            _effect.Projection = AvateeringXNA.projection;

			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
					_buffers.VertexCount, 0, _buffers.PrimitiveCount);
			}

			base.Draw(gameTime);
		}

		private BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox, GraphicsDevice graphicsDevice)
		{
			BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();

			boundingBoxBuffers.PrimitiveCount = 24;
			boundingBoxBuffers.VertexCount = 48;

			VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
				typeof(VertexPositionColor), boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			List<VertexPositionColor> vertices = new List<VertexPositionColor>();

			const float ratio = 5.0f;

			Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
			Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
			Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
			Vector3[] corners = boundingBox.GetCorners();

			// Corner 1.
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] + xOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - yOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - zOffset);

			// Corner 2.
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - xOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - yOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - zOffset);

			// Corner 3.
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - xOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] + yOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - zOffset);

			// Corner 4.
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + xOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + yOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] - zOffset);

			// Corner 5.
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + xOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] - yOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + zOffset);

			// Corner 6.
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - xOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - yOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] + zOffset);

			// Corner 7.
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] - xOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + yOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + zOffset);

			// Corner 8.
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + xOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + yOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + zOffset);

			vertexBuffer.SetData(vertices.ToArray());
			boundingBoxBuffers.Vertices = vertexBuffer;

			IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
			boundingBoxBuffers.Indices = indexBuffer;

			return boundingBoxBuffers;
		}

		private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
		{
			vertices.Add(new VertexPositionColor(position, Color.White));
		}

        private static BoundingBox CreateBoundingBox(Model model, Matrix translateMatrix)
		{
			Matrix[] boneTransforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(boneTransforms);

			BoundingBox result = new BoundingBox();
			foreach (ModelMesh mesh in model.Meshes)
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, translateMatrix * boneTransforms[mesh.ParentBone.Index]);
					if (meshPartBoundingBox != null)
						result = BoundingBox.CreateMerged(result, meshPartBoundingBox.Value);
				}

            
			return result;
		}

		private static BoundingBox? GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
		{
			if (meshPart.VertexBuffer == null)
				return null;

			Vector3[] positions = VertexElementExtractor.GetVertexElement(meshPart, VertexElementUsage.Position);
			if (positions == null)
				return null;

			Vector3[] transformedPositions = new Vector3[positions.Length];
			Vector3.Transform(positions, ref transform, transformedPositions);

			return BoundingBox.CreateFromPoints(transformedPositions);
		}
	}
}