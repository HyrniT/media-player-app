using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class Playlist : INotifyPropertyChanged
    {
        public Playlist() { }

        private string _playlistName;
        public string PlaylistName
        {
            get 
            {
                return _playlistName;
            }
            set
            { 
                _playlistName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PlaylistName"));
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlaylistName"));
            }

        }
        public string PlaylistPath { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private string repeat;

        public string Repeat { get => repeat; set => SetProperty(ref repeat, value); }

        private string random;

        public string Random { get => random; set => SetProperty(ref random, value); }

    }
}
