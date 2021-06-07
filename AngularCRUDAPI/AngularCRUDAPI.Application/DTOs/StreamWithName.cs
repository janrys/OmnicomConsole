using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.DTOs
{
    public class StreamWithName
    {
        public StreamWithName(Stream stream, string name)
        {
            this.Stream = stream;
            this.Name = name;
        }

        public Stream Stream { get; }

        public string Name { get; }
    }
}
