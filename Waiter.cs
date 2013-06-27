using System;

namespace ZenioxBot
{
    using System.Threading;

    /// <summary>
    /// A class for dealing with waiting for conditions with a deadline.
    /// </summary>
    internal class Waiter
    {
        private DateTime startTime;

        /// <summary>
        /// A waiter with a deadline.
        /// </summary>
        /// <param name="maxSeconds">The maximum number of seconds to wait, the deadline.</param>
        /// <param name="description">Used for messages when the deadline is met.</param>
        /// <param name="milliSecondsBetween">Sleep time between testing if the condition has been met.</param>
        public Waiter(double maxSeconds, string description, int milliSecondsBetween = 10)
        {
            this.MaxSeconds = maxSeconds;
            this.MilliSecondsBetween = milliSecondsBetween;
            this.Description = description;
        }

        /// <summary>
        /// A boolean condition
        /// </summary>
        /// <returns>True if the condition has been met.</returns>
        public delegate bool Condition();

        /// <summary>
        /// Get the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Get the maximum number of seconds to wait
        /// </summary>
        public double MaxSeconds { get; private set; }

        /// <summary>
        /// Get the number of milliseconds between test of the condition
        /// </summary>
        public int MilliSecondsBetween { get; private set; }

        /// <summary>
        /// Wait for <see cref="condition"/> for a maximum period of <see cref="maxSeconds"/> seconds.
        /// </summary>
        /// <param name="condition">The condition to wait for.</param>
        /// <exception cref="TimeoutException">Waited the maximum number of seconds.</exception>
        public void WaitFor(Condition condition)
        {
            this.startTime = DateTime.Now;
            while (!condition())
            {
                if (this.TimeOut())
                {
                    if (string.IsNullOrEmpty(this.Description))
                    {
                        throw new TimeoutException("Waited too long.");
                    }

                    throw new TimeoutException(string.Format("Waited too long for \"{0}\".", this.Description));
                }

                Thread.Sleep(this.MilliSecondsBetween);
            }
        }

        private bool TimeOut()
        {
            return DateTime.Now.Subtract(this.startTime).TotalSeconds > this.MaxSeconds;
        }
    }
}
