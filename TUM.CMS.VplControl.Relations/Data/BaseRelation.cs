using System;
using System.Collections;

namespace TUM.CMS.VplControl.Relations.Data
{
    public abstract class BaseRelation
    {
        public Guid Id;
        public Guid ModelId;
        public Guid ProjectId;
        public ICollection Collection;
    }
}
