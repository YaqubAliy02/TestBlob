namespace Testblob
{
    public class BlobDto
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public Stream Content { get; set; }
    }
}
