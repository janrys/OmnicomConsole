namespace AngularCrudApi.WebApi.Models
{
    public class ApplicationMetadata
    {
        public string Environment { get; set; }
        public string Mode { get; set; }
        public bool IsExportAllowed { get; set; }
        public bool IsImportAllowed { get; set; }
    }
}