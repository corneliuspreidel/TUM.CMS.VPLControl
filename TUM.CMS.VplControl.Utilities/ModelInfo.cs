using System;
using System.Collections.Generic;

namespace TUM.CMS.VplControl.Utilities
{
    public class ModelInfo
    {
        public string modelId;
        public List<string> elementIds;
        public ModelTypes modelType;

        public ModelInfo()
        {
            elementIds = new List<string>();
        }

        public ModelInfo(string modelId, List<string> elementIds, ModelTypes type)
        {
            this.modelId = modelId;
            this.elementIds = elementIds;
            this.modelType = type;
        }

    } 
}