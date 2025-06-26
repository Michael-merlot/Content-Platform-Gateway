using Gateway.Core.Models;

namespace Gateway.Core.Mappers;

/// <summary>Extensions for <see cref="Result{TError}"/> and <see cref="Result{T, TError}"/>.</summary>
public static class ResultExtensions
{
    /// <summary>Maps the error from <typeparamref name="TError"/> to <typeparamref name="TMappedError"/>.</summary>
    /// <param name="result">The <see cref="Result{T, TError}"/> to map.</param>
    /// <param name="map">The function which will make a mapping.</param>
    /// <typeparam name="T">The type of the <see cref="Result{T, TError}"/> value.</typeparam>
    /// <typeparam name="TError">The original type of the error.</typeparam>
    /// <typeparam name="TMappedError">The mapped type of the error.</typeparam>
    /// <returns>The <see cref="Result{T, TError}"/> with error mapped to <typeparamref name="TMappedError"/>.</returns>
    public static Result<T, TMappedError>
        MapError<T, TError, TMappedError>(this Result<T, TError> result, Func<TError, TMappedError> map) =>
        result.Match<Result<T, TMappedError>>(value => value,
            error => map(error));

    /// <summary>Maps the error from <typeparamref name="TError"/> to <typeparamref name="TMappedError"/>.</summary>
    /// <param name="result">The <see cref="Result{TError}"/> to map.</param>
    /// <param name="map">The function which will make a mapping.</param>
    /// <typeparam name="TError">The original type of the error.</typeparam>
    /// <typeparam name="TMappedError">The mapped type of the error.</typeparam>
    /// <returns>The <see cref="Result{TError}"/> with error mapped to <typeparamref name="TMappedError"/>.</returns>
    public static Result<TMappedError> MapError<TError, TMappedError>(this Result<TError> result, Func<TError, TMappedError> map) =>
        result.Match(Result<TMappedError>.Success,
            error => map(error));
}
