using System;
using Godot;
using Godot.Collections;

namespace ZombieRTSMapGeneration.Scripts
{
    public class Terrain : Spatial
    {
        private int _width;
        private int _height;
        private Array<Vector3> _vertices;
        private Array<Vector3> _normals;
        private ArrayMesh _temporaryMesh;
        private Dictionary<Vector2, float> _heightMapData;

        /**
     * Called when the node enters the scene tree for the first time.
     */
        public override void _Ready()
        {
            var heightMap = ResourceLoader.Load<Image>("res://HeightMap_IsleOfMan.jpg");
            
            _width = heightMap.GetWidth();
            _height = heightMap.GetHeight();
            _vertices = new Array<Vector3>();
            _normals = new Array<Vector3>();
            _temporaryMesh = new ArrayMesh();
            _heightMapData = new Dictionary<Vector2, float>();

            // Parse height map image
            heightMap.Lock(); // Prevent any external use
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    _heightMapData.Add(new Vector2(x, y), heightMap.GetPixel(x, y).r*10);
                }
            }
            heightMap.Unlock();

            // Generate terrain, add all triangle positions to the vertex array
            for (var x = 0; x < _width - 1; x++)
            {
                for (var y = 0; y < _height - 1; y++)
                {
                    MakePolygons(x, y);
                }
            }

            // Use surface tool to generate the mesh
            var surfaceTool = new SurfaceTool();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            surfaceTool.SetMaterial(ResourceLoader.Load<Material>("res://Materials/Terrain.tres"));

            for (var i = 0; i < _vertices.Count; i++)
            {
                var vertex = _vertices[i];
                surfaceTool.AddNormal(_normals[i]);
                surfaceTool.AddVertex(vertex);
            }

            surfaceTool.Commit(_temporaryMesh);
            GetNode<MeshInstance>("MeshInstance").Mesh = _temporaryMesh;
        }
        
        
        /**
         * Called every frame. 'delta' is the elapsed time since the previous frame.
         */
        public override void _Process(float delta)
        {
        }


        /**
         * Make the triangles to create the Quad
         */
        private void MakePolygons(int x, int y)
        {
            // Triangle 1
            var vertex1 = new Vector3(x, _heightMapData[new Vector2(x, y)], -y);
            var vertex2 = new Vector3(x, _heightMapData[new Vector2(x, y + 1)], -y - 1);
            var vertex3 = new Vector3(x + 1, _heightMapData[new Vector2(x + 1, y + 1)], -y - 1);
            
            _vertices.Add(vertex1);
            _vertices.Add(vertex2);
            _vertices.Add(vertex3);

            var face1 = vertex2 - vertex1;
            var face2 = vertex2 - vertex3;
            var normal = face1.Cross(face2);

            // Add each to the normals list
            for (var i = 0; i < 3; i++)
            {
                _normals.Add(normal);
            }
            
            // Triangle 2
            vertex1 = new Vector3(x, _heightMapData[new Vector2(x, y)], -y);
            vertex2 = new Vector3(x + 1, _heightMapData[new Vector2(x + 1, y + 1)], -y - 1);
            vertex3 = new Vector3(x + 1, _heightMapData[new Vector2(x + 1, y)], -y);
            _vertices.Add(vertex1);
            _vertices.Add(vertex2);
            _vertices.Add(vertex3);
            
            face1 = vertex2 - vertex1;
            face2 = vertex2 - vertex3;
            normal = face1.Cross(face2); // Cross product to get normals for both sides

            // Add each to the normals list
            for (var i = 0; i < 3; i++)
            {
                _normals.Add(normal);
            }
        }
    }
}