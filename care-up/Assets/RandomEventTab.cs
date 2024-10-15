using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using CareUp.Localize;

public class RandomEventTab : MonoBehaviour
{
    public class RandomEventData
    {
        public string idName = "";
        public List<string> groups = new List<string>();
        public string infoTitle = "";
        public string infoText = "";
        public List<QuizTab.Question> quastions = new List<QuizTab.Question>();
    }

    List<RandomEventData> randomEventsData = new List<RandomEventData>();
    QuizTab.Question currentQuastion;
    private EndScoreManager endScoreManager;
    private PlayerPrefsManager manager;
    public GameObject infoPanel;
    public Text infoPanelTitle;
    public Text infoPanelText;
    public GameObject aPanelBackButton;


    public GameObject quastionPanel;
    Text quastionTitle;
    Text quastionDesc;

    public GameObject wrongAnswerPanel;
    Text wrongAnswerPanelTitle;
    Text wrongAnswerPanelText;

    List<GameObject> panels = new List<GameObject>();

    int currentQuastionIndex = 0;
    int currentRandomEventIndex = -1;
    int currentAnswer = -1;
    public GameObject answerPanel;
    List<Button> answerButtons = new List<Button>();
    List<int> shuffledIndexes;
    Animation redPop;
    Animation greenPop;
    bool madeWrongAnswer = false;
    GameUI gameUI;
    ActionManager actionManager;
    // Start is called before the first frame update
    void Start()
    {
        endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
        manager = GameObject.FindObjectOfType<PlayerPrefsManager>(); 
        PlayerScript.randomEventTab = this;
        actionManager = GameObject.FindObjectOfType<ActionManager>();
        gameObject.SetActive(false);
        gameUI = GameObject.FindObjectOfType<GameUI>();
        gameUI.SetRandomEventTab(this);
        for(int i = 0; i < answerPanel.transform.childCount; i++)
        {
            answerButtons.Add(answerPanel.transform.GetChild(i).GetComponent<Button>());
        }
        quastionTitle = quastionPanel.transform.Find("quizElements/Title/title").GetComponent<Text>();
        quastionDesc = quastionPanel.transform.Find("quizElements/answerPanel/descriptionText").GetComponent<Text>();
        wrongAnswerPanelTitle = wrongAnswerPanel.transform.Find("infoElements/infoPanel/Panel/infoTitleText").GetComponent<Text>();
        wrongAnswerPanelText = wrongAnswerPanel.transform.Find("infoElements/infoPanel/Panel/infoTextPanel/ScrollViewDescription/Viewport/Content/Description").GetComponent<Text>();
        redPop = transform.Find("PopAnimation/RedPop").GetComponent<Animation>();
        greenPop = transform.Find("PopAnimation/GreenPop").GetComponent<Animation>();

        panels.Add(infoPanel);
        panels.Add(quastionPanel);
        panels.Add(wrongAnswerPanel);
    }

    public void AnswerButtonClicked(int value)
    {
        currentAnswer = value;
        if (IsAnswerCorrect(value))
        {
            CorrectAnswer(value);
        }
        else
        {
            WrongAnswer(value);
        }
    }

