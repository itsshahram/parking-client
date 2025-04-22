using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Parking.App.ANPR
{
    public class UserRect
    {
        private Canvas canvas;
        public Rect rect;
        private bool isClick = false;
        private bool isMoving = false;
        private Point oldPosition;
        private int sizeNodeRect = 5;
        private PosSizableRect nodeSelected = PosSizableRect.None;

        private enum PosSizableRect
        {
            UpMiddle,
            LeftMiddle,
            LeftBottom,
            LeftUp,
            RightUp,
            RightMiddle,
            RightBottom,
            BottomMiddle,
            None
        }

        public UserRect(Rect rect)
        {
            this.rect = rect;
        }

        public void AttachToCanvas(Canvas canvas)
        {
            this.canvas = canvas;
            if (canvas != null)
            {
                canvas.MouseDown += Canvas_MouseDown;
                canvas.MouseMove += Canvas_MouseMove;
                canvas.MouseUp += Canvas_MouseUp;
                //canvas.PaintSurface += Canvas_PaintSurface;
            }
        }

        private void Canvas_PaintSurface(object sender, EventArgs e)
        {
            Draw(canvas);
        }

        public void Draw(Canvas canvas)
        {
            // رسم مستطیل اصلی
            Rectangle rectShape = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            Canvas.SetLeft(rectShape, rect.X);
            Canvas.SetTop(rectShape, rect.Y);

            canvas.Children.Add(rectShape);

            // رسم گره‌های قابل تغییر
            foreach (PosSizableRect pos in Enum.GetValues(typeof(PosSizableRect)))
            {
                DrawNode(canvas, pos);
            }
        }

        private void DrawNode(Canvas canvas, PosSizableRect pos)
        {
            Rect node = GetNodeRect(pos);
            Rectangle nodeShape = new Rectangle
            {
                Width = node.Width,
                Height = node.Height,
                Fill = Brushes.Blue
            };

            Canvas.SetLeft(nodeShape, node.X);
            Canvas.SetTop(nodeShape, node.Y);

            canvas.Children.Add(nodeShape);
        }

        private Rect GetNodeRect(PosSizableRect position)
        {
            switch (position)
            {
                case PosSizableRect.LeftUp:
                    return new Rect(rect.X - sizeNodeRect / 2, rect.Y - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
                case PosSizableRect.RightUp:
                    return new Rect(rect.X + rect.Width - sizeNodeRect / 2, rect.Y - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
                case PosSizableRect.LeftBottom:
                    return new Rect(rect.X - sizeNodeRect / 2, rect.Y + rect.Height - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
                case PosSizableRect.RightBottom:
                    return new Rect(rect.X + rect.Width - sizeNodeRect / 2, rect.Y + rect.Height - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
                default:
                    return Rect.Empty;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isClick = true;
            oldPosition = e.GetPosition(canvas);
            nodeSelected = GetNodeSelectable(oldPosition);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isClick) return;

            Point currentPosition = e.GetPosition(canvas);
            Vector delta = currentPosition - oldPosition;

            if (nodeSelected == PosSizableRect.None)
            {
                // حرکت کل مستطیل
                rect.X += delta.X;
                rect.Y += delta.Y;
            }
            else
            {
                // تغییر اندازه مستطیل
                ResizeRectangle(nodeSelected, delta);
            }

            oldPosition = currentPosition;
            canvas.InvalidateVisual();
        }

        private void ResizeRectangle(PosSizableRect position, Vector delta)
        {
            switch (position)
            {
                case PosSizableRect.LeftUp:
                    rect.X += delta.X;
                    rect.Y += delta.Y;
                    rect.Width -= delta.X;
                    rect.Height -= delta.Y;
                    break;
                case PosSizableRect.RightBottom:
                    rect.Width += delta.X;
                    rect.Height += delta.Y;
                    break;
                    // سایر جهت‌ها را می‌توانید پیاده‌سازی کنید
            }

            // اطمینان از حداقل اندازه
            if (rect.Width < 10) rect.Width = 10;
            if (rect.Height < 10) rect.Height = 10;
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isClick = false;
        }

        private PosSizableRect GetNodeSelectable(Point point)
        {
            foreach (PosSizableRect pos in Enum.GetValues(typeof(PosSizableRect)))
            {
                if (GetNodeRect(pos).Contains(point))
                {
                    return pos;
                }
            }
            return PosSizableRect.None;
        }
    }
}