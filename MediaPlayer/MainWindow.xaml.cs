using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _mediaPlayerIsPlaying = false;
        private bool _userIsDraggingSlider = false;

        private string _currentFilePath = string.Empty;
        private string _currentPlaylistName = string.Empty;
        private int _currentIndexFile = -1;

        SqlConnection _connection = new SqlConnection("server=.; database=MediaPlayer;Trusted_Connection=yes");

        ObservableCollection<File> _files = new ObservableCollection<File>();
        ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>();

        Option option = new Option();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentFileName
        {
            get
            {
                var info = new FileInfo(_currentFilePath);
                return info.Name;
            }
            set { }
        }

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            sliVolume.ValueChanged += sliVolume_ValueChanged;
            sliVolume.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(sliVolume_MouseLeftButtonUp), true);
        }

        private void sliVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var volumn = sender as Slider;
            player.Volume = volumn.Value;
        }

        private void sliVolume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var volumn = sender as Slider;
            player.Volume = volumn.Value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlaylist();
            _mediaPlayerIsPlaying = false;
            option.Random = "Off";
            option.Repeat = "Off";
            option.Play = "icons/play.png";
            tbRandom.DataContext = option;
            tbRepeat.DataContext = option;
            btnPlayPause.DataContext = option;
        }

        private void SwitchScreen(string path)
        {
            FileInfo fi = new FileInfo(path);
            string extension = fi.Extension;

            if (extension != ".mp3")
            {
                _mediaPlayerIsPlaying = false;
                music.Visibility = Visibility.Collapsed;
                player.Visibility = Visibility.Visible;
            }
            else
            {
                _mediaPlayerIsPlaying = true;
                music.Visibility = Visibility.Visible;
                player.Visibility = Visibility.Collapsed;

                //playingSong.Text = fi.Name;
            }
        }

        private void LoadPlaylist()
        {
            _playlists.Clear();
            //Load all playlist
            string sql = "Select * from Playlist";

            _connection.Open();
            var command = new SqlCommand(sql, _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("PlaylistName"));
                var _playlist = new Playlist { PlaylistName = name };
                _playlists.Add(_playlist);
            }

            lisPlaylists.ItemsSource = _playlists;

            _connection.Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((player.Source != null) && (player.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = player.Position.TotalSeconds;
            }
        }
        private void btnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var screen = new AddPlaylistWindow();
            screen.Show();
            this.Close();
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Music files (*.mp3)|*.mp3|Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);

                var _file = new File { FileName = fileName, FilePath = filePath };
                _files.Add(_file);

                _connection.Open();
                string sql = $"INSERT INTO Song (SongName, SongPath, SongPlaylist) VALUES (N'{fileName}', N'{filePath}', N'{_currentPlaylistName}')";
                var command = new SqlCommand(sql, _connection);
                command.ExecuteNonQuery();

                _connection.Close();
            }

            lisFiles.ItemsSource = _files;
        }


        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            int index = lisFiles.SelectedIndex;

            option.Play = "icons/play.png";

            if (index >= 0 && index < _files.Count)
            {
                if (index == 0) index++;
                index--;
                string filePath = _files[index].FilePath;
                string fileName = _files[index].FileName;

                player.Source = new Uri(filePath);
                player.Play();
                _mediaPlayerIsPlaying = true;

                _currentFilePath = filePath;
                _currentFileName = fileName;
                _currentIndexFile = index;
                lisFiles.SelectedIndex = index;
                this.Title = $"Playlist: {_currentPlaylistName} | File: {_currentFileName}";
                SwitchScreen(_currentFilePath);
            }
            else
            {
                // do nothing
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int index = lisFiles.SelectedIndex;

            option.Play = "icons/play.png";

            if (index >= 0 && index < _files.Count)
            {
                if (index == _files.Count - 1) index--;
                index++;
                string filePath = _files[index].FilePath;
                string fileName = _files[index].FileName;

                player.Source = new Uri(filePath);
                player.Play();
                _mediaPlayerIsPlaying = true;

                _currentFilePath = filePath;
                _currentFileName = fileName;
                _currentIndexFile = index;
                lisFiles.SelectedIndex = index;
                this.Title = $"Playlist: {_currentPlaylistName} | File: {_currentFileName}";
                SwitchScreen(_currentFilePath);
            }
            else
            {
                // do nothing
            }
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayerIsPlaying == true)
            {
                player.Pause();
                _mediaPlayerIsPlaying = false;
                option.Play = "icons/play.png";
            }
            else
            {
                player.Play();
                _mediaPlayerIsPlaying = true;
                option.Play = "icons/pause.png";

            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            _mediaPlayerIsPlaying = false;
            option.Play = "icons/play.png";
        }


        private void sliProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;

        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            player.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbCurrentProgress.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss");
        }

        private void tbKeywordPlaylists_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = tbKeywordPlaylists.Text;
            ObservableCollection<Playlist> _subPlaylists = new ObservableCollection<Playlist>();
            for (int i = 0; i < _playlists.Count; i++)
            {
                if (_playlists[i].PlaylistName.Contains(keyword))
                {
                    _subPlaylists.Add(_playlists[i]);
                }
            }
            lisPlaylists.ItemsSource = _subPlaylists;
        }

        private void tbKeywordFilelists_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = tbKeywordFilelists.Text;
            ObservableCollection<File> _subFilelists = new ObservableCollection<File>();
            for (int i = 0; i < _files.Count; i++)
            {
                if (_files[i].FileName.Contains(keyword))
                {
                    _subFilelists.Add(_files[i]);
                }
            }
            lisFiles.ItemsSource = _subFilelists;
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            tbEndProgress.Text = TimeSpan.FromSeconds(player.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"mm\:ss");
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            _mediaPlayerIsPlaying = false;

            int index = lisFiles.SelectedIndex;
            string filePath = _files[index].FilePath;
            string fileName = _files[index].FileName;
            _currentFilePath = filePath;
            _currentFileName = fileName;
            if (option.Random == "On")
            {
                Random rng = new Random();
                int count = _files.Count;
                index = rng.Next(0, count);
                filePath = _files[index].FilePath;
                fileName = _files[index].FileName;

                player.Source = new Uri(filePath);
                player.Play();
                _mediaPlayerIsPlaying = true;

                _currentFilePath = filePath;
                _currentFileName = fileName;
                _currentIndexFile = index;
                _mediaPlayerIsPlaying = true;
            }
            
            if (option.Repeat == "On: File")
            {
                player.Source = new Uri(filePath);
                player.Play();
                _mediaPlayerIsPlaying = true;
            }
            if (option.Repeat == "On: Playlist")
            {
                _currentIndexFile++;
                int indexTemp = _currentIndexFile % _files.Count;

                filePath = _files[indexTemp].FilePath;
                fileName = _files[indexTemp].FileName;

                player.Source = new Uri(filePath);
                player.Play();
                _mediaPlayerIsPlaying = true;

                _currentFilePath = filePath;
                _currentFileName = fileName;
                _currentIndexFile = indexTemp;
                _mediaPlayerIsPlaying = true;
            }

            this.Title = $"Playlist: {_currentPlaylistName} | File: {_currentFileName}";
            SwitchScreen(_currentFilePath);
            //AddItemsToRentlyPlaylist(mePlayer.Source.ToString());
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            int indexPlaylist = lisPlaylists.SelectedIndex;
            string playlistName = _playlists[indexPlaylist].PlaylistName;

            _connection.Open();
            string sql = $"Delete From Playlist Where PlaylistName=N'{playlistName}'";
            var command = new SqlCommand(sql, _connection);
            command.ExecuteNonQuery();
            _connection.Close();

            _playlists.RemoveAt(indexPlaylist);
            LoadPlaylist();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _files.Clear();

            int index = lisPlaylists.SelectedIndex;

            if (index != -1)
            {
                string playlistName = _playlists[index].PlaylistName;
                _connection.Open();

                string sql = $"Select * From Song Where SongPlaylist = N'{playlistName}'";
                var command = new SqlCommand(sql, _connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(reader.GetOrdinal("SongName"));
                    string path = reader.GetString(reader.GetOrdinal("SongPath"));

                    var _file = new File { FileName = name, FilePath = path };
                    _files.Add(_file);
                }

                lisFiles.ItemsSource = _files;
                _currentPlaylistName = playlistName;

                _connection.Close();
                this.Title = $"Playlist: {_currentPlaylistName}";
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int indexPlaylist = lisPlaylists.SelectedIndex;
            string playlistName = _playlists[indexPlaylist].PlaylistName;

            _connection.Open();
            string sql = $"Delete From Playlist Where PlaylistName=N'{playlistName}'";
            var command = new SqlCommand(sql, _connection);
            command.ExecuteNonQuery();
            _connection.Close();

            _playlists.RemoveAt(indexPlaylist);
            _currentPlaylistName = playlistName;
            LoadPlaylist();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            int index = lisFiles.SelectedIndex;
            string songName = _files[index].FileName;

            _files.RemoveAt(index);
            lisFiles.ItemsSource = _files;

            _connection.Open();

            string sql = $"DELETE From Song where SongName = N'{songName}' AND SongPlaylist=N'{_currentPlaylistName}'";
            var command = new SqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            _connection.Close();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            int index = lisFiles.SelectedIndex;
            option.Play = "icons/pause.png";

            string filePath = _files[index].FilePath;
            string fileName = _files[index].FileName;

            player.Source = new Uri(filePath);
            player.Play();
            _mediaPlayerIsPlaying = true;

            _currentFilePath = filePath;
            _currentFileName = fileName;
            _currentIndexFile = index;
            this.Title = $"Playlist: {_currentPlaylistName} | File: {_currentFileName}";
            SwitchScreen(_currentFilePath);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            _files.Clear();

            int index = lisPlaylists.SelectedIndex;

            if (index != -1)
            {
                string playlistName = _playlists[index].PlaylistName;
                _connection.Open();

                string sql = $"Select * From Song Where SongPlaylist = N'{playlistName}'";
                var command = new SqlCommand(sql, _connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(reader.GetOrdinal("SongName"));
                    string path = reader.GetString(reader.GetOrdinal("SongPath"));

                    var _file = new File { FileName = name, FilePath = path };
                    _files.Add(_file);
                }

                lisFiles.ItemsSource = _files;

                _connection.Close();
                this.Title = $"Playlist: {_currentPlaylistName}";
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void ListViewItem_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            int index = lisFiles.SelectedIndex;
            option.Play = "icons/pause.png";

            string filePath = _files[index].FilePath;
            string fileName = _files[index].FileName;

            player.Source = new Uri(filePath);
            player.Play();
            _mediaPlayerIsPlaying = true;

            _currentFilePath = filePath;
            _currentFileName = fileName;
            _currentIndexFile = index;
            this.Title = $"Playlist: {_currentPlaylistName} | File: {_currentFileName}";
            SwitchScreen(_currentFilePath);
            //AddItemsToRentlyPlaylist(mePlayer.Source.ToString());
        }

        private void btnSearchFile_Click(object sender, RoutedEventArgs e)
        {
            string keyword = tbKeywordFilelists.Text;
            ObservableCollection<File> _subFilelists = new ObservableCollection<File>();
            for (int i = 0; i < _files.Count; i++)
            {
                if (_files[i].FileName.Contains(keyword))
                {
                    _subFilelists.Add(_files[i]);
                }
            }
            lisFiles.ItemsSource = _subFilelists;
        }

        private void btnSearchPlaylist_Click(object sender, RoutedEventArgs e)
        {
            string keyword = tbKeywordPlaylists.Text;
            ObservableCollection<Playlist> _subPlaylists = new ObservableCollection<Playlist>();
            for (int i = 0; i < _playlists.Count; i++)
            {
                if (_playlists[i].PlaylistName.Contains(keyword))
                {
                    _subPlaylists.Add(_playlists[i]);
                }
            }
            lisPlaylists.ItemsSource = _subPlaylists;
        }

        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (option.Repeat == "Off")
            {
                option.Repeat = "On: File";
                option.Random = "Off";
            }
            else if (option.Repeat == "On: File")
            {
                option.Repeat = "On: Playlist";
                option.Random = "Off";
            }    
            else
            {
                option.Repeat = "Off";
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            if (option.Random == "Off")
            {
                option.Random = "On";
                option.Repeat = "Off";
            }
            else
            {
                option.Random = "Off";
            }
        }
    }
}