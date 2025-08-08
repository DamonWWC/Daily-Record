using Flyleaf.Common;
using System;

namespace FlyleafLib1.Controls
{
    public class PluginControl: IPlugin
    {
        public PluginControl()
        {

        }


        public object CreateControl()
        {
            return new LiveControl();
        }
          
    }
}
