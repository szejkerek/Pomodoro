using System;
using System.Collections.Generic;
using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class RadioModelTests
    {
        /// <summary>Test <see cref="IRadioPlayer"/>: records what it was told to do, plays nothing real.</summary>
        private sealed class RecordingRadioPlayer : IRadioPlayer
        {
            public List<Uri> Loaded { get; } = new List<Uri>();
            public int PlayCount { get; private set; }
            public int PauseCount { get; private set; }
            public double LastVolume { get; private set; } = -1;

            public void Load(Uri stream)
            {
                Loaded.Add(stream);
            }

            public void Play()
            {
                PlayCount++;
            }

            public void Pause()
            {
                PauseCount++;
            }

            public double Volume
            {
                set => LastVolume = value;
            }
        }

        private static (RadioModel model, RecordingRadioPlayer player, SettingsService settings) Build(AppSettings seed)
        {
            RecordingRadioPlayer player = new RecordingRadioPlayer();
            SettingsService settings = new SettingsService(new InMemorySettingsStore(seed));
            RadioModel model = new RadioModel(player, settings);
            return (model, player, settings);
        }

        private static AppSettings Enabled()
        {
            return new AppSettings { FocusRadioEnabled = true };
        }

        private static IReadOnlyList<RadioStation> DefaultStations => new AppSettings().RadioStationList();

        [Fact]
        public void Entering_focus_loads_current_station_then_plays_when_enabled()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(Enabled());

            model.FollowFocus(focusActive: true);

            Assert.True(model.IsPlaying);
            Assert.True(model.IsActive);
            Assert.Single(player.Loaded);
            Assert.Equal(DefaultStations[0].StreamUri, player.Loaded[0]);
            Assert.Equal(1, player.PlayCount);
        }

        [Fact]
        public void Entering_focus_does_nothing_when_radio_disabled()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(new AppSettings());

            model.FollowFocus(focusActive: true);

            Assert.False(model.IsPlaying);
            Assert.False(model.IsActive);
            Assert.Empty(player.Loaded);
            Assert.Equal(0, player.PlayCount);
        }

        [Fact]
        public void Leaving_focus_pauses()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(Enabled());

            model.FollowFocus(focusActive: true);
            model.FollowFocus(focusActive: false);

            Assert.False(model.IsPlaying);
            Assert.False(model.IsActive);
            Assert.Equal(1, player.PauseCount);
        }

        [Fact]
        public void Re_entering_focus_reuses_the_loaded_stream_and_does_not_reload()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(Enabled());

            model.FollowFocus(focusActive: true);
            model.FollowFocus(focusActive: false);
            model.FollowFocus(focusActive: true);

            Assert.Single(player.Loaded);
            Assert.Equal(2, player.PlayCount);
        }

        [Fact]
        public void Mute_during_focus_pauses_then_unmute_resumes()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(Enabled());

            model.FollowFocus(focusActive: true);
            model.ToggleMute();

            Assert.True(model.IsMuted);
            Assert.False(model.IsPlaying);
            Assert.Equal(1, player.PauseCount);

            model.ToggleMute();

            Assert.False(model.IsMuted);
            Assert.True(model.IsPlaying);
            Assert.Equal(2, player.PlayCount);
        }

        [Fact]
        public void SelectCategory_switches_to_first_station_of_that_category_and_persists()
        {
            (RadioModel model, _, SettingsService settings) = Build(Enabled());

            model.SelectCategory("Synthwave");

            Assert.Equal("Synthwave", model.CurrentStation!.Category);
            Assert.Equal(DefaultStations[3].Name, model.CurrentStation!.Name);
            Assert.Equal(3, settings.Current.RadioStationIndex);
        }

        [Fact]
        public void Selecting_the_current_category_cycles_within_it_with_wraparound()
        {
            (RadioModel model, _, _) = Build(Enabled());

            model.SelectCategory("Lo-fi");
            Assert.Equal(DefaultStations[1].Name, model.CurrentStation!.Name);

            model.SelectCategory("Lo-fi");
            Assert.Equal(DefaultStations[2].Name, model.CurrentStation!.Name);

            model.SelectCategory("Lo-fi");
            Assert.Equal(DefaultStations[0].Name, model.CurrentStation!.Name);
        }

        [Fact]
        public void SelectCategory_reloads_and_resumes_only_when_already_playing()
        {
            (RadioModel model, RecordingRadioPlayer player, _) = Build(Enabled());

            model.FollowFocus(focusActive: true);
            int playsBefore = player.PlayCount;
            model.SelectCategory("Classical");

            Assert.Equal(DefaultStations[7].StreamUri, player.Loaded[^1]);
            Assert.Equal(playsBefore + 1, player.PlayCount);
        }

        [Fact]
        public void SelectCategory_ignores_an_unknown_category()
        {
            (RadioModel model, _, SettingsService settings) = Build(Enabled());

            model.SelectCategory("Polka");

            Assert.Equal(0, settings.Current.RadioStationIndex);
            Assert.Equal(DefaultStations[0].Name, model.CurrentStation!.Name);
        }

        [Fact]
        public void Categories_lists_each_distinct_category_once_in_order()
        {
            (RadioModel model, _, _) = Build(Enabled());

            Assert.Equal(
                new[] { "Lo-fi", "Synthwave", "Classical", "Pink noise", "Brown noise" },
                model.Categories);
        }

        [Theory]
        [InlineData(-0.5, 0.0)]
        [InlineData(1.5, 1.0)]
        [InlineData(0.3, 0.3)]
        public void SetVolume_clamps_and_persists(double input, double expected)
        {
            (RadioModel model, RecordingRadioPlayer player, SettingsService settings) = Build(new AppSettings());

            model.SetVolume(input);

            Assert.Equal(expected, model.Volume);
            Assert.Equal(expected, player.LastVolume);
            Assert.Equal(expected, settings.Current.RadioVolume);
        }

        [Fact]
        public void Constructor_restores_persisted_station_and_volume()
        {
            AppSettings seed = new AppSettings { RadioStationIndex = 2, RadioVolume = 0.8 };
            (RadioModel model, RecordingRadioPlayer player, _) = Build(seed);

            Assert.Equal(DefaultStations[2].Name, model.CurrentStation!.Name);
            Assert.Equal(0.8, model.Volume);
            Assert.Equal(0.8, player.LastVolume);
        }

        [Fact]
        public void Constructor_falls_back_to_first_station_for_out_of_range_index()
        {
            AppSettings seed = new AppSettings { RadioStationIndex = 99 };
            (RadioModel model, _, _) = Build(seed);

            Assert.Equal(DefaultStations[0].Name, model.CurrentStation!.Name);
        }

        [Fact]
        public void RadioStationList_parses_category_name_and_url()
        {
            AppSettings settings = new AppSettings
            {
                RadioStations = "Lo-fi | Chillhop | https://example.com/lofi"
            };

            RadioStation station = settings.RadioStationList()[0];

            Assert.Equal("Lo-fi", station.Category);
            Assert.Equal("Chillhop", station.Name);
            Assert.Equal("https://example.com/lofi", station.StreamUri.ToString());
        }

        [Fact]
        public void RadioStationList_accepts_legacy_lines_without_a_category()
        {
            AppSettings settings = new AppSettings
            {
                RadioStations = "Chillhop | https://example.com/lofi"
            };

            RadioStation station = settings.RadioStationList()[0];

            Assert.Equal(string.Empty, station.Category);
            Assert.Equal("Chillhop", station.Name);
        }

        [Fact]
        public void RadioStationList_skips_malformed_lines()
        {
            AppSettings settings = new AppSettings
            {
                RadioStations = "no separator here\nLo-fi | Bad | not-a-url\nClassical | WCPE | https://example.com/wcpe"
            };

            IReadOnlyList<RadioStation> stations = settings.RadioStationList();

            Assert.Single(stations);
            Assert.Equal("WCPE", stations[0].Name);
        }
    }
}
