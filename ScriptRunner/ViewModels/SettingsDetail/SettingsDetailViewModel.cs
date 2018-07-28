using Prism.Commands;
using ScriptRunner.Business;
using ScriptRunner.ViewModels.Base;
using ScriptRunner.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScriptRunner.ViewModels
{
    public class SettingsDetailViewModel : ContentViewModel
    {
        public event Action NewRecordAdded;
        public event Action<string> ShowBusy;
        public event Action StopBusy;

        private DbServerManager dbManager;
        bool canSave = false;

        public SettingsDetailViewModel() 
            : base("Add New Setting", "/Resources/add-new.png")
        {
            Databases = new ObservableCollection<DatabaseViewModel>();

            GetDbCommand = new DelegateCommand(GetDbList, CanGetDbList);
            SaveCommand = new DelegateCommand(Save, CanSave);
            ReSetCommand = new DelegateCommand(Reset, CanReset);

            dbManager = new DbServerManager();
            settingName = dbManager.GetSettingName();
            RaisePropertyChanged("SettingName");
        }

        public int Id { get; set; }

        private string serverName;
        public string ServerName
        {
            get { return serverName; }
            set
            {
                serverName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        private bool integratedSecurity;
        public bool IntegratedSecurity
        {
            get { return integratedSecurity; }
            set
            {
                integratedSecurity = value;
                RaisePropertyChanged("IntegratedSecurity");
            }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }

        private string settingName;
        public string SettingName
        {
            get { return settingName; }
            set
            {
                settingName = value;
                RaisePropertyChanged("SettingName");
            }
        }

        private ObservableCollection<DatabaseViewModel> databases;
        public ObservableCollection<DatabaseViewModel> Databases
        {
            get { return databases; }
            set
            {
                databases = value;
                RaisePropertyChanged("Databases");
            }
        }

        public DelegateCommand GetDbCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand ReSetCommand { get; set; }


        private void GetDbList()
        {
            if (ShowBusy != null)
                ShowBusy("Getting Master Data...");

            databases.Clear();
            var dbList  = dbManager.GetDbList(dbManager.CreateServerConnectionString(serverName, integratedSecurity, userName, password),Id);
            databases.AddRange(dbList);
            RaisePropertyChanged("Databases");

            canSave = true;
            SaveCommand.RaiseCanExecuteChanged();

            if (StopBusy != null)
                StopBusy();
        }

        private bool CanGetDbList()
        {
            return true;
        }

        private void Save()
        {
            if (ShowBusy != null)
                ShowBusy("Saving Setting...");

            var selectedDbList = databases.Where(t => t.IsSelected == true).ToList();

            try
            {
                var result = dbManager.SaveDbSettings(Id,serverName, integratedSecurity, userName, password, settingName, selectedDbList);
                if(Id==0)
                {
                    MessageBox.Show("New settings has been added", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Existing setting has been updated", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                NewRecordAdded();

                Reset();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error has been occured", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            if (StopBusy != null)
                StopBusy();
        }

        private bool CanSave()
        {
            return canSave;
        }

        private void Reset()
        {
            Id = 0;
            serverName = string.Empty;
            RaisePropertyChanged("ServerName");

            password = string.Empty;
            RaisePropertyChanged("Password");

            userName = string.Empty;
            RaisePropertyChanged("UserName");

            settingName = string.Empty;
            RaisePropertyChanged("SettingName");

            integratedSecurity = false;
            RaisePropertyChanged("IntegratedSecurity");

            databases.Clear();

            settingName = dbManager.GetSettingName();
            RaisePropertyChanged("SettingName");
        }

        private bool CanReset()
        {
            return true;
        }

        public void RecordEdited(SettingsHeaderViewModel item)
        {
            Id = item.Id;

            serverName = item.ServerName;
            RaisePropertyChanged("ServerName");

            password = item.Password;
            RaisePropertyChanged("Password");

            userName = item.UserName;
            RaisePropertyChanged("UserName");

            settingName = item.SettingName;
            RaisePropertyChanged("SettingName");

            integratedSecurity = item.IntegratedSecurity;
            RaisePropertyChanged("IntegratedSecurity");

            databases.Clear();

            GetDbList();
        }

    }
}
