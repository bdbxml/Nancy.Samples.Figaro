using System;
using System.Diagnostics;
using Figaro;

namespace Nancy.Demos.Figaro
{
    public sealed class HomeModule : NancyModule
    {
        private readonly FigaroDataContext context;
        public HomeModule(FigaroDataContext dataContext) : base("/")
        {
            context = dataContext;

            if (dataContext.GetLoadCount() == 0)
            {
                Get("/", _ => $"Current database record count is " + context.GetLoadCount());
            }
            else
                Get("/", o => View["Home/Index.xqy"]);

            /*
             * In this example we use the index to look up the metadata values on the category, and use it to get our counts.
             */
            Get("/indexed", p => {
                var qc = context.Manager.CreateQueryContext(EvaluationType.Eager);
                try
                {
                    var zero = context.Manager.CreateXmlResults();
                    zero.Add(new XmlValue(0));                                       
                    var s = new Stopwatch();
                    s.Start();
                    //using (var qc = context.Manager.CreateQueryContext(EvaluationType.Eager))
                    //{
                        qc.SetVariableValue("badges", context.IndexedCategoryCount("Badges") ?? zero);
                        qc.SetVariableValue("comments", context.IndexedCategoryCount("Comments") ?? zero);
                        qc.SetVariableValue("postHistory", context.IndexedCategoryCount("PostHistory") ?? zero);
                        qc.SetVariableValue("postLinks", context.IndexedCategoryCount("PostLinks") ?? zero);
                        qc.SetVariableValue("posts", context.IndexedCategoryCount("Posts") ?? zero);
                        qc.SetVariableValue("tags", context.IndexedCategoryCount("Tags") ?? zero);
                        qc.SetVariableValue("users", context.IndexedCategoryCount("Users") ?? zero);
                        qc.SetVariableValue("votes", context.IndexedCategoryCount("Votes") ?? zero);
                        qc.QueryTimeoutSeconds = 10;
                        s.Stop();
                        Console.WriteLine($"view prepared in {s.ElapsedMilliseconds} ms");
                        return View["Home/Indexed.xqy", qc];
                    //}
                }
                catch(Exception ex)
                {
                    Trace.WriteLine($"{ex.GetType()}:{ex.Message}\r\n{ex.StackTrace}");
                    return HttpStatusCode.Locked;
                }
            });

            Get("/Users", p =>
            {
                var qc = context.Manager.CreateQueryContext(EvaluationType.Eager);
                try
                {
                    var zero = context.Manager.CreateXmlResults();
                    zero.Add(new XmlValue(0));
                    var s = new Stopwatch();
                    s.Start();
                    qc.SetVariableValue("users", context.IndexedCategoryCount("Users") ?? zero);
                    qc.SetVariableValue("a", new XmlValue(0));
                    qc.SetVariableValue("b", new XmlValue(50));       
                    qc.SetVariableValue("cat","Users");

                    qc.QueryTimeoutSeconds = 10;
                    s.Stop();
                    Console.WriteLine($"view prepared in {s.ElapsedMilliseconds} ms");
                    return View["Home/Users.xqy", qc];
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"{ex.GetType()}:{ex.Message}\r\n{ex.StackTrace}");
                    return HttpStatusCode.Locked;
                }
            });

        }
    }
}
