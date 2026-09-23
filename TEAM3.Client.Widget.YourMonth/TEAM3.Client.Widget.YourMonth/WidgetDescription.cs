using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alkami.Client.Framework.Mvc;

namespace TEAM3.Client.Widget.YourMonth
{
    public class WidgetDescription : Alkami.Client.Framework.Mvc.WidgetDescription
    {
        private const string _name = "TEAM3YourMonth";
        
        public override string Name
        {
            get { return _name; }
        }

        public override string Title
        {
            get { return _name; }
        }
    }
}
