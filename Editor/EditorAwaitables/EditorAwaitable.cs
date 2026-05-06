using UnityEditor;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#else
using static UnityEditor.AssemblyReloadEvents;
#endif

namespace EditorAwaitables {
#if UNITY_6000_5_OR_NEWER
    public static partial class EditorAwaitables {
#else
    [InitializeOnLoad]
    public static class EditorAwaitable {
#endif
        static readonly CancellationTokenSource _exitAndDomainReloadCancellationTokenSource = new();

#if UNITY_6000_5_OR_NEWER
        [OnCodeInitializing]
        static void OnCodeInitializing() =>
            EditorApplication.quitting += _exitAndDomainReloadCancellationTokenSource.Cancel;

        [OnCodeUnloading]
        static void OnCodeUnloading() => _exitAndDomainReloadCancellationTokenSource.Cancel();
#else
        static EditorAwaitable() {
            EditorApplication.quitting += _exitAndDomainReloadCancellationTokenSource.Cancel;
            beforeAssemblyReload += _exitAndDomainReloadCancellationTokenSource.Cancel;
        }
#endif

        // Reuse completion sources to minimize allocations.
        static readonly ObjectPool<AwaitableCompletionSource> _pool = new(
            createFunc: () => new AwaitableCompletionSource(),
            actionOnRelease: source => source.Reset());

        /// Cancellation token that is automatically canceled before
        /// the domain reload or when the Editor is closed.
        /// Use this for canceling any awaited operations.<br/><br/>
        /// Very important to use this token with threads.<br/><br/>
        /// For non-parallel awaitables, this is only needed
        /// when you design your code for explicit cancellation.
        /// Otherwise, they are stopped automatically because
        /// they use `EditorApplication.update` internally.
        // ReSharper disable once InconsistentNaming
        public static CancellationToken exitAndDomainReloadCancellationToken
            => _exitAndDomainReloadCancellationTokenSource.Token;

        /// Resumes execution in the next Editor frame.
        public static async Awaitable NextFrameAsync(
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var completionSource = _pool.Get();
            EditorApplication.CallbackFunction onUpdate = () => completionSource.TrySetResult();

            try {
                // ReSharper disable once UseAwaitUsing
                using var register = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// Waits for the specified number of seconds.
        public static async Awaitable WaitForSecondsAsync(
            float seconds,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var completionSource = _pool.Get();
            var startTime = EditorApplication.timeSinceStartup;

            EditorApplication.CallbackFunction onUpdate = () => {
                if (EditorApplication.timeSinceStartup - startTime < seconds)
                    return;

                completionSource.TrySetResult();
            };

            try {
                // ReSharper disable once UseAwaitUsing
                using var _ = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// Waits until the specified condition is true.
        public static async Awaitable WaitUntilAsync(
            Func<bool> condition,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            if (condition == null)
                return;

            var completionSource = _pool.Get();

            EditorApplication.CallbackFunction onUpdate = () => {
                if (!condition())
                    return;

                completionSource.TrySetResult();
            };

            try {
                // ReSharper disable once UseAwaitUsing
                using var _ = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// Waits until the specified condition is true.
        public static async Awaitable WaitUntilAsync<TData>(
            TData data,
            Func<TData, bool> condition,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            if (condition == null)
                return;

            var completionSource = _pool.Get();

            EditorApplication.CallbackFunction onUpdate = () => {
                if (!condition(data))
                    return;

                completionSource.TrySetResult();
            };

            try {
                // ReSharper disable once UseAwaitUsing
                using var _ = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// Waits while the specified condition is true.
        public static async Awaitable WaitWhileAsync(
            Func<bool> condition,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            if (condition == null)
                return;

            var completionSource = _pool.Get();

            EditorApplication.CallbackFunction onUpdate = () => {
                if (condition())
                    return;

                completionSource.TrySetResult();
            };

            try {
                // ReSharper disable once UseAwaitUsing
                using var _ = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// Waits while the specified condition is true.
        public static async Awaitable WaitWhileAsync<TData>(
            TData data,
            Func<TData, bool> condition,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            if (condition == null)
                return;

            var completionSource = _pool.Get();

            EditorApplication.CallbackFunction onUpdate = () => {
                if (condition(data))
                    return;

                completionSource.TrySetResult();
            };

            try {
                // ReSharper disable once UseAwaitUsing
                using var _ = cancellationToken.Register(
                    callback: static state => ((AwaitableCompletionSource)state).TrySetCanceled(),
                    state: completionSource);

                EditorApplication.update += onUpdate;
                await completionSource.Awaitable;
            }
            finally {
                EditorApplication.update -= onUpdate;
                _pool.Release(completionSource);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}