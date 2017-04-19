using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// Enables you to detect whether your app is in design mode in the Xamarin Forms Previewer.
    /// </summary>
    public static class DesignMode
    {
        /// <summary>
        /// Gets a value that indicates whether the process is running in design mode.
        /// </summary>
        /// <value>True if the process is running in design mode; otherwise false.</value>
        public static bool DesignModeEnabled
        {
            get
            {
                return Application.Current == null;
            }
        }
    }
}
