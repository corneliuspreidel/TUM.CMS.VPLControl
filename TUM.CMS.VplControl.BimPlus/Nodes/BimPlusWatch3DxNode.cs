using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using BimPlus.Sdk.Data.DbCore;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using TUM.CMS.VplControl.Watch3Dx.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class BimPlusWatch3DxNode : Watch3DxNode
    {
        public BimPlusWatch3DxNode(Core.VplControl hostCanvas): base(hostCanvas)
        {
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            // Use Tesselation Render Technique
            _control.view1.RenderTechniquesManager = new TessellationTechniquesManager();
            _control.view1.RenderTechnique = _control.view1.RenderTechniquesManager.RenderTechniques[TessellationRenderTechniqueNames.PNTriangles];
            _control.view1.EffectsManager = new TessellationEffectsManager(_control.view1.RenderTechniquesManager);

            // _control.view1.RenderTechniquesManager = new DefaultRenderTechniquesManager();
            // _control.view1.RenderTechnique = _control.view1.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Blinn];
            // _control.view1.EffectsManager = new DefaultEffectsManager(_control.view1.RenderTechniquesManager);

            _control.RemoveGeometryModels();

            // Input is of Type GenericElement
            if (InputPorts[0].Data != null && InputPorts[0].Data.GetType() == typeof(List<DtObject>))
            {
                var elements = InputPorts[0].Data as List<DtObject>;

                // Loop the items of each list
                foreach (var item in elements)
                {
                    // Init the MeshBuilde
                    var mb = new MeshBuilder();

                    var points = item.AttributeGroups["geometry"].GetProperty("threejspoints") as List<Point3D>;
                    var indices = item.AttributeGroups["geometry"].GetProperty("geometryindices") as List<uint>;
                    var color = item.AttributeGroups["geometry"].GetProperty("color") is System.Drawing.Color
                        ? (System.Drawing.Color) item.AttributeGroups["geometry"].GetProperty("color")
                        : new System.Drawing.Color();

                    var listIndices = indices.Select(value => (int) value).ToList();

                    for (var i = 0; i < indices.Count; i++)
                    {
                        switch (indices[i])
                        {
                            case 0:
                                mb.AddPolygon(new List<Vector3> {
                                    new Vector3((float)points[listIndices[i]].X, (float)points[listIndices[i]].Y, (float)points[listIndices[i]].Z),
                                    new Vector3((float)points[listIndices[i + 1]].X, (float)points[listIndices[i + 1]].Y, (float)points[listIndices[i + 1]].Z),
                                    new Vector3((float)points[listIndices[i + 2]].X, (float)points[listIndices[i + 2]].Y, (float)points[listIndices[i + 2]].Z)
                                });
                                i = i + 3;
                                break;
                            case 1:
                                mb.AddPolygon(new List<Vector3> {
                                    new Vector3((float)points[listIndices[i]].X, (float)points[listIndices[i]].Y, (float)points[listIndices[i]].Z),
                                    new Vector3((float)points[listIndices[i + 1]].X, (float)points[listIndices[i + 1]].Y, (float)points[listIndices[i + 1]].Z),
                                    new Vector3((float)points[listIndices[i + 2]].X, (float)points[listIndices[i + 2]].Y, (float)points[listIndices[i + 2]].Z),
                                    new Vector3((float)points[listIndices[i + 3]].X, (float)points[listIndices[i + 3]].Y, (float)points[listIndices[i + 3]].Z)
                                });
                                i = i + 4;
                                break;
                        }
                    }


                    // for (var i = 0; i < indices.Count; i++)
                    // {
                    //     switch (indices[i])
                    //     {
                    //         case 0:
                    //             mb.AddTriangle(
                    //                 new Vector3((float)points[(int)indices[i]].X, (float)points[(int)indices[i]].Y, (float)points[(int)indices[i]].Z),
                    //                 new Vector3((float)points[(int)indices[i + 1]].X, (float)points[(int)indices[i + 1]].Y, (float)points[(int)indices[i + 1]].Z),
                    //                 new Vector3((float)points[(int)indices[i + 2]].X, (float)points[(int)indices[i + 2]].Y, (float)points[(int)indices[i + 2]].Z));
                    //             i = i + 3;
                    //             break;
                    //         case 1:
                    //             mb.AddQuad(
                    //                 new Vector3((float)points[(int)indices[i]].X, (float)points[(int)indices[i]].Y, (float)points[(int)indices[i]].Z),
                    //                 new Vector3((float)points[(int)indices[i + 1]].X, (float)points[(int)indices[i + 1]].Y, (float)points[(int)indices[i + 1]].Z),
                    //                 new Vector3((float)points[(int)indices[i + 2]].X, (float)points[(int)indices[i + 2]].Y, (float)points[(int)indices[i + 2]].Z),
                    //                 new Vector3((float)points[(int)indices[i + 3]].X, (float)points[(int)indices[i + 3]].Y, (float)points[(int)indices[i + 3]].Z));
                    //             i = i + 4;
                    //             break;
                    //     }
                    // }

                    mb.ComputeNormalsAndTangents(MeshFaces.QuadPatches, true);
                    var meshGeometry = mb.ToMeshGeometry3D();
                    var meshGeomModel = new MeshGeometryModel3D
                    {
                        Geometry = meshGeometry,
                        Material = PhongMaterials.Red,
                        // Material = new PhongMaterial
                        // {
                        //     AmbientColor = new Color4 { Alpha = color.A, Red = color.R, Blue = color.B, Green = color.G },
                        //     // DiffuseColor = new Color4 { Alpha = color.A, Red = color.R, Blue = color.B, Green = color.G },
                        //     // SpecularColor = new Color4 { Alpha = color.A, Red = color.R, Blue = color.B, Green = color.G },
                        //     // EmissiveColor = new Color4 { Alpha = color.A, Red = color.R, Blue = color.B, Green = color.G },
                        //     // SpecularShininess = 89.6f,
                        // },
                        Transform = new TranslateTransform3D()
                    };

                    // Add the Model to the viewport
                    base.VisualizeMesh(meshGeomModel);
                }
            }
            else
            {
                base.Calculate();
            }

        }

    }
}