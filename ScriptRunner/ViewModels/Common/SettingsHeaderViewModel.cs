using Prism.Commands;
using ScriptRunner.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunner.ViewModels.Common
{
    public class SettingsHeaderViewModel:BaseViewModel
    {
        public event Action<SettingsHeaderViewModel> RecordDeleted;
        public event Action<SettingsHeaderViewModel> RecordEdited;


        public SettingsHeaderViewModel()
        {
            Delete = new DelegateCommand(DeleteRecord);
            Edit = new DelegateCommand(EditRecord);
        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged("Id");
            }
        }

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


        public DelegateCommand Delete { get; set; }
        public DelegateCommand Edit { get; set; }

        private void DeleteRecord()
        {
            RecordDeleted(this);
        }

        private void EditRecord()
        {
            RecordEdited(this);
        }
    }
}
