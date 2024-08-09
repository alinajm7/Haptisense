// Editor/QuestionnaireEditorInspector.cs
    using UnityEditor;
    using UnityEngine;

namespace AliN.Questionnaire
{
#if UNITY_EDITOR
    [CustomEditor(typeof(QuestionnaireEditor))]
    public class QuestionnaireEditorInspector : Editor
    {
        public void Reset()
        {
            QuestionnaireEditor editorTarget = (QuestionnaireEditor)target;
            editorTarget.questions = ResourcesManager.Instance.LoadQuestionsListAndCreateNewQuestionnaire();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            QuestionnaireEditor editorTarget = (QuestionnaireEditor)target;

            if (GUILayout.Button("Load Questions"))
            {
                editorTarget.questions = ResourcesManager.Instance.LoadQuestionsListAndCreateNewQuestionnaire();
            }

            if (GUILayout.Button("Save Changes"))
            {
                ResourcesManager.Instance.SaveQuestionnaireListToJSON(editorTarget.questions);
            }

            if (GUILayout.Button("Update Indexes"))
            {

                editorTarget.questions = ResourcesManager.Instance.UpdateQuestionIndexes(editorTarget.questions);

            }
        }
    }
#endif
}
