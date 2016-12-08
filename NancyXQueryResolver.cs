using System;
using System.IO;
using Figaro;

namespace Nancy.Demos.Figaro
{
    /// <summary>
    /// The XQuery subsystem is extensible through language extension and by resolution. In this (latter) case, we are 
    /// telling the language engine where it can find our local module(s). See the documentation link below for details.
    /// </summary>
    /// <seealso cref="http://help.bdbxml.net/html/e1571f63-0de0-4119-8dd3-68dc8693f732.htm"/>
    public class NancyXQueryResolver: XQueryResolver
    {
        private string path;
        
        public NancyXQueryResolver(Uri uri, string rootPath) : base(uri)
        {
            path = rootPath;
        }

        public override XmlDocument ResolveDocument(XmlTransaction txn, XmlManager mgr, string uri)
        {
            return null;
        }

        public override bool ResolveCollection(XmlTransaction txn, XmlManager mgr, string uri, XmlResults collection)
        {
            return false;
        }

        public override XmlInputStream ResolveSchema(XmlTransaction txn, XmlManager mgr, string schemaLocation, string nameSpace)
        {
            return null;
        }

        public override XmlInputStream ResolveEntity(XmlTransaction txn, XmlManager mgr, string systemId, string publicId)
        {
            return null;
        }

        public override bool ResolveModuleLocation(XmlTransaction txn, XmlManager mgr, string nameSpace, XmlResults moduleLocations)
        {
            Console.WriteLine($"ResolveModuleLocation namespace:{nameSpace},moduleLocation: {moduleLocations.Count}");
            return true;
        }

        public override XmlInputStream ResolveModule(XmlTransaction txn, XmlManager mgr, string moduleLocation, string nameSpace)
        {
            if (moduleLocation.ToLower().Equals("nancy.xqm"))return mgr.CreateLocalFileInputStream(Path.Combine(path,"Home", moduleLocation));
            return null;
        }

        public override XmlExternalFunction ResolveExternalFunction(XmlTransaction txn, XmlManager mgr, string uri, string name, ulong numArgs)
        {
            if (uri != Uri.ToString()) return null;
            return new DecodeFunction();
        }

        protected override void Dispose(bool A_0)
        {
            base.Dispose(A_0);
        }
    }
}
