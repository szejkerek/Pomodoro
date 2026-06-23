using System.IO;
using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class JsonSessionLogTests : IDisposable
    {
        private readonly string logFilePath;

        public JsonSessionLogTests()
        {
            logFilePath = Path.Combine(Path.GetTempPath(), "pomodoro-test-" + Guid.NewGuid().ToString("N") + ".jsonl");
        }

        public void Dispose()
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
        }

        [Fact]
        public void A_missing_file_reads_as_an_empty_log()
        {
            JsonSessionLog log = new JsonSessionLog(logFilePath);

            Assert.Empty(log.All());
        }

        [Fact]
        public void Recorded_entries_survive_a_reload_from_the_same_file()
        {
            DateTime first = new DateTime(2026, 6, 23, 9, 0, 0);
            DateTime second = new DateTime(2026, 6, 23, 10, 0, 0);

            JsonSessionLog writer = new JsonSessionLog(logFilePath);
            writer.Record(new CompletedPomodoro(first, 1500, "task-7"));
            writer.Record(new CompletedPomodoro(second, 1500, null));

            JsonSessionLog reader = new JsonSessionLog(logFilePath);

            Assert.Equal(2, reader.All().Count);
            Assert.Equal(first, reader.All()[0].CompletedAt);
            Assert.Equal("task-7", reader.All()[0].TaskId);
            Assert.Null(reader.All()[1].TaskId);
        }

        [Fact]
        public void A_corrupt_line_is_skipped_so_the_rest_of_the_history_survives()
        {
            JsonSessionLog writer = new JsonSessionLog(logFilePath);
            writer.Record(new CompletedPomodoro(new DateTime(2026, 6, 23, 9, 0, 0), 1500, null));
            File.AppendAllText(logFilePath, "{ this is not valid json" + Environment.NewLine);
            writer = new JsonSessionLog(logFilePath);
            writer.Record(new CompletedPomodoro(new DateTime(2026, 6, 23, 10, 0, 0), 1500, null));

            JsonSessionLog reader = new JsonSessionLog(logFilePath);

            Assert.Equal(2, reader.All().Count);
        }
    }
}
