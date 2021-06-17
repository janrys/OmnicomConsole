namespace AngularCrudApi.Application.DTOs
{
    public class PackageContentWithCommands : PackageContent
    {
        public PackageContentWithCommands()
        {

        }

        public PackageContentWithCommands(PackageContent packageContent)
        {
            this.PackageNumber = packageContent.PackageNumber;
            this.ExportDate = packageContent.ExportDate;
            this.Author = packageContent.Author;
            this.Requests = packageContent.Requests;
            this.RequestChanges = packageContent.RequestChanges;
            this.Files = packageContent.Files;
        }

        public string Commands { get; set; }
    }
}
