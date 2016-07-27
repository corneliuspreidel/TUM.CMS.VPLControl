using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using BimPlus.Client.Integration;
using BimPlus.Sdk.Data.TenantDto;

namespace TUM.CMS.VplControl.BimPlus.Utilities
{
    public sealed class DataController : IPartImportsSatisfiedNotification
    {
        // Singleton
        private static DataController _instance;
        private static readonly object Padlock = new object();

        private DataController()
        {
            BimPlusModels = new Dictionary<Guid, DtoModel>();
        }

        [Import(typeof (IntegrationBase))]
        public IntegrationBase IntBase { get; set; }

        // BimPlus Models
        public Dictionary<Guid, DtoModel> BimPlusModels;

        public ContentPresenter ProjContPres { get; set; }

        public static DataController Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new DataController());
                }
            }
        }

        public void OnImportsSatisfied()
        {
            Instance.IntBase = IntBase;
        }
    }
}