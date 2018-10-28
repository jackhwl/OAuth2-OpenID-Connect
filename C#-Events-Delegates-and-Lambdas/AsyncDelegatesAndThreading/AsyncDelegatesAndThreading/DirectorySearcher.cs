namespace ThreadsAndDelegates
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    ///      This class is a Windows Forms control that implements a simple directory searcher.
    ///      You provide, through code, a search string and it will search directories on
    ///      a background thread, populating its list box with matches.
    /// </summary>
    public class DirectorySearcher : Control
    {

        public event EventHandler SearchComplete;

        // Define a special delegate that handles marshaling
        // lists of file names from the background directory search
        // thread to the thread that contains the list box.
        private delegate void FileListDelegate(string[] files, int startIndex, int count);

        private ListBox _ListBox;
        private string _SearchCriteria;
        private bool _Searching;
        private bool _DeferSearch;
        private Thread _SearchThread;
        private FileListDelegate _FileListDelegate;
        private EventHandler _OnSearchComplete;

        public DirectorySearcher()
        {
            _ListBox = new ListBox();
            _ListBox.Dock = DockStyle.Fill;

            Controls.Add(_ListBox);

            _FileListDelegate = new FileListDelegate(AddFiles);
            _OnSearchComplete = new EventHandler(OnSearchComplete);
        }

        public string SearchCriteria
        {
            get
            {
                return _SearchCriteria;
            }
            set
            {
                // If currently searching, abort
                // the search and restart it after
                // setting the new criteria.
                //
                bool wasSearching = Searching;

                if (wasSearching)
                {
                    StopSearch();
                }

                _ListBox.Items.Clear();
                _SearchCriteria = value;

                if (wasSearching)
                {
                    BeginSearch();
                }
            }
        }

        public bool Searching
        {
            get
            {
                return _Searching;
            }
        }

        public void BeginSearch()
        {
            if (Searching) return;

            // Start the search if the handle has
            // been created. Otherwise, defer it until the
            // handle has been created.
            if (IsHandleCreated)
            {
                _SearchThread = new Thread(new ThreadStart(ThreadProcedure));
                _Searching = true;
                _SearchThread.Start();
            }
            else
            {
                _DeferSearch = true;
            }
        }

        private void ThreadProcedure()
        {
            try
            {
                string localSearch = SearchCriteria;

                // Now, search the file system.
                //
                RecurseDirectory(localSearch);
            }
            finally
            {
                // You are done with the search, so update.
                //
                _Searching = false;

                // Raise an event that notifies the user that
                // the search has terminated.  
                // You do not have to do this through a marshaled call, but
                // marshaling is recommended for the following reason:
                // Users of this control do not know that it is
                // multithreaded, so they expect its events to 
                // come back on the same thread as the control
                BeginInvoke(_OnSearchComplete, new object[] { this, EventArgs.Empty });
            }
        }

        private void OnSearchComplete(object sender, EventArgs e)
        {
            if (SearchComplete != null)
            {
                SearchComplete(sender, e);
            }
        }

        public void StopSearch()
        {
            if (!_Searching)
            {
                return;
            }

            if (_SearchThread.IsAlive)
            {
                _SearchThread.Abort();
                _SearchThread.Join();
            }

            _SearchThread = null;
            _Searching = false;
        }

        /// <summary>
        /// Recurses the given path, adding all files on that path to 
        /// the list box. After it finishes with the files, it
        /// calls itself once for each directory on the path.
        /// </summary>
        /// <param name="searchPath"></param>
        private void RecurseDirectory(string searchPath)
        {
            // Split searchPath into a directory and a wildcard specification.
            //
            string directory = Path.GetDirectoryName(searchPath);
            string search = Path.GetFileName(searchPath);

            if (directory == null || search == null)
            {
                return;
            }

            string[] files;

            try
            {
                files = Directory.GetFiles(directory, search);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }

            int startingIndex = 0;

            while (startingIndex < files.Length)
            {
                // Batch up 20 files at once, unless at the
                // end.
                //
                int count = 20;
                if (count + startingIndex >= files.Length)
                {
                    count = files.Length - startingIndex;
                }

                // Begin the cross-thread call. 
                IAsyncResult r = BeginInvoke(_FileListDelegate, new object[] { files, startingIndex, count });
                startingIndex += count;
            }

            // Now that you have finished the files in this directory, recurse for
            // each subdirectory.
            string[] directories = Directory.GetDirectories(directory);
            foreach (string d in directories)
            {
                RecurseDirectory(Path.Combine(d, search));
            }
        }

        //Data routed from secondary thread by back to main thread
        //so safe to update _Listbox
        private void AddFiles(string[] files, int startIndex, int count)
        {
            while (count-- > 0)
            {
                _ListBox.Items.Add(files[startIndex + count]);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // If the handle is being destroyed and you are not
            // recreating it, then abort the search.
            if (!RecreatingHandle)
            {
                StopSearch();
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_DeferSearch)
            {
                _DeferSearch = false;
                BeginSearch();
            }
        }
    }
}

