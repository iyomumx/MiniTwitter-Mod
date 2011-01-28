using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace MiniTwitter.Extensions
{
    static class DispatcherExtension
    {
        public static void Invoke(this DispatcherObject obj, Action action)
        {
            if (obj == null)
            {
                return;
            }
            if (obj.CheckAccess())
            {
                action();
            }
            else
            {
                obj.Dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
        }

        public static void Invoke<T>(this DispatcherObject obj, Action<T> action, T arg)
        {
            if (obj == null)
            {
                return;
            }
            if (obj.CheckAccess())
            {
                action(arg);
            }
            else
            {
                obj.Dispatcher.Invoke(DispatcherPriority.Normal, action, arg);
            }
        }

        public static TResult Invoke<TResult>(this DispatcherObject obj, Func<TResult> action)
        {
            if (obj == null)
            {
                return default(TResult);
            }
            if (obj.CheckAccess())
            {
                return action();
            }
            else
            {
                return (TResult)obj.Dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
        }

        public static void AsyncInvoke(this DispatcherObject obj, Action action)
        {
            if (obj == null)
            {
                return;
            }
            obj.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        public static void AsyncInvoke<T>(this DispatcherObject obj, Action<T> action, T arg)
        {
            if (obj == null)
            {
                return;
            }
            obj.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action, arg);
        }

        public static void AsyncInvoke<T>(this DispatcherObject obj, Action<T> action, T arg, DispatcherPriority priority)
        {
            if (obj == null)
            {
                return;
            }
            obj.Dispatcher.BeginInvoke(priority, action, arg);
        }
    }
}
