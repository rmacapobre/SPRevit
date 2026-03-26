using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class CheckboxTreeView : TreeView
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        protected override void WndProc(ref Message m)
        {
            // Disable double click on checkbox
            if (m.Msg == WM_LBUTTONDBLCLK && this.CheckBoxes)
            {
                var localPos = this.PointToClient(Cursor.Position);
                var hitTestInfo = this.HitTest(localPos);
                if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                {
                    m.Msg = WM_LBUTTONDOWN;
                }
            }

            base.WndProc(ref m);
        }
    }
}
