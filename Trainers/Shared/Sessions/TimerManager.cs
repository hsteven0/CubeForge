using CubeForge.Controls;
using CubeForge.Views;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CubeForge.Trainers.Shared
{
    public class TimerManager
    {
        private const int ArmDelayMs = 500;

        private readonly SessionPanel panel;
        private readonly SessionManager sessions = new();
        private readonly DispatcherTimer timer = new();
        private readonly DispatcherTimer armTimer = new();

        private DateTime? startTime;
        private DateTime? spacebarDownTime;
        private bool spacebarIsDown;

        public bool IsRunning => startTime != null;
        public Func<bool>? BeforeStart { get; set; }
        public Action<bool>? AfterStop { get; set; }

        public TimerManager(SessionPanel panel)
        {
            this.panel = panel;
            timer.Interval = TimeSpan.FromMilliseconds(35);
            timer.Tick += OnTimerTick;
            armTimer.Interval = TimeSpan.FromMilliseconds(30);
            armTimer.Tick += OnArmTick;
        }

        public void Init()
        {
            panel.SelectedSessionChanged += OnSessionChanged;
            panel.CreateSessionRequested += OnCreateSession;
            panel.ConfirmRenameSessionRequested += OnRenameSession;
            panel.ConfirmDeleteSessionRequested += OnDeleteSession;
            panel.SetSolveHistory(sessions.CurrentSession.Rows);
            UpdateStats();
        }

        public void HandleSpaceDown()
        {
            if (spacebarIsDown)
                return;

            spacebarIsDown = true;
            spacebarDownTime = DateTime.Now;

            if (!IsRunning)
            {
                panel.SetTimerColor(new SolidColorBrush(Color.FromRgb(239, 68, 68)));
                panel.SetTimerStatus("Hold...", new SolidColorBrush(Color.FromRgb(239, 68, 68)));
                armTimer.Start();
            }
        }

        public void HandleSpaceUp()
        {
            if (!spacebarIsDown || spacebarDownTime == null)
                return;

            TimeSpan held = DateTime.Now - spacebarDownTime.Value;
            spacebarIsDown = false;
            spacebarDownTime = null;
            armTimer.Stop();

            if (!IsRunning && held.TotalMilliseconds < ArmDelayMs)
            {
                panel.SetTimerColor(Brushes.White);
                panel.SetTimerStatus("Hold Space to arm timer", new SolidColorBrush(Color.FromRgb(156, 163, 175)));
                return;
            }

            Toggle();
        }

        public void Toggle()
        {
            if (IsRunning)
            {
                Stop(recordSolve: true);
                return;
            }

            if (BeforeStart != null && !BeforeStart())
            {
                panel.ResetTimerDisplay();
                return;
            }

            Start();
        }

        public bool Stop(bool recordSolve)
        {
            if (!IsRunning || startTime == null)
                return false;

            TimeSpan elapsed = DateTime.Now - startTime.Value;
            startTime = null;
            timer.Stop();

            panel.SetTimerColor(Brushes.White);
            panel.SetTimerStatus("Hold Space to arm timer", new SolidColorBrush(Color.FromRgb(156, 163, 175)));
            panel.SetTimerText(sessions.FormatTime(elapsed));

            if (recordSolve)
            {
                UpdateDelta(elapsed);
                sessions.AddSolve(elapsed);
                UpdateStats();
            }

            AfterStop?.Invoke(recordSolve);
            return true;
        }

        public void ClearStats()
        {
            Stop(recordSolve: false);
            sessions.ClearSession();
            panel.ResetTimerDisplay();
            UpdateStats();
        }

        private void Start()
        {
            startTime = DateTime.Now;
            panel.SetTimerText("0.00");
            panel.SetTimerColor(Brushes.White);
            panel.SetTimerDelta("");
            panel.SetTimerStatus("Timer running", new SolidColorBrush(Color.FromRgb(156, 163, 175)));
            timer.Start();
        }

        private void UpdateDelta(TimeSpan latest)
        {
            string deltaText = sessions.FormatDelta(latest, out bool faster);
            panel.SetTimerDelta(deltaText);

            if (string.IsNullOrWhiteSpace(deltaText))
                return;

            panel.SetTimerDelta(deltaText, faster ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(239, 68, 68)));
        }

        private void UpdateStats()
        {
            SessionStats stats = sessions.GetStats();
            panel.SetStats(stats.CurrentTime, stats.BestTime, stats.CurrentMo3, stats.BestMo3, stats.CurrentAo3, stats.BestAo3, stats.CurrentAo5, stats.BestAo5, stats.CurrentAo12, stats.BestAo12, stats.Summary);

            if (sessions.CurrentSession.Times.Count == 0)
                panel.SetTimerDelta("");
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!IsRunning || startTime == null)
                return;

            TimeSpan elapsed = DateTime.Now - startTime.Value;
            panel.SetTimerText(sessions.FormatTime(elapsed));
        }

        private void OnArmTick(object? sender, EventArgs e)
        {
            if (!spacebarIsDown || spacebarDownTime == null || IsRunning)
                return;

            TimeSpan held = DateTime.Now - spacebarDownTime.Value;

            if (held.TotalMilliseconds >= ArmDelayMs)
            {
                panel.SetTimerColor(new SolidColorBrush(Color.FromRgb(34, 197, 94)));
                panel.SetTimerStatus("Ready", new SolidColorBrush(Color.FromRgb(34, 197, 94)));
                armTimer.Stop();
            }
        }

        private void OnCreateSession(object? sender, EventArgs e)
        {
            Stop(recordSolve: false);
            string baseName = "Session";
            int number = 1;
            string name;

            do
            {
                name = $"{baseName} {number}";
                number++;
            }
            while (sessions.Sessions.ContainsKey(name));

            Session session = new(name);
            sessions.Sessions[name] = session;
            panel.AddSessionName(name);
            panel.SelectLastSession();
        }

        private void OnRenameSession(object? sender, EventArgs e)
        {
            string oldName = panel.SelectedSessionName;
            string newName = panel.RenameInputText;

            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                return;

            if (newName == oldName)
            {
                panel.CloseRenamePopup();
                return;
            }

            if (sessions.Sessions.ContainsKey(newName))
            {
                MessageBox.Show("A session with that name already exists.");
                return;
            }

            sessions.Sessions.Remove(oldName);
            sessions.CurrentSession.Name = newName;
            sessions.Sessions[newName] = sessions.CurrentSession;
            panel.RenameSelectedSession(newName);
            panel.CloseRenamePopup();
        }

        private void OnDeleteSession(object? sender, EventArgs e)
        {
            panel.CloseDeletePopup();

            if (panel.SessionCount <= 1)
                return;

            string name = panel.SelectedSessionName;

            if (string.IsNullOrWhiteSpace(name))
                return;

            sessions.Sessions.Remove(name);
            panel.RemoveSelectedSession();
            panel.SelectFirstSession();
        }

        private void OnSessionChanged(object? sender, EventArgs e)
        {
            string name = panel.SelectedSessionName;

            if (string.IsNullOrWhiteSpace(name))
                return;

            if (!sessions.Sessions.TryGetValue(name, out Session? session))
                return;

            Stop(recordSolve: false);
            sessions.CurrentSession = session;
            panel.SetSolveHistory(session.Rows);
            panel.ResetTimerDisplay();
            UpdateStats();
        }
    }
}
