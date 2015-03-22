using System;

namespace FOG
{
    /// <summary>
    /// Store neccesary notification information
    /// </summary>
    public class Notification
    {
        //Define variables
        public String Title { get; set; }
        public String Message { get; set; }
        public int Duration { get; set; }
		
        public Notification()
        {
            Title = "";
            Message = "";
            Duration = 10;
        }
		
        public Notification(String title, String message, int duration)
        {
            Title = title;
            Message = message;
            Duration = duration;
        }
    }
}
