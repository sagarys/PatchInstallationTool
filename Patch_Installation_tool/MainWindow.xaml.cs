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
            ExecuteCommand("python product.py");
            string[] Product_list = System.IO.File.ReadAllLines(@"GM_Prod_List.txt");
            
            foreach (var prod in Product_list)
            {
                var prodDetails = prod.Split(':');
                GmProdList.Add(new Gm_Product_Details(prodDetails[0].Replace(" ",""),prodDetails[1],prodDetails[2],prodDetails[3]));
                cboProducts.Items.Add(prodDetails[0].Replace(" ", ""));
            }
            cboProducts.SelectedIndex = 0;
            radioEnglish.IsChecked = true;
            radioServer.IsChecked = true;
            txtEmailAddr.Text = Environment.UserName+"@efi.com";
        }
        public MainWindow()
        {
            InitializeComponent();
            list_of_products();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtBuildPath.Text = @"\\bauser\Fiery-products\Sustaining_builds\\"+cboProducts.SelectedValue.ToString()+"\\GM";
            string[] pactches = System.IO.File.ReadAllLines(@"Prod_Patch_List.txt");
            foreach (var preq in pactches)
            {
                var prod = preq.Split(':').First();
                if(prod == cboProducts.SelectedValue.ToString())
                {
                    var patchList = preq.Split(':').Last();
                    lblPrerequisite.Content = patchList;
                    break;
                }
            }
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
           
            temp.Add("Email", txtEmailAddr.Text.ToString());
            var tt =  JsonConvert.SerializeObject(temp);
            File.WriteAllText(@"product_Details.json", tt);
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
    }
}
