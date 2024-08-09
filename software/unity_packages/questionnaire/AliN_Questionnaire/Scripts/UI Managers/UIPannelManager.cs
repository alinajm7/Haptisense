using AliN.Microcontroller;
using UnityEngine;
using UnityEngine.UI;


namespace AliN.Questionnaire
{
    public class UIPanelManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] panels;
        [SerializeField] private float transitionDuration = 1.0f;
        [SerializeField] private bool firstPanelChangeByTime = false;
        [SerializeField] private float firstPanelShowTime = 3.0f;

        private int currentPanelIndex = 0;

        private void Awake()
        {
            CheckAndAddGraphicRaycaster();
            InitializePanels();
        }

        private void CheckAndAddGraphicRaycaster()
        {
            if (!GetComponent<GraphicRaycaster>())
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void InitializePanels()
        {
            foreach (GameObject panel in panels)
            {
                if (!panel.TryGetComponent(out CanvasGroup canvasGroup))
                {
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0;
                panel.SetActive(false);
            }

            if (panels.Length > 0)
            {
                panels[currentPanelIndex].SetActive(true);
                panels[currentPanelIndex].GetComponent<CanvasGroup>().alpha = 1;
                if (firstPanelChangeByTime) StartCoroutine(ShowFirstPanel());
            }
        }

        private System.Collections.IEnumerator ShowFirstPanel()
        {
            CanvasGroup tmp = panels[currentPanelIndex].GetComponent<CanvasGroup>();

            tmp.alpha = 0;

            float startTime = Time.time;
            float elapsedTime = 0;

            // Fade out the current panel
            while (elapsedTime < transitionDuration)
            {
                float normalizedTime = elapsedTime / transitionDuration;
                tmp.alpha = Mathf.Lerp(0, 1, normalizedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(firstPanelShowTime);

            ShowNextPanel();
        }

        public void ShowNextPanel()
        {
            int nextPanelIndex = (currentPanelIndex + 1) % panels.Length;
            StartCoroutine(TransitionPanels(currentPanelIndex, nextPanelIndex));
        }

        public void ShowPreviousPanel()
        {
            int previousPanelIndex = (currentPanelIndex - 1 + panels.Length) % panels.Length;
            StartCoroutine(TransitionPanels(currentPanelIndex, previousPanelIndex));
        }

        public void ShowPanelByIndex(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= panels.Length)
            {
                Debug.LogWarning("Invalid panel index provided.");
                return;
            }

            if (targetIndex == currentPanelIndex)
            {
                Debug.Log("Already on the target panel.");
                return;
            }

            StartCoroutine(TransitionPanels(currentPanelIndex, targetIndex));
        }

        private System.Collections.IEnumerator TransitionPanels(int fromIndex, int toIndex)
        {
            GameObject currentPanel = panels[fromIndex];
            GameObject nextPanel = panels[toIndex];

            CanvasGroup currentCanvasGroup = currentPanel.GetComponent<CanvasGroup>();
            CanvasGroup nextCanvasGroup = nextPanel.GetComponent<CanvasGroup>();

            float startTime = Time.time;
            float elapsedTime = 0;

            // Activate and fade in the next panel
            nextPanel.SetActive(true);

            // Fade out the current panel
            while (elapsedTime < transitionDuration)
            {
                float normalizedTime = elapsedTime / transitionDuration;
                currentCanvasGroup.alpha = Mathf.Lerp(1, 0, normalizedTime);
                nextCanvasGroup.alpha = Mathf.Lerp(0, 1, normalizedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            currentCanvasGroup.alpha = 0;
            currentPanel.SetActive(false);

            nextCanvasGroup.alpha = 1;

            currentPanelIndex = toIndex;
        }

        public void ResetForNewExperiment()
        {
            ResourcesManager.Instance.NewExperiment();
            
        }
        public void DestryAllSingletonsObjects() 
        {
            Destroy(MicrocontrollerCommunicationManager.Instance.gameObject);
            Destroy(ResourcesManager.Instance.gameObject);
            Destroy(QuestionnaireEditor.Instance.gameObject);
        }
    }
}
