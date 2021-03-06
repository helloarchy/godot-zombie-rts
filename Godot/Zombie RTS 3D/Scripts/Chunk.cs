using Godot;

namespace ZombieRTS.Scripts
{
    public class Chunk : Spatial
    {
        private MeshInstance _meshInstance; // Terrain on the chunk
        private const int HeightMultiplier = 70;
        private readonly OpenSimplexNoise _noise;
        private readonly int _chunkSize;
        public readonly int X; // (x,z) location on the noise map for the chunk data
        public readonly int Z;
        public bool DoRemove = true; // Flag chunk for removal


        /**
         * Initialise a new chunk
         */
        public Chunk()
        {
        }

        public Chunk(OpenSimplexNoise noise, int x, int z, int chunkSize)
        {
            _noise = noise;
            X = x;
            Z = z;
            _chunkSize = chunkSize;
        }


        /**
         * Called when the node enters the scene tree for the first time.
         */
        public override void _Ready()
        {
            GenerateChunk();
            GenerateWater();
        }


        /**
         * 
         */
        private void GenerateChunk()
        {
            // Set up the plane mesh
            var planeMesh = new PlaneMesh
            {
                Size = new Vector2(_chunkSize, _chunkSize),
                SubdivideDepth = (int) (_chunkSize * 0.5),
                SubdivideWidth = (int) (_chunkSize * 0.5),
                Material = ResourceLoader.Load<ShaderMaterial>("Materials/Terrain.tres")
            };

            var surfaceTool = new SurfaceTool();
            var dataTool = new MeshDataTool();

            /* Create mesh using the plane and extract the array mesh from it,
             * use data tool for manipulating each vertex */
            surfaceTool.CreateFrom(planeMesh, 0);
            var arrayPlane = surfaceTool.Commit();
            var unused = dataTool.CreateFromSurface(arrayPlane, 0); // Discard return

            // Set the height (y) of each vertex using noise values
            for (var i = 0; i < dataTool.GetVertexCount(); i++)
            {
                var vertex = dataTool.GetVertex(i);
                vertex.y = _noise.GetNoise3d(vertex.x + X, vertex.y, vertex.z + Z) * HeightMultiplier;
                dataTool.SetVertex(i, vertex);
            }

            // Clear old mesh before creating the new one
            for (var s = 0; s < arrayPlane.GetSurfaceCount(); s++)
            {
                arrayPlane.SurfaceRemove(s);
            }

            // Create the new mesh
            dataTool.CommitToSurface(arrayPlane);
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            surfaceTool.CreateFrom(arrayPlane, 0);
            surfaceTool.GenerateNormals();

            _meshInstance = new MeshInstance {Mesh = surfaceTool.Commit()};
            _meshInstance.CreateTrimeshCollision();
            _meshInstance.CastShadow = GeometryInstance.ShadowCastingSetting.Off;
            AddChild(_meshInstance);
        }


        /**
         * 
         */
        private void GenerateWater()
        {
            // Set up the plane mesh
            var planeMesh = new PlaneMesh
            {
                Size = new Vector2(_chunkSize, _chunkSize),
                Material = ResourceLoader.Load<SpatialMaterial>("Materials/Water.tres")
            };

            AddChild(new MeshInstance {Mesh = planeMesh});
        }
    }
}