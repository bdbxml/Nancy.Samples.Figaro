using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Figaro;

namespace Nancy.Demos.Figaro
{
    /// <summary>
    /// Sample data management object for using the Figaro database library.
    /// </summary>
    public class FigaroDataContext : IDisposable
    {
        /// <summary>
        /// Gets or sets the database environment.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/html/5f9eb5f1-764f-4a58-af59-fed2c87ad6bc.htm"/>
        public FigaroEnv Environment { get; }

        /// <summary>
        /// Gets or sets the database manager.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/html/514038c7-547b-476e-8bda-69428f315172.htm"/>
        public XmlManager Manager { get; }

        /// <summary>
        /// Gets the Figaro database.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/html/ccfbe603-567a-4e3f-a616-123a787a8ac6.htm"/>
        public Container BeerDb { get; }

        private bool disposing;

        protected internal readonly XQueryResolver resolver;


        public bool Initialized { get; private set; }

        /// <summary>
        /// the default metadata URI.
        /// </summary>
        public const string META_URI = "http://schemas.bdbxml.net/metadata";

        /// <summary>
        /// Initialize the Figaro data objects via Figaro.Configuration 
        /// </summary>
        public FigaroDataContext(string rootPath)
        {
            Initialized = false;
            //The Figaro.Configuration will create the FigaroEnv object for the XmlManager it is 
            // assigned to, so we can simply retrieve the reference to it from the manager and 
            // avoid creating multiple instances and adding additional, unnecessary reference 
            // instances. Otherwise, we'd simply create it first and assign to the manager.          

            Environment = new FigaroEnv();
            Environment.SetThreadCount(20);
            Environment.SetCacheSize(new EnvCacheSize(1, 0), 1);
            Environment.SetCacheMax(new EnvCacheSize(2, 0));

            //http://help.bdbxml.net/html/M_Figaro_FigaroEnv_SetMaxSequentialWriteOperations.htm
            //Environment.SetMaxSequentialWriteOperations(10, 1000000); // set to 1 second (1000000 nanoseconds)

            // Configuring the Locking Subsystem: http://help.bdbxml.net/html/6c964163-f0d1-4b9e-97dc-38b1ab02a895.htm
            //http://help.bdbxml.net/html/M_Figaro_FigaroEnv_SetLockPartitions.htm
            Environment.SetLockPartitions(20);
            // Configuring Deadlock Detection: http://help.bdbxml.net/html/99788b9d-b930-4191-96f3-311f0b8ffebf.htm
            // DeadlockDetectType Enumeration: http://help.bdbxml.net/html/T_Figaro_DeadlockDetectType.htm
            Environment.DeadlockDetectPolicy = DeadlockDetectType.Oldest;
            Environment.SetMaxLockers(5000);
            Environment.SetMaxLocks(50000);
            Environment.SetMaxLockedObjects(50000);

            var path = Path.Combine(rootPath, "data");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            //Console.WriteLine("data directory is " + path);

            // to log transactions in memory:
            // see http://help.bdbxml.net/html/b166adda-4545-403d-a034-66a1d2774004.htm for details.
            //Environment.SetLogOptions(EnvLogOptions.InMemory, true);
            //Environment.SetLogOptions(EnvLogOptions.AutoRemove, true);
            Environment.SetMaxFileDescriptors(100);
            Environment.SetLogBufferSize(1024 * 1024 * 750);
            Environment.MaxLogSize = 1024 * 1024 * 100;

            Environment.SetMaxTransactions(500);
            Environment.SetLogOptions(EnvLogOptions.Direct, true);
            Environment.SetEnvironmentOption(EnvConfig.MultiVersion, true);
            Environment.SetEnvironmentOption(EnvConfig.DirectDB, true);

            //Environment.SetTimeout(10000,EnvironmentTimeoutType.Lock);
            Environment.SetTimeout(10000, EnvironmentTimeoutType.Transaction);
            Environment.SetTimeout(1000, EnvironmentTimeoutType.Lock);

            /* Enable message events for tracing purposes */
            //Environment.OnProcess += Environment_OnProcess;
            //Environment.OnMessage += Environment_OnMessage;
            Environment.OnErr += Environment_OnErr;

            Environment.ErrEventEnabled = true;
            //Environment.MessageEventEnabled = true;
            //Environment.ProcessEventEnabled = true;

            Environment.Open(path, EnvOpenOptions.SystemSharedMem | EnvOpenOptions.Recover | EnvOpenOptions.TransactionDefaults | EnvOpenOptions.Create | EnvOpenOptions.Thread);

            Manager = new XmlManager(Environment, ManagerInitOptions.AllOptions);

            // for resolving XQuery constructs - for more info:
            //http://help.bdbxml.net/html/e1571f63-0de0-4119-8dd3-68dc8693f732.htm
            resolver = new NancyXQueryResolver(new Uri("http://modules.bdbxml.net/nancy/"), rootPath);
            Manager.RegisterResolver(resolver);

            /*
             * open the container
             */
            using (var tx = Manager.CreateTransaction(TransactionType.SyncTransaction))
            {
                try
                {
                    //more info on ContainerConfig: http://help.bdbxml.net/html/b54e4294-4814-404f-a15f-32162b672260.htm
                    BeerDb = Manager.OpenContainer(tx,"beer.dbxml",
                        new ContainerConfig
                        {
                            MultiVersion = true,
                            AllowCreate = true,
                            Threaded = true,
                            IndexNodes = ConfigurationState.Off,
                            Transactional = true,
                            NoMMap = false,
                            Statistics = ConfigurationState.On
                        });
                    tx.Commit();
                    BeerDb.AddAlias("beer");
                    ConfigureContainerIndex();
                }
                catch (Exception)
                {
                    tx.Abort();
                    throw;
                }
            }

            Initialized = true;
        }

