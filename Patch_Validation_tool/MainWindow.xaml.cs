﻿using System;
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
            }
            else
            {
                MessageBox.Show("File write error ");
            }
        }
        public void Get_Patch_Details()
        {
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
                        txtPreReq.Text = prereqlist+Path.GetFileName(txtPatchpath.Text);
                        found = true;
                        break;
                    }
                    count++;
                }
                if(!found)
                {
                    MessageBox.Show("Invalid Patch !!!!!!!!!!");
                }
            }
            else
            {
                MessageBox.Show("Enter Patch location !!!!!");
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
            if ((GmProdList[cboProducts.SelectedIndex].OsType.Contains("Win 7")  || GmProdList[cboProducts.SelectedIndex].OsType.Contains("Window 8.1"))
                && radioJapanese.IsChecked== true)
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
                txtBuildPath.Text = @"\\bauser\Fiery-products\Sustaining_builds\\"+cboProducts.SelectedValue.ToString()+"\\GM";
            string[] pactches = System.IO.File.ReadAllLines(@"Prod_Patch_List.txt");
            foreach (var preq in pactches)
            {
                var prod = preq.Split(':').First();
                if(prod == cboProducts.SelectedValue.ToString())
                {
                    var patchList = preq.Split(':').Last();
                    txtPrereqFrmPdl.Text = patchList;
                    break;
                }
            }
            txtPreReq.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Dictionary<string, string> temp = new Dictionary<string, string>();
            temp.Add("Product", cboProducts.SelectedValue.ToString());
            temp.Add("calculus_name", GmProdList[cboProducts.SelectedIndex].CalculusName);
            temp.Add("osType", GmProdList[cboProducts.SelectedIndex].OsType);
            temp.Add("Installer_patch", txtBuildPath.Text.ToString());
            temp.Add("Prerequisite", txtPreReq.Text.ToString());
            if (radioEnglish.IsChecked == true)
            {
                temp.Add("Language", "English");
            }
            else
            {
                temp.Add("Language", "Japanese");
            }
            if (radioServer.IsChecked == true)
            {
                temp.Add("ServerType","Server");
                if (txtIpAdress.Text == "")
                {
                    MessageBox.Show("IP Address is empty");
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
                else
                    temp.Add("IP_Adress", txtIpAdress.Text);
            }
            else
            {
                temp.Add("ServerType", "VM");
                temp.Add("IP_Adress","");
            }
            temp.Add("InstallOnServer", chkInstallerPath.IsChecked.ToString());
            temp.Add("Email", txtEmailAddr.Text.ToString());
            var tt =  JsonConvert.SerializeObject(temp);
            File.WriteAllText("product_Details.json", tt);
            var retStatus = ExecuteCommand("python Trigger_PatchInstallation_request.py product_Details.json");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow; // set the cursor back to arrow
            if (retStatus == 3)
                MessageBox.Show("All the Prerequisite are not present in the \\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches\\ and \\\\pdlfiles\\pdlfiles\\eng\\Sustaining_Patches\\  Location!!!");
            else if (retStatus != 0)
                MessageBox.Show("Error Contact sagar.s@efi.com !!!");
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
    }
}