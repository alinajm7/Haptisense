using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

namespace AliN.Questionnaire
{
    public class QuestionUIGenerator : MonoBehaviour
    {
        GameObject questionPrefab;
        GameObject questionTextPrefab;
        GameObject mandatoryAnswerMessagePrefab;
        GameObject answersPrefab;
        GameObject answersHorizontalLayoutPrefab;
        GameObject inputFieldAnswerPrefab;
        GameObject dropDownAnswerPrefab;
        GameObject togglePrefab;

        [Header("UI Manager Object")]
        public UIPanelManager UIManager;
        public Button buttonNext;
        public Button buttonBack;

        [Header("Info")]
        // [HideInInspector]

        public bool allAnswered_CanProceed = false;

        [Header("Question Numbers. starts from 1")]
        public int fromIndex = 1;
        public int toIndex = 10;

        [Header("Message Settings")]
        public string mandatoryMessage = "*";
        public string mandatoryEmptyMessage = "Please answer this question to continue.";



        [Header("Settings")]
        public bool showNumberOfQuestions = true;
        public bool redNotAnsweredQuestions = false;

        private Color originalQuestionTextColor;


        // private bool mandatoryQuestionsAnswered = false;
        //private Dictionary<int, Answer> collectedAnswers = new Dictionary<int, Answer>();

        private void Awake()
        {
            questionPrefab = Resources.Load<GameObject>("UIPrefabs/QuestionPrefab");
            questionTextPrefab = Resources.Load<GameObject>("UIPrefabs/QuestionTextPrefab");
            mandatoryAnswerMessagePrefab = Resources.Load<GameObject>("UIPrefabs/MandatoryAnswerMessagePrefab");
            answersPrefab = Resources.Load<GameObject>("UIPrefabs/AnswersPrefab");
            answersHorizontalLayoutPrefab = Resources.Load<GameObject>("UIPrefabs/AnswersHorizontalLayoutPrefab");
            inputFieldAnswerPrefab = Resources.Load<GameObject>("UIPrefabs/InputFieldAnswerPrefab");
            dropDownAnswerPrefab = Resources.Load<GameObject>("UIPrefabs/DropDownAnswerPrefab");
            togglePrefab = Resources.Load<GameObject>("UIPrefabs/TogglePrefab");

            originalQuestionTextColor = questionTextPrefab.GetComponent<TextMeshProUGUI>().color;

            CheckButtonAreassaigned();
        }
        private void Start()
        {
            CheckUIManagerIsAssigned();
        }

        private void CheckUIManagerIsAssigned()
        {
            // Check if UIPanelManager is not assigned
            if (UIManager == null)
            {
                // Find the object with UIPanelManager component in the scene
                UIManager = GameObject.FindObjectOfType<UIPanelManager>();

                // Check if found
                if (UIManager == null)
                {
                    Debug.LogError("No UIPanelManager found in the scene.");
                }
                else
                {
                    Debug.Log("UIPanelManager found and assigned.");
                }
            }
        }

        private void CheckButtonAreassaigned()
        {
            // Check if goNextPanelButton is assigned
            if (buttonNext != null)
            {
                buttonNext.onClick.AddListener(GoToNextPannel_AfterCheckingAnswers);
            }
            else
            {
                Debug.LogWarning("Button for going Next not assigned automatically.");
            }

            // Check if goPreviousPanelButton is assigned
            if (buttonBack != null)
            {
                buttonBack.onClick.AddListener(GoToPreviousPannel);
            }
            else
            {
                Debug.LogWarning("Button for going back not assigned automatically.");
            }
        }

        public void OnEnable()
        {
            GenerateUI();
        }

