using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsStatusMonitor.Helpers
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Periodically repeats the observable sequence exposing a responses or failures.
        /// </summary>
        /// <typeparam name="T">The type of the sequence response values.</typeparam>
        /// <param name="source">The source observable sequence to re-subscribe to after each <paramref name="period"/>.</param>
        /// <param name="period">The period of time to wait before subscribing to the <paramref name="source"/> sequence. Subsequent subscriptions will occur this period after the previous sequence completes.</param>
        /// <param name="scheduler">The <see cref="IScheduler"/> to use to schedule the polling.</param>
        /// <returns>Returns an infinite observable sequence of values or errors.</returns>
        public static IObservable<Try<T>> Poll<T>(this IObservable<T> source, TimeSpan period)
        {
            return Observable.Timer(TimeSpan.Zero, period)
                .SelectMany(_ => source) //Flatten the response sequence.
                .Select(Try<T>.Create) //Project successful values to the Try<T> return type.
                .Catch<Try<T>, Exception>(ex => Observable.Return(Try<T>.Fail(ex))) //Project exceptions to the Try<T> return type
                .Repeat(); //Loop
        }
    }
}
