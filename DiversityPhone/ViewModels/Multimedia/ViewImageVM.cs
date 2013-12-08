namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    public class ViewImageVM : ViewPageVMBase<MultimediaObject>, IDeletePageVM
    {
        readonly IStoreImages ImageStore;

        private Tuple<Stream, BitmapImage> _CurrentImage;

        private void SetCurrentImage(Tuple<Stream, BitmapImage> value)
        {
            if (_CurrentImage != value)
            {
                CleanupCurrentImage();
                _CurrentImage = value;
                this.RaisePropertyChanged(x => x.CurrentImage);
            }
        }

        private void CleanupCurrentImage()
        {
            var streamAndImage = _CurrentImage;
            if (streamAndImage != null)
            {
                if (streamAndImage.Item2 != null)
                {
                    streamAndImage.Item2.UriSource = null;
                }
                if (streamAndImage.Item1 != null)
                {
                    streamAndImage.Item1.Dispose();
                }
                _CurrentImage = null;
            }
        }

        public BitmapImage CurrentImage
        {
            get
            {
                return (_CurrentImage != null) ? _CurrentImage.Item2 : null;
            }
        }

        public ICommand Delete { get; private set; }


        public ViewImageVM(
            PageVMServices Services,
            IStoreImages ImageStore)
            : base(Services, mmo => mmo.MediaType == MediaType.Image)
        {
            this.ImageStore = ImageStore;

            Services.Messenger
                .Listen<IElementVM<MultimediaObject>>(MessageContracts.VIEW)
                .Where(vm => vm.Model.MediaType == MediaType.Image && !vm.Model.IsNew())
                .Subscribe(x => Current = x);


            //View Old image
            CurrentModelObservable
                .ObserveOn(Services.Dispatcher)
                .Where(mmo => !mmo.IsNew())
                .Select(mmo =>
                {
                    var img = new BitmapImage();
                    Stream stream = null;
                    try
                    {
                        stream = ImageStore.GetMultimedia(mmo.Uri);
                        img.SetSource(stream);
                        return Tuple.Create(stream, img);
                    }
                    catch (Exception)
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                        return null;
                    }
                })
                .Subscribe(SetCurrentImage);

            var delete = new ReactiveCommand();
            Delete = delete;

            CurrentObservable
                .SampleMostRecent(delete)
                .SelectMany(toBeDeleted => Services.Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                    .Where(x => x)
                    .Select(_ => toBeDeleted)
                )
                .ObserveOn(Services.Dispatcher)
                .Do(_ => SetCurrentImage(null))
                .ToMessage(Services.Messenger, MessageContracts.DELETE);

            Services.Activation.OnDeactivation()
                .Subscribe(_ => CleanupCurrentImage());
        }


    }
}
