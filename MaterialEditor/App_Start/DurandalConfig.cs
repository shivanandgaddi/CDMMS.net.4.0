using System;
using System.Web.Optimization;

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(CenturyLink.Network.Engineering.Material.Editor.App_Start.DurandalConfig), "PreStart")]

namespace CenturyLink.Network.Engineering.Material.Editor.App_Start
{
    public static class DurandalConfig
    {
        public static void PreStart()
        {
            // Add your start logic here
            DurandalBundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}