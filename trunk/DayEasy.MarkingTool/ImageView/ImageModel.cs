using System.ComponentModel;
using System.IO;

namespace DayEasy.MarkingTool.ImageView
{
    public class ImageModel : INotifyPropertyChanged
    {
        private string _fileName = string.Empty;
        private int _thrumbHeight;

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == value) return;
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public string DisplayFileName
        {
            get { return string.IsNullOrWhiteSpace(_fileName) ? string.Empty : Path.GetFileName(_fileName); }
        }

        public int ThrumbHeight
        {
            get { return _thrumbHeight; }
            set
            {
                if (_thrumbHeight == value) return;
                _thrumbHeight = value;
                OnPropertyChanged("ThrumbHeight");
            }
        }

        public ImageModel()
        { }

        public ImageModel(string fileName, int thrumbHeight)
            : this()
        {
            _fileName = fileName;
            _thrumbHeight = thrumbHeight;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
