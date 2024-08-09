using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AliN.Questionnaire
{
    [Serializable]
    public class Questionnaire
    {
        //Question
        [Header("Data Label")]
        public string questionLabel;
        [Header("Question Number")]
        public int questionIndex;
        [Header("Category")]
        public string questionCategory;
        public string questionText;
        [Header("Response Settings")]
        public List<string> optionalAnswers;
        public bool mandatory;
        public bool acceptMultipleChoice;
        public bool showinDropDown;
        public bool automatedDataCollection;

        [HideInInspector]
        //TimeStamps
        [Header("Timestamps")]
        public List<long> timestampShowingTheQuestion;
        [HideInInspector]
        public List<long> timestampAnsweringTheQuestion;
        [HideInInspector]
       
        
        //Answers
        [Header("Answers")]
        //public List<string> SelectedAnswers;
        //[HideInInspector]
        //public List<int> SelectedAnswersIndex;
        //[HideInInspector]
        //public string AnswerInText;
        [SerializeField]
        private List<string> _selectedAnswers;
        //[HideInInspector]
        public List<string> SelectedAnswers
        {
            get { return _selectedAnswers; }
            set
            {
                if (!Enumerable.SequenceEqual(_selectedAnswers, value))
                {
                    _selectedAnswers = value;
                    AddTimeStampToAnsweringQuestion();
                }
            }
        }
        [SerializeField]
        private List<int> _selectedAnswersIndex;
        //[HideInInspector]
        public List<int> SelectedAnswersIndex
        {
            get { return _selectedAnswersIndex; }
            set
            {
                if (!Enumerable.SequenceEqual(_selectedAnswersIndex, value))
                {
                    _selectedAnswersIndex = value;
                    AddTimeStampToAnsweringQuestion();
                }
            }
        }
        [SerializeField]
        private string _answerInText;
        //[HideInInspector]
        public string AnswerInText
        {
            get { return _answerInText; }
            set
            {
                if (_answerInText != value)
                {
                    _answerInText = value;
                    AddTimeStampToAnsweringQuestion();
                }
            }
        }

        public void AddTimeStampToAnsweringQuestion()
        {
            long timeInMilliSecFromStartExperiment = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            timestampAnsweringTheQuestion.Add(timeInMilliSecFromStartExperiment);
        }
        public void AddTimeStampToShowingQuestion()
        {
            long timeInMilliSecFromStartExperiment = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            timestampShowingTheQuestion.Add(timeInMilliSecFromStartExperiment);
        }
    }

    [Serializable]
    public class QuestionnaireList
    {
        public List<Questionnaire> ListOfQuestions;

        public Tuple<string, string>[] GetQuestionAnswers()
        {
            Tuple<string, string>[] result = new Tuple<string, string>[ListOfQuestions.Count];

            for (int i = 0; i < ListOfQuestions.Count; i++)
            {
                Questionnaire question = ListOfQuestions[i];
                string answer;
                if (question.optionalAnswers.Count > 0)
                {
                    answer = question.SelectedAnswers.Count > 0 ? string.Join(", ", question.SelectedAnswers) : "-";
                }
                else
                {
                    answer = !string.IsNullOrEmpty(question.AnswerInText) ? question.AnswerInText : "-";
                }

                result[i] = new Tuple<string, string>(question.questionLabel, answer);
            }

            return result;
        }

        public Tuple<string, string>[] GetQuestionAnswersByIndex()
        {
            Tuple<string, string>[] result = new Tuple<string, string>[ListOfQuestions.Count];

            for (int i = 0; i < ListOfQuestions.Count; i++)
            {
                var question = ListOfQuestions[i];
                string answer;
                if (question.optionalAnswers.Count > 0)
                {
                    answer = question.SelectedAnswersIndex.Count > 0 ? string.Join(", ", question.SelectedAnswersIndex) : "-";

                    //exception: If question label is Birth Year then index should be equal to the year number no the number of value in the list
                    if (question.questionLabel == "Birth Year")
                    {
                        answer = question.SelectedAnswers.Count > 0 ? string.Join(", ", question.SelectedAnswers) : "-";
                    }

                }
                else
                {
                    answer = !string.IsNullOrEmpty(question.AnswerInText) ? question.AnswerInText : "-";
                }

                result[i] = new Tuple<string, string>(question.questionLabel, answer);
            }

            return result;
        }

    }
}




