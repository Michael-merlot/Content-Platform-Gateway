namespace Gateway.Core.Models;

/// <summary>
/// Represents the outcome of an operation that either succeeds without returning a value or fails with an error.
/// </summary>
/// <typeparam name="TError">
/// The type that describes the error when the operation fails. Typically an enum or a dedicated error record.
/// </typeparam>
public readonly record struct Result<TError>
{
    /// <summary><c>true</c> when the operation succeeded; otherwise, <c>false</c>.</summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The error returned when the operation fails; <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public TError? Error { get; }

    private Result(bool isSuccess, TError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Creates a successful <see cref="Result{TError}"/>.</summary>
    /// <returns>A result whose <see cref="IsSuccess"/> is <c>true</c>.</returns>
    public static Result<TError> Success() =>
        new(true, default);

    /// <summary>Creates a failed <see cref="Result{TError}"/>.</summary>
    /// <param name="error">The error that describes the failure.</param>
    /// <returns>A result whose <see cref="IsSuccess"/> is <c>false</c>.</returns>
    public static Result<TError> Failure(TError error) =>
        new(false, error);

    /// <summary>Executes one of the provided delegates according to the result state.</summary>
    /// <param name="onSuccess">Invoked when <see cref="IsSuccess"/> is <c>true</c>.</param>
    /// <param name="onError">Invoked when <see cref="IsSuccess"/> is <c>false</c>.</param>
    public void Match(Action onSuccess, Action<TError> onError)
    {
        if (IsSuccess)
            onSuccess();
        else
            onError(Error!);
    }

    /// <summary>Executes one of the provided delegates according to the result state and returns its value.</summary>
    /// <param name="onSuccess">Function invoked when <see cref="IsSuccess"/> is <c>true</c>.</param>
    /// <param name="onError">Function invoked when <see cref="IsSuccess"/> is <c>false</c>.</param>
    /// <typeparam name="TResult">The type of the value returned by either delegate.</typeparam>
    /// <returns>The value produced by the executed delegate.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<TError, TResult> onError) =>
        IsSuccess ? onSuccess() : onError(Error!);

    /// <summary>Asynchronously executes one of the provided delegates according to the result state.</summary>
    /// <param name="onSuccess">An asynchronous delegate invoked when <see cref="IsSuccess"/> is <c>true</c>.</param>
    /// <param name="onError">An asynchronous delegate invoked when <see cref="IsSuccess"/> is <c>false</c>.</param>
    /// <returns>A task that completes when the selected delegate completes.</returns>
    public Task MatchAsync(Func<Task> onSuccess, Func<TError, Task> onError) =>
        IsSuccess
            ? onSuccess()
            : onError(Error!);

    /// <summary>
    /// Asynchronously executes one of the provided delegates according to the result state and returns its value.
    /// </summary>
    /// <param name="onSuccess">Asynchronous function invoked when <see cref="IsSuccess"/> is <c>true</c>.</param>
    /// <param name="onError">Asynchronous function invoked when <see cref="IsSuccess"/> is <c>false</c>.</param>
    /// <typeparam name="TResult">The type of the value returned by either delegate.</typeparam>
    /// <returns>A task that represents the asynchronous operation and yields the delegate’s return value.</returns>
    public Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> onSuccess, Func<TError, Task<TResult>> onError) =>
        IsSuccess
            ? onSuccess()
            : onError(Error!);

    /// <summary>
    /// Converts an <typeparamref name="TError"/> value to a failed <see cref="Result{TError}"/> using an implicit cast.
    /// </summary>
    /// <param name="error">The error to wrap in the result.</param>
    public static implicit operator Result<TError>(TError error) =>
        Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that either succeeds and returns a value of type <typeparamref name="T"/> or fails with an error of
/// type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned when the operation succeeds.</typeparam>
/// <typeparam name="TError">The type that describes the error when the operation fails.</typeparam>
public readonly record struct Result<T, TError>
{
    /// <summary><c>true</c> when the operation succeeded; otherwise, <c>false</c>.</summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The value returned when the operation succeeds; <c>null</c> when <see cref="IsSuccess"/> is <c>false</c>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// The error returned when the operation fails; <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public TError? Error { get; }

    private Result(bool isSuccess, T? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Creates a successful <see cref="Result{T, TError}"/>.</summary>
    /// <param name="value">The value produced by the successful operation.</param>
    /// <returns>A result whose <see cref="IsSuccess"/> is <c>true</c>.</returns>
    public static Result<T, TError> Success(T value) =>
        new(true, value, default);

    /// <summary>Creates a failed <see cref="Result{T, TError}"/>.</summary>
    /// <param name="error">The error that describes the failure.</param>
    /// <returns>A result whose <see cref="IsSuccess"/> is <c>false</c>.</returns>
    public static Result<T, TError> Failure(TError error) =>
        new(false, default, error);

    /// <summary>Executes one of the provided delegates according to the result state.</summary>
    /// <param name="onSuccess">Invoked with the <typeparamref name="T"/> value when <see cref="IsSuccess"/> is <c>true</c>.</param>
    /// <param name="onError">Invoked with the <typeparamref name="TError"/> value when <see cref="IsSuccess"/> is <c>false</c>.</param>
    public void Match(Action<T> onSuccess, Action<TError> onError)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onError(Error!);
    }

    /// <summary>Executes one of the provided delegates according to the result state and returns its value.</summary>
    /// <param name="onSuccess">
    /// Function invoked with the <typeparamref name="T"/> value when <see cref="IsSuccess"/> is <c>true</c>.
    /// </param>
    /// <param name="onError">
    /// Function invoked with the <typeparamref name="TError"/> value when <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <typeparam name="TResult">The type of the value returned by either delegate.</typeparam>
    /// <returns>The value produced by the executed delegate.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onError) =>
        IsSuccess ? onSuccess(Value!) : onError(Error!);

    /// <summary>Asynchronously executes one of the provided delegates according to the result state.</summary>
    /// <param name="onSuccess">
    /// An asynchronous delegate invoked with the <typeparamref name="T"/> value when <see cref="IsSuccess"/> is <c>true</c>.
    /// </param>
    /// <param name="onError">
    /// An asynchronous delegate invoked with the <typeparamref name="TError"/> value when <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <returns>A task that completes when the selected delegate completes.</returns>
    public Task MatchAsync(Func<T, Task> onSuccess, Func<TError, Task> onError) =>
        IsSuccess
            ? onSuccess(Value!)
            : onError(Error!);

    /// <summary>
    /// Asynchronously executes one of the provided delegates according to the result state and returns its value.
    /// </summary>
    /// <param name="onSuccess">
    /// Asynchronous function invoked with the <typeparamref name="T"/> value when <see cref="IsSuccess"/> is <c>true</c>.
    /// </param>
    /// <param name="onError">
    /// Asynchronous function invoked with the <typeparamref name="TError"/> value when <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <typeparam name="TResult">The type of the value returned by either delegate.</typeparam>
    /// <returns>A task that represents the asynchronous operation and yields the delegate’s return value.</returns>
    public Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<TError, Task<TResult>> onError) =>
        IsSuccess
            ? onSuccess(Value!)
            : onError(Error!);

    /// <summary>
    /// Converts a value of type <typeparamref name="T"/> to a successful <see cref="Result{T, TError}"/> using an implicit cast.
    /// </summary>
    /// <param name="value">The value to wrap in the result.</param>
    public static implicit operator Result<T, TError>(T value) =>
        Success(value);

    /// <summary>
    /// Converts an <typeparamref name="TError"/> value to a failed <see cref="Result{T, TError}"/> using an implicit cast.
    /// </summary>
    /// <param name="error">The error to wrap in the result.</param>
    public static implicit operator Result<T, TError>(TError error) =>
        Failure(error);
}
