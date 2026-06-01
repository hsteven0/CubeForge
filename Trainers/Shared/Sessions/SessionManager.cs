using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CubeForge.Views
{
    public class SessionManager
    {
        public Dictionary<string, Session> Sessions { get; } = new();
        public Session CurrentSession { get; set; } = new Session("Default Session");

        public SessionManager()
        {
            Sessions[CurrentSession.Name] = CurrentSession;
        }

        public SessionStats GetStats()
        {
            List<TimeSpan> times = CurrentSession.Times;

            if (times.Count == 0)
                return SessionStats.Empty();

            TimeSpan mean = TimeSpan.FromMilliseconds(times.Average(t => t.TotalMilliseconds));

            return new SessionStats
            {
                CurrentTime = FormatTime(times[^1]),
                BestTime = FormatTime(times.Min()),
                CurrentMo3 = RecentMean(3),
                BestMo3 = FormatBestMean(3),
                CurrentAo3 = FormatAverageOfRecent(3),
                BestAo3 = FormatBestAverage(3),
                CurrentAo5 = FormatAverageOfRecent(5),
                BestAo5 = FormatBestAverage(5),
                CurrentAo12 = FormatAverageOfRecent(12),
                BestAo12 = FormatBestAverage(12),
                Summary = $"Solve: {times.Count}/{times.Count}\nMean: {FormatTime(mean)}"
            };
        }

        public SolveHistoryRow AddSolve(TimeSpan elapsed)
        {
            CurrentSession.Times.Add(elapsed);

            var row = new SolveHistoryRow
            {
                Number = CurrentSession.Times.Count,
                Time = FormatTime(elapsed),
                Ao5 = FormatAverageOfRecent(5),
                Ao12 = FormatAverageOfRecent(12)
            };

            CurrentSession.Rows.Insert(0, row);
            return row;
        }

        public void ClearSession()
        {
            CurrentSession.Times.Clear();
            CurrentSession.Rows.Clear();
        }

        public string FormatDelta(TimeSpan latest, out bool faster)
        {
            faster = false;

            if (CurrentSession.Times.Count == 0)
                return "";

            TimeSpan previous = CurrentSession.Times[^1];
            double deltaSeconds = latest.TotalSeconds - previous.TotalSeconds;

            faster = deltaSeconds < 0;

            return faster ? $"({deltaSeconds:0.00})" : $"(+{deltaSeconds:0.00})";
        }

        private string RecentMean(int count)
        {
            if (CurrentSession.Times.Count < count)
                return "--";

            double averageMs = CurrentSession.Times.Skip(CurrentSession.Times.Count - count).Average(t => t.TotalMilliseconds);
            return FormatTime(TimeSpan.FromMilliseconds(averageMs));
        }

        private string FormatBestMean(int count)
        {
            if (CurrentSession.Times.Count < count)
                return "--";

            double best = double.MaxValue;

            for (int i = 0; i <= CurrentSession.Times.Count - count; i++)
            {
                double average = CurrentSession.Times.Skip(i).Take(count).Average(t => t.TotalMilliseconds);
                best = Math.Min(best, average);
            }

            return FormatTime(TimeSpan.FromMilliseconds(best));
        }

        private string FormatAverageOfRecent(int count)
        {
            if (CurrentSession.Times.Count < count)
                return "--";

            List<TimeSpan> recent = CurrentSession.Times.Skip(CurrentSession.Times.Count - count).Take(count).OrderBy(t => t).ToList();

            if (count >= 5)
            {
                recent.RemoveAt(recent.Count - 1);
                recent.RemoveAt(0);
            }

            double averageMs = recent.Average(t => t.TotalMilliseconds);
            return FormatTime(TimeSpan.FromMilliseconds(averageMs));
        }

        private string FormatBestAverage(int count)
        {
            if (CurrentSession.Times.Count < count)
                return "--";

            double best = double.MaxValue;

            for (int i = 0; i <= CurrentSession.Times.Count - count; i++)
            {
                List<TimeSpan> window = CurrentSession.Times.Skip(i).Take(count).OrderBy(t => t).ToList();

                if (count >= 5)
                {
                    window.RemoveAt(window.Count - 1);
                    window.RemoveAt(0);
                }

                double average = window.Average(t => t.TotalMilliseconds);
                best = Math.Min(best, average);
            }

            return FormatTime(TimeSpan.FromMilliseconds(best));
        }

        public string FormatTime(TimeSpan time)
        {
            return time.TotalSeconds.ToString("0.00");
        }
    }

    public class SessionStats
    {
        public string CurrentTime { get; set; } = "--";
        public string BestTime { get; set; } = "--";
        public string CurrentMo3 { get; set; } = "--";
        public string BestMo3 { get; set; } = "--";
        public string CurrentAo3 { get; set; } = "--";
        public string BestAo3 { get; set; } = "--";
        public string CurrentAo5 { get; set; } = "--";
        public string BestAo5 { get; set; } = "--";
        public string CurrentAo12 { get; set; } = "--";
        public string BestAo12 { get; set; } = "--";
        public string Summary { get; set; } = "Solve: 0/0\nMean: --";

        public static SessionStats Empty()
        {
            return new SessionStats();
        }
    }

    public class Session
    {
        public string Name { get; set; }
        public List<TimeSpan> Times { get; } = new();
        public ObservableCollection<SolveHistoryRow> Rows { get; } = new();

        public Session(string name)
        {
            Name = name;
        }
    }

    public class SolveHistoryRow
    {
        public int Number { get; set; }
        public string Time { get; set; } = "";
        public string Ao5 { get; set; } = "";
        public string Ao12 { get; set; } = "";
    }
}
