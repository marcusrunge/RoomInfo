using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModelLibrary
{
    public class FileItem : INotifyPropertyChanged
    {
        IEventAggregator _eventAggregator;
        string _fileName = default(string);
        public string FileName { get => _fileName; set { SetProperty(ref _fileName, value); } }

        bool _isSelected = default(bool);
        public bool IsSelected { get => _isSelected; set { SetProperty(ref _isSelected, value); } }

        Uri _imageSource = default(Uri);
        public Uri ImageSource { get => _imageSource; set { SetProperty(ref _imageSource, value); } }

        public int Id { get; set; }

        public FileItem()
        {
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _eventAggregator.GetEvent<FileItemSelectionChangedUpdatedEvent>().Subscribe((i) => IsSelected = i == Id);
        }

        private ICommand _selectCommand;        
        public ICommand SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((param) =>
        {
            _eventAggregator.GetEvent<FileItemSelectionChangedUpdatedEvent>().Publish(Id);
        }));

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }
        void RaisePropertyChanged([CallerMemberName]string propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
    }
}
