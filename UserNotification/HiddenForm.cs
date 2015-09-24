using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FOG.Core;


namespace UserNotification
{
    public partial class HiddenForm : Form
    {
        private static List<NotificationGUI> _notifications = new List<NotificationGUI>();

        public HiddenForm()
        {
            Bus.SetMode(Bus.Mode.Client);
            Bus.Subscribe(Bus.Channel.Notification, OnNotification);
            InitializeComponent();
        }

        private static void OnNotification(dynamic data)
        {
            var notForm = new NotificationGUI();
            notForm.UpdateLocation(new Point(100 * _notifications.Count, 200));
            notForm.Show();
            _notifications.Add(notForm);
        }
    }
}
