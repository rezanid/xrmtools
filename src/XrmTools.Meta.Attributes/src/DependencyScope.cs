namespace XrmTools
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class DependencyScope<TTag> : IDisposable
    {
        private static readonly AsyncLocal<DependencyScope<TTag>> s_current = new AsyncLocal<DependencyScope<TTag>>();
        public static DependencyScope<TTag> Current { get { return s_current.Value; } }

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Dictionary<string, object>> _named = new Dictionary<Type, Dictionary<string, object>>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly DependencyScope<TTag> _parent;

        public DependencyScope()
        {
            _parent = s_current.Value;
            s_current.Value = this;
        }

        public T Set<T>(T instance) { _services[typeof(T)] = instance; return instance; }

        public T SetAndTrack<T>(T instance) where T : IDisposable
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Set(instance);
            _disposables.Add(instance);
            return instance;
        }

        public T Set<T>(string name, T instance)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var dict = GetNameMap(typeof(T));
            dict[name] = instance;
            return instance;
        }

        public T SetAndTrack<T>(string name, T instance) where T : IDisposable
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Set(name, instance);
            _disposables.Add(instance);
            return instance;
        }

        public bool TryGet<T>(out T value)
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGet<T>(string name, out T value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_named.TryGetValue(typeof(T), out var dict) && dict.TryGetValue(name, out var obj))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public T Require<T>() => _services.TryGetValue(typeof(T), out var obj)
            ? (T)obj
            : throw new InvalidOperationException("Service not available in scope: " + typeof(T).FullName);

        public T Require<T>(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_named.TryGetValue(typeof(T), out var dict) && dict.TryGetValue(name, out var obj))
                return (T)obj;
            throw new InvalidOperationException("Named service not available in scope: " + typeof(T).FullName + " (" + name + ")");
        }

        public void Dispose()
        {
            // restore parent (to support nesting)
            s_current.Value = _parent;
            for (int i = _disposables.Count - 1; i >= 0; i--) { try { _disposables[i].Dispose(); } catch { } }
            _disposables.Clear(); _services.Clear(); _named.Clear();
        }

        private Dictionary<string, object> GetNameMap(Type t)
        {
            if (!_named.TryGetValue(t, out var dict))
            {
                dict = new Dictionary<string, object>(StringComparer.Ordinal);
                _named[t] = dict;
            }
            return dict;
        }
    }
}
