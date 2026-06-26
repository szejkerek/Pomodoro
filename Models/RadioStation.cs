using System;

namespace Pomodoro.Models
{
    /// <summary>One internet-radio preset: a display name and the stream it points at.</summary>
    public sealed class RadioStation
    {
        public RadioStation(string name, Uri streamUri, string category)
        {
            Name = name;
            StreamUri = streamUri;
            Category = category;
        }

        public string Name { get; }

        public Uri StreamUri { get; }

        /// <summary>The kind of sound (e.g. "Lo-fi", "Synthwave", "Brown noise"); shown in the player.</summary>
        public string Category { get; }
    }
}
