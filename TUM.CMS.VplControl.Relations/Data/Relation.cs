using System;
using System.Collections.ObjectModel;

namespace TUM.CMS.VplControl.Relations.Data
{
    public class Relation: BaseRelation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Relation(Guid modelId, Guid projectId)
        {
            Id = Guid.NewGuid();
            ModelId = modelId;
            ProjectId = projectId;
            // Create
            base.Collection = new ObservableCollection<Tuple<Guid, Guid>>();
        }

        public static Relation Flip(Relation relation)
        {
            var res = new Relation(relation.ModelId, relation.ProjectId);
            var collection = res.Collection as ObservableCollection<Tuple<Guid, Guid>>;
            if (collection != null)
                foreach (var item in collection)
                {
                    collection.Add(new Tuple<Guid, Guid>(item.Item2, item.Item1));
                }

            return relation;
        }
    }
}
