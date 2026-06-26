using System;
using System.Collections.Generic;
using Pomodoro.Models;

namespace Pomodoro.Services
{
    /// <summary>
    /// Owns the focus-radio flow: which station is selected, the volume, and whether it's playing.
    /// Drives an <see cref="IRadioPlayer"/> and persists the station/volume through
    /// <see cref="SettingsService"/> (the one place settings are saved). The window binds to its
    /// state and re-renders on <see cref="Changed"/>. Playback follows the focus session: it starts
    /// when focus begins (opt-in via <see cref="AppSettings.FocusRadioEnabled"/>) and stops when it
    /// ends — the only manual control during focus is mute. The station catalog is whatever the user
    /// configured in <see cref="AppSettings.RadioStationList"/>; an empty catalog means no playback.
    /// </summary>
    public sealed class RadioModel
    {
        private const double MinVolume = 0.0;
        private const double MaxVolume = 1.0;

        private readonly IRadioPlayer player;
        private readonly SettingsService settings;

        private int stationIndex;
        private bool isStreamLoaded;
        private bool isFocusActive;

        public RadioModel(IRadioPlayer player, SettingsService settings)
        {
            this.player = player;
            this.settings = settings;

            stationIndex = ClampStationIndex(settings.Current.RadioStationIndex);
            Volume = Clamp(settings.Current.RadioVolume, MinVolume, MaxVolume);
            player.Volume = Volume;
        }

        /// <summary>Fired when station, volume, mute, or playback state changes, so the window re-renders.</summary>
        public event Action? Changed;

        private IReadOnlyList<RadioStation> Stations => settings.Current.RadioStationList();

        public bool HasStations => Stations.Count > 0;

        public RadioStation? CurrentStation => HasStations ? Stations[ClampStationIndex(stationIndex)] : null;

        public string? CurrentCategory => CurrentStation?.Category;

        /// <summary>The distinct, non-empty categories across the configured stations, in list order.</summary>
        public IReadOnlyList<string> Categories
        {
            get
            {
                List<string> categories = new List<string>();
                foreach (RadioStation station in Stations)
                {
                    if (station.Category.Length > 0 && categories.Contains(station.Category) == false)
                    {
                        categories.Add(station.Category);
                    }
                }

                return categories;
            }
        }

        public bool IsPlaying { get; private set; }

        public bool IsMuted { get; private set; }

        public double Volume { get; private set; }

        /// <summary>True when the panel should show: focus is active and the radio is enabled in settings.</summary>
        public bool IsActive => isFocusActive && settings.Current.FocusRadioEnabled;

        /// <summary>Focus started or stopped — start the radio on entry (unless muted), stop it on exit.</summary>
        public void FollowFocus(bool focusActive)
        {
            if (isFocusActive == focusActive)
            {
                return;
            }

            isFocusActive = focusActive;
            ApplyPlayback();
            Changed?.Invoke();
        }

        /// <summary>Silence the radio without leaving focus; toggling back resumes it.</summary>
        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            ApplyPlayback();
            Changed?.Invoke();
        }

        /// <summary>
        /// Switch to the given category: jump to its first station, or — if the current station is
        /// already in that category — advance to the next station within it (wrapping). Reloads the
        /// stream, persists the choice, and resumes only if already playing.
        /// </summary>
        public void SelectCategory(string category)
        {
            IReadOnlyList<RadioStation> stations = Stations;
            List<int> categoryIndexes = new List<int>();
            for (int index = 0; index < stations.Count; index++)
            {
                if (string.Equals(stations[index].Category, category, StringComparison.OrdinalIgnoreCase))
                {
                    categoryIndexes.Add(index);
                }
            }

            if (categoryIndexes.Count == 0)
            {
                return;
            }

            int positionInCategory = categoryIndexes.IndexOf(stationIndex);
            int target = positionInCategory >= 0
                ? categoryIndexes[(positionInCategory + 1) % categoryIndexes.Count]
                : categoryIndexes[0];

            if (target == stationIndex)
            {
                return;
            }

            stationIndex = target;
            settings.Update(current => current.RadioStationIndex = stationIndex);

            LoadCurrentStation();

            if (IsPlaying)
            {
                player.Play();
            }

            Changed?.Invoke();
        }

        public void SetVolume(double value)
        {
            double clamped = Clamp(value, MinVolume, MaxVolume);
            Volume = clamped;
            player.Volume = clamped;
            settings.Update(current => current.RadioVolume = clamped);
            Changed?.Invoke();
        }

        private void ApplyPlayback()
        {
            bool shouldPlay = isFocusActive && settings.Current.FocusRadioEnabled && IsMuted == false && HasStations;
            if (shouldPlay == IsPlaying)
            {
                return;
            }

            if (shouldPlay)
            {
                EnsureStreamLoaded();
                player.Play();
                IsPlaying = true;
                return;
            }

            player.Pause();
            IsPlaying = false;
        }

        private void EnsureStreamLoaded()
        {
            if (isStreamLoaded)
            {
                return;
            }

            LoadCurrentStation();
        }

        private void LoadCurrentStation()
        {
            RadioStation? station = CurrentStation;
            if (station == null)
            {
                return;
            }

            player.Load(station.StreamUri);
            isStreamLoaded = true;
        }

        private int ClampStationIndex(int index)
        {
            int count = Stations.Count;
            if (count == 0 || index < 0 || index >= count)
            {
                return 0;
            }

            return index;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
