
using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using QL4BIMspatial;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Utilities;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public enum GeometryOperationEnum
    {
        IOverlapOperator, IContainOperator

    }
    public static class GeometryOperations
    {
        // QL4BIM Inits
        private static UnityContainer container = new UnityContainer();
        private static MainInterface ql4Spatial = new MainInterface(container);

        public static Relation OverlapOperator(ModelInfo modelInfo1, ModelInfo modelInfo2)
        {
            if (modelInfo1 != null && modelInfo2 != null)
            { }
            else { return null; }
            // QL4BIMspatial.IContainOperator

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            ModelController _modelController = ModelController.Instance;

            var model1 = _modelController.GetModel(modelInfo1.modelId) as IfcModel;
            var model2 = _modelController.GetModel(modelInfo2.modelId) as IfcModel;

            var result = new Relation(Guid.Parse(modelInfo1.modelId), Guid.Parse(modelInfo2.modelId));
            var resCollection = result.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            var elements1 = model1.GetElements(modelInfo1.elementIds);
            var elements2 = model1.GetElements(modelInfo2.elementIds);

            var list_1 = GeometryHandler.CreateTriangleMeshes(elements1, model1.xModelContext);
            var list_2 = GeometryHandler.CreateTriangleMeshes(elements2, model2.xModelContext);

            // Init the Settings for the operators
            var settings = container.Resolve<ISettings>();

            settings.Direction.RaysPerSquareMeter = 100;
            settings.Direction.PositiveOffset = 100;
            // The mapping of the operators is because of the transformation of the model
            // Init the Operator 

            var op = container.Resolve<IOverlapOperator>();

            // var op = container.Resolve<ITouchOperator>();


            foreach (var item1 in list_1)
            {
                foreach (var item2 in list_2)
                {
                    var abs = op.Overlap(item1, item2);
                    if (abs)
                        resCollection.Add(new Tuple<Guid, Guid>(IfcGuid.FromIfcGuid(item1.Name), IfcGuid.FromIfcGuid(item2.Name)));
                }
            }
            return result;
        }
    }
}
