using HelloSign;
using System.Collections.Generic;

namespace HelloSignSampleApi.Custom
{
    public class RequestSign
    {
        public List<CustomField> CustomFields { get; set; }
        public string TemplateId { get; set; }

    }
}
