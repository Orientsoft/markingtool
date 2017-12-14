using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Deyi.Tool
{
    public class ThrumbView : ViewBase
    {
        protected override object DefaultStyleKey { get { return new ComponentResourceKey(GetType(), "ThrumbView"); } }

        protected override object ItemContainerDefaultStyleKey { get { return new ComponentResourceKey(GetType(), "ThrumbViewItem"); } }

        
    }

    public class ThrumbViewModel : INotifyPropertyChanged
    {
        private string _fileName = string.Empty;
        private int _thrumbHeight;

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        public string DisplayFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fileName))
                {
                    return string.Empty;
                }

                return Path.GetFileName(_fileName);
            }
        }

        public int ThrumbHeight
        {
            get { return _thrumbHeight; }
            set
            {
                if (_thrumbHeight != value)
                {
                    _thrumbHeight = value;
                    OnPropertyChanged("ThrumbHeight");
                }
            }
        }

        public ThrumbViewModel()
        { }

        public ThrumbViewModel(string fileName, int thrumbHeight)
            : this()
        {
            _fileName = fileName;
            _thrumbHeight = thrumbHeight;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThrumbViewModelCollection : ObservableCollection<ThrumbViewModel>
    { }
}
