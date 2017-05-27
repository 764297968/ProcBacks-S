namespace SqliteHelper
{
    using System;
    using System.Runtime.CompilerServices;

    public class Parameter
    {
        public Parameter(string parameterName, string parameterValue)
        {
            this.parameterName = parameterName;
            this.parameterValue = parameterValue;
        }

        public Parameter(string parameterName, string parameterValue, SelectMode selectMode) : this(parameterName, parameterValue)
        {
            this.selectMode = selectMode;
        }

        public string parameterName { get; set; }

        public string parameterValue { get; set; }

        public SelectMode selectMode { get; set; }
    }
}

