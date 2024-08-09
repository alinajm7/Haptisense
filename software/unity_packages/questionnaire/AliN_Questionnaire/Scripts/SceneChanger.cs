using UnityEngine;
using UnityEngine.SceneManagement;
using AliN.Questionnaire;
using AliN.Microcontroller;

public class SceneChanger : MonoBehaviour
{
    [Header("Criteria, if there is any")]
    public  QuestionUIGenerator checkingComponent;

 
    // Method to change to a specified scene by name
    public void ChangeSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Method to change to a specified scene by index
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Method to navigate to the next scene by index
    public void NextScene_Check_Save_Data()
    {
        // Save data
        OnlySaveAnswersToQuestionnair();

        if (checkingComponent != null)
        {
            checkingComponent.CheckAllTheDisplayedMandatoryQuestionsAreAnswered();
        }

        // Check the boolean property from the component
        if (checkingComponent == null || checkingComponent.allAnswered_CanProceed)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // Loop back to the first scene if at the last scene
            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                nextSceneIndex = 0;
            }

            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Cannot proceed to the next scene.");
        }
    }

    public void OnlySaveAnswersToQuestionnair()
    {
        // Save data
        ResourcesManager.Instance.SaveOrUpdateQuestionnaireAnswers();
    }

    // Method to navigate to the Next scene by index
    public void OnlyGoToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Loop back to the first scene if at the last scene
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    // Method to go to a specified scene by index
    public void GoToSceneByIndex(int sceneIndex)
    {
        //save data
        OnlySaveAnswersToQuestionnair();

        // Check if the specified scene index is valid
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("Invalid scene index. Scene not found.");
        }
    }

    // Method to navigate to the previous scene by index
    public void PreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        // Loop back to the last scene if at the first scene
        if (previousSceneIndex < 0)
        {
            previousSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        }

        SceneManager.LoadScene(previousSceneIndex);
    }

    public void FinishExperimentActions()
    {
        // Save data
        ResourcesManager.Instance.SaveOrUpdateQuestionnaireAnswers();
        MicrocontrollerCommunicationManager.Instance.CloseTheOpenPort();
    }
}
