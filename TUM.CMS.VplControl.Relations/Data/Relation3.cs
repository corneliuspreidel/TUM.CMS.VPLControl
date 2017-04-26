using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TUM.CMS.VplControl.Relations.Data
{
    public class Relation3: BaseRelation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Relation3(Guid modelId, Guid projectId)
        {
            Id = Guid.NewGuid();
            ModelId = modelId;
            ProjectId = projectId;
            // Create
            Collection = new ObservableCollection<Tuple<Guid, Guid, Guid>> ();
        }

        // public Relation Projection()
        // {
        //     var res = new Relation(ModelId, ProjectId);
        //     var collection = res.Collection as ObservableCollection<Tuple<Guid, Guid>>;
        // 
        //     foreach (var item in collection)
        //     {
        //         collection.Add(new Tuple<Guid, Guid>(item.Item1, item.Item3));
        //     }
        //     return res;
        // }

        public static Relation3 Join(Relation relation1, Relation relation2)
        {
            var res = new Relation3(relation1.ModelId, relation1.ProjectId);
            var collection = res.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;

            var observableCollection1 = relation1.Collection as ObservableCollection<Tuple<Guid, Guid>>;
            if (observableCollection1 == null) return null;
            var observableCollection2 = relation2.Collection as ObservableCollection<Tuple<Guid, Guid>>;
            if (observableCollection2 == null) return null;

            foreach (var item in observableCollection1)
            { 
                foreach (var item2 in observableCollection2)
                {
                    if (item.Item2.Equals(item2.Item1))
                    {
                        collection.Add(new Tuple<Guid, Guid, Guid>(item.Item1, item.Item2, item2.Item2));
                    }
                }
            }
            return res;
        }

        public static Relation Projection(Relation3 relation, int row)
        {
            // resulting Relation
            var res = new Relation(relation.ModelId, relation.ProjectId);
            var rescollection = res.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            // Source Relation
            var srcCollection = relation.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;

            foreach (var item in srcCollection)
            {
                switch (row)
                {
                    case 1:
                        rescollection.Add(new Tuple<Guid, Guid>(item.Item2, item.Item3));
                        break;
                    case 2:
                        rescollection.Add(new Tuple<Guid, Guid>(item.Item1, item.Item3));
                        break;
                    case 3:
                        rescollection.Add(new Tuple<Guid, Guid>(item.Item1, item.Item2));
                        break;
                }
            }
            return res;
        }
    }
}
