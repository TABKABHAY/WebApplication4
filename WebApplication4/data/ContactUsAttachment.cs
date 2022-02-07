using System;
using System.Collections.Generic;

#nullable disable

namespace WebApplication4
{
    public partial class ContactUsAttachment
    {
        public int ContactUsAttachmentId { get; set; }
        public string Name { get; set; }
        public byte[] FileName { get; set; }
    }
}
