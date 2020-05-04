namespace HelloSignSampleApi.Custom
{
    public class Response<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
