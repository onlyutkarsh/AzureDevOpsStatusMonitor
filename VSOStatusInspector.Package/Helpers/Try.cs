using System;
using System.Collections.Generic;
using System.Globalization;

namespace VSTSStatusMonitor.Helpers
{
    public abstract class Try<T>
    {
        private Try()
        {
        }

        public static Try<T> Create(T value)
        {
            return new Success(value);
        }

        public static Try<T> Fail(Exception value)
        {
            return new Error(value);
        }

        public abstract TResult Switch<TResult>(Func<T, TResult> caseValue, Func<Exception, TResult> caseError);
        public abstract void Switch(Action<T> caseValue, Action<Exception> caseError);

        private sealed class Success : Try<T>, IEquatable<Success>
        {
            private readonly T _value;

            public Success(T value)
            {
                _value = value;
            }

            public override TResult Switch<TResult>(Func<T, TResult> caseValue, Func<Exception, TResult> caseError)
            {
                return caseValue(_value);
            }

            public override void Switch(Action<T> caseValue, Action<Exception> caseError)
            {
                caseValue(_value);
            }

            public bool Equals(Success other)
            {
                if (ReferenceEquals(other, this))
                    return true;
                if (other == null)
                    return false;
                return EqualityComparer<T>.Default.Equals(_value, other._value);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Success);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<T>.Default.GetHashCode(_value);
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.CurrentCulture, "Success({0})", _value);
            }
        }

        private sealed class Error : Try<T>, IEquatable<Error>
        {
            private readonly Exception _exception;

            public Error(Exception exception)
            {
                if (exception == null)
                    throw new ArgumentNullException(nameof(exception));
                _exception = exception;
            }

            public override TResult Switch<TResult>(Func<T, TResult> caseValue, Func<Exception, TResult> caseError)
            {
                return caseError(_exception);
            }

            public override void Switch(Action<T> caseValue, Action<Exception> caseError)
            {
                caseError(_exception);
            }

            public bool Equals(Error other)
            {
                if (ReferenceEquals(other, this))
                    return true;
                if (other == null)
                    return false;
                return Equals(_exception, other._exception);
            }
            private static bool Equals(Exception a, Exception b)
            {
                if (a == null && b == null)
                    return true;
                if (a == null || b == null)
                    return false;
                if (a.GetType() != b.GetType())
                    return false;
                if (!string.Equals(a.Message, b.Message))
                    return false;
                return Equals(a.InnerException, b.InnerException);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Error);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<Exception>.Default.GetHashCode(_exception);
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.CurrentCulture, "Error({0})", _exception);
            }
        }
    }
}