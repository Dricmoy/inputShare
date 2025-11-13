using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace monitor_gui_dotnet
{
    public class MonitorManagerForm : Form
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;

        private enum DeviceType
        {
            Local,
            External
        }

        private class MonitorInfo
        {
            public required Screen Screen { get; set; }
            public Rectangle Rect { get; set; }          // scaled GUI rectangle
            public Point LastSnappedPos { get; set; }
            public DeviceType DeviceType { get; set; }
            public string DeviceId { get; set; } = "local";
        }

        private bool identifyMode = false;
        private List<Form> identifyOverlays = new List<Form>();
        private bool showMonitorDetails = false;

        private List<MonitorInfo> monitors = new List<MonitorInfo>();
        private MonitorInfo? draggingMonitor = null;
        private Point dragOffset;

        public MonitorManagerForm()
        {
            this.Text = "Monitor Manager";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.DoubleBuffered = true;

            this.MouseDown += MonitorManagerForm_MouseDown;
            this.MouseMove += MonitorManagerForm_MouseMove;
            this.MouseUp += MonitorManagerForm_MouseUp;

            LoadMonitors();
            CenterMonitors();

            this.Resize += (s, e) => RecenterMonitorLayout();
            this.Layout += (s, e) => RecenterMonitorLayout();

            CheckBox showDetailsCheckBox = new CheckBox
            {
                Text = "Show Details",
                Checked = false,
                Dock = DockStyle.Bottom,
                Height = 20,
                Padding = new Padding(20, 0, 0, 0)
            };
            showDetailsCheckBox.CheckedChanged += (s, e) =>
            {
                showMonitorDetails = showDetailsCheckBox.Checked;
                Invalidate();
            };
            this.Controls.Add(showDetailsCheckBox);

            Button identifyButton = new Button
            {
                Text = "Identify",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            identifyButton.Click += (s, e) =>
            {
                identifyMode = !identifyMode;
                if (identifyMode)
                    StartIdentify();
                else
                    StopIdentify();
            };
            this.Controls.Add(identifyButton);

            Button saveButton = new Button
            {
                Text = "Save Layout",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            saveButton.Click += (s, e) => SaveMonitorLayout();
            this.Controls.Add(saveButton);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var monitor in monitors)
            {
                Rectangle rect = monitor.Rect;

                Color bgColor = monitor.Screen.Primary ? Color.FromArgb(200, 220, 240) : Color.FromArgb(230, 230, 230);
                if (monitor == draggingMonitor)
                    bgColor = Color.FromArgb(180, 200, 230);

                using (Brush brush = new SolidBrush(bgColor))
                using (var path = CreateRoundedRectangle(rect, 6))
                {
                    e.Graphics.FillPath(brush, path);

                    Color borderColor = monitor.DeviceType == DeviceType.Local ? Color.DarkGray : Color.DarkGreen;
                    using (Pen borderPen = new Pen(borderColor, 1.5f))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }

                int monitorIndex = monitors.IndexOf(monitor);
                string prefix = monitor.DeviceType == DeviceType.Local ? "Local-" : "External-";
                string monitorIdentifier = prefix + (monitorIndex + 1);

                if (showMonitorDetails)
                {
                    string text = $"{monitorIdentifier}\n{monitor.Screen.Bounds.Width}x{monitor.Screen.Bounds.Height}\n[{monitor.DeviceType}]";
                    using (Brush textBrush = new SolidBrush(Color.Black))
                    {
                        e.Graphics.DrawString(text, this.Font, textBrush, rect.X + 6, rect.Y + 6);
                    }
                }
                else
                {
                    using (Font font = new Font("Arial", 16, FontStyle.Bold))
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;

                        using (Brush textBrush = new SolidBrush(Color.Black))
                        {
                            e.Graphics.DrawString(monitorIdentifier, font, textBrush, rect, sf);
                        }
                    }
                }
            }
        }

        private System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
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

                var candidatePositions = new[]
                {
                    new Point(other.Rect.Left - dragging.Rect.Width, other.Rect.Y),
                    new Point(other.Rect.Right, other.Rect.Y),
                    new Point(other.Rect.X, other.Rect.Bottom),
                    new Point(other.Rect.X, other.Rect.Top - dragging.Rect.Height)
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
                        Rectangle adjustedRect = candidateRect;

                        if (candidate.X < dragging.Rect.X)
                            adjustedRect.X = other.Rect.Right;
                        else if (candidate.X > dragging.Rect.X)
                            adjustedRect.X = other.Rect.Left - dragging.Rect.Width;

                        if (candidate.Y < dragging.Rect.Y)
                            adjustedRect.Y = other.Rect.Bottom;
                        else if (candidate.Y > dragging.Rect.Y)
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

                this.Cursor = Cursors.Default;

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
                    Cursor.Current = Cursors.SizeAll;
                    this.Cursor = Cursors.SizeAll;
                    break;
                }
            }
        }

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

        private void StartIdentify()
        {
            StopIdentify();

            for (int i = 0; i < monitors.Count; i++)
            {
                var monitor = monitors[i];
                int monitorNumber = i + 1;

                Form overlay = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    Bounds = monitor.Screen.Bounds,
                    TopMost = true,
                    ShowInTaskbar = false,
                    BackColor = Color.LimeGreen,
                    TransparencyKey = Color.LimeGreen
                };

                overlay.Paint += (sender, e) =>
                {
                    string text = monitorNumber.ToString();
                    Font font = new Font("Arial", 72, FontStyle.Bold);
                    SizeF textSize = e.Graphics.MeasureString(text, font);

                    float diameter = Math.Max(textSize.Width, textSize.Height) + 20;
                    float circleX = (overlay.ClientSize.Width - diameter) / 2;
                    float circleY = (overlay.ClientSize.Height - diameter) / 2;

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(Brushes.Black, circleX, circleY, diameter, diameter);

                    e.Graphics.DrawString(
                        text,
                        font,
                        Brushes.White,
                        (overlay.ClientSize.Width - textSize.Width) / 2,
                        (overlay.ClientSize.Height - textSize.Height) / 2);
                };

                int exStyle = GetWindowLong(overlay.Handle, GWL_EXSTYLE);
                SetWindowLong(overlay.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);

                overlay.Show();
                identifyOverlays.Add(overlay);
            }
        }

        private void StopIdentify()
        {
            foreach (var overlay in identifyOverlays)
            {
                overlay.Close();
            }
            identifyOverlays.Clear();
        }

        private void SaveMonitorLayout()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Save Monitor Layout"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var layoutData = new
                {
                    layout = new List<object>()
                };

                foreach (var m in monitors)
                {
                    string prefix = m.DeviceType == DeviceType.Local ? "local-" : "external-";
                    int index = monitors.FindIndex(x => x == m) + 1;

                    layoutData.layout.Add(new
                    {
                        id = prefix + index,
                        deviceType = m.DeviceType.ToString(),
                        bounds = new
                        {
                            x = m.Screen.Bounds.X,
                            y = m.Screen.Bounds.Y,
                            width = m.Screen.Bounds.Width,
                            height = m.Screen.Bounds.Height
                        }
                    });
                }

                string json = JsonSerializer.Serialize(layoutData, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(sfd.FileName, json);
                MessageBox.Show("Layout saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