        public void GenerateUI()
        {
            int numberOfQuestions = ResourcesManager.Instance.questionnaire.ListOfQuestions.Count;
            if (numberOfQuestions > 0)
            {
                if (fromIndex > 0 && toIndex > 0)
                {
                    if (fromIndex > numberOfQuestions || toIndex > numberOfQuestions)
                    {
                        Debug.LogWarning("Selected range of question is invalid : Question index is bigger than total number of questions");
                        fromIndex = numberOfQuestions;
                        toIndex = numberOfQuestions;
                    }

                    ClearUI();

                    for (int i = fromIndex - 1; i <= toIndex - 1 && i < ResourcesManager.Instance.questionnaire.ListOfQuestions.Count; i++)
                    {
                        int tmpIndex = i;
                        Questionnaire questionData = ResourcesManager.Instance.questionnaire.ListOfQuestions[i];

                        //Add Timestamp when showing the question
                        ResourcesManager.Instance.questionnaire.ListOfQuestions[tmpIndex].AddTimeStampToShowingQuestion();


                        if (!questionData.automatedDataCollection)
                        {
                            GameObject questionObject = Instantiate(questionPrefab, this.transform);
                            questionObject.name = "Question" + (i + 1);

                            TextMeshProUGUI questionText = Instantiate(questionTextPrefab, questionObject.transform).GetComponent<TextMeshProUGUI>();
                            TextMeshProUGUI mandatoryMessageText = Instantiate(mandatoryAnswerMessagePrefab, questionText.transform).GetComponent<TextMeshProUGUI>();

                            questionText.text = showNumberOfQuestions ? (i + 1).ToString() + " . " + questionData.questionText : questionData.questionText;


                            if (questionData.mandatory)
                            {
                                mandatoryMessageText.enabled = true;
                                mandatoryMessageText.text = mandatoryMessage;
                                // mandatoryQuestionsAnswered = false; // Set to false when a new mandatory question is encountered

                            }
                            else
                            {
                                mandatoryMessageText.enabled = false;
                            }

                            GameObject answersObject = Instantiate(answersPrefab, questionObject.transform);
                            HorizontalLayoutGroup answersLayout = Instantiate(answersHorizontalLayoutPrefab, answersObject.transform).GetComponent<HorizontalLayoutGroup>();

                            //Create Input Text Field
                            if (questionData.optionalAnswers.Count == 0)
                            {
                                GameObject inputField = Instantiate(inputFieldAnswerPrefab, answersLayout.transform);
                                TMP_InputField input = inputField.GetComponent<TMP_InputField>();

                                input.onValueChanged.AddListener(delegate { OnInputValueChanged(tmpIndex, input); });

                                if (!string.IsNullOrEmpty(questionData.AnswerInText) )
                                {
                                    input.text = questionData.AnswerInText;
                                    // mandatoryQuestionsAnswered = true;
                                }
                            }
                            else //Create DropDown or Toggle
                            {
                                if (questionData.showinDropDown) //Create DropDown 
                                {
                                    GameObject dropDownObject = Instantiate(dropDownAnswerPrefab, answersLayout.transform);
                                    TMP_Dropdown dropdown = dropDownObject.GetComponent<TMP_Dropdown>();

                                    dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(tmpIndex, dropdown); });

                                    dropdown.ClearOptions();
                                    dropdown.AddOptions(new List<string> { "Select an option" });
                                    dropdown.AddOptions(questionData.optionalAnswers);
                                    dropdown.value = 0;  // Select the placeholder

                                    

                                    // Check if there are selected answers for this question
                                    if (questionData.SelectedAnswersIndex.Count > 0 )
                                    {
                                        int selectedOptionIndex = questionData.SelectedAnswersIndex[0];
                                        dropdown.value = selectedOptionIndex;
                                        //  mandatoryQuestionsAnswered = true;
                                    }
                                }
                                else  //Create Toggles 
                                {
                                    foreach (string answerOption in questionData.optionalAnswers)
                                    {
                                        Toggle toggle = Instantiate(togglePrefab, answersLayout.transform).GetComponent<Toggle>();

                                        //Add listener for TimeStamp when use Answer
                                        toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(tmpIndex, toggle); });

                                        toggle.GetComponentInChildren<TextMeshProUGUI>().text = answerOption;

                                        // Check if there are selected answers for this question
                                        if (questionData.SelectedAnswersIndex.Contains((toggle.transform.GetSiblingIndex()) + 1) )
                                        {
                                            toggle.isOn = true;
                                            //  mandatoryQuestionsAnswered = true;
                                        }

                                        if (!questionData.acceptMultipleChoice)
                                        {
                                            toggle.group = answersLayout.GetComponent<ToggleGroup>();
                                        }
                                    }
                                }
                            }
                            // if (allMandatoryQuestionsAnswered) allMandatoryQuestionsAnswered = mandatoryQuestionsAnswered;
                        }
                    }
                }
                else { Debug.LogWarning("Selected range of question is invalid : Question index can not be 0 or negetive value"); }
            }
            else { Debug.LogError("Resource manager Not found in the scene"); }
        }

        public bool CheckAllTheDisplayedMandatoryQuestionsAreAnswered()
        {
            bool allMandatoryQuestionsAnswered = true;
            bool mandatoryQuestionsAnswered = false;

            foreach (Transform questionObject in this.transform)
            {
                int questionIndex;
                if (int.TryParse(questionObject.name.Replace("Question", ""), out questionIndex))
                {
                    questionIndex--;

                    if (ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex].mandatory)
                    {
                        TMP_Dropdown dropdown = questionObject.GetComponentInChildren<TMP_Dropdown>();
                        TMP_InputField input = questionObject.GetComponentInChildren<TMP_InputField>();
                        HorizontalLayoutGroup horizontalLayoutGroup = questionObject.GetComponentInChildren<HorizontalLayoutGroup>();
                        TextMeshProUGUI questionText = questionObject.GetComponentInChildren<TextMeshProUGUI>();
                        Transform mandatoryMessageTransform = questionObject.transform.Find("QuestionTextPrefab(Clone)/MandatoryAnswerMessagePrefab(Clone)");

                        // Check dropdown case
                        if (dropdown != null)
                        {
                            bool isAnswerEmpty = dropdown.value == 0 ? true : false;
                            if (redNotAnsweredQuestions) questionText.color = isAnswerEmpty ? Color.red : originalQuestionTextColor;
                            if (mandatoryMessageTransform != null)
                            {
                                TextMeshProUGUI textMeshPro = mandatoryMessageTransform.GetComponent<TextMeshProUGUI>();

                                if (textMeshPro != null)
                                {
                                    mandatoryQuestionsAnswered = isAnswerEmpty ? false : true;
                                    textMeshPro.text = isAnswerEmpty ? mandatoryEmptyMessage : mandatoryMessage;
                                    textMeshPro.enabled = isAnswerEmpty;
                                }
                            }

                        }
                        // Handle input field case
                        else if (input != null)
                        {
                            bool isAnswerEmpty = string.IsNullOrEmpty(input.text);

                            if (redNotAnsweredQuestions) questionText.color = isAnswerEmpty ? Color.red : originalQuestionTextColor;

                            if (mandatoryMessageTransform != null)
                            {
                                TextMeshProUGUI textMeshPro = mandatoryMessageTransform.GetComponent<TextMeshProUGUI>();
                                if (textMeshPro != null)
                                {
                                    mandatoryQuestionsAnswered = isAnswerEmpty ? false : true;
                                    textMeshPro.text = isAnswerEmpty ? mandatoryEmptyMessage : mandatoryMessage;
                                    textMeshPro.enabled = isAnswerEmpty;
                                }
                            }

                        }
                        // Handle toggle case
                        else if (horizontalLayoutGroup != null)
                        {
                            bool hasSelectedAnswers = ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex].SelectedAnswers.Count > 0;
                            if (redNotAnsweredQuestions) questionText.color = hasSelectedAnswers ? originalQuestionTextColor : Color.red;
                            if (mandatoryMessageTransform != null)
                            {
                                TextMeshProUGUI textMeshPro = mandatoryMessageTransform.GetComponent<TextMeshProUGUI>();
                                if (textMeshPro != null)
                                {
                                    mandatoryQuestionsAnswered = hasSelectedAnswers ? true : false;
                                    textMeshPro.text = hasSelectedAnswers ? mandatoryMessage : mandatoryEmptyMessage;
                                    textMeshPro.enabled = !hasSelectedAnswers;
                                }
                            }
                        }
                    }

                    if (allMandatoryQuestionsAnswered) allMandatoryQuestionsAnswered = mandatoryQuestionsAnswered;
                }
            }
            allAnswered_CanProceed = allMandatoryQuestionsAnswered;
            return allMandatoryQuestionsAnswered;
        }



        public void ClearUI()
        {

            foreach (Transform child in this.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void GoToNextPannel_AfterCheckingAnswers()
        {
            CheckAllTheDisplayedMandatoryQuestionsAreAnswered();
            if (CheckAllTheDisplayedMandatoryQuestionsAreAnswered())
            {
                UIManager.ShowNextPanel();
            }
        }
        public void GoToPreviousPannel()
        {
            CheckAllTheDisplayedMandatoryQuestionsAreAnswered();
            UIManager.ShowPreviousPanel();
        }

        public void SetTimestampForAnswerOfThisQuestion(int currentQuestionIndex)
        {
                ResourcesManager.Instance.questionnaire.ListOfQuestions[currentQuestionIndex].AddTimeStampToAnsweringQuestion();
        }

        public void SetFixValueForQuestionUnixTimestampDate(int questionIndex)
        {
            if (ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex].automatedDataCollection)
            {

                // Capture the start time of the experiment in UTC
                DateTimeOffset utcStartTime = DateTimeOffset.UtcNow;

                // Convert UTC to local time
                DateTimeOffset localStartTime = utcStartTime.ToLocalTime();
                string isoDate = localStartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AnswerInText = isoDate;
                // Capture Unix timestamp in milliseconds
                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AddTimeStampToAnsweringQuestion();
            }
            else
            {
                Debug.LogWarning("Question " + questionIndex + " is not set to automatedDataCollection");
            }
        }
        public void SetFixValueForQuestionUnixTimestampTimeMilliSec(int questionIndex)
        {
            if (ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].automatedDataCollection)
            {

                // Capture the start time of the experiment in UTC
                DateTimeOffset utcStartTime = DateTimeOffset.UtcNow;

                // Convert UTC to local time
                DateTimeOffset localStartTime = utcStartTime.ToLocalTime();
                string isoTimeWithMilliseconds = localStartTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AnswerInText = isoTimeWithMilliseconds;

                // Capture Unix timestamp in milliseconds
                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AddTimeStampToAnsweringQuestion();
            }
            else
            {
                Debug.LogWarning("Question " + questionIndex + " is not set to automatedDataCollection");
            }
        }


        public void SetFixValueForQuestionTextCheckPoint(int questionIndex)
        {
            if (ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].automatedDataCollection)
            {
                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AnswerInText = "CheckPoint " + ResourcesManager.Instance.CheckPointIndex.ToString("00");

                ResourcesManager.Instance.CheckPointIndex++;

                // Capture Unix timestamp in milliseconds
                ResourcesManager.Instance.questionnaire.ListOfQuestions[questionIndex - 1].AddTimeStampToAnsweringQuestion();

            }
            else
            {
                Debug.LogWarning("Question " + (questionIndex - 1) + " is not set to automatedDataCollection");
            }
        }

        public void OnToggleValueChanged(int index, Toggle toggle)
        {
            SetTimestampForAnswerOfThisQuestion(index);

            // save Selected Answers and Index 
            if (toggle.isOn)
            {
                ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswers.Add(toggle.GetComponentInChildren<TextMeshProUGUI>().text);
                ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswersIndex.Add((toggle.transform.GetSiblingIndex()) + 1);
            }
            else
            {
                ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswers.Remove(toggle.GetComponentInChildren<TextMeshProUGUI>().text);
                ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswersIndex.Remove((toggle.transform.GetSiblingIndex()) + 1);
            }


            Debug.Log("ToggleValue Saved " + index + "   Selected Items= " + string.Join(",", ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswers) + "   Index: " + string.Join(",", ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswersIndex));
        }

        public void OnDropdownValueChanged(int index, TMP_Dropdown dropDown)
        {
            SetTimestampForAnswerOfThisQuestion(index);

            // save Answers and Index of answers 
            ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswers.Add(dropDown.options[dropDown.value].text);
            ResourcesManager.Instance.questionnaire.ListOfQuestions[index].SelectedAnswersIndex.Add(dropDown.value);

            Debug.Log("DropDownValue Saved " + index + "   Value= " + dropDown.options[dropDown.value].text + "   Index: " + dropDown.value);
        }

        public void OnInputValueChanged(int index, TMP_InputField inputFieldObject)
        {
            SetTimestampForAnswerOfThisQuestion(index);

            // save Answer conetnt
            ResourcesManager.Instance.questionnaire.ListOfQuestions[index].AnswerInText = inputFieldObject.GetComponent<TMP_InputField>().text;

            Debug.Log("inputText Saved " + index + "   Content= " + inputFieldObject.GetComponent<TMP_InputField>().text);

        }

        void OnDisable()
        {
            foreach (Transform questionObject in this.transform)
            {
                // Your existing logic to get the question UI elements
                Toggle toggle = questionObject.GetComponentInChildren<Toggle>();
                TMP_Dropdown dropdown = questionObject.GetComponentInChildren<TMP_Dropdown>();
                TMP_InputField input = questionObject.GetComponentInChildren<TMP_InputField>();

                int tmpIndex;
                if (int.TryParse(questionObject.name.Replace("Question", ""), out tmpIndex))
                {

                    if (toggle != null)
                    {
                        toggle.onValueChanged.RemoveListener(delegate { OnToggleValueChanged(tmpIndex, toggle); });
                    }
                    if (dropdown != null)
                    {
                        dropdown.onValueChanged.RemoveListener(delegate { OnDropdownValueChanged(tmpIndex, dropdown); });
                    }
                    if (input != null)
                    {
                        input.onValueChanged.RemoveListener(delegate { OnInputValueChanged(tmpIndex, input); });
                    }
                }
            }
        }

    }
}
