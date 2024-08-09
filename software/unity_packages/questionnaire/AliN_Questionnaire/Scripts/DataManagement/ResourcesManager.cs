using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace AliN.Questionnaire
{
    public class ResourcesManager : Singleton<ResourcesManager>
    {
        public string savingByATitleFilename = "Collected data by Title"; // Add this field to specify filename from Unity Inspector
        public string savingByAIndexFilename = "Collected data by Index"; // Add this field to specify filename from Unity Inspector
        public string participantSubFolderName = "Participant DataLogs";
        public string participantFullReportFilename = "Participant data"; // Add this field to specify filename from Unity Inspector
        public string oldStructuredFilesSubFolderName = "Older Structure Questionnaires";
        public string separatorCharacter = ";";
        [Header("Key Indices for Comparison")]
        [Tooltip("Specify the indices of key values for comparison. For example, to compare the 1st and 3rd values, specify 0 and 2.")]
        public List<int> keyValueIndices;

        public QuestionnaireList questionnaire; // just for demostration and editing purpose

        [HideInInspector]
        public int CheckPointIndex = 1;


        private string dataLogsFolderName = "DataLogs";
        private string inputDataFolderName = "InputData";

        public enum DataPathType
        {
            DataLogs,
            InputData
        }

        Dictionary<string, string> knownExtensions = new Dictionary<string, string>()
{
    {"Questions", ".json"},
    {"Guide To Questions File", ".txt"}
    // Can add more file names and their corresponding extensions as needed
};

        private void Start()
        {
            NewExperiment();
        }

        public void NewExperiment()
        {                     
            CheckPointIndex = 1;
            InitializeDirectories();
            LoadQuestionsListAndCreateNewQuestionnaire();
            CheckFileStructureChangedAndRenameExistingFilesName();
        }

        private void Update()
        {
            // Check if the Alt key is held down
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                // Check if the 'S' key was pressed while Ctrl is held down
                if (Input.GetKeyDown(KeyCode.S))
                {
                    // Call your SaveQuestionnaireData method
                    SaveQuestionnaireAnswerAsNewRecord();
                }

                if (Input.GetKeyDown(KeyCode.U))
                {
                    // Call your SaveQuestionnaireData method
                    SaveOrUpdateQuestionnaireAnswers();
                }
            }
        }

        private void CreateSubfolder(string parentDirectory, string subfolderName)
        {
            string subfolderPath = Path.Combine(parentDirectory, subfolderName);
            if (!Directory.Exists(subfolderPath))
            {
                Directory.CreateDirectory(subfolderPath);
            }
        }

        private void InitializeDirectories()
        {
            string dataLogsPath, inputDataPath;

            dataLogsPath = GetDirectoryPath(DataPathType.DataLogs);
            inputDataPath = GetDirectoryPath(DataPathType.InputData);

            // Create directories if they do not exist
            if (!Directory.Exists(dataLogsPath)) Directory.CreateDirectory(dataLogsPath);
            if (!Directory.Exists(inputDataPath)) Directory.CreateDirectory(inputDataPath);

            // Create subfolders for "Old Structures" and "Participant Data Log"
            CreateSubfolder(dataLogsPath, oldStructuredFilesSubFolderName);
            CreateSubfolder(dataLogsPath, participantSubFolderName);

            // Copy Questions.json from Resources folder if it doesn't exist
            string questionsPath = Path.Combine(inputDataPath, "Questions.json");
            if (!File.Exists(questionsPath))
            {
                // Copy all assets from Resources/InputData to ExperimentData/InputData
                TextAsset[] allTextAssets = Resources.LoadAll<TextAsset>(inputDataFolderName);

                foreach (TextAsset textAsset in allTextAssets)
                {
                    string fileName = textAsset.name;

                    // Append the file extension based on known file names
                    if (knownExtensions.ContainsKey(fileName))
                    {
                        fileName += knownExtensions[fileName];
                    }
                    else
                    {
                        fileName += ".txt";  // default extension if unknown
                    }

                    File.WriteAllText(Path.Combine(inputDataPath, fileName), textAsset.text);
                }

            }
        }

        public QuestionnaireList LoadQuestionsListAndCreateNewQuestionnaire()
        {

            string directory, filePath;

            directory = GetDirectoryPath(DataPathType.InputData);

            filePath = Path.Combine(directory, "Questions.json");
            Debug.Log("Application Path:" + directory);

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                QuestionnaireList questionnaireList = JsonUtility.FromJson<QuestionnaireList>(jsonContent);

                if (questionnaireList.ListOfQuestions.Count > 0)
                {
                    // Creating an array according to the list of questions in json file
                    questionnaire = new QuestionnaireList();

                    // Copy all the materials in the json to a array for public access
                    questionnaire.ListOfQuestions = questionnaireList.ListOfQuestions;

                    for (int i = 0; i < questionnaire.ListOfQuestions.Count; i++)
                    {
                        questionnaire.ListOfQuestions[i].questionIndex = i + 1;
                    }


                    //Debug.Log("Questionnaire list loaded and set in questionnaire Array. length of array is: " + questionnaire.ListOfQuestions.Count);
                    return questionnaire;
                }
                else
                {

                    Debug.LogError("Failed to parse JSON data or Questions list is null.");
                    return null;
                }

            }
            else
            {

                Debug.LogError("Failed to load JSON file.");
                return null;
            }



        }

        // Method to get the number of sets of data in the data log
        public int GetNumberOfSetOfDataInDataLog()
        {
            string directory = GetDirectoryPath(DataPathType.DataLogs);
            string path = Path.Combine(directory, $"{savingByAIndexFilename}.txt");
            int numberofDataLine;
            if (File.Exists(path))
            {
                // Minus one to discount the header row
                numberofDataLine = File.ReadLines(path).Count() - 1;
            }
            else
            {
                numberofDataLine = 0;
            }
            return numberofDataLine;
        }

        // Saving DataLog
        public void SaveQuestionnaireAnswerAsNewRecord()
        {
            // run two times to save sata by indexes and by titles

            string directory = GetDirectoryPath(DataPathType.DataLogs);

            //Save txt file of answers by answer's index
            string pathFileByIndex = Path.Combine(directory, $"{savingByAIndexFilename}.txt");
            Tuple<string, string>[] questionAnswersByIndex = questionnaire.GetQuestionAnswersByIndex();
            CheckFileExistOrIFFirstTimeAddTitle(pathFileByIndex, questionAnswersByIndex);

            //Save txt file of answers by acual answers(words)
            string pathFileByTitle = Path.Combine(directory, $"{savingByATitleFilename}.txt");
            Tuple<string, string>[] questionAnswersByTitle = questionnaire.GetQuestionAnswers();
            CheckFileExistOrIFFirstTimeAddTitle(pathFileByTitle, questionAnswersByTitle);
        }

        private void CheckFileExistOrIFFirstTimeAddTitle( string path, Tuple<string,string>[] questionAnswers)
        {
            // Check if file already exists
            if (File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    // Write answers in a new line
                    for (int i = 0; i < questionAnswers.Length; i++)
                    {
                        sw.Write(questionAnswers[i].Item2);
                        if (i < questionAnswers.Length - 1)
                        {
                            sw.Write(separatorCharacter);
                        }
                    }
                    sw.WriteLine();
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    // Write question indices in the first row
                    for (int i = 0; i < questionAnswers.Length; i++)
                    {
                        sw.Write(questionAnswers[i].Item1);
                        if (i < questionAnswers.Length - 1)
                        {
                            sw.Write(separatorCharacter);
                        }
                    }
                    sw.WriteLine();

                    // Write answers in the second row
                    for (int i = 0; i < questionAnswers.Length; i++)
                    {
                        sw.Write(questionAnswers[i].Item2);
                        if (i < questionAnswers.Length - 1)
                        {
                            sw.Write(separatorCharacter);
                        }
                    }
                    sw.WriteLine();
                }

                Debug.Log($"Questionnaire data saved successfully to {path}");
            }
        }

        // Saveing Questionnaire
        public void SaveOrUpdateQuestionnaireAnswers()
        {
            string directory = GetDirectoryPath(DataPathType.DataLogs);

            //Save txt file of answers by answer's index
            string pathFileByIndex = Path.Combine(directory, $"{savingByAIndexFilename}.txt");
            Tuple<string, string>[] questionAnswersByIndex = questionnaire.GetQuestionAnswersByIndex();
            AddOrReplaceDataToFile(pathFileByIndex, questionAnswersByIndex);

            //Save txt file of answers by acual answers(words)
            string pathFileByTitle = Path.Combine(directory, $"{savingByATitleFilename}.txt");
            Tuple<string, string>[] questionAnswersByTitle = questionnaire.GetQuestionAnswers();
            AddOrReplaceDataToFile(pathFileByTitle, questionAnswersByTitle);

            // Save as JSON
            SaveQuestionnaireAsJSON();
        }

        //private void AddOrReplaceDataToFile(string path, Tuple<string, string>[] questionAnswers)
        //{
        //    List<string> allLines = new List<string>();

        //    // Read existing data and add to allLines list
        //    if (!File.Exists(path))
        //    {
        //        SaveQuestionnaireAnswerAsNewRecord();
        //    }
        //    else
        //    {
        //        allLines = new List<string>(File.ReadAllLines(path));
        //        string newDataLine = string.Join(separatorCharacter, questionAnswers.Select(x => x.Item2));

        //        // Replace or add new data line based on ID
        //        int lastIndexFound = -1;
        //        for (int i = 0; i < allLines.Count; i++)
        //        {
        //            string[] existingData = allLines[i].Split(new string[] { separatorCharacter }, StringSplitOptions.None);
        //            if (existingData.Length > 0 && existingData[0] == questionAnswers[0].Item2)
        //            {
        //                lastIndexFound = i;  // Store index of the last line found with the same ID
        //            }
        //        }

        //        // Replace last occurrence or add if ID not found
        //        if (lastIndexFound != -1)
        //        {
        //            allLines[lastIndexFound] = newDataLine;  // Replace existing line with last found ID
        //        }
        //        else
        //        {
        //            allLines.Add(newDataLine);  // ID not found, add a new line
        //        }

        //        // Save the modified or new data back to file
        //        File.WriteAllLines(path, allLines);

        //        Debug.Log($"Questionnaire data updated successfully to {path}");
        //    }
        //}

        private void AddOrReplaceDataToFile(string path, Tuple<string, string>[] questionAnswers)
        {
            List<string> allLines = new List<string>();

            if (!File.Exists(path))
            {
                SaveQuestionnaireAnswerAsNewRecord();
                return;
            }

            allLines = new List<string>(File.ReadAllLines(path));
            string newDataLine = string.Join(separatorCharacter, questionAnswers.Select(x => x.Item2));

            int lastIndexFound = -1;
            for (int i = 0; i < allLines.Count; i++)
            {
                string[] existingData = allLines[i].Split(new string[] { separatorCharacter }, StringSplitOptions.None);

                // Check if the data matches based on keyValueIndices
                bool allKeysMatch = true;
                foreach (int index in keyValueIndices)
                {
                    if (index >= existingData.Length || existingData[index] != questionAnswers[index].Item2)
                    {
                        allKeysMatch = false;
                        break;
                    }
                }

                if (allKeysMatch)
                {
                    lastIndexFound = i;  // Store index of the last line found with matching keys
                }
            }

            if (lastIndexFound != -1)
            {
                allLines[lastIndexFound] = newDataLine;  // Replace existing line with last found keys
            }
            else
            {
                allLines.Add(newDataLine);  // Keys not found, add a new line
            }

            File.WriteAllLines(path, allLines);

            Debug.Log($"Questionnaire data updated successfully to {path}");
        }

        public void SaveQuestionnaireAsJSON()
        {
            string directory = GetDirectoryPath(DataPathType.DataLogs);
            string participantDataLogSubfolder = Path.Combine(directory, participantSubFolderName);

            string prefix = "";
            foreach (int index in keyValueIndices)
            {
                if (index < questionnaire.ListOfQuestions.Count) // Ensure we don't go out of bounds
                {
                    if (questionnaire.ListOfQuestions[index].AnswerInText.Length > 0)
                    {
                        prefix += questionnaire.ListOfQuestions[index].AnswerInText.ToString() + "_";
                    }else
                    {                     
                        if (questionnaire.ListOfQuestions[index].SelectedAnswers.Count > 0) // prevent error when I test and there is no Saved Value for KeyValues
                        {
                            int lastAnswerIndex = questionnaire.ListOfQuestions[index].SelectedAnswers.Count - 1;
                            Debug.Log("Index: " + index + "   LastAnswerIndex: " + lastAnswerIndex);
                            prefix += questionnaire.ListOfQuestions[index].SelectedAnswers[lastAnswerIndex].ToString() + "_";
                        }                      
                    }                   
                }
            }

            string filePath = Path.Combine(participantDataLogSubfolder, $"{prefix}{participantFullReportFilename}.json");


            string jsonContent = JsonUtility.ToJson(questionnaire, true);  // Convert the Questionnaire to JSON
            File.WriteAllText(filePath, jsonContent);  // Write JSON to file
            questionnaire.ListOfQuestions[0].ToString();

            Debug.Log($"Questionnaire data saved successfully to {filePath}");
        }

        private string GetDirectoryPath(DataPathType pathType)
        {
            string directory;
            string folderName = pathType == DataPathType.DataLogs ? dataLogsFolderName : inputDataFolderName;

            if (Application.isEditor)
            {
                directory = Path.Combine(Application.dataPath, "AliN_Questionnaire/Resources/", folderName);
            }
            else
            {
                directory = Path.Combine(Application.dataPath, "AliN_Questionnaire/ExperimentData/", folderName);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }


        public QuestionnaireList UpdateQuestionIndexes(QuestionnaireList questions)
        {
            for (int i = 0; i < questions.ListOfQuestions.Count; i++)
            {
                questions.ListOfQuestions[i].questionIndex = i + 1; // Index starts at 1, not 0         
            }

            // You could call your JSON saving method here if you like
            SaveQuestionnaireListToJSON(questions);
            return questions;
        }

        public void SaveQuestionnaireListToJSON(QuestionnaireList questionnaireList)
        {
            string directory, filePath;

            if (Application.isEditor)
            {
                //directory = Path.Combine(Application.dataPath, "Resources/", inputDataFolderName);
                directory = GetDirectoryPath(DataPathType.InputData);
            }
            else
            {
                //directory = Path.Combine(Application.dataPath, "ExperimentData/", inputDataFolderName);
                directory = GetDirectoryPath(DataPathType.InputData);
            }

            filePath = Path.Combine(directory, "Questions.json");

            // Convert the QuestionnaireList to JSON
            string jsonContent = JsonUtility.ToJson(questionnaireList, true);

            // Write JSON to file
            File.WriteAllText(filePath, jsonContent);

            Debug.Log($"QuestionnaireList data saved successfully to {filePath}");
        }

        private void CheckFileStructureChangedAndRenameExistingFilesName()
        {
            try
            {
                string directory = GetDirectoryPath(DataPathType.DataLogs);
                string oldStructuresSubfolder = Path.Combine(directory, oldStructuredFilesSubFolderName);
                string participantDataLogSubfolder = Path.Combine(directory, participantSubFolderName);


                // Paths for both txt files and JSON file
                string indexPath = Path.Combine(directory, $"{savingByAIndexFilename}.txt");
                string titlePath = Path.Combine(directory, $"{savingByATitleFilename}.txt");
                string jsonFilePath = Path.Combine(participantDataLogSubfolder, $"{participantFullReportFilename}.json");

              

                var questionAnswers = questionnaire.GetQuestionAnswersByIndex();
                string newLabelsLine = string.Join(separatorCharacter, questionAnswers.Select(x => x.Item1));
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HH-mm");

                // Check and rename the index file
                RenameFileIfStructureChanged(indexPath, Path.Combine(oldStructuresSubfolder, $"{savingByAIndexFilename} changed structure on {timestamp}.txt"), newLabelsLine);

                // Check and rename the title file
                RenameFileIfStructureChanged(titlePath, Path.Combine(oldStructuresSubfolder, $"{savingByATitleFilename} changed structure on {timestamp}.txt"), newLabelsLine);

                // Check and rename the JSON file
                RenameFileIfStructureChanged(jsonFilePath, Path.Combine(participantDataLogSubfolder, $"{participantFullReportFilename} changed structure on {timestamp}.json"), newLabelsLine);
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while checking and renaming files: " + ex.Message);
            }
        }

        private void RenameFileIfStructureChanged(string currentFilePath, string newFilePath, string newLabelsLine)
        {
            try
            {
                if (File.Exists(currentFilePath))
                {
                    string[] existingLines = File.ReadAllLines(currentFilePath);
                    if (existingLines.Length > 0 && existingLines[0] != newLabelsLine)
                    {
                        File.Move(currentFilePath, newFilePath);
                        Debug.LogWarning("File changed :  " + currentFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while renaming the file: " + ex.Message);
            }
        }
    }
}


