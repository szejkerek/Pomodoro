namespace Pomodoro.Presentation
{
    /// <summary>
    /// Maps a heatmap cell's pomodoro count to an alpha, ramping from <c>minAlpha</c> at zero to fully
    /// opaque at the busiest cell (<c>peak</c>). Pure: the cell colour comes from the source, the heat
    /// only sets opacity.
    /// </summary>
    public static class HeatScale
    {
        public static byte Alpha(int filled, int peak, byte minAlpha)
        {
            if (peak <= 0)
            {
                return minAlpha;
            }

            double intensity = (double)filled / peak;
            return (byte)(minAlpha + intensity * (0xFF - minAlpha));
        }
    }
}
