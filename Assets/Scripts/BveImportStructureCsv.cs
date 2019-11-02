using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using mattatz.MeshBuilderSystem;

public class BveImportStructureCsv : MonoBehaviour
{
    public class BveCsvStatement
    {
        public struct BveCsvStatementTextureCoordination
        {
            public int VertexIndex;
            public Vector2 TextureCoordination;
        }

        public struct BveCsvStatementCube
        {
            public float R1;
            public float R2;
            public float R3;
        }

        public struct BveCsvStatementCylinder
        {
            public int N;
            public float R1;
            public float R2;
            public float H;
        }

        public struct BveCsvRotation
        {
            public Vector3 Axis;
            public float Theta;
        }

        public enum CommandType
        {
            nop,
            createmeshbuilder,
            cube,
            cylinder,
            addvertex,
            addface,
            addface2,
            generatenormals,
            setcolor,
            translate,
            rotate,
            loadtexture,
            settexturecoordinates,
            setdecaltransparentcolor,
        };

        public string[] Arguments { get; private set; } = null;
        public CommandType Command { get; private set; } = CommandType.nop;

        public Vector3 ArgumentVector3
        {
            get
            {
                var x = float.Parse(Arguments[0]);
                var y = float.Parse(Arguments[1]);
                var z = float.Parse(Arguments[2]);


                return new Vector3(x, y, z);
            }
        }

        public BveCsvRotation ArgumentRotation
        {
            get
            {
                var x = float.Parse(Arguments[0]);
                var y = float.Parse(Arguments[1]);
                var z = float.Parse(Arguments[2]);
                var theta = float.Parse(Arguments[3]);


                return new BveCsvRotation { Axis = new Vector3(x, y, z), Theta = theta };
            }
        }

        public BveCsvStatementTextureCoordination ArgumentTextureCoordination
        {
            get
            {
                var vertexIndex = int.Parse(Arguments[0]);
                var tu = float.Parse(Arguments[1]);
                var tv = float.Parse(Arguments[2]);


                return new BveCsvStatementTextureCoordination { VertexIndex = vertexIndex, TextureCoordination = new Vector2(tu, tv) };
            }
        }

        public char[] ArgumentColor4
        {
            get
            {
                var r = int.Parse(Arguments[0]);
                var g = int.Parse(Arguments[1]);
                var b = int.Parse(Arguments[2]);
                var a = (int)255;


                if (Arguments.Length >= 4)
                {
                    a = int.Parse(Arguments[3]);
                }


                return new char[4] { (char)r, (char)g, (char)b, (char)a };
            }
        }

        public BveCsvStatementCube ArgumentCube
        {
            get
            {
                var R1 = float.Parse(Arguments[0]);
                var R2 = R1;
                var R3 = R1;


                if (Arguments.Length >= 3)
                {
                    R2 = float.Parse(Arguments[1]);
                    R3 = float.Parse(Arguments[2]);
                }


                return new BveCsvStatementCube { R1 = R1, R2 = R2, R3 = R3 };
            }
        }

        public BveCsvStatementCylinder ArgumentCylinder
        {
            get
            {
                var N  = int.Parse(Arguments[0]);
                var R1 = float.Parse(Arguments[1]);
                var R2 = float.Parse(Arguments[2]);
                var H  = float.Parse(Arguments[3]);


                return new BveCsvStatementCylinder { N = N, R1 = R1, R2 = R2, H = H };
            }
        }

        public int[] ArgumentIndices
        {
            get
            {
                return Array.ConvertAll(Arguments, int.Parse);
            }
        }

        public string ArgumentString
        {
            get
            {
                return Arguments[0];
            }
        }


        public BveCsvStatement(string line)
        {
            Debug.Log(line);

            var splitted = TrimAll(line.Split(','));


            if ((splitted == null) || (splitted.Length < 1))
            {
                return;
            }


            var isFoundCommand = Enum.TryParse(splitted[0].ToLower(), out CommandType commandType);

            if (!isFoundCommand)
            {
                return;
            }


            Arguments = new string[splitted.Length - 1];
            Array.Copy(splitted, 1, Arguments, 0, Arguments.Length);


            Command = commandType;
        }


        private string[] TrimAll(string[] words)
        {
            var result = new List<string>();


            foreach (var word in words)
            {
                var trimmed = Trim(word);


                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }


                result.Add(trimmed);
            }


            return result.ToArray();
        }

