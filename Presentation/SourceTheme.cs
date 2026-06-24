using System.Windows.Media;
using Pomodoro.Models;

namespace Pomodoro.Presentation
{
    public readonly struct SourcePalette
    {
        public SourcePalette(Color color, string label)
        {
            Color = color;
            Label = label;
        }

        public Color Color { get; }

        /// <summary>Legend caption, e.g. "Todoist · life".</summary>
        public string Label { get; }
    }

    /// <summary>Single source of truth for each task source's colour and context label (life/hobby/work).</summary>
    public static class SourceTheme
    {
        private static readonly SourcePalette Todoist = new SourcePalette(Color.FromRgb(0x4C, 0xAF, 0x50), "Todoist · life");
        private static readonly SourcePalette ClickUp = new SourcePalette(Color.FromRgb(0x7E, 0x57, 0xC2), "ClickUp · hobby");
        private static readonly SourcePalette Asana = new SourcePalette(Color.FromRgb(0xFB, 0x8C, 0x00), "Asana · work");

        public static SourcePalette For(TaskSource source)
        {
            if (source == TaskSource.ClickUp)
            {
                return ClickUp;
            }

            if (source == TaskSource.Asana)
            {
                return Asana;
            }

            return Todoist;
        }
    }
}
