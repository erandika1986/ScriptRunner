using ScriptRunner.Business;
using ScriptRunner.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using ScriptRunner.ViewModels.Common;

namespace ScriptRunner.ViewModels
{
    public class SettingsViewModel : ContentViewModel
    {
        private DbServerManager dbManager;
        public event Action<SettingsHeaderViewModel> RecordEdited;
        public event Action<string> ShowBusy;
        public event Action StopBusy;

        public SettingsViewModel()
            :base("Settings", "/Resources/settings.png")
        {
            dbManager = new DbServerManager();
            settings = new ObservableCollection<SettingsHeaderViewModel>();
            Init();
        }


        private void Init()
        {
            if(ShowBusy!=null)
                ShowBusy("Loading data...");
            var records = dbManager.GetAllSettings();
            foreach(var item in records)
            {
                item.RecordDeleted += Item_RecordDeleted;
                item.RecordEdited += Item_RecordEdited;
                settings.Add(item);
            }
            RaisePropertyChanged("Settings");

            if(StopBusy!=null)
                StopBusy();
        }

        private void Item_RecordEdited(Common.SettingsHeaderViewModel obj)
        {
            RecordEdited(obj);
        }

        private void Item_RecordDeleted(Common.SettingsHeaderViewModel obj)
        {
            ShowBusy("deleting selected item...");
            try
            {
                dbManager.DeleteSetting(obj.Id);
                settings.Remove(obj);
                RaisePropertyChanged("Settings");
                MessageBox.Show("Selected setting has been deleted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error has been occured", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            StopBusy();
        }

        private ObservableCollection<SettingsHeaderViewModel> settings;
        public ObservableCollection<SettingsHeaderViewModel> Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        public void ReloadData()
        {
            settings.Clear();
            RaisePropertyChanged("Settings");

            Init();
        }

    }


}
