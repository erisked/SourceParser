using System;
using System.Collections.Generic;
using System.Text;

namespace SourceParser
{
    class Sources
    {
        public String[] _includes;
        public String[] _targetLibs;
        public String[] _sources;
        public String _binPlace;

        public Sources[] nextSources;
        public Sources[] PrevSources;
    }
}
