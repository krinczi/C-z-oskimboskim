using System;

namespace POMIDOR.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ReportFieldAttribute : Attribute
    {
        public string Label { get; }
        public int Order { get; }
        public ReportFieldAttribute(string label, int order = 0)
        {
            Label = label; Order = order;
        }
    }
}
