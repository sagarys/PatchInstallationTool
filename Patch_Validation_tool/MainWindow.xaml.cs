using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Input;

namespace Patch_Installation_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<Gm_Product_Details> gmProdList = new List<Gm_Product_Details>();

        internal static List<Gm_Product_Details> GmProdList { get => gmProdList; set => gmProdList = value; }
        internal const string BUILD_PATH = @"\\bauser\Fiery-products\Sustaining_builds\\";
        public static int ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            process.BeginErrorReadLine();

            process.WaitForExit();
            var temp = process.ExitCode;
            process.Close();
            return temp;
        }
        public void list_of_products()
        {
            if (ExecuteCommand("python product.py") == 0)
            {
                string[] Product_list = System.IO.File.ReadAllLines(@"GM_Prod_List.txt");

                foreach (var prod in Product_list)
                {
                    var prodDetails = prod.Split(':');
                    GmProdList.Add(new Gm_Product_Details(prodDetails[0].Replace(" ", ""), prodDetails[1], prodDetails[2], prodDetails[3]));
                    cboProducts.Items.Add(prodDetails[0].Replace(" ", ""));
                }
                cboProducts.SelectedIndex = 0;
                radioEnglish.IsChecked = true;
                radioServer.IsChecked = true;
                txtEmailAddr.Text = Environment.UserName + "@efi.com";
                chkInstallerPath.IsChecked = true;
                if (chkInstallerPath.IsChecked == false)
                    txtBuildPath.IsEnabled = false;
                txtPatchpath.IsEnabled = false;
                radioBangalore.IsChecked = true;
                radioGM.IsChecked = true;
            }
            else
            {
                MessageBox.Show("File write error ");
                Environment.Exit(1);
            }
        }
        public void Get_Patch_Details()
        {
            if (txtPatchpath.Text == "")
                return;

            string line;
            bool found = false;
            int count = 0;
            // Read the file and display it line by line.  
            if (File.Exists(txtPatchpath.Text))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(txtPatchpath.Text);
                while ((line = file.ReadLine()) != null && count < 25)
                {
                    if (line.Contains("Prerequisites"))
                    {
                        var prereqlist = line.Split(':').Last();
                        //prereqlist = prereqlist.Remove(prereqlist.Length - 1);
                        if (prereqlist.Trim() == "NONE")
                        {
                            prereqlist = "";
                        }
                        List<string> prereqPstemp = prereqlist.Split(',').Select(x => x + ".ps").ToList();
                        prereqPstemp.RemoveAt(prereqPstemp.Count - 1);
                        prereqlist = string.Join(",", prereqPstemp);
                        prereqlist += ",";
                        txtPreReq.Text = prereqlist + Path.GetFileName(txtPatchpath.Text);
                        txtPreReq.Text = txtPreReq.Text.Trim();
                        found = true;
                        break;
                    }
                    count++;
                }
                if (!found)
                {
                    MessageBox.Show("Invalid Patch !!!!!!!!!!");
                }
            }
            else
            {
                MessageBox.Show("Not a valid Patch !!!!");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            list_of_products();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Remove this code once J VM's are available...
            if ((GmProdList[cboProducts.SelectedIndex].OsType.Contains("Win 7") || GmProdList[cboProducts.SelectedIndex].OsType.Contains("Window 8.1"))
                && radioJapanese.IsChecked == true)
            {
                radioVM.IsChecked = false;
                radioServer.IsChecked = true;
                radioVM.IsEnabled = false;
            }
            else
            {
                radioVM.IsEnabled = true;
            }

            if (chkInstallerPath.IsChecked == true)
            {
                if (radioGM.IsChecked == true)
                    txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "GM");
                if (radioDebug.IsChecked == true)
                    txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "Debug");
                if (radioRelease.IsChecked == true)
                    txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "Release");
            }
            string[] pactches = System.IO.File.ReadAllLines(@"Prod_Patch_List.txt");
            var found = false;
            foreach (var preq in pactches)
            {
                var prod = preq.Split(':').First();
                if (prod == cboProducts.SelectedValue.ToString())
                {
                    var patchList = preq.Split(':').Last();
                    txtPrereqFrmPdl.Text = patchList;
                    found = true;
                    break;
                }
            }
            if (!found)
                txtPrereqFrmPdl.Text = "";
            txtPreReq.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (radioFremont.IsChecked == true)
                copyBuildToSyncServer();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (chkPatchLoc.IsChecked == true)
            {
                if (txtPatchpath.Text != "")
                {
                    var pdlloc = "\\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\" + cboProducts.SelectedValue;
                    var copyPatch = "copy_file.bat " + "\"" + Path.GetDirectoryName(txtPatchpath.Text) + "\"" +
                                                          " \"" + pdlloc + "\"" + " \"" + Path.GetFileName(txtPatchpath.Text) + "\"" +
                                                          " " + Path.GetFileName(txtPatchpath.Text) + ".txt\"";
                    if (ExecuteCommand(copyPatch) != 0)
                    {
                        MessageBox.Show("Patch " + Path.GetFileName(txtPatchpath.Text) + " copy failed to \\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\");
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                        return;
                    }
                    txtPrereqFrmPdl.Text += ',' + Path.GetFileName(txtPatchpath.Text);
                }
                else
                {
                    MessageBox.Show("Patch location is empty");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
            }

            var txtrrereq = txtPreReq.Text.Split(',');
            foreach (var prereq in txtrrereq)
            {
                if (!txtPrereqFrmPdl.Text.Contains(prereq))
                {
                    MessageBox.Show(prereq + " Not present in the \\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
            }

            Dictionary<string, string> product_Details_json = new Dictionary<string, string>();
            product_Details_json.Add("Product", cboProducts.SelectedValue.ToString());
            product_Details_json.Add("calculus_name", GmProdList[cboProducts.SelectedIndex].CalculusName);
            product_Details_json.Add("osType", GmProdList[cboProducts.SelectedIndex].OsType);
            product_Details_json.Add("Installer_path", txtBuildPath.Text.ToString());
            product_Details_json.Add("Prerequisite", txtPreReq.Text.ToString());
            if (radioEnglish.IsChecked == true)
            {
                product_Details_json.Add("Language", "English");
            }
            else
            {
                product_Details_json.Add("Language", "Japanese");
            }
            if (radioServer.IsChecked == true)
            {
                product_Details_json.Add("ServerType", "Server");
                if (txtIpAdress.Text == "")
                {
                    MessageBox.Show("IP Address is empty");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
                else
                    product_Details_json.Add("IP_Adress", txtIpAdress.Text);
            }
            else
            {
                product_Details_json.Add("ServerType", "VM");
                product_Details_json.Add("IP_Adress", "");
            }
            if (chkenbleSSH.IsChecked == true)
                product_Details_json.Add("Enable_CFF", "True");
            else
                product_Details_json.Add("Enable_CFF", "False");
            product_Details_json.Add("WithInstaller", chkInstallerPath.IsChecked.ToString());
            product_Details_json.Add("Email", txtEmailAddr.Text.ToString());
            var tt = JsonConvert.SerializeObject(product_Details_json);
            File.WriteAllText("product_Details.json", tt);
            var retStatus = ExecuteCommand("python Trigger_PatchInstallation_request.py product_Details.json");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; // set the cursor back to arrow
            if (retStatus == 4)
                MessageBox.Show(cboProducts.SelectedValue.ToString() + " Fodler does not exists in the " +
                                "\\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\ and \\\\pdlfiles\\pdlfiles\\eng\\Sustaining_Patches\\  Location!!!");
            if (retStatus == 3)
                MessageBox.Show("All the Prerequisite are not present in the \\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\ and \\\\pdlfiles\\pdlfiles\\eng\\Sustaining_Patches\\  Location!!!");
            else if (retStatus != 0)
                MessageBox.Show("Error Contact sagar.s@efi.com with product_Details.json file!!!");
            else
                MessageBox.Show("Sucess CalculusBuild is triggered ");
        }

        private void RadioVM_Checked(object sender, RoutedEventArgs e)
        {
            txtIpAdress.IsEnabled = false;
        }

        private void RadioServer_Checked(object sender, RoutedEventArgs e)
        {
            txtIpAdress.IsEnabled = true;
            chkenbleSSH.IsEnabled = true;
        }

        private void ChkInstallerPath_Checked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.Text = @"\\bauser\Fiery-products\Sustaining_builds\\" + cboProducts.SelectedValue.ToString() + "\\GM";
            txtBuildPath.IsEnabled = true;
            radioVM.IsEnabled = true;
        }
        private void ChkInstallerPath_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.Text = "";
            txtBuildPath.IsEnabled = false;
            radioVM.IsChecked = false;
            radioVM.IsEnabled = false;
            radioServer.IsChecked = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            txtPatchpath.IsEnabled = true;
        }
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            txtPatchpath.Text = "";
            txtPatchpath.IsEnabled = false;
        }

        private void TxtPatchpath_LostFocus(object sender, RoutedEventArgs e)
        {
            Get_Patch_Details();
        }
        private void ChkPatchLoc_Checked(object sender, RoutedEventArgs e)
        {
            txtPatchpath.IsEnabled = true;
        }

        private void ChkPatchLoc_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPatchpath.IsEnabled = false;
            txtPatchpath.Text = "";
        }
        private void RadioJapanese_Checked(object sender, RoutedEventArgs e)
        {
            //Remove this code once J VM's are available...
            if (GmProdList[cboProducts.SelectedIndex].OsType.Contains("Win 7") || GmProdList[cboProducts.SelectedIndex].OsType.Contains("Window 8.1"))
            {
                radioVM.IsChecked = false;
                radioServer.IsChecked = true;
                radioVM.IsEnabled = false;
            }
        }
        private void RadioJapanese_Unchecked(object sender, RoutedEventArgs e)
        {
            //Remove this code once J VM's are available...
            radioVM.IsEnabled = true;
        }

        private void copyBuildToSyncServer()
        {
            MessageBoxResult m = MessageBox.Show("Build needs to be copied into the sync server", "Build Copy !!!", MessageBoxButton.YesNo);
            if (m == MessageBoxResult.Yes)
            {
                chkPatchLoc.IsChecked = false;
                chkPatchLoc.IsEnabled = false;
                txtPatchpath.Text = "";
                txtPreReq.Text = "";
                txtPreReq.IsEnabled = false;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                var syncserverLoc = "\\\\bawdfs01\\OUTBOX\\TO-FC\\Sustaining_Builds";
                var installerPath = txtBuildPath.Text;
                var copyBuild = "copy_dir.bat " + "\"" + txtBuildPath.Text + "\"" +
                                                      " \"" + Path.Combine(syncserverLoc, cboProducts.SelectedValue.ToString()) + "\"" +
                                                      " " + cboProducts.SelectedValue.ToString() + ".txt\"";
                if (ExecuteCommand(copyBuild) != 0)
                {
                    MessageBox.Show("Build copy failed to sync server \\\\bawdfs01\\OUTBOX\\TO-FC\\Sustaining_Builds");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; // set the cursor back to arrow
                    return;
                }
                else
                {
                    MessageBox.Show("Build copied to sync server ....Please check after 30 mn in the below location " +
                        "\\\\fcwdfs01\\INBOX\\FROM-BLR\\Sustaining_Builds\\ " + cboProducts.SelectedValue.ToString() + "!!!");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; // set the cursor back to arrow
                    Environment.Exit(2);
                }
            }
        }
        private void RadioFremont_Checked(object sender, RoutedEventArgs e)
        {
            copyBuildToSyncServer();
        }

        private void RadioBangalore_Checked(object sender, RoutedEventArgs e)
        {
            chkPatchLoc.IsEnabled = true;
            txtPreReq.IsEnabled = true;
        }
        private void RadioServer_Unchecked(object sender, RoutedEventArgs e)
        {
            chkenbleSSH.IsEnabled = false;
        }

        private void RadioButtonDebug_Checked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.Text = "";
            txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "Debug");
            txtBuildPath.IsEnabled = false;
        }

        private void RadioButtonRelease_Checked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.Text = "";
            txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "Release");
            txtBuildPath.IsEnabled = false;
        }

        private void RadioButtonGM_Checked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.Text = "";
            txtBuildPath.Text = Path.Combine(BUILD_PATH, cboProducts.SelectedValue.ToString(), "GM");
            txtBuildPath.IsEnabled = false;
        }

        private void RadioButtonCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtBuildPath.IsEnabled = true;
        }
    }
}