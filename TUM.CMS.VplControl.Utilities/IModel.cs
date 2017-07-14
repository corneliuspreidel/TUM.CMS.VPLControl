using System;
using System.Collections.Generic;

namespace TUM.CMS.VplControl.Utilities
{
    public enum ModelTypes
    {
        IFC,
        BimPlus
    }

    public abstract class IModel
    {
        public string name;
        public Guid id;
        public ModelTypes modelType;

        public IModel(ModelTypes type)
        {
            id = Guid.NewGuid();
            modelType = type;
        }
    }

    
}
