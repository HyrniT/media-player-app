using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Data.SqlClient;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for AddPlaylistWindow.xaml
    /// </summary>
    public partial class AddPlaylistWindow : Window
    {
        ObservableCollection<File> _fileOfPlaylist = new ObservableCollection<File>();

        public AddPlaylistWindow()
        {
            InitializeComponent();
        }

        SqlConnection _connection = new SqlConnection("server=.; database=MediaPlayer;Trusted_Connection=yes");

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Music files (*.mp3)|*.mp3|Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                var _file = new File { FileName = fileName,  FilePath = filePath };
                _fileOfPlaylist.Add(_file);

                _connection.Open();
                string playlistName = tbxPlaylistName.Text;
                string sql = $"INSERT INTO Song (SongName, SongPath, SongPlaylist) VALUES (N'{fileName}', N'{filePath}', N'{playlistName}')";
                var command = new SqlCommand(sql, _connection);
                command.ExecuteNonQuery();

                _connection.Close();
            }

            listview_songOfPlaylist.ItemsSource = _fileOfPlaylist;
        }

        private void removeSongPlaylist_click(object sender, RoutedEventArgs e)
        {
            string playlistName = tbxPlaylistName.Text;

            int index = listview_songOfPlaylist.SelectedIndex;
            string songName = _fileOfPlaylist[index].FileName;

            _fileOfPlaylist.RemoveAt(index);
            listview_songOfPlaylist.ItemsSource = _fileOfPlaylist;

            _connection.Open();

            string sql = $"DELETE From Song where SongName = N'{songName}' AND SongPlaylist=N'{playlistName}'";
            var command = new SqlCommand(sql, _connection);
            command.ExecuteNonQuery();

            _connection.Close();
        }

        private void btnCreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            string playlistName = tbxPlaylistName.Text;

            _connection.Open();

            string sql = $"INSERT INTO Playlist (PlaylistName) VALUES (N'{playlistName}')";
            var command = new SqlCommand(sql, _connection);
            int isSuccess = command.ExecuteNonQuery();

            if (isSuccess != 0)
            {
                MessageBox.Show($"Created playlist: {playlistName}");
                var screen = new MainWindow();
                screen.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Error");
            }
            _connection.Close();


        }
    }
}
