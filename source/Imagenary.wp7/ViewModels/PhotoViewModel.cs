using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Imagenary.Annotations;

namespace Imagenary.ViewModels
{
    public class PhotoViewModel : INotifyPropertyChanged
    {
        private string _image;
        private string _author;
        private string _comment;
        private DateTime _createdAt;

        public long Id { get; set; }
        public string Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                OnPropertyChanged();
            }
        }
        public string Author
        {
            get { return _author; }
            set
            {
                if (value == _author) return;
                _author = value;
                OnPropertyChanged();
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                OnPropertyChanged();
            }
        }
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set
            {
                if (value.Equals(_createdAt)) return;
                _createdAt = value;
                OnPropertyChanged();
            }
        }
        public string FullImage { get; set; }

        public string FileKey
        {
            get { return Image.Replace("/", "") + ".post"; }
        }

        public string FileImageKey
        {
            get { return Image.Replace("/", ""); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}