namespace AngularCrudApi.Domain.Entities
{
    public class RequestChange
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public Release Request { get; set; }
        public int SequenceNumber { get; set; }
        public string CodebookName { get; set; }
        public string ChangeType { get; set; }
        public string RecordId { get; set; }
        public string Change { get; set; }
        public string Command { get; set; }

    }
}
