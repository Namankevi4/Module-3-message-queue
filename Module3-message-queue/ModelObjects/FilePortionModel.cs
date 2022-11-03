using System;
using System.Collections.Generic;
using System.Text;

namespace ModelObjects
{
    public class FilePortionModel
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public int SequenceNumber { get; set; }
        public long FileSize { get; set; }
        public byte[] Body { get; set; }
    }
}
