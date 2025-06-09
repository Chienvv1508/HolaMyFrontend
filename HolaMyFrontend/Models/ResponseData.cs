namespace HolaMyFrontend.Models
{
    public class ResponseData<T> where T : class
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public T? data { get; set; }

        public ResponseData(int statusCode, string message, T data)
        {
            this.statusCode = statusCode;
            this.message = message;
            this.data = data;
        }
    }
}
