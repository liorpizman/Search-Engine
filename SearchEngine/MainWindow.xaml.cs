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
using System.Threading;

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
        public static bool m_invertedIndexIsLoaded = false;

        /// <summary>
        /// constructor of MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            languagesComboBox.Items.Insert(0, "Choose...");
            languagesComboBox.SelectedIndex = 0;
            cityComboBox.Items.Insert(0, new ComboBoxItem(false, "Choose..."));
            cityComboBox.SelectedIndex = 0;
            EntitiesComboBox.Items.Insert(0, "Choose...");
            EntitiesComboBox.SelectedIndex = 0;
            EnablePart2Buttons(false);
            SearchEntitiesButton.IsEnabled = false;
            ResultsCheckBox.SetCurrentValue(System.Windows.Controls.CheckBox.IsCheckedProperty, true);
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
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        inputPathText.Text = fbd.SelectedPath;
                        model.setInputPath(inputPathText.Text);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }

        }

        /// <summary>
        /// method of output button browse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_browse_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        outputPathText.Text = fbd.SelectedPath;
                        model.setOutPutPath(outputPathText.Text);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
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
            try
            {
                EnablePart1Buttons(false);
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
                model.RunIndexing();
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
                EnablePart1Buttons(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method of reset button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                languagesComboBox.Items.Clear();
                languagesComboBox.Items.Insert(0, "Choose...");
                languagesComboBox.SelectedIndex = 0;
                dictionaryListBox.ItemsSource = null;
                model.ClearMemory();
                if (string.IsNullOrEmpty(outputPathText.Text))
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
                GC.Collect();
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method of display button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayDicButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnablePart1Buttons(false);
                dictionaryListBox.ItemsSource = null;
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
                EnablePart1Buttons(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method of load button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadDicButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnablePart1Buttons(false);
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
                            EnablePart1Buttons(true);
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
                            EnablePart1Buttons(true);
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
                EnablePart1Buttons(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to enable or disable buttons
        /// </summary>
        /// <param name="enable"></param>
        private void EnablePart1Buttons(bool enable)
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
            try
            {
                model.indexer.dictionaries = dictionaries;
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// class to represt a choice of a city in the GUI
        /// </summary>
        public class ComboBoxItem
        {
            /// <summary>
            /// fields of ComboBoxItem
            /// </summary>
            public Boolean IsChecked
            { get; set; }

            public String CityName
            { get; set; }

            /// <summary>
            /// Constructor of ComboBoxItem
            /// </summary>
            /// <param name="isChecked">if the item is checked</param>
            /// <param name="cityName">the citys</param>
            public ComboBoxItem(bool isChecked, string cityName)
            {
                this.IsChecked = isChecked;
                this.CityName = cityName;
            }
        }

        /// <summary>
        /// method when Semantic check box is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SemanticCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            model.setSemantic(true);
        }

        /// <summary>
        /// method when Semantic check box is unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SemanticCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            model.setSemantic(false);
        }

        /// <summary>
        /// method to search user's single query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchQueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool validOutput = Directory.Exists(outputQueryPath.Text);
                queryResultsListBox.ItemsSource = null;
                queryResultsListBox.Items.Clear();
                EntitiesListBox.ItemsSource = null;
                EntitiesListBox.Items.Clear();
                if (string.IsNullOrEmpty(inputQueryPath.Text))
                {
                    string message = "Please enter a valid query!";
                    string caption = "No query in input";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else if (string.IsNullOrEmpty(outputQueryPath.Text))
                {
                    string message = "Please enter a valid output path for query results!";
                    string caption = "Invalid outPut Path";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else if (!validOutput)
                {
                    string message = "Please enter a valid OutPut Query Path!";
                    string caption = "Error Detected in OutPut";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else if (!m_invertedIndexIsLoaded)
                {
                    string message = "Please load Inverted Index files!";
                    string caption = "Error Detected in Input";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else
                {
                    if (model._dictionaries != null)
                    {
                        model.setQueryOutPutPath(outputQueryPath.Text);
                        model.RunQuery(inputQueryPath.Text, AddCitiesToFilter());
                        var toShow = new Dictionary<string, double>();
                        ShowSingleQueryResults(toShow, model.m_searcher.query);
                        queryResultsListBox.ItemsSource = toShow;
                        string message = "Done searching for the query!";
                        string caption = "Results have written";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                        updateEntitiesListBox();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to show results of single query
        /// </summary>
        /// <param name="showResults"></param>
        /// <param name="q"></param>
        private void ShowSingleQueryResults(Dictionary<string, double> showResults, Query q)
        {
            try
            {
                var res = q.m_docsRanks.ToDictionary(pair => pair.Key, pair => pair.Value.GetTotalScore());
                res = res.OrderByDescending(j => j.Value).ToDictionary(p => p.Key, p => p.Value);
                int i = 1;
                foreach (string doc in res.Keys)
                {
                    if (i > 50)
                    {
                        break;
                    }
                    showResults.Add("QueryID: " + q.m_ID + ", DocNo : " + doc, res[doc]);
                    i++;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to update entities in the list box
        /// </summary>
        private void updateEntitiesListBox()
        {
            try
            {
                EntitiesComboBox.ItemsSource = null;
                EntitiesComboBox.Items.Clear();
                EntitiesComboBox.Items.Insert(0, "Choose...");
                EntitiesComboBox.SelectedIndex = 0;
                EntitiesListBox.Items.Clear();
                int i = 1;
                var res = model.m_searcher.query.m_docsRanks.OrderByDescending(j => j.Value.GetTotalScore()).ToDictionary(p => p.Key, p => p.Value);
                foreach (string entity in res.Keys)
                {
                    if (i > 50)
                    {
                        break;
                    }
                    EntitiesComboBox.Items.Insert(i++, entity);
                }
                SearchEntitiesButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to open file dialog for get file of queries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browswFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "txt files (*.txt)|*.txt";
                    openFileDialog.Title = "Open Text File";
                    DialogResult result = openFileDialog.ShowDialog();
                    if (result.ToString().Equals("OK"))
                    {
                        inputFileQueryPath.Text = openFileDialog.FileName;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to search user's file of queries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchQueryFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                queryResultsListBox.ItemsSource = null;
                queryResultsListBox.Items.Clear();
                EntitiesListBox.ItemsSource = null;
                EntitiesListBox.Items.Clear();
                bool validInput = File.Exists(inputFileQueryPath.Text);
                bool validOutput = Directory.Exists(outputQueryPath.Text);
                if (!validInput)
                {
                    string message = "Please enter a valid Path of Query File !";
                    string caption = "Error Detected in Input";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else if (!validOutput)
                {
                    string message = "Please enter a valid OutPut Query Path!";
                    string caption = "Error Detected in OutPut";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else if (!m_invertedIndexIsLoaded)
                {
                    string message = "Please load Inverted Index files!";
                    string caption = "Error Detected in Input";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    return;
                }
                else
                {
                    if (model._dictionaries != null)
                    {
                        model.setQueryInputPath(inputFileQueryPath.Text);
                        model.setQueryOutPutPath(outputQueryPath.Text);
                        model.RunFileQueries(inputFileQueryPath.Text, AddCitiesToFilter());
                        var toShow = new Dictionary<string, double>();
                        foreach (Query q in model.m_searcher.m_toShowQueries)
                        {
                            ShowSingleQueryResults(toShow, q);
                        }
                        queryResultsListBox.ItemsSource = toShow;
                        string message = "Done searching for the query File!";
                        string caption = "Results have written";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                        updateEntitiesListBox();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to get the folder for output od query's results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseOutputQueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result.ToString().Equals("OK") && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        outputQueryPath.Text = fbd.SelectedPath;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to clear all results up until now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetQueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                queryResultsListBox.ItemsSource = null;
                queryResultsListBox.Items.Clear();
                EntitiesListBox.ItemsSource = null;
                EntitiesComboBox.Items.Clear();
                EntitiesComboBox.Items.Insert(0, "Choose...");
                EntitiesComboBox.SelectedIndex = 0;
                languagesComboBox.Items.Clear();
                languagesComboBox.Items.Insert(0, "Choose...");
                languagesComboBox.SelectedIndex = 0;
                SearchEntitiesButton.IsEnabled = false;
                if (!Directory.Exists(outputQueryPath.Text))
                {
                    string message2 = "Your output path is invalid. Please Try again!";
                    string caption2 = "Error in reset";
                    MessageBoxButtons buttons2 = MessageBoxButtons.OK;
                    DialogResult result2 = System.Windows.Forms.MessageBox.Show(message2, caption2, buttons2);
                    return;
                }
                string pathResults = System.IO.Path.Combine(outputQueryPath.Text, "results.txt");
                if (File.Exists(pathResults))
                {
                    File.Delete(pathResults);
                }
                string message = "Queries were reset successfuly!";
                string caption = "Reset Search Engine Process";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to load languages
        /// </summary>
        private void LoadLanguages()
        {
            try
            {
                string[] AllLines;
                if (StemmingCheckBox.IsChecked == true)
                {
                    AllLines = File.ReadAllLines(System.IO.Path.Combine(System.IO.Path.Combine(outputPathText.Text, "WithStem"), "Languages.txt"));
                }
                else
                {
                    AllLines = File.ReadAllLines(System.IO.Path.Combine(System.IO.Path.Combine(outputPathText.Text, "WithOutStem"), "Languages.txt"));
                }
                for (int i = 1, j = 0; j < AllLines.Length; i++, j++)
                {
                    languagesComboBox.Items.Insert(i, AllLines[j]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to load cities
        /// </summary>
        private void LoadCitiesToFilter()
        {
            try
            {
                string[] AllLines, cityData;
                List<ComboBoxItem> citiesFilter = new List<ComboBoxItem>();
                if (StemmingCheckBox.IsChecked == true)
                {
                    AllLines = File.ReadAllLines(System.IO.Path.Combine(System.IO.Path.Combine(outputPathText.Text, "WithStem"), "Cities.txt"));
                }
                else
                {
                    AllLines = File.ReadAllLines(System.IO.Path.Combine(System.IO.Path.Combine(outputPathText.Text, "WithOutStem"), "Cities.txt"));
                }
                foreach (string line in AllLines)
                {
                    cityData = line.Split(new string[] { "(#)" }, StringSplitOptions.None);
                    if (cityData.Length >= 1)
                    {
                        citiesFilter.Add(new ComboBoxItem(false, cityData[0]));
                    }
                }
                cityComboBox.ItemsSource = null;
                cityComboBox.Items.Clear();
                cityComboBox.ItemsSource = citiesFilter;
                if (cityComboBox.Items.Count >= 1)
                {
                    cityComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to load all files of inverted index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadInvertedIndexButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                model.m_searcher = null;
                model.ClearDocumentsInformation();
                if (string.IsNullOrEmpty(inputPathText.Text) || string.IsNullOrEmpty(outputPathText.Text))
                {
                    string message1 = "Please fill the input and output paths!";
                    string caption1 = "Error Detected in InPut\\Output";
                    MessageBoxButtons buttons1 = MessageBoxButtons.OK;
                    DialogResult result1 = System.Windows.Forms.MessageBox.Show(message1, caption1, buttons1);
                    return;
                }
                if (!Directory.Exists(inputPathText.Text))
                {
                    string message1 = "The directory in the input is not valid!";
                    string caption1 = "Error Detected in InPut";
                    MessageBoxButtons buttons1 = MessageBoxButtons.OK;
                    DialogResult result1 = System.Windows.Forms.MessageBox.Show(message1, caption1, buttons1);
                    return;
                }
                if (!Directory.Exists(outputPathText.Text))
                {
                    string message1 = "The directory in the output is not valid!";
                    string caption1 = "Error Detected in OutPut";
                    MessageBoxButtons buttons1 = MessageBoxButtons.OK;
                    DialogResult result1 = System.Windows.Forms.MessageBox.Show(message1, caption1, buttons1);
                    return;
                }
                model.setInputPath(inputPathText.Text);
                model.setOutPutPath(outputPathText.Text);
                if (!model.CheckFilesExists(outputPathText.Text))
                {
                    string message1 = "Not all index files are exist in the outPut file.";
                    string caption1 = "Error Detected in Load";
                    MessageBoxButtons buttons1 = MessageBoxButtons.OK;
                    DialogResult result1 = System.Windows.Forms.MessageBox.Show(message1, caption1, buttons1);
                    return;
                }
                model.SetPostingLines();
                LoadCitiesToFilter();
                LoadLanguages();
                model.LoadDictionary();
                model.LoadDocumentsInformation();
                m_invertedIndexIsLoaded = true;
                string message2 = "Load Inverted Index files is done!";
                string caption2 = "Load Finished";
                MessageBoxButtons buttons2 = MessageBoxButtons.OK;
                DialogResult result2 = System.Windows.Forms.MessageBox.Show(message2, caption2, buttons2);
                EnablePart2Buttons(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// method to enable or disable buttons
        /// </summary>
        /// <param name="enable"></param>
        private void EnablePart2Buttons(bool enable)
        {
            searchQueryButton.IsEnabled = enable;
            browswFileButton.IsEnabled = enable;
            searchQueryFileButton.IsEnabled = enable;
            browseOutputQueryButton.IsEnabled = enable;
            resetQueryButton.IsEnabled = enable;
        }

        /// <summary>
        /// method to save results in a txt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            model.setSaveResults(true);
        }

        /// <summary>
        /// method to do not save results in a txt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            model.setSaveResults(false);
        }

        /// <summary>
        /// method to add new cities to filter
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> AddCitiesToFilter()
        {
            try
            {
                Dictionary<string, string> filter = new Dictionary<string, string>();
                foreach (ComboBoxItem city in cityComboBox.ItemContainerGenerator.Items)
                {
                    if (city.IsChecked)
                    {
                        filter.Add(city.CityName, "");
                    }
                }
                return filter;
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// method to show entities of current document to the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchEntitiesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EntitiesComboBox.SelectedIndex == 0)
                {
                    string message2 = "Please Choose Document!";
                    string caption2 = "Invalid Choice";
                    MessageBoxButtons buttons2 = MessageBoxButtons.OK;
                    DialogResult result2 = System.Windows.Forms.MessageBox.Show(message2, caption2, buttons2);
                    EnablePart2Buttons(true);
                    return;
                }
                EntitiesListBox.ItemsSource = null;
                string docno = EntitiesComboBox.SelectedItem.ToString();
                var res = model.m_searcher.docInformation[docno].m_Entities.ToDictionary(pair => pair.Key, pair => pair.Value);
                res = res.OrderByDescending(j => j.Value).ToDictionary(p => p.Key, p => p.Value);
                EntitiesListBox.ItemsSource = res;
            }
            catch (Exception exception)
            {
                Console.WriteLine();
            }

        }
    }
}