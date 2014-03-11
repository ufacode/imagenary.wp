namespace Imagenary.Api
{
    public class PhotoUploadResponse
    {
        public RequestStatus Status { get; set; }
        public PhotoContainer Photo { get; set; }

        public class RequestStatus
        {
            public int Code { get; set; }
            public string Msg { get; set; }
        }

        public class Image
        {
            public string Thumb { get; set; }
            public string Full { get; set; }
        }

        public class PhotoContainer
        {
            public string Id { get; set; }
            public Image Image { get; set; }
        }
    }
}