using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcWritePropertyNode : Node
    {
        private ModelController modelController;
        private Xbim3DModelContext context;
        private IfcStore xModel;

        private List<IIfcProduct> _elements;

        private IfcWritePropertyNodeControl _control;

        public IfcWritePropertyNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            modelController = ModelController.Instance;
            AddInputPortToNode("ModelInfo", typeof(ModelInfo));

            _control = new IfcWritePropertyNodeControl();
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var modelInfo = InputPorts[0].Data as ModelInfo;
            if (modelInfo == null)
                return;

            var model = modelController.GetModel(modelInfo.modelId) as IfcModel;
            if (model == null)
                return;

            // Get the model content
            xModel = model.GetModel();
            context = model.xModelContext;

            _elements = model.GetAllElements();


            // Write a Property for each element
            var propertySet = _control.PropertySetTextBox.Text;
            var property = _control.PropertyTextBox.Text;

            // foreach (var item in _elements)
            // {
            //     (item as IfcProduct).PropertySets.FirstOrDefault().
            // }

            foreach (var element in _elements)
            {
                using (var txn = xModel.BeginTransaction("Add some Properties Wall"))
                {
                    CreateSimpleProperty(xModel, element);
                    txn.Commit();
                }
            }
        }

        private static void CreateSimpleProperty(IfcStore model, IIfcProduct product)
        {
            var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
            {
                psv.Name = "IfcPropertySingleValue:Time";
                psv.Description = "";
                psv.NominalValue = new IfcTimeMeasure(150.0);
                psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.TIMEUNIT;
                    siu.Name = IfcSIUnitName.SECOND;
                });
            });

            //lets create the IfcElementQuantity
            var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
            {
                ps.Name = "Test:IfcPropertySet";
                ps.Description = "Property Set";
                ps.HasProperties.Add(ifcPropertySingleValue);
            });

            //need to create the relationship
            model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
            {
                rdbp.Name = "Property Association";
                rdbp.Description = "IfcPropertySet associated to wall";
                rdbp.RelatedObjects.Add(product as IfcProduct);
                rdbp.RelatingPropertyDefinition = ifcPropertySet;
            });

        }


        public override Node Clone()
        {
            return new IfcWritePropertyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
