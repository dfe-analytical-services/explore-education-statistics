using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static System.Char;
using static System.String;
using static System.Text.RegularExpressions.Regex;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreatePublicationViewModel
    {
        [Required]
        public string Title { get; set; }

        private string _slug;
        
        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle() : _slug;
            set => _slug = value; 
        }

        [Required]
        public Guid TopicId { get; set; }

        public Guid? MethodologyId { get; set; }

        [Required]
        public Guid ContactId { get; set; }

        private string SlugFromTitle()
        {
            var removeNonAlphaNumeric = new string(Title.Where(c => IsLetter(c) || IsWhiteSpace(c)).ToArray()).Trim();
            var toLower = new string(removeNonAlphaNumeric.Select(ToLower).ToArray());
            var removeMultipleSpaces = Replace(toLower, @"\s+", " ");
            var replaceSpaces = new string(removeMultipleSpaces.Select(c => IsWhiteSpace(c) ? '-' : c).ToArray());
            return replaceSpaces;
        }
    }
}