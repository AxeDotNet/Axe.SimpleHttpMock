using System;
using System.Linq;

namespace Axe.SimpleHttpMock.ServerImpl.Handlers.UriTemplates
{
    class UriTemplateElement
    {
        public UriTemplateElement(string segmentValue)
        {
            string variableName = GetVariableName(segmentValue);
            IsVariable = variableName != null;
            Value = IsVariable ? variableName : segmentValue;
        }

        static string GetVariableName(string segmentValue)
        {
            if (segmentValue.Length == 0) { return null; }
            bool mayBeVariable = segmentValue[0] == '{';
            if (!mayBeVariable) { return null; }
            if (segmentValue.Length < 3)
            {
                throw new FormatException($"Invalid variable format: {segmentValue}");
            }

            if (segmentValue[segmentValue.Length - 1] != '}')
            {
                throw new FormatException($"Missing closing bracket: {segmentValue}");
            }

            string variableName = segmentValue.Substring(1, segmentValue.Length - 2);
            if (variableName.Any(c => c == '{' || c == '}'))
            {
                throw new FormatException($"Variable name cannot contains reserved characters: {segmentValue}");
            }

            return variableName;
        }

        public string Value { get; }

        public bool IsVariable { get; }

        public bool IsMatch(string pathSegment)
        {
            return IsVariable || Value.Equals(pathSegment, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}