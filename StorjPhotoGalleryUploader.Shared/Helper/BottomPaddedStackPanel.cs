using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace StorjPhotoGalleryUploader.Helper
{
    public partial class BottomPaddedStackPanel : StackPanel
    {
        private ScrollViewer scroll = null;

        public BottomPaddedStackPanel()
        {
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Children.Count > 0)
            {
                if (scroll == null)
                    InitScrollViewerData();

                Size panelSize = base.MeasureOverride(availableSize);
                var lastChild = this.Children[this.Children.Count - 1];

                if (scroll == null || scroll.ViewportHeight == 0)
                    return panelSize;

                // add more space at the bottom to be able to scroll the last item into view
                if (lastChild.DesiredSize.Height < scroll.ViewportHeight)
                    panelSize.Height += 110;

                return panelSize;

            }
            return new Size();
        }

        private void InitScrollViewerData()
        {
            var lastChild = this.Children[this.Children.Count - 1];
            var lv = ItemsControl.ItemsControlFromItemContainer(lastChild);
            Border rootBorder = VisualTreeHelper.GetChild(lv, 0) as Border;
            scroll = rootBorder.Child as ScrollViewer;
            scroll.SizeChanged += (o, ev) => InvalidateMeasure();
        }
    }
}
