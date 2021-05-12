namespace AngularCrudApi.Domain.Entities
{
    public class ReleaseChange
    {
        public int Id { get; set; }
        public int ReleaseId { get; set; }
        public Release Release { get; set; }
        public int SequenceNumber { get; set; }
        public string CodebookName { get; set; }
        public string ChangeType { get; set; }
        public string RecordId { get; set; }
        public string Command { get; set; }

    }
}
