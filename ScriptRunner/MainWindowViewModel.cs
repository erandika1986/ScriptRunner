using ScriptRunner.ViewModels;
using ScriptRunner.ViewModels.Base;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScriptRunner
{
    public class MainWindowViewModel : BaseViewModel
    {
        private SettingsDetailViewModel settingsDetail;
        private SettingsViewModel settingsVm;
        private FileBrowserViewModel fileBrowser;

        public MainWindowViewModel()
        {
            Contents = new ObservableCollection<ContentViewModel>();
            isShowBusy = true;
            RaisePropertyChanged("IsShowBusy");
            busyMessage = "Launching....";
            RaisePropertyChanged("BusyMessage");

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += timer_Tick;
            timer.Start();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                (sender as DispatcherTimer).Stop();
                Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Init()
        {
            fileBrowser = new FileBrowserViewModel();
            Contents.Add(fileBrowser);

            settingsVm = new SettingsViewModel();
            settingsVm.RecordEdited += SettingsVm_RecordEdited;
            Contents.Add(settingsVm);

            settingsDetail = new SettingsDetailViewModel();
            settingsDetail.NewRecordAdded += SettingsDetail_NewRecordAdded;
            Contents.Add(settingsDetail);

            fileBrowser.ShowBusy += FileBrowser_ShowBusy;
            fileBrowser.StopBusy += FileBrowser_StopBusy;

            settingsVm.ShowBusy += FileBrowser_ShowBusy;
            settingsVm.StopBusy += FileBrowser_StopBusy;

            settingsDetail.ShowBusy += FileBrowser_ShowBusy;
            settingsDetail.StopBusy += FileBrowser_StopBusy;

            CurrentContent = Contents.FirstOrDefault();

            isShowBusy = false;
            RaisePropertyChanged("IsShowBusy");
        }

        private void FileBrowser_StopBusy()
        {
            isShowBusy = false;
            RaisePropertyChanged("IsShowBusy");
        }

        private void FileBrowser_ShowBusy(string message)
        {
            isShowBusy = true;
            RaisePropertyChanged("IsShowBusy");

            busyMessage = message;
            RaisePropertyChanged("BusyMessage");
        }

        private void SettingsDetail_NewRecordAdded()
        {
            settingsVm.ReloadData();
            fileBrowser.ReloadData();
        }

        private void SettingsVm_RecordEdited(ViewModels.Common.SettingsHeaderViewModel obj)
        {
            CurrentContent = Contents.LastOrDefault();
            settingsDetail.RecordEdited(obj);
        }

        public ObservableCollection<ContentViewModel> Contents { get; private set; }

        private ContentViewModel currentContent;
        public ContentViewModel CurrentContent
        {
            get { return currentContent; }
            set
            {
                currentContent = value;
                RaisePropertyChanged("CurrentContent");
                currentContent.Update();
            }
        }

        private bool isShowBusy;
        public bool IsShowBusy
        {
            get { return isShowBusy; }
            set
            {
                isShowBusy = value;
                RaisePropertyChanged("IsShowBusy");
            }
        }

        private string busyMessage;
        public string BusyMessage
        {
            get { return busyMessage; }
            set
            {
                busyMessage = value;
                RaisePropertyChanged("BusyMessage");
            }
        }

    }


}
