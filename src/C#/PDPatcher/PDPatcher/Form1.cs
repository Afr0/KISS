using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LogThis;
using KISS;

namespace PDPatcher
{
    public partial class Form1 : Form
    {
        private Requester m_Requester;
        private ManifestFile m_ClientManifest, m_PatchManifest;
        //Files that make up the difference between client's version and patch version.
        private List<PatchFile> m_PatchDiff = new List<PatchFile>();
        private int m_NumFilesDownloaded = 0;

        public Form1()
        {
            InitializeComponent();

            m_ClientManifest = new ManifestFile(File.Open("Client.manifest", FileMode.Open));

            m_Requester = new Requester("https://dl.dropboxusercontent.com/u/257809956/PatchManifest.manifest");

            m_Requester.OnFetchedManifest += new FetchedManifestDelegate(m_Requester_OnFetchedManifest);
            m_Requester.OnTick += new DownloadTickDelegate(m_Requester_OnTick);
            m_Requester.OnFetchedFile += new FetchedFileDelegate(m_Requester_OnFetchedFile);
            Logger.OnMessageLogged += new MessageLoggedDelegate(Logger_OnMessageLogged);

            m_Requester.Initialize();
        }

        /// <summary>
        /// Another file was fetched!
        /// </summary>
        /// <param name="FileStream">Stream of the file that was fetched.</param>
        private void m_Requester_OnFetchedFile(Stream FileStream)
        {
            BinaryWriter Writer = new BinaryWriter(File.Create("Tmp\\" + m_PatchManifest.PatchFiles[m_NumFilesDownloaded].Address));
            BinaryReader Reader = new BinaryReader(FileStream);
            
            Writer.Write(Reader.ReadBytes((int)FileStream.Length - 1));
            Writer.Close();
            Reader.Close();

            if (m_NumFilesDownloaded < m_PatchDiff.Count)
            {
                m_NumFilesDownloaded++;
                m_Requester.FetchFile(m_PatchDiff[m_NumFilesDownloaded].URL);
            }
        }

        /// <summary>
        /// The manifest was fetched.
        /// </summary>
        /// <param name="Manifest">The patch manifest that was fetched.</param>
        private void m_Requester_OnFetchedManifest(ManifestFile Manifest)
        {
            m_PatchManifest = Manifest;

            //Versions didn't match, do update.
            if (m_ClientManifest.Version != m_PatchManifest.Version)
            {
                int ClientIndex = 0;

                for(int i = 0; i < m_ClientManifest.PatchFiles.Count; i++)
                {
                    string PatchName = Path.GetFileName(m_PatchManifest.PatchFiles[i].Address);
                    string ClientName = Path.GetFileName(m_ClientManifest.PatchFiles[i].Address);

                    //This is slow, hashes will probably have to be binary to
                    //be fast above 1000 files.
                    if ((m_PatchManifest.PatchFiles[i].FileHash != m_ClientManifest.PatchFiles[i].FileHash) ||
                        (PatchName != ClientName))
                        m_PatchDiff.Add(m_PatchManifest.PatchFiles[i]);

                    ClientIndex++;
                }

                //TODO: What to do if patch manifest > client manifest?

                Directory.CreateDirectory("Tmp");
                m_Requester.FetchFile(m_PatchDiff[0].URL);
            }
            else
            {
                MessageBox.Show("Your client is up to date!\n Exiting...");
                Application.Exit();
            }
        }

        /// <summary>
        /// The requester ticked, supplying information about a download in progress.
        /// </summary>
        /// <param name="State">The state of the download in progress.</param>
        private void m_Requester_OnTick(RequestState State)
        {
            if(LblSpeed.InvokeRequired)
                this.Invoke(new MethodInvoker(() => { LblSpeed.Text = State.KBPerSec.ToString() + " KB/Sec"; }));
            else
                LblSpeed.Text = State.KBPerSec.ToString() + "KB / Sec";

            if (PrgFile.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                    {
                        PrgFile.Step = (int)(PrgFile.Width * (State.PctComplete / State.ContentLength));
                        PrgFile.PerformStep();
                    }));
            }
            else
            {
                PrgFile.Step = (int)(PrgFile.Width * (State.PctComplete / State.ContentLength));
                PrgFile.PerformStep();
            }
        }

        /// <summary>
        /// KISS threw an exception.
        /// </summary>
        /// <param name="Msg">The message that was logged.</param>
        private void Logger_OnMessageLogged(LogMessage Msg)
        {
            switch (Msg.Level)
            {
                case LogLevel.error:
                    Log.LogThis(Msg.Message, eloglevel.error);
                    break;
                case LogLevel.info:
                    Log.LogThis(Msg.Message, eloglevel.info);
                    break;
                case LogLevel.warn:
                    Log.LogThis(Msg.Message, eloglevel.warn);
                    break;
            }
        }
    }
}
