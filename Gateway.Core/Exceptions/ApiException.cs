using System;

namespace Gateway.Core.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string? Technical { get; }
        public string ErrorId { get; }

        /// <summary>
        /// создает новый экземпляр исключения API
        /// </summary>
        /// <param name="message">сообщение об ошибке</param>
        /// <param name="statusCode">HTTP-код статуса (по умолчанию 500)</param>
        /// <param name="technical">техническая информация</param>
        public ApiException(string message, int statusCode = 500, string? technical = null)
            : base(message)
        {
            StatusCode = statusCode;
            Technical = technical;
            ErrorId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// создает новый экземпляр исключения API с вложенным исключением
        /// </summary>
        /// <param name="message">сообщение об ошибке</param>
        /// <param name="innerException">вложенное исключение</param>
        /// <param name="statusCode">HTTP-код статуса (по умолчанию 500)</param>
        /// <param name="technical">техническая информация</param>
        public ApiException(string message, Exception innerException, int statusCode = 500, string? technical = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Technical = technical;
            ErrorId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// создает стандартное исключение "Not Found" (404)
        /// </summary>
        /// <param name="entityName">название сущности</param>
        /// <param name="id">идентификатор</param>
        /// <returns>экземпляр ApiException</returns>
        public static ApiException NotFound(string entityName, object id)
        {
            return new ApiException(
                $"{entityName} с идентификатором '{id}' не найден",
                404,
                $"Entity '{entityName}' with id '{id}' was not found"
            );
        }

        /// <summary>
        /// создает стандартное исключение "Bad Request" (400)
        /// </summary>
        /// <param name="message">сообщение об ошибке</param>
        /// <param name="technical">техническая информация</param>
        /// <returns>экземпляр ApiException</returns>
        public static ApiException BadRequest(string message, string? technical = null)
        {
            return new ApiException(
                message,
                400,
                technical
            );
        }

        /// <summary>
        /// создает стандартное исключение "Unauthorized" (401)
        /// </summary>
        /// <param name="message">сообщение об ошибке</param>
        /// <returns>экземпляр ApiException</returns>
        public static ApiException Unauthorized(string message = "Требуется авторизация")
        {
            return new ApiException(
                message,
                401,
                "User is not authenticated"
            );
        }

        /// <summary>
        /// создает стандартное исключение "Forbidden" (403)
        /// </summary>
        /// <param name="message">сообщение об ошибке</param>
        /// <returns>экземпляр ApiException</returns>
        public static ApiException Forbidden(string message = "Недостаточно прав для выполнения операции")
        {
            return new ApiException(
                message,
                403,
                "User does not have permission to perform this operation"
            );
        }

        /// <summary>
        /// создает стандартное исключение "Service Unavailable" (503)
        /// </summary>
        /// <param name="serviceName">название сервиса</param>
        /// <param name="innerException">вложенное исключение</param>
        /// <returns>экземпляр ApiException</returns>
        public static ApiException ServiceUnavailable(string serviceName, Exception innerException)
        {
            return new ApiException(
                $"Сервис '{serviceName}' недоступен, попробуйте позже",
                innerException,
                503,
                $"Service '{serviceName}' is unavailable: {innerException.Message}"
            );
        }
    }
}
