using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoFrameExtractor
{
    public abstract class InputSource
    {
        internal abstract string InputCommand { get; }
    }
}
