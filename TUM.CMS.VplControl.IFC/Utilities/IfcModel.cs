using System.Collections.Generic;
using System.Linq;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public class IfcModel: IModel
    {

        public IfcStore xModel;
        public Xbim3DModelContext xModelContext;
        public string filePath;

        public IfcModel(ModelTypes type, string filePath) : base(type)
        {
            this.filePath = filePath;
        }

        public Xbim3DModelContext GetModelContext()
        {
            return xModelContext;
        }

        public List<IIfcProduct> GetAllElements()
        {
            xModel = GetModel();

            if (xModel == null)
                return null;

            if (xModelContext == null)
                return null;

            return xModel.Instances.OfType<IIfcProduct>().ToList();
        }


        public List<IIfcProduct> GetElements(List<string> elementIds)
        {
            xModel = GetModel();

            if (xModel == null)
                return null;

            if (xModelContext == null)
                return null;
            

            var resultList = new List<IIfcProduct>();
            foreach (var item in xModel.Instances.OfType<IIfcProduct>())
            {
                if (elementIds.Contains(item.GlobalId.ToString()))
                    resultList.Add(item);
                // Loop through Entities and visualze them in the viewport
                // if (elementIds != null)
                // {
                //     var res = new HashSet<IfcGloballyUniqueId>(elementIds);
                // }
            }

            return resultList;
        }


        public IfcStore GetModel(bool writeAccess = false)
        {
            if (writeAccess == false)
            {
                return IfcStore.Open(filePath, null, false);
            }
            else
            {
                return IfcStore.Open(filePath);
            }
        }
    }
}
