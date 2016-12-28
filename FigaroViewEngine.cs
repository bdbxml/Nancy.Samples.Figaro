using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Figaro;
using Nancy.ViewEngines;

namespace Nancy.Demos.Figaro
{
    public class FigaroViewEngine : IViewEngine
    {
        private readonly FigaroDataContext context;
        public FigaroViewEngine(FigaroDataContext dataContext)
        {
            context = dataContext;
        }

        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
            Console.WriteLine("Figaro view engine initialized.");
        }

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            var w = new Stopwatch();
            w.Start();
            //use the snapshot transaction to ensure we don't interrupt any write operations.
            int i = 0;
            while (true)
            {
                using (var tx = context.Manager.CreateTransaction(TransactionType.SnapshotTransaction))
                {
                    try
                    {
                        //can't put a using on it so let's make sure it gets disposed
                        QueryContext qc;

                        //pull the query from the parameter so we can use below (and wrap/dispose our XmlResults appropriately)
                        string queryContents;
                        using (var r = viewLocationResult.Contents.Invoke())
                        {
                            queryContents = r.ReadToEnd();
                        }

                        //currently our model simply passes in a QueryContext if it's needed
                        if (model == null)
                        {
                            qc = context.Manager.CreateQueryContext(EvaluationType.Eager);
                        }
                        else
                            qc = model as QueryContext;

                        qc?.SetNamespace("db", FigaroDataContext.META_URI);
                        qc?.SetNamespace("nancy",context.resolver.Uri.ToString());
                        //http://xqilla.sourceforge.net/ExtensionFunctions
                        qc?.SetNamespace("xqilla", "http://xqilla.sourceforge.net/Functions");
                        var sbResp = new StringBuilder();

                        if (i == 0)
                        {
                            var xp = context.Manager.Prepare(tx, queryContents, qc);
                            Trace.WriteLine(xp.QueryPlan);
                            Trace.Flush();
                            xp.Dispose();
                        }
                        i++;
                        using (var results = context.Manager.Query(tx, queryContents, qc))
                        {
                            tx.Commit();

                            while (results.HasNext())
                            {
                                var d = results.NextDocument();
                                if (d != null)
                                    sbResp.Append(d.GetContentAsTextReader().ReadToEnd());
                                var v = results.NextValue();
                                if (v != null)
                                    sbResp.Append(v);
                            }
                        }
                        try
                        {
                            return new Response
                            {
                                Contents = stream =>
                                 {
                                     using (var sr = new StreamWriter(stream))
                                     {
                                         sr.Write(sbResp.ToString());
                                         sr.Flush();
                                     }
                                 }
                            };
                        }
                        finally
                        {
                            qc?.Dispose();
                        }
                    }
                    catch(XQueryException ex)
                    {
                        Console.WriteLine($"{ex.GetType()}: {ex.Message}, line {ex.QueryLine}, column {ex.QueryColumn}");
                        throw;
                    }
                    catch (XmlException ex)
                    {                        
                        if (!tx.Committed) tx.Abort();
                        Console.WriteLine($"{DateTime.Now} {ex.ExceptionCategory} in view engine: {ex.Message}");
                        Console.WriteLine($"{ex.StackTrace}");
                        if (i == 9) throw;
                        Thread.Sleep(100);
                    }
                    catch(Exception ex)
                    {
                        if (!tx.Committed) tx.Abort();
                        Console.WriteLine($"{ex.GetType()} in view engine: {ex.Message}");
                    }
                    finally
                    {
                        w.Stop();
                        Console.WriteLine($"view rendered in {w.ElapsedMilliseconds} ms ");
                    }
                }
            }
        }

        public IEnumerable<string> Extensions => new[] { "xqy", "xq"/*, "xqm"*/ };
    }
}
