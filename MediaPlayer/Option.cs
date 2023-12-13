using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class Option : INotifyPropertyChanged
    {
        
        public string Random { get; set; }
        public string Repeat { get; set; }

        public string Play { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
