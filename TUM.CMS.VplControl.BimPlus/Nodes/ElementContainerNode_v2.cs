using System;
using System.Collections.Generic;
using BimPlus.Sdk.Data.DbCore;
using BimPlus.Sdk.Data.Geometry;
using BimPlus.Sdk.Data.TenantDto;
using Nemetschek.NUtilLibrary;
using Newtonsoft.Json.Linq;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementContainerNode_v2 : Node
    {
        private readonly DataController _controller;
        private List<DtObject> _elements;

        public ElementContainerNode_v2(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            AddInputPortToNode("Input", typeof (object));
            AddOutputPortToNode("Elements", typeof (object));

            DataContext = this;
        }

        
        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;

            var model = new DtoModel();

            if (InputPorts[0].Data.GetType() == typeof (DtoShortProject))
            {
                var proj = InputPorts[0].Data as DtoShortProject;
                if (proj == null) return;

                model = _controller.IntBase.APICore.DtObjects.GetObjectGeometryAsThreeJs(proj.Id);

            }
            else if (InputPorts[0].Data.GetType() == typeof (DtoDivision))
            {
                var div = InputPorts[0].Data as DtoDivision;
                if (div == null) return;

                model = _controller.IntBase.APICore.DtObjects.GetObjectGeometryAsThreeJs(div.Id);
            }

            if (model == null) return;

            _elements = new List<DtObject>();
            foreach (var elem in model.Objects)
            {
                var threejsSting = elem.AttributeGroups["geometry"]["threejs"] as JObject;

                if (threejsSting == null) continue;

                var geom = threejsSting.ToObject(typeof(DbGeometry));

                // Set the color ... 
                var colorid = Convert.ToInt32(threejsSting.SelectToken("metadata").SelectToken("colorid"));
                var col = Convert.ToInt64(model.Colors[colorid]);

                ThreeJsAdapter.ParseThreeJsGeometry(geom as DbGeometry, elem, (int)col);
                _elements.Add(elem);
            }
        } 

        

        public override Node Clone()
        {
            return new ElementContainerNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void LoadGeometricRepresentation(List<DtObject> elements)
        {
            try
            {
                foreach (var elem in elements)
                {
                    if(elem.Id != Guid.Empty)
                        _controller.IntBase.APICore.DtObjects.GetObjectGeometryAsThreeJs(elem.Id);

                    if (elem.Id == Guid.Empty)
                        elem.Id = Guid.NewGuid();

                    var data = elem.AttributeGroups["geometry"]["picture"].ToString();

                    if (data != null)
                    {
                        var bytePolyeder = Convert.FromBase64String(data);
                        var NULLcompress = Convert.ToInt32(elem.AttributeGroups["geometry"]["compress"]);
                        var compress = (byte) (NULLcompress == null ? 1 : NULLcompress);

                        CBaseElementPolyeder poly = null;
                        try
                        {
                            if (Convert.ToInt32(elem.AttributeGroups["geometry"]["type"]) == 7)
                                poly = CsgGeometry.Create(bytePolyeder, compress);
                            else
                            {
                                poly = DeserializeVarbinaryMax.DeserializePolyeder(bytePolyeder, compress);

                                if (poly.face.Count == 0 && poly.edge.Count > 0)
                                    poly = Tubes.CreateTube(poly);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        // var stringMatrix = elem.AttributeGroups["geometry"].Attributes["matrix"].ToString();
                        var stringMatrix = elem.AttributeGroups["geometry"]["matrix"].ToString();
                        if (!string.IsNullOrEmpty(stringMatrix) && poly != null)
                        {
                            /*
                            var mat = new CMatrix(Convert.FromBase64String(stringMatrix));
                            poly.MultiplyWithMatrix(mat.Values);
                            */
                        }
                        // var col = elem.AttributeGroups["geometry"].Attributes["color"];
                        var col = elem.AttributeGroups["geometry"]["color"];
                        // try
                        // {
                        //     elem.Material = col != null
                        //         ? Color.FromArgb((int) Convert.ToUInt32(col))
                        //         : Color.Transparent;
                        // }
                        // catch (Exception)
                        // {
                        //     baseElem.Material = col != null ? Color.FromArgb(Convert.ToInt32(col)) : Color.Transparent;
                        // }
                        // 
                        // if (col == null || baseElem.Material == Color.FromArgb(255, 128, 128, 128))
                        // {
                        //     if (poly != null)
                        //     {
                        //         baseElem.Material = poly.IsSpecialColorDefined()
                        //             ? Color.FromArgb((int) poly.GetSpecialColor())
                        //             : Color.FromArgb(255, 128, 128, 128);
                        //     }
                        //     else
                        //         baseElem.Material = Color.FromArgb(255, 128, 128, 128);
                        // }
                        // 
                        // if (poly != null)
                        // {
                        //     poly.ID = baseElem.Id;
                        //     baseElem.Polyeder = poly;
                        //     poly.Radius = null;
                        //     poly.Bendingrolle = null;
                        // }
                        // else
                        // {
                        //     Debug.Print(
                        //         $"Invalid geometry for {((GenericElement) baseElem).TypeName} with ID {baseElem.Id}!");
                        // }
                    }

                    // elem.LoadState |= ElementLoadState.GeometryLoaded;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}