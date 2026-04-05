using System;

namespace TextEditor
{
    public class SyntaxError
    {
        public string Fragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public int TokenIndex { get; set; }
        public int CharPosition { get; set; }
        public int Line { get; set; }

        public SyntaxError(string fragment, string location, string description,
                          int tokenIndex, int charPosition, int line)
        {
            Fragment = fragment;
            Location = location;
            Description = description;
            TokenIndex = tokenIndex;
            CharPosition = charPosition;
            Line = line;
        }
    }
}