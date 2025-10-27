using System;

namespace POMIDOR.Models
{
    public class PomodoroSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? TaskId { get; set; }   // jak przypinasz sesję do zadania, to ustawiaj
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public int DurationMinutes => (int)Math.Max(0, (EndUtc - StartUtc).TotalMinutes);
        public DateTime DayLocal => EndUtc.ToLocalTime().Date;
    }
}