        /// <summary>
        /// Add an indexing strategy for metadata-based lookups of different message types.
        /// </summary>
        public void ConfigureContainerIndex()
        {

            /*
             * Add an indexing strategy for our metadata lookup
             */

            // IndexingStrategy helps us build the string that defines our index.
            // See http://help.bdbxml.net/html/T_Figaro_IndexingStrategy.htm for more information.
            // (note: IndexingStrategy is not an IDisposable)
            // 
            // in this case, we're building a non-unique, node path, metadata string equality index.
            var strat = new IndexingStrategy(false, IndexPathType.NodePath, IndexNodeType.Metadata, IndexKeyType.Equality, XmlDatatype.String);

            using (var tx = Manager.CreateTransaction())
            {
                try
                {
                    using (var uc = Manager.CreateUpdateContext()) { BeerDb.AddIndex(tx, META_URI, "category", strat.ToString(), uc); }
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Abort();
                    Console.WriteLine($"[ConfigureContainerIndex] {ex.GetType()}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// use the index to look up record names
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <seealso cref="http://help.bdbxml.net/html/0d794f6b-d027-4592-9767-34595cef9552.htm"/>
        public XmlResults IndexedCategoryCount(string category)
        {
            for (var i = 0; i < 300; i++)
            {
                if (disposing) return null;

                using (var tx = Manager.CreateTransaction(TransactionType.SnapshotTransaction))
                {
                    try
                    {
                        using (var lookup = Manager.CreateIndexLookup(BeerDb, META_URI, "category", "node-metadata-equality-string",
                            new XmlValue(category), IndexLookupOperation.Equal))
                        {
                            using (var qc = Manager.CreateQueryContext(EvaluationType.Eager))
                            {
                                qc.QueryTimeoutSeconds = 30;
                                using (var results = lookup.Execute(tx, qc))
                                {
                                    tx.Commit();
                                    var ret = Manager.CreateXmlResults();
                                    ret.Add(new XmlValue(results.Count));
                                    return ret;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tx.Abort();
                        Console.WriteLine($"Index lookup: {ex.Message}");
                        if (i == 299) throw;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// In this current version of our example, we don't know what our user data looks like - we just know we have a string 
        /// representation of Xml. Let's take that data and save it in the database, so we can perform analysis on it later.
        /// </summary>
        /// <param name="itemData">The string we pulled from the Users file.</param>
        /// <param name="category">The item category, used for sorting/querying later.</param>
        public void InsertItem(string itemData, string category)
        {
            // set up to retry 20 times
            while (true)
            {
                if (disposing) return;
                using (var tx = Manager.CreateTransaction(TransactionType.SnapshotTransaction))
                {
                    try
                    {
                        var doc = Manager.CreateDocument();
                        doc.SetContent(itemData);
                        doc.SetMetadata(META_URI, "category", new XmlValue(category));
                        doc.Name = category;
                        using (var uc = Manager.CreateUpdateContext()) { BeerDb.PutDocument(tx, doc, uc, PutDocumentOptions.GenerateFileName); }
                        //using (var uc = Manager.CreateUpdateContext()) { BeerDb.PutDocument(doc, uc,PutDocumentOptions.GenerateFileName); }
                        tx.Commit();
                        doc.Dispose();
                        break;
                    }
                    catch (Exception ex)
                    {
                        tx.Abort();
                        Console.WriteLine("{0} {1}: write error: {2}", DateTime.Now, ex.GetType(), ex.Message);
                        //give the reader a chance to retry before resuming writes
                        Thread.Sleep(300);
                    }
                }
            }
        }
        /// <summary>
        /// Get the number of records currently in the user database.
        /// </summary>
        /// <returns></returns>
        public ulong GetLoadCount()
        {
            if (disposing) return 0;
            using (var tx = Manager.CreateTransaction(TransactionType.SnapshotTransaction))
            {
                try
                {
                    return BeerDb.GetNumDocuments(tx);
                }
                catch (Exception)
                {
                    tx.Abort();
                }
                finally
                {
                    if (!tx.Aborted) tx.Commit();
                }
            }
            return 0;
        }

        /// <summary>
        /// Inserts an XML record pulled from a URL.
        /// </summary>
        /// <param name="url">The URL to extract XML from.</param>
        /// <returns>The name of the record.</returns>
        public string InsertRecordFromUrl(string url)
        {
            var name = "record" + DateTime.Now.ToFileTimeUtc();
            var stm = Manager.CreateUrlInputStream(string.Empty, url);
            var doc = Manager.CreateDocument();
            doc.SetContentAsInputStream(stm);
            doc.Name = name;

            //add metadata to the record.
            doc.SetMetadata("http://schemas.bdbxml.net/metadata", "CreatedDate",
                new XmlValue(DateTime.Now.ToString()));
            using (var uc = Manager.CreateUpdateContext()) { BeerDb.PutDocument(doc, uc); }
            return name;
        }

        /// <summary>
        /// Insert a System.Xml.XmlDocument into the database.
        /// </summary>
        /// <param name="doc">The document to insert.</param>
        /// <returns>The document name, for lookup purposes.</returns>
        public string InsertRecord(System.Xml.XmlDocument doc)
        {
            if (disposing) return string.Empty;
            var name = "record" + DateTime.Now.ToFileTimeUtc();
            var xml = Manager.CreateDocument(doc);
            xml.Name = name;
            //add metadata to the record.
            xml.SetMetadata("http://schemas.bdbxml.net/metadata", "CreatedDate",
            new XmlValue(DateTime.Now.ToString()));
            using (var uc = Manager.CreateUpdateContext()) { BeerDb.PutDocument(xml, uc); }
            return name;
        }

        /// <summary>
        /// Environment progress event handler. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnProgress(object sender, ProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case FeedbackEvent.Other:
                    Trace.WriteLine($"feedback event {e.PercentComplete}%");
                    break;
                case FeedbackEvent.Recover:
                    Trace.WriteLine($"feedback event {e.PercentComplete}%");
                    break;
                case FeedbackEvent.Upgrade:
                    Trace.WriteLine($"feedback event {e.PercentComplete}%");
                    break;
            }
        }

        /// <summary>
        /// Handle panic and write failure events here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnProcess(object sender, ProcessEventArgs e)
        {
            if (e.EventType == EnvironmentEvent.Panic)
                Trace.WriteLine("A panic event occurred!", "Panic");
            if (e.EventType == EnvironmentEvent.WriteEventFailure)
                Trace.WriteLine("A write event failed to occur!", "Fatal");
        }

        /// <summary>
        /// For diagnostic message events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnMessage(object sender, MsgEventArgs e)
        {
            Trace.WriteLine($"{e.Message}", "Message");
        }

        /// <summary>
        /// For writing error related events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnErr(object sender, ErrEventArgs e)
        {
            Trace.WriteLine($"{e.Prefix}: {e.Message}", "Error");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposing = true;
            Console.WriteLine("FigaroDataContext disposing...");
            // gracefully exit from our resources. Note that these need to be closed in 
            // the sequence shown.
            resolver?.Dispose();
            BeerDb.Dispose();
            Manager?.Dispose();
            GC.Collect(2, GCCollectionMode.Optimized);
            Environment?.Dispose();
        }
    }
}
