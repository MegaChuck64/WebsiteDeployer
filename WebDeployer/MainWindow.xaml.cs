using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using GIT = LibGit2Sharp.Commands;

namespace WebDeployer
{



    public partial class MainWindow : Window
    {

        string path = Path.GetFullPath(@"..\..\..") + @"\MegaChuck64.github.io\";

        string credentialPath = Path.GetFullPath(@"..\..\..\..\..\..\gitCred.txt");


        public MainWindow()
        {
            InitializeComponent();


            try
            {
                using (var fs = new FileStream(credentialPath, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        usernameTxt.Text = sr.ReadLine();
                        passwordBx.Password = sr.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                console.Text += "\n" + e.Message;
            }

        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //check if we have the repo already downloaded
                bool isRepo = Repository.IsValid(path);

                string repoPath = path;
                //if not, download it,
                if (!isRepo)
                {
                    repoPath = Repository.Clone(@"https://github.com/megachuck64/megachuck64.github.io.git", path);
                    console.Text += "\n Repo being cloned...";
                }

                using (var repo = new Repository(repoPath))
                {

                    //pull
                    PullOptions options = new PullOptions
                    {
                        FetchOptions = new FetchOptions
                        {
                            CredentialsProvider = new CredentialsHandler(
                                (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = usernameTxt.Text,
                                Password = passwordBx.Password
                            })
                        },

                    };


                    var signature = new Signature(
                        new Identity(usernameTxt.Text, "cscarey@email.neit.edu"),
                        DateTimeOffset.Now);


                    var checkoutOptions = new CheckoutOptions
                    {
                        CheckoutModifiers = CheckoutModifiers.Force
                    };

                    GIT.Checkout(repo, repo.Branches.First(x => x.CanonicalName.Contains("master")), checkoutOptions);

                    GIT.Pull(repo, signature, options);
                }
            }
            catch (Exception ex)
            {
                console.Text += "\n" + ex.Message;
            }

            console.Text += "\n" + "Succesfully downloaded latest";

            if (openCode_tgl.IsChecked == true)
            {
                OpenCode();
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Repository repo = null;
            if (repo == null)
            {
                try
                {
                    //check if we have the repo already downloaded
                    bool isRepo = Repository.IsValid(path);

                    string repoPath = path;

                    if (isRepo)
                    {
                        repo = new Repository(repoPath);
                    }
                    else
                    {
                        console.Text += "\n Repo does not exist. Perhaps download first?";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    console.Text += "\n" + ex.Message + "\nUpload failed";


                }

            }

            //at this point we definitely have a repo

            try
            {
                GIT.Checkout(repo, repo.Branches.First(x => x.CanonicalName.Contains("master")));
                //add all changes
                GIT.Stage(repo, "*");

                var signature = new Signature(
                        new Identity(usernameTxt.Text, "cscarey@email.neit.edu"),
                        DateTimeOffset.Now);

                // Commit to the repository
                Commit commit = repo.Commit(commitTxt.Text, signature, signature);
                console.Text += $"\nCommited '{commitTxt.Text}'.";
                commitTxt.Clear();


            }
            catch (Exception ex)
            {
                console.Text += $"\n{ex.Message}";


            }


            try
            {
                PushOptions options = new PushOptions
                {
                    CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = usernameTxt.Text,
                                Password = passwordBx.Password
                            })
                };
                repo.Network.Push(repo.Branches.First(x => x.CanonicalName.Contains("master")), options);
                console.Text += "\nSuccesfully uploaded changes.";

            }
            catch (Exception ex)
            {
                console.Text += $"\n{ex.Message}";

            }

            repo.Dispose();
        }

        void OpenCode()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = @"C:\Users\cjsco\AppData\Local\Programs\Microsoft VS Code\Code.exe";
                p.StartInfo.Arguments = "\"" + path + "\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = path;
                p.StartInfo.CreateNoWindow = false;
                p.Start();

                console.Text += "\n VS-Code opened.";
            }
            catch(Exception ex)
            {
                console.Text += $"\n{ex.Message}";
            }
        }
    }
}
