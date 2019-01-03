using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Media;

namespace ModelLibrary
{
    public class FileItem : BindableBase
    {
        IEventAggregator _eventAggregator;        

        string _fileName = default(string);
        public string FileName { get => _fileName; set { SetProperty(ref _fileName, value); } }

        bool _isSelected = default(bool);
        public bool IsSelected { get => _isSelected; set { SetProperty(ref _isSelected, value); } }

        Uri _imageUri = default(Uri);
        public Uri ImageUri { get => _imageUri; set { SetProperty(ref _imageUri, value); } }

        ImageSource _imageSource = default(ImageSource);
        public ImageSource ImageSource { get => _imageSource; set { SetProperty(ref _imageSource, value); } }

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
    }
}
