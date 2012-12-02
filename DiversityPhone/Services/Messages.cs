﻿using DiversityPhone.Services;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
namespace DiversityPhone.Messages
{
    public enum DialogType
    {
        OK,
        YesNo
    }

    public static class VMMessages
    {
        public const string USED_EVENTPROPERTY_IDS = "UEID";
    }

    public static class MessageContracts
    {
        public const string SAVE = "Save";
        public const string DELETE = "Delete";
        public const string USE = "Use";

        public const string START = "Start";
        public const string STOP = "Stop";

        public const string VIEW = "View";
        public const string EDIT = "Edit";

        //Events
        public const string INIT = "Init";
        public const string REFRESH = "Refresh";
        public const string CLEAN = "Clean";
    }

    public class NavigationMessage
    {
        public Page Destination { get; private set; }
        public string Context { get; private set; }
        public ReferrerType ReferrerType { get; private set; }
        public string Referrer { get; private set; }

        public NavigationMessage(Page destination, string ctx=null, ReferrerType refType = ReferrerType.None, string referrer = null)
        {
            this.Destination = destination;
            this.Context = ctx;
            this.ReferrerType = refType;
            this.Referrer = referrer;
        }        
    }

    public class EventMessage 
    {
        public static EventMessage Default = new EventMessage();

        private EventMessage()
        {

        }
    
    }

    public static class MessengerMixin
    {
        public static IDisposable ToMessage<T>(this IObservable<T> This, string messageContract = null)
        {
            if (This == null)
                throw new ArgumentNullException("This");

            var msngr = MessageBus.Current;

            if (msngr == null) 
                throw new InvalidOperationException("No default Messenger");

            return This.Subscribe(x => msngr.SendMessage(x, messageContract));
        } 
    }
}