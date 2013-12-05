namespace DiversityPhone.Model
{

    public static class MessageContracts
    {
        public const string SAVE_NEW = "Add";
        public const string SAVE = "Save";
        public const string DELETE = "Delete";

        public const string USE = "Use";

        public const string START = "Start";
        public const string STOP = "Stop";

        public const string VIEW = "View";
        public const string EDIT = "Edit";
        public const string NEW = "New";

        //Events
        /// <summary>
        /// Signals an application wide initialization prompting all VMs to reload data.
        /// i.e. when setup is finished, or new data has been downloaded
        /// </summary>
        public const string INIT = "Init";
    }

    public class EventMessage
    {
        public static EventMessage Default = new EventMessage();

        private EventMessage()
        {

        }

    }
}