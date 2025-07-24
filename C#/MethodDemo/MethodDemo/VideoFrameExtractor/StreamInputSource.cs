using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    public class StreamInputSource : InputSource
    {
        internal override string InputCommand { get; }

        public StreamInputSource(Uri streamUri)
        {
            this.InputCommand = $"-i {streamUri}";
        }

        public StreamInputSource(string streamUri)
        {
            this.InputCommand = $"-i {streamUri}";
        }
    }
}
