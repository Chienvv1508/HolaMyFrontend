namespace HolaMyFrontend.Models
{
    public class ResponseDTO<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ResponseDTO(int statusCode, string message, T data)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }

        public static ResponseDTO<T> Fail(string message)
        {
            return new ResponseDTO<T>(500, message, default);
        }
        public static ResponseDTO<T> Success(T data)
        {
            return new ResponseDTO<T>(200, "Success", data);
        }
    }
}
