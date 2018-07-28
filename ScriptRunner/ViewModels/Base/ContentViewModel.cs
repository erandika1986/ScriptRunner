using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunner.ViewModels.Base
{
    public abstract class ContentViewModel : BaseViewModel
    {



        public ContentViewModel(string header, string icon)
        {
            Header = header;
            Icon = icon;
        }

        public string Header { get; private set; }
        public string Icon { get; private set; }

        public virtual void Update()
        {

        }
    }
}
