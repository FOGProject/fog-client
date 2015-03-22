using System;

namespace FOG
{
    /// <summary>
    /// The base of all FOG Modules
    /// </summary>
    public abstract class AbstractModule
    {

        //Basic variables every module needs
        public String Name { get; protected set; }
        public String Description { get; protected set; }
        public String EnabledURL { get; protected set; }

        protected AbstractModule()
        {
            Name = "Generic Module";
            Description = "Generic Description";
            EnabledURL = "/service/servicemodule-active.php";
        }

        /// <summary>
        /// Called to start the module. Filters out modules that are disabled on the server
        /// </summary>
        public virtual void start()
        {
            LogHandler.Log(Name, "Running...");
            if (isEnabled())
            {
                doWork();
            }
        }

        /// <summary>
        /// Called after start() filters out disabled modules. Contains the module's functionality
        /// </summary>
        protected abstract void doWork();

        /// <summary>
        /// Check if the module is enabled
        /// </summary>
        /// <returns>True if the module is enabled</returns>
        public Boolean isEnabled()
        {
            var moduleActiveResponse = CommunicationHandler.GetResponse(EnabledURL + "?moduleid=" + Name.ToLower());
            return !moduleActiveResponse.Error;
        }

    }
}