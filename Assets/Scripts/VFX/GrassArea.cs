using System;
using UnityEditor;
using UnityEngine;

namespace Solis.VFX
{
    [ExecuteInEditMode]
    public class GrassArea : MonoBehaviour
    {
        private static readonly int _HeightMapMax = Shader.PropertyToID("_HeightMapMax");
        private static readonly int _HeightMap = Shader.PropertyToID("_HeightMap");
        private static readonly int _GrassMap = Shader.PropertyToID("_GrassMap");

        private Mesh _mesh;
        
        [Header("REFERENCES")]
        public Material material;
        public Texture2D heightMap;
        public Texture2D grassMap;
        public bool useGrassMap;
        
        [Header("SETTINGS")]
        public Vector3 size = new(100, 10, 100);

        private void OnEnable()
        {
            _mesh = _GenerateMesh();
        }

        private void OnValidate()
        {
            _mesh = _GenerateMesh();
        }
        
        private void OnDisable()
        {
            DestroyImmediate(_mesh);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + new Vector3(size.x / 2, size.y/2, size.z / 2), size);
        }
        
        private void Update()
        {
            var propBlock = new MaterialPropertyBlock();
            propBlock.SetFloat(_HeightMapMax, size.y);
            if(heightMap != null)
                propBlock.SetTexture(_HeightMap, heightMap);
            if(useGrassMap && grassMap != null)
                propBlock.SetTexture(_GrassMap, grassMap);
            Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, material, gameObject.layer, null, 0, propBlock);
        }
        
        private Mesh _GenerateMesh()
        {
            const float maxSizePerQuad = 10;
            
            var subDivX = Mathf.CeilToInt(size.x / maxSizePerQuad);
            var subDivY = Mathf.CeilToInt(size.z / maxSizePerQuad);
            
            var gX = size.x / subDivX;
            var gY = size.z / subDivY;
            
            var mesh = new Mesh();
            
            var vertices = new Vector3[(subDivX + 1) * (subDivY + 1)];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[subDivX * subDivY * 6];
            
            for (var y = 0; y <= subDivY; y++)
            {
                for (var x = 0; x <= subDivX; x++)
                {
                    var i = x + y * (subDivX + 1);
                    vertices[i] = new Vector3(x * gX, 0, y * gY);
                    uvs[i] = new Vector2(x / (float)subDivX, y / (float)subDivY);
                }
            }
            
            for (var y = 0; y < subDivY; y++)
            {
                for (var x = 0; x < subDivX; x++)
                {
                    var i = x + y * subDivX;
                    var v = x + y * (subDivX + 1);
                    triangles[i * 6 + 0] = v;
                    triangles[i * 6 + 1] = v + subDivX + 1;
                    triangles[i * 6 + 2] = v + 1;
                    triangles[i * 6 + 3] = v + 1;
                    triangles[i * 6 + 4] = v + subDivX + 1;
                    triangles[i * 6 + 5] = v + subDivX + 2;
                }
            }
            
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            
            mesh.RecalculateNormals();
            mesh.bounds = new Bounds(transform.position + size / 2, size); 
            return mesh;
        }
        
        #if UNITY_EDITOR
        [SerializeField]
        private bool hierarchyOnly = true;
        [SerializeField]
        private int bakeResolution = 1024;
        
        public void CreateBakeMap()
        {
            var tex = new Texture2D(bakeResolution, bakeResolution);
            var stepX = size.x / bakeResolution;
            var stepY = size.z / bakeResolution;

            var t = transform;
            var worldHeight = size.y * t.lossyScale.y;
        
            for (var y = 0; y < bakeResolution; y++)
            {
                for (var x = 0; x < bakeResolution; x++)
                {
                    var lPos = new Vector3(x * stepX, 0, y * stepY);
                    var worldPos = t.TransformPoint(lPos);

                    var tested = 0f;
                    var found = false;
                    
                    while (tested < worldHeight)
                    {
                        if (Physics.Raycast(worldPos + new Vector3(0, worldHeight - tested, 0), Vector3.down,
                                out var hit, worldHeight - tested))
                        {
                            if (!hierarchyOnly || hit.collider.transform.IsChildOf(t))
                            {
                                var height = 1 - (hit.distance + tested) / size.y;
                                tex.SetPixel(x, y, new Color(height, height, height));
                                found = true;
                                break;
                            }

                            tested += hit.distance + 0.1f;
                        }
                        else
                            break;
                    }
                    
                    if(!found)
                        tex.SetPixel(x, y, new Color(0, 0, 0));
                }
            }

            tex.Apply();

            if (heightMap == null)
            {
                var path = $"Assets/GrassAreaHeight_{DateTime.Now.Ticks}.png";
                System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
                //load asset
                AssetDatabase.Refresh();
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.isReadable = true;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
                heightMap = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(heightMap);
                System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
                //reimport asset
                AssetDatabase.Refresh();
            }
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GrassArea))]
    public class GrassAreaEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var grassArea = (GrassArea) target;
            if (GUILayout.Button("Bake"))
            {
                grassArea.CreateBakeMap();
            }
        }
    }
#endif
}