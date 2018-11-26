using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;


namespace SearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool m_doStemming = false;
        public static bool isResetOn = false;

        public MainWindow()
        {
            InitializeComponent();
            languagesComboBox.Items.Insert(0, "Choose...");
            languagesComboBox.SelectedIndex = 0;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            m_doStemming = true;
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            m_doStemming = false;
        }

        private void Input_browse_Clicked(object sender, System.EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    inputPathText.Text = fbd.SelectedPath;
                    string[] directories = Directory.GetDirectories(Directory.GetDirectories(fbd.SelectedPath)[0]);
                    System.Windows.Forms.MessageBox.Show("Directories found: " + directories.Length.ToString(), "Message");
                }
            }
        }

        private void Output_browse_Clicked(object sender, System.EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    outputPathText.Text = fbd.SelectedPath;
                    string[] directories = Directory.GetDirectories(Directory.GetDirectories(fbd.SelectedPath)[0]);
                    System.Windows.Forms.MessageBox.Show("Directories found: " + directories.Length.ToString(), "Message");
                }
            }
        }

        private void Exit_button_Clicked(object sender, System.EventArgs e)
        {
            string message = "Do you really want to exit?";
            string caption = "Search Engine";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
            result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void Run_button_Clicked(object sender, System.EventArgs e)
        {
            bool validInput = Directory.Exists(inputPathText.Text);
            bool validOutput = Directory.Exists(outputPathText.Text);
            if (!validInput)
            {
                string message = "Please enter a valid Input Path!";
                string caption = "Error Detected in Input";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            if (!validOutput)
            {
                string message = "Please enter a valid OutPut Path!";
                string caption = "Error Detected in OutPut";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }

            // run all methods here!!!
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (outputPathText.Text.Equals(""))
            {
                string message = "The are no dictionary or posting files in the path you specified!";
                string caption = "Reset Search Engine Process";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            else
            {
                string pathWithStem = System.IO.Path.Combine(outputPathText.Text, "WithStem");
                string pathWithOutStem = System.IO.Path.Combine(outputPathText.Text, "WithOutStem");
                if (Directory.Exists(pathWithStem))
                    Directory.Delete(pathWithStem, true);
                if (Directory.Exists(pathWithOutStem))
                    Directory.Delete(pathWithOutStem, true);
                string message = "The search engine initialization was successful!";
                string caption = "Reset Search Engine Process";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
        }
    }
}
