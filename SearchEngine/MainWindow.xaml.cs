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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// fields of MainWindow
        /// </summary>
        public static Model model = new Model();

        /// <summary>
        /// constructor of MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            languagesComboBox.Items.Insert(0, "Choose...");
            languagesComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// method when check box is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            model.setStemming(true);
        }

        /// <summary>
        /// method when check box is unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            model.setStemming(false);
        }

        /// <summary>
        /// method of input button browse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// method of output button browse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// method of exit button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// method of run button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_button_Clicked(object sender, System.EventArgs e)
        {
            EnableButtons(false);
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
            model.setInputPath(inputPathText.Text);
            model.setOutPutPath(outputPathText.Text);
            TimeSpan runTime = TimeSpan.Zero;
            DateTime startTime = DateTime.Now;
            model.Run();
            runTime = runTime.Add(DateTime.Now - startTime);
            int i = 1;
            foreach (string language in model.indexer.m_Languages)
            {
                languagesComboBox.Items.Insert(i++, language);
            }
            StringBuilder outPutMessage = new StringBuilder("The execution is finished!");
            outPutMessage.AppendLine();
            outPutMessage.AppendLine("Total run time: " + runTime);
            outPutMessage.AppendLine("Total unique terms indexed: " + model.indexer.uniqueCorpusCounter);
            outPutMessage.AppendLine("Total documents indexed: " + model.indexer.docCounter);

            string caption2 = "Mission Completed Successfully";
            MessageBoxButtons buttons2 = MessageBoxButtons.OK;
            DialogResult result2 = System.Windows.Forms.MessageBox.Show(outPutMessage.ToString(), caption2, buttons2);

            EnableButtons(true);
        }

        /// <summary>
        /// method of reset button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            languagesComboBox.Items.Clear();
            languagesComboBox.Items.Insert(0, "Choose...");
            languagesComboBox.SelectedIndex = 0;
            dictionaryListBox.Items.Clear();
            model.ClearMemory();
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

        /// <summary>
        /// method of display button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayDicButton_Click(object sender, RoutedEventArgs e)
        {
            EnableButtons(false);
            // dictionaryListBox.Items.Clear();
            if (model.indexer != null)
            {
                var res = model.indexer.dictionaries.SelectMany(dict => dict)
            .ToDictionary(pair => pair.Key, pair => pair.Value.tfc);
                dictionaryListBox.ItemsSource = res;
                string message = "Mission done!";
                string caption = "Dictionary display";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            else
            {
                string message = "Dictionary does not exist!\n\nPlease Load the Dictionary or Run the search engine first.";
                string caption = "Dictionary display";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            EnableButtons(true);
        }

        /// <summary>
        /// method of load button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadDicButton_Click(object sender, RoutedEventArgs e)
        {
            EnableButtons(false);
            bool existWithStem = File.Exists(System.IO.Path.Combine(outputPathText.Text, "WithStem\\Dictionary.bin"));
            bool existWithoutStem = File.Exists(System.IO.Path.Combine(outputPathText.Text, "WithOutStem\\Dictionary.bin"));
            if (model.indexer != null)
            {
                if (!model.m_doStemming) // Without Stemming
                {
                    if (existWithoutStem)
                    {
                        if (model.lastRunStem)// last run stem - false when the last run was without stemming
                        {
                            Deserialize(System.IO.Path.Combine(outputPathText.Text, "WithOutStem"));
                        }
                    }
                    else
                    {
                        string msg = "Dictionary with out stemming does not exist!\n\nPlease Run the search engine first.";
                        string cap = "Dictionary Load";
                        MessageBoxButtons bttns = MessageBoxButtons.OK;
                        DialogResult res = System.Windows.Forms.MessageBox.Show(msg, cap, bttns);
                        EnableButtons(true);
                        return;
                    }
                }
                else // With Stemming
                {
                    if (existWithStem)
                    {
                        if (!model.lastRunStem)// last run stem - true when the last run was with stemming
                        {
                            Deserialize(System.IO.Path.Combine(outputPathText.Text, "WithStem"));
                        }
                    }
                    else
                    {
                        string msg = "Dictionary with stemming does not exist!\n\nPlease Run the search engine first.";
                        string cap = "Dictionary Load";
                        MessageBoxButtons bttns = MessageBoxButtons.OK;
                        DialogResult res = System.Windows.Forms.MessageBox.Show(msg, cap, bttns);
                        EnableButtons(true);
                        return;
                    }

                }
                string message = "Done loading the dictionary!";
                string caption = "Dictionary Load";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            else
            {
                string message = "Dictionary does not exist!\n\nPlease Run the search engine first.";
                string caption = "Dictionary Load";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            EnableButtons(true);
        }

        /// <summary>
        /// method to enable or disable buttons
        /// </summary>
        /// <param name="enable"></param>
        private void EnableButtons(bool enable)
        {
            runButton.IsEnabled = enable;
            resetButton.IsEnabled = enable;
            loadDicButton.IsEnabled = enable;
            displayDicButton.IsEnabled = enable;
            browseInputButton.IsEnabled = enable;
            browseOutputButton.IsEnabled = enable;
            exitButton.IsEnabled = enable;
        }

        /// <summary>
        /// method for loading dictionary from disk
        /// </summary>
        /// <param name="path">the path of the dictionary in the disk</param>
        private void Deserialize(string path)
        {
            Dictionary<string, IndexTerm>[] dictionaries = null;
            FileStream fs = new FileStream(System.IO.Path.Combine(path, "Dictionary.bin"), FileMode.Open, FileAccess.Read, FileShare.None);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                dictionaries = (Dictionary<string, IndexTerm>[])formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
            model.indexer.dictionaries = dictionaries;
        }

    }
}
