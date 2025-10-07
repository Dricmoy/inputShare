using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace monitor_gui_dotnet
{
    public class MonitorManagerForm : Form
    {
        private enum DeviceType
        {
            Local,
            External
        }
        private class MonitorInfo
        {
            public required Screen Screen { get; set; }
            public Rectangle Rect { get; set; }
            public Point LastSnappedPos { get; set; } // store last valid snap position
            public DeviceType DeviceType { get; set; } // Local or External
            public string DeviceId { get; set; } = "local"; // unique device id
        }

        private List<MonitorInfo> monitors = new List<MonitorInfo>();
        private MonitorInfo? draggingMonitor = null;
        private Point dragOffset;

        public MonitorManagerForm()
        {
            this.Text = "Monitor Manager";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true;

            this.MouseDown += MonitorManagerForm_MouseDown;
            this.MouseMove += MonitorManagerForm_MouseMove;
            this.MouseUp += MonitorManagerForm_MouseUp;

            LoadMonitors();
            CenterMonitors();

            this.Resize += (s, e) => RecenterMonitorLayout();
            this.Layout += (s, e) => RecenterMonitorLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (var monitor in monitors)
            {
                Brush brush = monitor.Screen.Primary ? Brushes.LightBlue : Brushes.LightGray;

                if (monitor == draggingMonitor)
                    brush = new SolidBrush(Color.FromArgb(128, Color.Blue));

                e.Graphics.FillRectangle(brush, monitor.Rect);

                // Different border for local vs external monitors
                Pen borderPen = monitor.DeviceType == DeviceType.Local ? Pens.Black : Pens.Green;
                e.Graphics.DrawRectangle(borderPen, monitor.Rect);

                string text = $"{monitor.Screen.DeviceName}\n{monitor.Screen.Bounds.Width}x{monitor.Screen.Bounds.Height}" +
                              $"\n[{monitor.DeviceType}]";

                e.Graphics.DrawString(text, this.Font, Brushes.Black, monitor.Rect.Location + new Size(5, 5));
            }
        }

        private bool IsOverlapping(Rectangle rect, MonitorInfo ignoreMonitor)
        {
            foreach (var other in monitors)
            {
                if (other == ignoreMonitor) continue;
                if (rect.IntersectsWith(other.Rect))
                    return true;
            }
            return false;
        }

        private void LoadMonitors()
        {
            monitors.Clear();
            var screens = Screen.AllScreens;
            if (screens.Length == 0) return;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var screen in screens)
            {
                minX = Math.Min(minX, screen.Bounds.X);
                minY = Math.Min(minY, screen.Bounds.Y);
                maxX = Math.Max(maxX, screen.Bounds.Right);
                maxY = Math.Max(maxY, screen.Bounds.Bottom);
            }

            int totalWidth = maxX - minX;
            int totalHeight = maxY - minY;

            float scale = Math.Min((float)this.ClientSize.Width / totalWidth,
                                    (float)this.ClientSize.Height / totalHeight) * 0.7f;

            foreach (var screen in screens)
            {
                int width = (int)(screen.Bounds.Width * scale);
                int height = (int)(screen.Bounds.Height * scale);

                int x = (int)((screen.Bounds.X - minX) * scale);
                int y = (int)((screen.Bounds.Y - minY) * scale);

                monitors.Add(new MonitorInfo
                {
                    Screen = screen,
                    Rect = new Rectangle(x, y, width, height),
                    LastSnappedPos = new Point(x, y),
                    DeviceType = DeviceType.Local,
                    DeviceId = "local"
                });
            }
        }

        private void CenterMonitors()
        {
            if (monitors.Count == 0) return;

            // Compute bounding box of current layout
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var monitor in monitors)
            {
                minX = Math.Min(minX, monitor.Rect.Left);
                minY = Math.Min(minY, monitor.Rect.Top);
                maxX = Math.Max(maxX, monitor.Rect.Right);
                maxY = Math.Max(maxY, monitor.Rect.Bottom);
            }

            int groupWidth = maxX - minX;
            int groupHeight = maxY - minY;

            int offsetX = (ClientSize.Width - groupWidth) / 2 - minX;
            int offsetY = (ClientSize.Height - groupHeight) / 2 - minY;

            foreach (var m in monitors)
            {
                m.Rect = new Rectangle(
                    m.Rect.X + offsetX,
                    m.Rect.Y + offsetY,
                    m.Rect.Width,
                    m.Rect.Height
                );
                m.LastSnappedPos = m.Rect.Location;
            }
        }

        private void RecenterMonitorLayout()
        {
            if (monitors.Count == 0) return;

            // Find the bounding box of all monitors
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var m in monitors)
            {
                minX = Math.Min(minX, m.Rect.Left);
                minY = Math.Min(minY, m.Rect.Top);
                maxX = Math.Max(maxX, m.Rect.Right);
                maxY = Math.Max(maxY, m.Rect.Bottom);
            }

            int groupWidth = maxX - minX;
            int groupHeight = maxY - minY;

            int offsetX = (ClientSize.Width - groupWidth) / 2 - minX;
            int offsetY = (ClientSize.Height - groupHeight) / 2 - minY;

            // Shift all monitors by offset
            foreach (var m in monitors)
            {
                m.Rect = new Rectangle(
                    m.Rect.X + offsetX,
                    m.Rect.Y + offsetY,
                    m.Rect.Width,
                    m.Rect.Height
                );
                m.LastSnappedPos = m.Rect.Location;
            }

            Invalidate();
        }

        private Point FindSnapPosition(MonitorInfo dragging, Point desired)
        {
            Point bestPosition = desired;
            int bestDistance = int.MaxValue;

            foreach (var other in monitors)
            {
                if (other == dragging) continue;

                // Candidate positions to snap to
                var candidatePositions = new[]
                {
            new Point(other.Rect.Left - dragging.Rect.Width, other.Rect.Y), // Snap right side
            new Point(other.Rect.Right, other.Rect.Y),                     // Snap left side
            new Point(other.Rect.X, other.Rect.Bottom),                   // Snap above
            new Point(other.Rect.X, other.Rect.Top - dragging.Rect.Height) // Snap below
        };

                foreach (var candidate in candidatePositions)
                {
                    Rectangle candidateRect = new Rectangle(candidate.X, candidate.Y, dragging.Rect.Width, dragging.Rect.Height);

                    if (!IsOverlapping(candidateRect, dragging))
                    {
                        int dist = Math.Abs(candidate.X - desired.X) + Math.Abs(candidate.Y - desired.Y);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestPosition = candidate;
                        }
                    }
                    else
                    {
                        // Adjust to exactly edge without overlap
                        Rectangle adjustedRect = candidateRect;

                        if (candidate.X < dragging.Rect.X) // left snap
                            adjustedRect.X = other.Rect.Right;
                        else if (candidate.X > dragging.Rect.X) // right snap
                            adjustedRect.X = other.Rect.Left - dragging.Rect.Width;

                        if (candidate.Y < dragging.Rect.Y) // top snap
                            adjustedRect.Y = other.Rect.Bottom;
                        else if (candidate.Y > dragging.Rect.Y) // bottom snap
                            adjustedRect.Y = other.Rect.Top - dragging.Rect.Height;

                        if (!IsOverlapping(adjustedRect, dragging))
                        {
                            int dist = Math.Abs(adjustedRect.X - desired.X) + Math.Abs(adjustedRect.Y - desired.Y);
                            if (dist < bestDistance)
                            {
                                bestDistance = dist;
                                bestPosition = adjustedRect.Location;
                            }
                        }
                    }
                }
            }

            // If still overlapping at the chosen position, revert to last valid position
            Rectangle testRect = new Rectangle(bestPosition.X, bestPosition.Y, dragging.Rect.Width, dragging.Rect.Height);
            if (IsOverlapping(testRect, dragging))
                return dragging.LastSnappedPos;

            return bestPosition;
        }

        private void MonitorManagerForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (draggingMonitor != null && e.Button == MouseButtons.Left)
            {
                int newX = e.X - dragOffset.X;
                int newY = e.Y - dragOffset.Y;

                draggingMonitor.Rect = new Rectangle(newX, newY, draggingMonitor.Rect.Width, draggingMonitor.Rect.Height);
                draggingMonitor.LastSnappedPos = draggingMonitor.Rect.Location;

                Invalidate();
            }
        }

        private void MonitorManagerForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (draggingMonitor != null)
            {
                var snapPos = FindSnapPosition(draggingMonitor, draggingMonitor.Rect.Location);
                draggingMonitor.Rect = new Rectangle(snapPos.X, snapPos.Y, draggingMonitor.Rect.Width, draggingMonitor.Rect.Height);

                draggingMonitor.LastSnappedPos = draggingMonitor.Rect.Location;
                draggingMonitor = null;
                RecenterMonitorLayout();
                Invalidate();
            }
        }


        private void MonitorManagerForm_MouseDown(object? sender, MouseEventArgs e)
        {
            foreach (var monitor in monitors)
            {
                if (monitor.Rect.Contains(e.Location))
                {
                    draggingMonitor = monitor;
                    dragOffset = new Point(e.X - monitor.Rect.X, e.Y - monitor.Rect.Y);
                    break;
                }
            }
        }

        // Stub for receiving external monitors (will be needed later on for allowing external PC's monitors to be configurable from this UI) 
        public void ReceiveExternalMonitorInfo(string deviceId, List<Screen> externalScreens)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var screen in externalScreens)
            {
                minX = Math.Min(minX, screen.Bounds.X);
                minY = Math.Min(minY, screen.Bounds.Y);
                maxX = Math.Max(maxX, screen.Bounds.Right);
                maxY = Math.Max(maxY, screen.Bounds.Bottom);
            }

            int totalWidth = maxX - minX;
            int totalHeight = maxY - minY;

            float scale = Math.Min((float)this.ClientSize.Width / totalWidth,
                                    (float)this.ClientSize.Height / totalHeight) * 0.7f;

            foreach (var screen in externalScreens)
            {
                int width = (int)(screen.Bounds.Width * scale);
                int height = (int)(screen.Bounds.Height * scale);

                int x = (int)((screen.Bounds.X - minX) * scale);
                int y = (int)((screen.Bounds.Y - minY) * scale);

                monitors.Add(new MonitorInfo
                {
                    Screen = screen,
                    Rect = new Rectangle(x, y, width, height),
                    LastSnappedPos = new Point(x, y),
                    DeviceType = DeviceType.External,
                    DeviceId = deviceId
                });
            }

            RecenterMonitorLayout();
            Invalidate();
        }
    }
}
