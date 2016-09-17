using System.ComponentModel.DataAnnotations;

namespace InjectRequiredAttribute.ExampleTarget
{
    public class TargetModel
    {
        public string PropertyToGetAnAttribute { get; set; }

        public string PropertyThatHasNoTranslation { get; set; }

        [Required]
        public string PropertyToLeaveAsIs { get; set; }

        public class TargetDto
        {
            public string PropertyToGetAnAttribute { get; set; }
        }
    }
}