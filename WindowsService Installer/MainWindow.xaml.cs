using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Management;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace WindowsService_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string fileName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            radioInstall.IsChecked = true;
            txtIns1.Text = " 1. Click on Install Service in Select Operation.";
            txtIns2.Text = " 2. Enter the Service Name you want to give.";
            txtIns3.Text = " 3. Enter the Display Name you want to give.";
            txtIns4.Text = " 4. Select the exe file you want to install as a service.";
            txtIns5.Text = " 5. Click on Install to install the service.";
            txtIns7.Text = " 1. Click on Delete Service in Select Operation.";
            txtIns8.Text = " 2. Enter the Service Name you want to delete.";
            txtIns9.Text = " 3. Click on Delete to delete the service.";
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog serviceDialog = new OpenFileDialog();
            serviceDialog.Title = "Select Service Executable file";
            serviceDialog.Filter = "All supported executable|*.exe;";
            if (serviceDialog.ShowDialog() == true)
            {
                if (serviceDialog.FileName != "")
                {
                    txtServiceFilePath.Text = serviceDialog.FileName;
                    fileName = serviceDialog.FileName;
                }
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (btnInstall.Content.ToString() == "Install")
            {
                if (string.IsNullOrEmpty(txtServiceName.Text) || string.IsNullOrEmpty(txtDisplayName.Text) || string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Pelase provide all the required details");
                }
                else
                {
                    ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == txtServiceName.Text);
                    if (ctl == null)
                    {
                        string servname = txtServiceName.Text;
                        string dispName = txtDisplayName.Text;
                        string command = @"create " + servname + " binpath= \"" + fileName + "\" DisplayName= \"" + dispName + "\" start= auto";
                        bool IsTheSame = IsPathAlreadyUsed();
                        if (IsTheSame == true)
                        {
                            MessageBox.Show("A service is already using this path. Please choose another path.");
                        }
                        else
                        {
                            var confirmCreate = MessageBox.Show("Do you really want to create a windows service named " + servname + " ?", "Confirm Service Creation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                            if (confirmCreate == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.CreateNoWindow = true;
                                    startInfo.UseShellExecute = false;
                                    startInfo.RedirectStandardOutput = true;
                                    startInfo.RedirectStandardError = true;
                                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    startInfo.FileName = "sc.exe";
                                    startInfo.Arguments = command;
                                    using (Process exeProcess = Process.Start(startInfo))
                                    {
                                        string outputStr = exeProcess.StandardOutput.ReadToEnd();
                                        if (outputStr.Trim() == "[SC] CreateService SUCCESS")
                                        {
                                            outputStr = "Service Successfully Installed";
                                        }
                                        string errorStr = exeProcess.StandardError.ReadToEnd();
                                        exeProcess.WaitForExit();
                                        if (errorStr != string.Empty || outputStr != string.Empty)
                                        {
                                            MessageBox.Show(outputStr + errorStr);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("The Service Name you've specified already exists.");
                    }
                }
            }
            if (btnInstall.Content.ToString() == "Delete")
            {
                if (string.IsNullOrEmpty(txtServiceName.Text))
                {
                    MessageBox.Show("Pelase provide Service Name");
                }
                else
                {
                    ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == txtServiceName.Text);
                    if (ctl == null)
                    {
                        MessageBox.Show("The Service you've specified does not exists.");
                    }
                    else
                    {
                        string servname = txtServiceName.Text;
                        string command = "delete " + servname;
                        var confirmDelete = MessageBox.Show("Do you really want to delete the windows service named " + servname + " ?", "Confirm Service Deletion", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        if (confirmDelete == MessageBoxResult.Yes)
                        {
                            try
                            {
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardOutput = true;
                                startInfo.RedirectStandardError = true;
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                startInfo.FileName = "sc.exe";
                                startInfo.Arguments = command;
                                using (Process exeProcess = Process.Start(startInfo))
                                {
                                    string outputStr = exeProcess.StandardOutput.ReadToEnd();
                                    if (outputStr.Trim() == "[SC] DeleteService SUCCESS")
                                    {
                                        outputStr = "Service Successfully Deleted";
                                    }
                                    string errorStr = exeProcess.StandardError.ReadToEnd();
                                    exeProcess.WaitForExit();
                                    if (errorStr != string.Empty || outputStr != string.Empty)
                                    {
                                        MessageBox.Show(outputStr + errorStr);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void radioInstall_Checked(object sender, RoutedEventArgs e)
        {
            btnInstall.Content = "Install";
            operationHeader.Text = "Install Service";
            tbDisplayName.Visibility = Visibility.Visible;
            tbSelectService.Visibility = Visibility.Visible;
            txtDisplayName.Visibility = Visibility.Visible;
            BtnUBrowse.Visibility = Visibility.Visible;
            txtServiceFilePath.Visibility = Visibility.Visible;
            btnInstall.SetValue(Grid.RowProperty, 5);
        }

        private void radioDelete_Checked(object sender, RoutedEventArgs e)
        {
            btnInstall.Content = "Delete";
            operationHeader.Text = "Delete Service";
            tbDisplayName.Visibility = Visibility.Hidden;
            tbSelectService.Visibility = Visibility.Hidden;
            txtDisplayName.Visibility = Visibility.Hidden;
            BtnUBrowse.Visibility = Visibility.Hidden;
            txtServiceFilePath.Visibility = Visibility.Hidden;
            btnInstall.SetValue(Grid.RowProperty, 4);
        }

        private string servicePath(string serviceName)
        {
            string currentserviceExePath = string.Empty;
            using (ManagementObject wmiService = new ManagementObject("Win32_Service.Name='" + serviceName + "'"))
            {
                wmiService.Get();
                currentserviceExePath = wmiService["PathName"].ToString();
            }
            return currentserviceExePath;
        }

        private bool IsPathAlreadyUsed()
        {
            bool IsUsed = false;
            ServiceController[] controller = ServiceController.GetServices();
            foreach (ServiceController cont in controller)
            {
                string path = servicePath(cont.ServiceName);
                string a = System.Text.RegularExpressions.Regex.Replace(fileName, @"\s", "");
                string b = System.Text.RegularExpressions.Regex.Replace(path, @"\s", "");
                IsUsed = String.Compare(a, b, true) == 0;
                if (IsUsed == true)
                {
                    break;
                }
            }
            return IsUsed;
        }
    }
}
