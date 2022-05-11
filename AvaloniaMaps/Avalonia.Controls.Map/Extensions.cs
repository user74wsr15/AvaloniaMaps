using System;
using Avalonia;

namespace AvaloniaMaps.Avalonia.Controls.Map;

public static class Extensions
{
    /// <summary>
    /// Subscribes an element handler to an observable sequence.
    /// </summary>
    /// <param name="observable"></param>
    /// <param name="action">Action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public static IDisposable Subscribe(this IObservable<AvaloniaPropertyChangedEventArgs> observable, Action action)
    {
        return observable.Subscribe(e => { action(); });
    }
    
    /// <summary>
    /// Remaps value from one range to another.
    /// </summary>
    /// <param name="value">Value to remap.</param>
    /// <param name="from1">Min value of first range.</param>
    /// <param name="to1">Max value of first range.</param>
    /// <param name="from2">Min value of second range.</param>
    /// <param name="to2">Max value of second range.</param>
    /// <returns></returns>
    public static double Remap(this double value, double from1, double to1, double from2, double to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static double Delta(this double value, double other)
    {
        return Math.Abs(value - other);
    }

    public static double Wrap(this double value, double by)
    {
        value %= by;
        if (value < 0)
            value += by;

        return value;
    }
}