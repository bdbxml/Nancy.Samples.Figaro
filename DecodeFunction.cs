using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Figaro;
using Nancy.Helpers;

namespace Nancy.Demos.Figaro
{
    public class DecodeFunction: XmlExternalFunction
    {
        /// <summary>
        /// Take our 'About Me' section and decode the HTML so it displays properly.
        /// </summary>
        /// <param name="txn"></param>
        /// <param name="mgr"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override XmlResults Execute(XmlTransaction txn, XmlManager mgr, XmlArguments args)
        {
            var ret = mgr.CreateXmlResults();
            var about = args.GetArguments(0);
            ret.Add(new XmlValue(HttpUtility.HtmlDecode(about.NextValue().AsString)));
            return ret;
        }
    }
}
