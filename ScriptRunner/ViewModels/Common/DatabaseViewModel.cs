using ScriptRunner.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunner.ViewModels.Common
{
    public class DatabaseViewModel:BaseViewModel
    {
        //public int Id { get; set; }
        //public string Name { get; set; }
        //public bool IsSelected { get; set; }

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

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("Name");

            }
        }

    }
}
