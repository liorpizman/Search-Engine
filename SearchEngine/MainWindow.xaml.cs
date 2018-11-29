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
using InfoRetrieval;


namespace SearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Model model = new Model();

        public MainWindow()
        {
            InitializeComponent();
            languagesComboBox.Items.Insert(0, "Choose...");
            languagesComboBox.SelectedIndex = 0;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            model.setStemming(true);
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            model.setStemming(false);
        }

        private void Input_browse_Clicked(object sender, System.EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    inputPathText.Text = fbd.SelectedPath;
                    //Model.inputPath = inputPathText.Text;
                    model.setInputPath(inputPathText.Text);
                    //string[] directories = Directory.GetDirectories(Directory.GetDirectories(fbd.SelectedPath)[0]);
                    //System.Windows.Forms.MessageBox.Show("Directories found: " + directories.Length.ToString(), "Message");
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
                    //Model.outPutPath = outputPathText.Text;
                    model.setOutPutPath(outputPathText.Text);
                    //string[] directories = Directory.GetDirectories(Directory.GetDirectories(fbd.SelectedPath)[0]);
                    //System.Windows.Forms.MessageBox.Show("Directories found: " + directories.Length.ToString(), "Message");
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
            runButton.IsEnabled = false;
            resetButton.IsEnabled = false;
            loadDicButton.IsEnabled = false;
            displayDicButton.IsEnabled = false;
            browseInputButton.IsEnabled = false;
            browseOutputButton.IsEnabled = false;
            exitButton.IsEnabled = false;

            bool validInput = Directory.Exists(inputPathText.Text);
            bool validOutput = Directory.Exists(outputPathText.Text);
            if (!validInput)
            {
                string message = "Please enter a valid Input Path!";
                string caption = "Error Detected in Input";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                return;
            }
            if (!validOutput)
            {
                string message = "Please enter a valid OutPut Path!";
                string caption = "Error Detected in OutPut";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                return;
            }
            // run all methods here!!!
            if (model.inputPath.Equals(""))
            {
                model.setInputPath(inputPathText.Text);
            }
            if (model.outPutPath.Equals(""))
            {
                model.setOutPutPath(outputPathText.Text);
            }
            model.Run();
            string message2 = "The execution is finished!";
            string caption2 = "Mission Completed Successfully";
            MessageBoxButtons buttons2 = MessageBoxButtons.OK;
            DialogResult result2 = System.Windows.Forms.MessageBox.Show(message2, caption2, buttons2);

            runButton.IsEnabled = true;
            resetButton.IsEnabled = true;
            loadDicButton.IsEnabled = true;
            displayDicButton.IsEnabled = true;
            browseInputButton.IsEnabled = true;
            browseOutputButton.IsEnabled = true;
            exitButton.IsEnabled = true;
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
