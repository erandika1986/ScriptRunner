using Prism.Commands;
using ScriptRunner.Business;
using ScriptRunner.ViewModels.Base;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptRunner.ViewModels.Common;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace ScriptRunner.ViewModels
{
    public class FileBrowserViewModel : ContentViewModel
    {
        public event Action<string> ShowBusy;
        public event Action StopBusy;
        private DbServerManager dbManager;

        public FileBrowserViewModel() 
            : base("Run Script", "/Resources/script.png")
        {
            dbManager = new DbServerManager();
            BrowseCommand = new DelegateCommand(Browse);
            RunCommand = new DelegateCommand(Run, CanRun);

            settings = new ObservableCollection<SettingsHeaderViewModel>();
            logs = new ObservableCollection<ScriptRunResult>();

            Init();
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                RaisePropertyChanged("FilePath");
            }
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

        private SettingsHeaderViewModel selectedSetting;
        public SettingsHeaderViewModel SelectedSetting
        {
            get { return selectedSetting; }
            set
            {
                selectedSetting = value;
                RaisePropertyChanged("SelectedSetting");
            }
        }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                script = value;
                RaisePropertyChanged("Script");
            }
        }

        public string FileName { get; set; }


        private ObservableCollection<ScriptRunResult> logs;
        public ObservableCollection<ScriptRunResult> Logs
        {
            get { return logs; }
            set
            {
                logs = value;
                RaisePropertyChanged("Logs");
            }
        }



        public DelegateCommand BrowseCommand { get; set; }
        public DelegateCommand RunCommand { get; set; }

        private void Init()
        {
            var allSettings = dbManager.GetAllSettings();
            settings.AddRange(allSettings);
            RaisePropertyChanged("Settings");

            if(settings.Count>0)
            {
                selectedSetting = settings.FirstOrDefault();
                RaisePropertyChanged("SelectedSetting");
            }

        }

        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SQL files (*.sql)|*.sql";
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Title = "Please select a script file to run.";
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.SafeFileName;
                filePath = openFileDialog.FileName;
                RaisePropertyChanged("FilePath");
                RunCommand.RaiseCanExecuteChanged();

                script = File.ReadAllText(openFileDialog.FileName);
                RaisePropertyChanged("Script");
            }

        }

        private void Run()
        {
            if (ShowBusy != null)
                ShowBusy("Executing Selected SQL Script.....");

            try
            {
                logs.Clear();
                var response = dbManager.RunScript(script, FileName, selectedSetting.Id);

                logs.AddRange(response);
                RaisePropertyChanged("Logs");

                MessageBox.Show("Script execution completed for all the database. Please view detail result view for more information.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error has been occured.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            if (StopBusy != null)
                StopBusy();
        }

        private bool CanRun()
        {
            if(string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            else
            {
                return true;
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
