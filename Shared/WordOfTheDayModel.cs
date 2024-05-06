using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class WordOfTheDayModel
    {
        public string Word { get; set; } 
        public string Translation { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
