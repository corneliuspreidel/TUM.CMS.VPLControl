using System;
using System.Collections.Generic;
using System.Linq;
using BimPlus.Sdk.Data.DbCore;

namespace TUM.CMS.VplControl.BimPlus.Utilities
{
    public enum ModelType
    {
        BimPlusModel,
        IfcModel
    }

    public class ModelInfo
    {
        // Member variables, to identify the subset of a model
        public DataController _controller;
        public string ProjectId;
        public string ModelId;
        public List<Guid> ElementIds;
        public ModelType ModelType;

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelInfo(string projectId, string modelId, List<Guid> elementIds, ModelType modelType)
        {
            ProjectId = projectId;
            ModelId = modelId;
            ElementIds = elementIds;
            ModelType = modelType;

            _controller = DataController.Instance;
        }

        public List<DtObject> GetCurrentElements()
        {
            if (ModelType == ModelType.BimPlusModel)
            {
                return _controller.BimPlusModels[Guid.Parse(ModelId)].Objects.Where(item => ElementIds.Contains(item.Id)).ToList();
            }

            return null;
        }

    }
}
