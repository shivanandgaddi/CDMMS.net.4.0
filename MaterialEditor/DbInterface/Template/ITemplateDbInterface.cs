using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;

namespace CenturyLink.Network.Engineering.Material.Editor.DbInterface.Template
{
    public interface ITemplateDbInterface
    {
        string DbConnectionString
        {
            get;
        }

        void StartTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void Dispose();

        ITemplate GetTemplate(long templateId, bool isBaseTemplate);
    }
}
