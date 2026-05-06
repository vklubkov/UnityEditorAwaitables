# Editor Awaitables for Unity

Use async/await in Unity Editor instead of Editor Coroutines.

```csharp
await EditorAwaitable.NextFrameAsync();
await EditorAwaitable.WaitForSecondsAsync(seconds);
await EditorAwaitable.WaitUntilAsync(() => condition);
await EditorAwaitable.WaitWhileAsync(() => condition);
```

Reasoning: `Awaitable.NextFrameAsync()` and `Awaitable.WaitForSecondsAsync()` will never complete when called from Editor code outside of Play Mode, as they are designed for the game loop.



## Installation

1. Open the Unity Package Manager.
2. Click the `+` button and choose `Install package from git URL...`
3. Enter the following URL: `https://github.com/vklubkov/UnityEditorAwaitables.git`.

   Or, if you want to use SSH: `git@github.com:vklubkov/UnityEditorAwaitables.git`.



## API

### NextFrameAsync

Delays the execution until the next `EditorApplication.update`.

```csharp
await EditorAwaitable.NextFrameAsync();
await EditorAwaitable.NextFrameAsync(cancellationToken);
```

### WaitForSecondsAsync

Waits for the specified number of seconds.

```csharp
await EditorAwaitable.WaitForSecondsAsync(seconds);
await EditorAwaitable.WaitForSecondsAsync(seconds, cancellationToken);
```
### WaitUntilAsync

Waits for the specified condition to become `true`. Despite the presence of the `data` parameter, there is an internal closure for the `EditorApplication.update` subscription that will occur regardless of the `data` parameter presence.


```csharp
await EditorAwaitable.WaitUntilAsync(() => condition);
await EditorAwaitable.WaitUntilAsync(() => condition, cancellationToken);
await EditorAwaitable.WaitUntilAsync(data, static data => data.Condition);
await EditorAwaitable.WaitUntilAsync(data, static data => data.Condition, cancellationToken);
```

### WaitWhileAsync

Waits for the specified condition to become `false`. Despite the presence of the `data` parameter, there is an internal closure for the `EditorApplication.update` subscription that will occur regardless of the `data` parameter presence.


```csharp
await EditorAwaitable.WaitWhileAsync(() => condition);
await EditorAwaitable.WaitWhileAsync(() => condition, cancellationToken);
await EditorAwaitable.WaitWhileAsync(data, static data => data.Condition);
await EditorAwaitable.WaitWhileAsync(data, static data => data.Condition, cancellationToken);
```

### Cancellation Token

Editor Awaitables provide the cancellation token for Editor exit and domain reloads: 

```csharp
var cancellationToken = EditorAwaitables.exitAndDomainReloadCancellationToken;
```
It is very important to use this token with threads. 

However, Editor Awaitables are stopped automatically because they use `EditorApplication.update` internally. You still may use this token (or any custom one) with these awaitables, if you want explicit cancellation. Note that the token throws `OperationCanceledException` when canceled. You need to handle this exception.



## Threads

You can use `Awaitable.MainThreadAsync()` and `Awaitable.BackgroundThreadAsync()`, they both work in Editor. 

Don't forget to use the cancellation token for background threads, especially for the long-running ones, for example:

```csharp
static async void ThreadCancellation() {
   try {
       var cancellationToken = EditorAwaitable.exitAndDomainReloadCancellationToken;
       await Awaitable.BackgroundThreadAsync();
       
       while (!cancellationToken.IsCancellationRequested)
           Thread.Yield();

       cancellationToken.ThrowIfCancellationRequested();
   } catch (OperationCanceledException) {
       return; // Here, the Editor may already be closed, or the domain may be reloaded.
   } catch (Exception e) {
       await Awaitable.MainThreadAsync();
       Debug.LogException(e);
       return;
   }

   await Awaitable.MainThreadAsync();
   Debug.Log("Thread Operation Completed Successfully");
}
```


## Exception Handling

For async/await, proper exception handling is always required, and Editor Awaitables aren't unique in this regard. It is a good idea to handle all exceptions in a non-awaitable method that is using awaitables.

```csharp
async void MethodWithAwaitables() {
    try {
        await EditorAwaitable.WaitForSecondsAsync(5, EditorAwaitable.exitAndDomainReloadCancellationToken);
    } catch (OperationCanceledException) {
        // Keep empty in case cancellation is not an error.
    }
    catch (Exception e) {
        Debug.LogException(e); // Print exception to the console.
    }
}
```

Without the try-cacth block, the exceptions will either be hidden or printed with a delay, depending on where you handle them.



## Allocations

This is not an allocation-free implementation. `EditorApplication.update` subscriptions always capture some data. Also, while `AwaitableCompletionSource`'s are reused via an object pool, they still have to be allocated at least once each.

While this isn't as important for editor code as it is for a game, it's still something to consider when developing a performance-heavy Editor extension.



## Additional Information

[Unity Awaitable manual](https://docs.unity3d.com/6000.3/Documentation/Manual/async-await-support.html)

[In this gamedev.stackexchange.com answer](https://gamedev.stackexchange.com/a/212425/64120) I explained how to deal with cancellation and error handling of Unity Awaitables.



## AI use disclosure

- After I implemented the `NextFrameAsync` method, I asked AI to generate the `WaitForSecondsAsync`, `WaitUntilAsync` and `WaitWhileAsync` methods. Then I reimplented them manually, to properly handle all the edge cases. Some implementation details were discussed with AI.

- AI autocompletion in the IDE was used.



## License

[MIT](LICENSE.md)

```
MIT License

Copyright (c) 2026 Vladimir Klubkov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```