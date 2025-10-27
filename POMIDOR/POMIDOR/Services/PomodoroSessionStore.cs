using System;
using System.Collections.Generic;
using System.Linq;
using POMIDOR.Models;
using POMIDOR.Services.Storage;

namespace POMIDOR.Services
{
    public sealed class PomodoroSessionEventArgs : EventArgs
    {
        public PomodoroSession Session { get; }
        public PomodoroSessionEventArgs(PomodoroSession s) => Session = s;
    }

    public static class PomodoroSessionStore
    {
        private static readonly IRepository<PomodoroSession> _repo =
            new JsonFileRepository<PomodoroSession>(AppPaths.PomodoroSessionsFile);

        public static event EventHandler<PomodoroSessionEventArgs>? SessionAppended;

        public static List<PomodoroSession> LoadAll() => _repo.LoadAll().ToList();

        public static void Append(PomodoroSession session)
        {
            _repo.Append(session);
            SessionAppended?.Invoke(null, new PomodoroSessionEventArgs(session));
        }
    }
}
