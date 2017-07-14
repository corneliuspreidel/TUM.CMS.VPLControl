using System.Collections.Generic;

namespace TUM.CMS.VplControl.Utilities
{
    public sealed class ModelController
    {
        // Singleton
        private static ModelController _instance;
        private static readonly object Padlock = new object();
        public List<IModel> ModelStorage;

        private ModelController()
        {
            ModelStorage = new List<IModel>();
        }

        public static ModelController Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new ModelController());
                }
            }
        }

        /// <summary>
        /// Models will be added to the DataController.
        /// 
        /// GUID is the file Path.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddModel( IModel model)
        {
            ModelStorage.Add(model);
            return true;
        }

        /// <summary>
        /// Models will be removed from the DataController.
        /// 
        /// Benefit: No big DataController
        /// </summary>
        /// <param name="fileString"></param>
        /// <returns></returns>
        public bool RemoveModel(IModel model)
        {
            ModelStorage.Remove(model);
            return true;
        }

        public IModel GetModel(string id)
        {
            foreach (var model in ModelStorage)
            {
                if (Equals(model.id.ToString(), id))
                    return model;
            }
            return null;
        }
    }
}
