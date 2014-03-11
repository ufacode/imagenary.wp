using System;
using System.Collections.Generic;

namespace Imagenary.Api
{
    public class Image
    {
        public string Thumb { get; set; }
        public string Full { get; set; }
        public string Box { get; set; }
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Photo
    {
        public int Id { get; set; }
        public DateTime created_at { get; set; }
        public object Latitude { get; set; }
        public object Longitude { get; set; }
        public List<object> Tags { get; set; }
        public string Comment { get; set; }
        public Image Image { get; set; }
        public Author Author { get; set; }
    }



    public class PhotoResponse
    {
        public Status Status { get; set; }
        public List<Photo> Photos { get; set; }
    }
}