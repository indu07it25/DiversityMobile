﻿
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Collections;
using System;
using DiversityPhone.ViewModels;
using DiversityPhone.Services;
using DiversityPhone.Messages;
using System.Reactive.Disposables;
using DiversityPhone.Model;
namespace DiversityPhone.View.Appbar
{
    public class NewMultimediaAppBarUpdater
    {
        PhoneApplicationPage _page;
        IApplicationBar _appbar;
        ElementMultimediaVM _vm;
        IMultimediaOwner _mmowner;

        ApplicationBarIconButton _image, _audio, _video;
        IList _buttons, _mmobuttons;
        IDisposable _back_key;

        public NewMultimediaAppBarUpdater(PhoneApplicationPage page, ElementMultimediaVM vm)
        {
            _page = page;
            _appbar = _page.ApplicationBar;
            _vm = vm;
            _back_key = Disposable.Empty;

            if (_vm == null)
                throw new ArgumentNullException("vm");

            _vm.NewMultimediaObservable
                .Subscribe(m => show_mmo_buttons(m));

            _mmobuttons = new List<object>();
            _image =
                new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.feature.camera.rest.png", UriKind.RelativeOrAbsolute),
                    IsEnabled = true,
                    Text = "image"
                };            
            _mmobuttons.Add(_image);

            _audio =
                new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.feature.audio.rest.png", UriKind.RelativeOrAbsolute),
                    IsEnabled = true,
                    Text = "audio"
                };
            
            _mmobuttons.Add(_audio);

            _video =
                new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.feature.video.rest.png", UriKind.RelativeOrAbsolute),
                    IsEnabled = true,
                    Text = "video"
                };            
            _mmobuttons.Add(_video);

            Observable.Merge(
                Observable.FromEventPattern<object, EventArgs>(_image, "Click")
                    .Select(_ => MediaType.Image),
                Observable.FromEventPattern<object, EventArgs>(_audio, "Click")
                    .Select(_ => MediaType.Image),
                Observable.FromEventPattern<object, EventArgs>(_video, "Click")
                    .Select(_ => MediaType.Image)
                    )
                    .Do(_ => restore_buttons())
                    .Select(media => new MultimediaObjectVM(new MultimediaObject() { MediaType = media, OwnerType = _mmowner.OwnerType, RelatedId = _mmowner.OwnerID }) as IElementVM<MultimediaObject>)
                    .ToMessage(MessageContracts.EDIT);

            
            _buttons = new List<object>();
            foreach (var btn in _appbar.Buttons)
                _buttons.Add(btn);
        }

        void show_mmo_buttons(IMultimediaOwner mmowner)
        {
            _mmowner = mmowner;
            _appbar.Buttons.Clear();
            foreach (var btn in _mmobuttons)
            {
                _appbar.Buttons.Add(btn);
            }
            _back_key = Disposable.Create(() => _page.BackKeyPress -= backkeyhandler);
            _page.BackKeyPress += backkeyhandler;
        }

        void backkeyhandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;            
            restore_buttons();
        }

        void restore_buttons()
        {
            _appbar.Buttons.Clear();
            foreach (var btn in _buttons)
            {
                _appbar.Buttons.Add(btn);
            }
            _back_key.Dispose();
        }
    }
}
