
using InTheHand.Forms;
using InTheHand.Forms.Platform.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly:ExportRenderer(typeof(AutoViewCell), typeof(AutoViewCellRenderer))]
namespace InTheHand.Forms.Platform.iOS
{
    public sealed class AutoViewCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            ViewCell vc = item as ViewCell;

            if (vc != null)
            {
                var sr = vc.View.Measure(tv.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

                if (vc.Height != sr.Request.Height)
                {
                    vc.ForceUpdateSize();

                    sr = vc.View.Measure(tv.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    vc.Height = sr.Request.Height;
                }
            }

            return base.GetCell(item, reusableCell, tv);
        }
    }
}