using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class File : INotifyPropertyChanged
    {
        public File() { }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public double FilePlayedTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
