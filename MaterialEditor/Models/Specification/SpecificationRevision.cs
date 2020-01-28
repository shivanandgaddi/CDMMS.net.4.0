using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Specification
{
    public abstract class SpecificationRevision : Specification
    {
        private long revisionId = 0;

        public SpecificationRevision()
        {
        }

        public SpecificationRevision(long specificationRevisionId)
        {
            revisionId = specificationRevisionId;
        }

        public SpecificationRevision(long specificationRevisionId, long specificationId) : base(specificationId)
        {
            revisionId = specificationRevisionId;
        }

        public SpecificationRevision(long specificationRevisionId, long specificationId, string specificationName) : base(specificationId, specificationName)
        {
            revisionId = specificationRevisionId;
        }

        public long RevisionId
        {
            get
            {
                return revisionId;
            }
            set
            {
                revisionId = value;
            }
        }

        public bool IsRecordOnly
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get;
            set;
        }

        public bool IsPropagated
        {
            get;
            set;
        }

        public bool IsDeleted
        {
            get;
            set;
        }
    }
}