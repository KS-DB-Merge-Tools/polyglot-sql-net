using System;
using System.Collections.Generic;
using System.Text;

namespace PolyglotSql.Models
{
    public record VersionInfo
    {
        public string NativeRuntimeVersion { get; internal set; }
        public string NativeExpectedVersion => "0.5.1";
        public string WrapperVersion { get; internal set; }
    }
}