        private string Trim(string statement)
        {
            statement = statement.Trim();
            return Regex.Replace(statement, ";.*", string.Empty);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    
    public void Parse(string path)
    {
        var rootDirectory = Path.GetDirectoryName(path);
        var rootGameObject = new GameObject(Path.GetFileName(path));
        var lines = File.ReadAllLines(path);

        rootGameObject.transform.SetParent(transform, false);


        var currentMesh = (Mesh)null;
        var vertexList = new List<Vector3>();
        var uvList = new List<Vector2>();
        var indexList = new List<int>();
        
        var matrixStack = new Stack<Transform>();
        var texture = (Texture2D)null;
        var gameObject = (GameObject)null;
        var meshRenderer = (MeshRenderer)null;
        var meshFilter = (MeshFilter)null;
        var currentColor = Color.white;

        var isSplittedNormals = false;
        var renderingOrderIndex = 0;


        foreach (var line in lines)
        {
            var statement = new BveCsvStatement(line);


            if (statement.Command == BveCsvStatement.CommandType.nop)
            {
                continue;
            }


            switch (statement.Command)
            {
                case BveCsvStatement.CommandType.createmeshbuilder:
                    {
                        gameObject = new GameObject("MeshBuilder");
                        gameObject.transform.SetParent(rootGameObject.transform, false);

                        meshRenderer = gameObject.AddComponent<MeshRenderer>();
                        meshFilter = gameObject.AddComponent<MeshFilter>();

                        currentMesh = new Mesh();
                        vertexList = new List<Vector3>();
                        uvList = new List<Vector2>();
                        indexList = new List<int>();
                        texture = (Texture2D)null;
                        isSplittedNormals = false;
                        currentColor = Color.white;

                        meshFilter.mesh = currentMesh;
                        meshRenderer.material = Instantiate(Resources.Load(@"Materials/Standard") as Material);
                    }

                    break;

                case BveCsvStatement.CommandType.cube:
                    {
                        currentMesh = CubeBuilder.Build(statement.ArgumentCube.R1 * 2.0f, statement.ArgumentCube.R2 * 2.0f, statement.ArgumentCube.R3 * 2.0f);
                        meshFilter.mesh = currentMesh;
                    }
                    break;

                case BveCsvStatement.CommandType.cylinder:
                    {
                        if (statement.ArgumentCylinder.R1 == 0.0f)
                        {
                            currentMesh = ConeBuilder.Build(statement.ArgumentCylinder.N, statement.ArgumentCylinder.R2, statement.ArgumentCylinder.H);
                        }
                        else if (statement.ArgumentCylinder.R2 == 0.0f)
                        {
                            currentMesh = ConeBuilder.Build(statement.ArgumentCylinder.N, statement.ArgumentCylinder.R1, statement.ArgumentCylinder.H * -1.0f);
                        }
                        else
                        {
                            var r1 = statement.ArgumentCylinder.R1;
                            var r2 = statement.ArgumentCylinder.R2;
                            var isClosed = true;


                            if (r1 < 0.0f || r2 < 0.0f)
                            {
                                if (r1 < 0.0f)
                                {
                                    r1 = r2;
                                }
                                else
                                {
                                    r2 = r1;
                                }


                                isClosed = false;
                            }


                            currentMesh = CylinderBuilder.Build(r1, statement.ArgumentCylinder.H, statement.ArgumentCylinder.N, isClosed);
                            meshFilter.mesh = currentMesh;
                        }
                    }
                    break;

                case BveCsvStatement.CommandType.addvertex:
                    {
                        vertexList.Add(statement.ArgumentVector3);
                        uvList.Add(Vector2.zero);

                        currentMesh.SetVertices(vertexList);
                    }
                    break;

                case BveCsvStatement.CommandType.addface:
                case BveCsvStatement.CommandType.addface2:
                    {
                        var indices = statement.ArgumentIndices;

                        if (statement.Command == BveCsvStatement.CommandType.addface2)
                        {
                            var reversedIndices = indices.Clone() as int[];
                            Array.Reverse(reversedIndices);

                            var exntendedArray = new int[indices.Length * 2];
                            Array.Copy(indices, exntendedArray, indices.Length);
                            Array.Copy(reversedIndices, 0, exntendedArray, reversedIndices.Length, reversedIndices.Length);

                            indices = exntendedArray;
                        }


                        var triangulatedIndices = new List<int>();
                        var v0 = indices[0];
                        var v1 = indices[1];
                        var v2 = indices[2];

                        triangulatedIndices.Add(v0);
                        triangulatedIndices.Add(v1);
                        triangulatedIndices.Add(v2);


                        for (var i = 3; i < indices.Length; ++i)
                        {
                            triangulatedIndices.Add(v0);
                            triangulatedIndices.Add(v2);
                            triangulatedIndices.Add(indices[i]);

                            v2 = indices[i];
                        }


                        indexList.AddRange(triangulatedIndices);
                        
                        currentMesh.SetIndices(indexList.ToArray(), MeshTopology.Triangles, 0);
                    }
                    break;

                case BveCsvStatement.CommandType.generatenormals:
                    {
                        var splittedVetexList = new List<Vector3>();
                        var splittedIndexList = new List<int>();
                        var SplittedUvList = new List<Vector2>();

                        foreach (var index in indexList)
                        {
                            splittedVetexList.Add(vertexList[index]);
                            SplittedUvList.Add(Vector2.zero);
                        }

                        for (var i = 0; i < indexList.Count; ++i)
                        {
                            splittedIndexList.Add(i);
                        }

                        currentMesh.SetVertices(splittedVetexList);
                        currentMesh.SetIndices(splittedIndexList.ToArray(), MeshTopology.Triangles, 0);
                        uvList = SplittedUvList;

                        currentMesh.RecalculateNormals();
                        isSplittedNormals = true;
                    }
                    break;


                case BveCsvStatement.CommandType.setcolor:
                    {
                        currentColor = new Color(statement.ArgumentColor4[0] / 255.0f, statement.ArgumentColor4[1] / 255.0f, statement.ArgumentColor4[2] / 255.0f, statement.ArgumentColor4[3] / 255.0f);
                        meshRenderer.material.color = currentColor;
                    }
                    break;

                case BveCsvStatement.CommandType.translate:
                    {
                        gameObject.transform.localPosition = statement.ArgumentVector3;//.Translate(statement.ArgumentVector3, Space.Self);
                    }
                    break;

                case BveCsvStatement.CommandType.rotate:
                    {
                        //gameObject.transform.Rotate(statement.ArgumentRotation.Axis, statement.ArgumentRotation.Theta, Space.Self);
                        gameObject.transform.localRotation = Quaternion.AngleAxis(statement.ArgumentRotation.Theta, statement.ArgumentRotation.Axis);
                    }
                    break;

                case BveCsvStatement.CommandType.loadtexture:
                    {
                        var fullPath = Path.Combine(rootDirectory, statement.ArgumentString);
                        var fileBytes = File.ReadAllBytes(fullPath);
                        var imageBytes = (byte[])null;
                        var width = 0;
                        var height = 0;
                        var isAlphaTransparency = false;

                        switch (Path.GetExtension(fullPath).ToLower())
                        {
                            case ".bmp":
                            case ".png":
                            case ".jpg":
                            case ".jpeg":
                                using (var memoryStream = new MemoryStream(fileBytes))
                                {
                                    var result = StbImageSharp.ImageResult.FromMemory(fileBytes, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

                                    width = result.Width;
                                    height = result.Height;

                                    imageBytes = result.Data;
                                }

                                break;

                            default:
                                break;
                        }


                        if (imageBytes == null)
                        {
                            continue;
                        }

                        if (width == 0)
                        {
                            texture = new Texture2D(4, 4);
                            texture.LoadImage(imageBytes);
                        }
                        else
                        {
                            texture = new Texture2D(width, height);
                            var colorList = new List<Color32>();
                            

                            for (var i = 0; i < imageBytes.Length; i += 4)
                            {
                                var color = new Color32(imageBytes[i + 0], imageBytes[i + 1], imageBytes[i + 2], imageBytes[i + 3]);


                                if (color.a < 255)
                                {
                                    isAlphaTransparency = true;
                                }


                                colorList.Add(color);
                            }

                            texture.SetPixels32(colorList.ToArray());
                            texture.Apply();
                        }

                        texture.filterMode = FilterMode.Bilinear;


                        if (isAlphaTransparency)
                        {
                            meshRenderer.material = Instantiate(Resources.Load(@"Materials/Transparent") as Material);
                            meshRenderer.material.mainTexture = texture;
                            meshRenderer.material.color = currentColor;
                            meshRenderer.material.renderQueue += renderingOrderIndex;

                            ++renderingOrderIndex;
                        }
                        else
                        {
                            meshRenderer.material.mainTexture = texture;
                        }
                    }
                    break;

                case BveCsvStatement.CommandType.settexturecoordinates:
                    {
                        var vertexIndex = statement.ArgumentTextureCoordination.VertexIndex;
                        var coord = statement.ArgumentTextureCoordination.TextureCoordination;

                        if (isSplittedNormals)
                        {
                            for (var i = 0; i < indexList.Count; ++i)
                            {
                                if (indexList[i] == vertexIndex)
                                {
                                    uvList[i] = coord;
                                }
                            }
                        }
                        else
                        {
                            uvList[vertexIndex] = coord;
                        }
                        
                        
                        currentMesh.SetUVs(0, uvList);
                    }
                    break;

                case BveCsvStatement.CommandType.setdecaltransparentcolor:
                    {
                        var pixels = texture.GetPixels32();


                        for (var i = 0; i < pixels.Length; ++i)
                        {
                            var pixel = pixels[i];


                            if ((statement.ArgumentColor4[0] == pixel.r) && (statement.ArgumentColor4[1] == pixel.g) && (statement.ArgumentColor4[2] == pixel.b))
                            {
                                pixel.a = 0;
                                pixels[i] = pixel;
                            }
                        }


                        texture.SetPixels32(pixels);
                        texture.Apply();
                        
                        meshRenderer.material = Instantiate(Resources.Load(@"Materials/Transparent") as Material);
                        meshRenderer.material.mainTexture = texture;
                        meshRenderer.material.color = currentColor;
                        meshRenderer.material.renderQueue += renderingOrderIndex;

                        ++renderingOrderIndex;
                    }
                    break;
            }
        }
    }
}
