using System.Text.Json.Serialization;

namespace IdentityService.Models.Response
{
    public class ResultResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Errors { get; set; }

        public ResultResponse()
        { }

        public ResultResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static ResultResponse Ok(string message = "Success")
            => new(true, message);

        public static ResultResponse Fail(string message)
            => new(false, message);

        public static ResultResponse Fail(string message, object? errors = null)
        => new(false, message)
        {
            Errors = errors
        };
    }

    public class ResultResponse<T> : ResultResponse
    {
        public T? Data { get; set; }

        public ResultResponse()
        { }

        public ResultResponse(bool success, string message)
            : base(success, message)
        {
        }

        public ResultResponse(bool success, string message, T? data)
            : base(success, message)
        {
            Data = data;
        }

        public static ResultResponse<T> Ok(T data, string message = "Success")
            => new(true, message, data);

        public new static ResultResponse<T> Fail(string message)
            => new(false, message);
    }
}