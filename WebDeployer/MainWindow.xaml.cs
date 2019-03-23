using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using GIT = LibGit2Sharp.Commands;
namespace WebDeployer
{



    public partial class MainWindow : Window
    {
        string credentialPath = "gc.pr";


        public MainWindow()
        {
            InitializeComponent();

            LoadCredentials();
            
        }



        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //check if we have the repo already downloaded
                bool isRepo = Repository.IsValid(repoTxt.Text);

                string repoPath = repoTxt.Text;
                //if not, download it,
                if (!isRepo)
                {
                    repoPath = Repository.Clone(urlTxt.Text, repoTxt.Text);
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
                        new Identity(usernameTxt.Text, emailTxt.Text),
                        DateTimeOffset.Now);


                    var checkoutOptions = new CheckoutOptions
                    {
                        CheckoutModifiers = CheckoutModifiers.Force
                    };

                    GIT.Checkout(repo, repo.Branches.First(x => x.CanonicalName.Contains("master")), checkoutOptions);

                    GIT.Pull(repo, signature, options);

                    console.Text += "\n" + "Succesfully downloaded latest";

                    if (openCode_tgl.IsChecked == true)
                    {
                        OpenCode();
                    }
                }
            }
            catch (Exception ex)
            {
                console.Text += "\n" + ex.Message;
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
                    bool isRepo = Repository.IsValid(repoTxt.Text);

                    string repoPath = repoTxt.Text;

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
                        new Identity(usernameTxt.Text, emailTxt.Text),
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
                p.StartInfo.FileName =  codePathTxt.Text;
                p.StartInfo.Arguments = "\"" + repoTxt.Text + "\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = repoTxt.Text;
                p.StartInfo.CreateNoWindow = false;
                p.Start();

                console.Text += "\n VS-Code opened.";
            }
            catch (Exception ex)
            {
                console.Text += $"\n{ex.Message}";
            }
        }

        private void FindRepoBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Repo directory.",
                SelectedPath = Path.GetDirectoryName(Path.GetFullPath(@"..\..")),
            };
            folder.ShowDialog();

            repoTxt.Text = folder.SelectedPath;
        }

        private async void UpdateCredsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string body = "";
                body += $"{usernameTxt.Text}\n";
                body += $"{passwordBx.Password}\n";
                body += $"{codePathTxt.Text}\n";
                body += $"{emailTxt.Text}\n";
                body += $"{repoTxt.Text}\n";
                body += $"{urlTxt.Text}\n";



                // Encrypt the string to an array of bytes.
                //byte[] encrypted = Encryption.EncryptStringToBytes(body, myRijndael.Key, myRijndael.IV);

                var encrypted = Encryption.StringToBytes(body, null, DataProtectionScope.CurrentUser);
               // var encBytes = Encoding.UTF8.GetBytes(encrypted);

                var pth = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, credentialPath);


                using (var fs = new FileStream(pth, FileMode.Create))
                {
                    await fs.WriteAsync(encrypted, 0, encrypted.Length);
                }



            }
            catch (Exception ex)
            {
                console.Text += $"\n{ex.Message}";
            }

            console.Text += "\nUpdated Credentials.";




        }


        private async void LoadCredentials()
        {

            try
            {


                var pth = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, credentialPath);
                var encodedBytes = new byte[0x1000];

                using (var fs = new FileStream(pth, FileMode.OpenOrCreate))
                {
                    await fs.ReadAsync(encodedBytes, 0, 4096);
                }

                //string body = Convert.ToBase64String(encodedBytes);

                ////string body = Convert.ToBase64String(encodedBytes);
                //var bodyBytes = Encoding.UTF8.GetBytes(body);

                //var decodedString = "";// DecryptString(Convert.ToBase64String(encodedBytes));
                //var encryptedString = Encoding.UTF8.GetString(encodedBytes);
                var decodedString = Encryption.BytesToString(encodedBytes, null, DataProtectionScope.CurrentUser);

                var lines = decodedString.Split('\n');
                usernameTxt.Text = lines[0];
                passwordBx.Password = lines[1];
                codePathTxt.Text = lines[2];
                emailTxt.Text = lines[3];
                repoTxt.Text = lines[4];
                urlTxt.Text = lines[5];
            }
            catch (Exception e)
            {
                console.Text += $"\n{e.Message}";
            }


            console.Text += "\nLoaded Credentials.";

        }

        private void FindCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog
            {
                DefaultExt = ".exe",
                InitialDirectory = Environment.SystemDirectory

            };
            file.ShowDialog();

            codePathTxt.Text = file.FileName;
        }
    }

}