    bool IsAnswerCorrect(int value)
    {
        bool result = false;

        if (shuffledIndexes[value] == randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex].answerID)
        {
            result = true;
        }
        return result;
    }

    public void FinishEvent()
    {
        if (currentRandomEventIndex != -1)
        {
            if (IsAnswerCorrect(currentAnswer))
            {
                if ((currentQuastionIndex + 1) < (randomEventsData[currentRandomEventIndex].quastions.Count))
                {
                    currentQuastionIndex++;
                    madeWrongAnswer = false;
                    BuildQuastionsPanel();
                    SwitchScreen(1);
                }
                else
                {
                    actionManager.RemoveRandomEventIndex(0);
                    randomEventsData.Remove(randomEventsData[currentRandomEventIndex]);
                    currentRandomEventIndex = -1;
                    if (actionManager.currentRandomEventIndices.Count > 0)
                    {
                        NextRandomEvent();
                    }
                    else
                    {
                        gameObject.SetActive(false);
                        PlayerScript.actionsLocked = false;
                    }
                }

            }
            else
            {
                BuildQuastionsPanel();
                SwitchScreen(1);
            }
        }
        else
        {
            gameObject.SetActive(false);
            PlayerScript.actionsLocked = false;
        }
    }

    public static List<int> BuildShuffledList(int numberOfElements)
    {
        List<int> _list = new List<int>();
        for (int i = 0; i < numberOfElements; i++)
        {
            _list.Add(i);
        }

        for (int i = 0; i < _list.Count; i++)
        {
            int temp = _list[i];
            int randomIndex = Random.Range(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }

    int SelectRandomEvent()
    {
        int result = -1;
        List<string> currentEventGroups = ActionManager.randomEventBookmaks[actionManager.currentRandomEventIndices[0]].GetEventGroups();
        List<int> acceptableEventsIndices = new List<int>();
        for (int i = 0; i < currentEventGroups.Count; i++)
        {
            for(int j = 0; j < randomEventsData.Count; j++)
            {
                if((randomEventsData[j].idName == currentEventGroups[i]) || (randomEventsData[j].groups.Contains(currentEventGroups[i])))
                {
                    acceptableEventsIndices.Add(j);
                    continue;
                }
            }
        }
        if (acceptableEventsIndices.Count > 0)
        {
            result = acceptableEventsIndices[Random.Range(0, acceptableEventsIndices.Count)];
        }
        return result;
    }

    public void NextRandomEvent()
    {
        madeWrongAnswer = false;
        currentQuastionIndex = 0;
        if (randomEventsData.Count == 0)
        {
            actionManager.RemoveRandomEventIndex(-1);
            return;
        }
        if (actionManager.currentRandomEventIndices.Count == 0)
            return;

        currentRandomEventIndex = SelectRandomEvent();
        if (currentRandomEventIndex == -1)
        {
            actionManager.RemoveRandomEventIndex(-1);
            return;
        }

        PlayerScript.actionsLocked = true;
        gameObject.SetActive(true);

        infoPanelTitle.text = randomEventsData[currentRandomEventIndex].infoTitle;
        infoPanelText.text = randomEventsData[currentRandomEventIndex].infoText;
        BuildQuastionsPanel();
        SwitchScreen(0);
        if (endScoreManager != null)
        {
            for(int k = 0; k < randomEventsData[currentRandomEventIndex].quastions.Count; k++)
            {
                endScoreManager.randQuestionsTexts.Add(randomEventsData[currentRandomEventIndex].infoText + ". " +
                    randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex]);
                endScoreManager.randomEventQuestionsCount++;
            }
        }
        else
        {
            Debug.LogWarning("No EndScoreManager found. Start from 1st scene.");
        }
    }


    public void CorrectAnswer(int value)
    {
        //@
        wrongAnswerPanelTitle.text = LocalizationManager.GetValueIfKey("[heel goed!]");
        //@
        wrongAnswerPanelText.text = LocalizationManager.GetValueIfKey(
            randomEventsData[currentRandomEventIndex].quastions[
                currentQuastionIndex].answers[shuffledIndexes[value]].descr);
        
        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
        if (inGameLocalEditTool != null)
        {
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(wrongAnswerPanelTitle.gameObject, "[heel goed!]");
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(wrongAnswerPanelText.gameObject, 
                randomEventsData[currentRandomEventIndex].quastions[
                    currentQuastionIndex].answers[shuffledIndexes[value]].descr);
        }

        ActionManager.CorrectAction();
        SwitchScreen(2);
        greenPop.Play();
        
        if (!madeWrongAnswer)
        {
            GameObject.Find("GameLogic").GetComponent<ActionManager>().UpdatePointsDirectly(
                randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex].points);
        }
    }

    public void WrongAnswer(int value)
    {
        if (!madeWrongAnswer)
        {
            GameObject.Find("GameLogic").GetComponent<ActionManager>().ActivatePenalty();
            if (endScoreManager != null)
            {
                int wrongIndex = endScoreManager.GetRandWrongIndexe(randomEventsData[currentRandomEventIndex].infoText + ". " +
                    randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex]);
                if (wrongIndex != -1)
                    endScoreManager.randomWrongIndexes.Add(wrongIndex);
            }
            else
            {
                Debug.LogWarning("No EndScoreManager found. Start from 1st scene.");
            }
        }
        //@
        wrongAnswerPanelTitle.text = LocalizationManager.GetValueIfKey("[helaas dit antwoord323]");
        //@
        wrongAnswerPanelText.text = LocalizationManager.GetValueIfKey(
            randomEventsData[currentRandomEventIndex].quastions[
                currentQuastionIndex].answers[shuffledIndexes[value]].descr);

        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
        if (inGameLocalEditTool != null)
        {
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(wrongAnswerPanelTitle.gameObject, "[helaas dit antwoord323]");
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(wrongAnswerPanelText.gameObject, 
                randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex].answers[shuffledIndexes[value]].descr);
        }
        ActionManager.WrongAction(false);
        madeWrongAnswer = true;
        SwitchScreen(2);
        redPop.Play();
    }

    void BuildQuastionsPanel()
    {
        shuffledIndexes = BuildShuffledList(randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex].answers.Count);
        quastionDesc.text = "";
        quastionTitle.text = randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex].text;
        QuizTab.Question currentQuestion = randomEventsData[currentRandomEventIndex].quastions[currentQuastionIndex];
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (currentQuestion.answers.Count > i)
            {
                answerButtons[i].gameObject.SetActive(true);
                string buttonText = currentQuestion.answers[shuffledIndexes[i]].text;
                string cheatSimbol = "";
                if (PlayerPrefsManager.GetDevMode())
                {
                    ObjectsIDsController objectsIDsController = GameObject.FindAnyObjectByType<ObjectsIDsController>();
                    if (objectsIDsController != null && objectsIDsController.cheat)
                    {
                        if (IsAnswerCorrect(i))
                            cheatSimbol = "@";
                    }
                }
                answerButtons[i].gameObject.GetComponentInChildren<Text>().text = cheatSimbol + buttonText;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

    }
    public void SwitchScreen(int value)
    {
        aPanelBackButton.SetActive(value == 1);
        for(int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(i == value);
        }
    }


    public void Init(List<string> eventFileNames)
    {
        List<string> _names = new List<string>();
        // _names.Add("RandomEvent");
        for (int i = 0; i < eventFileNames.Count; i++)
        {
            if (eventFileNames[i] != "")
                _names.Add(eventFileNames[i]);
        }
        foreach (string n in _names)
        {
            TextAsset textAsset = (TextAsset)Resources.Load("Xml/RandomEvent/" + n);
            XmlDocument xmlFile = new XmlDocument();
            xmlFile.LoadXml(textAsset.text);
            XmlNodeList _events = xmlFile.FirstChild.NextSibling.ChildNodes;
            foreach (XmlNode s in _events)
            {
                RandomEventData eventData = new RandomEventData();

                eventData.idName = s.Attributes["id_name"].Value;
                eventData.groups.AddRange(s.Attributes["group"].Value.Split(','));

                XmlNode infoNode = s.SelectSingleNode("info");
                eventData.infoTitle = infoNode.Attributes["title"].Value;
                eventData.infoText = infoNode.Attributes["text"].Value;

                foreach(XmlNode q in s)
                {
                    if (q.Name == "question")
                    {
                        QuizTab.Question question = new QuizTab.Question();
                        question.answers = new List<QuizTab.Answer>();
                        question.text = q.Attributes["text"].Value;
                        int.TryParse(q.Attributes["answer"].Value, out question.answerID);

                        if (q.Attributes["points"] != null)
                            int.TryParse(q.Attributes["points"].Value, out question.points);
                        else
                            question.points = 1;
                        --question.answerID;

                        XmlNodeList answers = q.ChildNodes;
                        int j = 0;
                        foreach (XmlNode a in answers)
                        {
                            QuizTab.Answer t = new QuizTab.Answer();
                            t.text = a.Attributes["text"].Value;
                            t.descr = (a.Attributes["descr"] != null) ? a.Attributes["descr"].Value : "";
                            t.isCorrect = question.answerID == j;
                            question.answers.Add(t);
                            j++;
                        }
                        eventData.quastions.Add(question);
                    }
                }
                randomEventsData.Add(eventData);
            }
        }
    }
}