using UnityEngine;
using System;
using UnityEngine.UI;
using BuildTimestampDisplay;
using UnityEngine.EventSystems;
using CareUp.Localize;

namespace MBS
{
    public class WUUGLoginGUI : WUUGLoginLocalisation
    {
        public UILangSwitch uILangSwitch;
        double startTime;
        public Button passVisibilityToggle;
        public GameObject LoginGUIBackButton;
        public GameObject LoginGUIText;
        public enum eWULUGUIState { Inactive, Active }
        [Serializable]
        public struct WUPanels
        {
            public GameObject
            login_menu,
            login_screen,
            register_screen,
            password_reset_screen,
            password_change_screen,
            post_login_menu_screen,
            personal_info_screen,
            high_score_screen,
            shop_screen,
            localization_screen,
            serialnumber_screen,
            order_screen,
            termsandcondition_screen,
            terms_condition_screen,
            voorwaarden_screen,
            error_pop_up,
            error_login_pop_up,
            registration_pop_up,
            custom_1,
            SessionTimeOutPanel,
            start_menu,
            debug_options,
            inputPopPanel;
        }

        [Serializable]
        public struct WUInputFields
        {
            public InputField
            login_username,
            login_password,
            register_username,
            register_password,
            register_verify,
            register_email,
            pass_reset_username,
            pass_reset_email,
            pass_change_old,
            pass_change_new,
            pass_change_verify,
            personal_name,
            personal_surname,
            personal_display_name,
            personal_nickname,
            personal_aol,
            personal_yim,
            personal_jabber,
            personal_email,
            personal_url,
            personal_bio,
            serial_number,
            popup_input;
        }

        [SerializeField] BuildTimestamp buildTimestamp;
        [SerializeField] string format = "yyyy/MM/dd HH:mm:ss";
        [SerializeField] float utcOffsetHours;

        [SerializeField] private Image LoginUsernameField = default(Image);
        [SerializeField] private Image LoginPasswordField = default(Image);
        [SerializeField] private GameObject loginPasswordVisibilityOn = default;
        [SerializeField] private GameObject loginPasswordVisibilityOff = default;
        //[SerializeField] private GameObject popupLoginPasswordVisibilityOn = default; value never used
        //[SerializeField] private GameObject popupLoginPasswordVisibilityOff = default; value never used
        //[SerializeField] private Button loginPasswordVisibility = default; value never used

        [SerializeField] private Image RegUsernameField = default(Image);
        [SerializeField] private Image RegEmailField = default(Image);
        [SerializeField] private Image RegPasswordField = default(Image);
        [SerializeField] private Image RegRepeatPasswordField = default(Image);

        [SerializeField] private Text ErrorText = default(Text);
        [SerializeField] private Text ErrorLoginText = default(Text);
        [SerializeField] private Text SuccesText = default(Text);

        private bool removeText = false;
        private bool passwordVisible = false;


        //void ShowLoginPasswordInput(bool showLogin = true)
        //{
        //    popupCahngeIsLogin = showLogin;
        //    panels.inputPopPanel.SetActive(true);
        //    Text inputTitle = panels.inputPopPanel.transform.Find("IP1/IP2/InputTitle").GetComponent<Text>();
        
        //    if (showLogin)
        //    {
        //        inputTitle.text = "Gebruikersnaam";
        //        fields.popup_input.contentType = InputField.ContentType.Standard;
        //        fields.popup_input.lineType = InputField.LineType.SingleLine;
        //        fields.popup_input.text = fields.login_username.text;
        //        passVisibilityToggle.gameObject.SetActive(false);
                
        //    }
        //    else
        //    {
        //        inputTitle.text = "Wachtwoord";
        //        fields.popup_input.lineType = InputField.LineType.SingleLine;
        //        fields.popup_input.text = fields.login_password.text;
        //        fields.popup_input.contentType = passwordVisible ? InputField.ContentType.Standard : InputField.ContentType.Password;
        //        passVisibilityToggle.gameObject.SetActive(true);
        //    }
        //    fields.popup_input.ForceLabelUpdate();
        //    EventSystem.current.SetSelectedGameObject(fields.popup_input.gameObject);

        //}

        public void HideLoginPasswordInput()
        {
            panels.inputPopPanel.SetActive(false);
        }

        void Update()
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            LoginGUIBackButton.SetActive(false);
            LoginGUIText.SetActive(false);
            if (startTime >= 0 && Time.time - startTime > 0.1)
            {
                DisplayScreen(panels.login_screen);
                startTime = -1;
            }
#endif
            //if (fields.login_username.isFocused)
            //{
            //    ShowLoginPasswordInput();
            //}
            //else if (fields.login_password.isFocused)
            //{
            //    ShowLoginPasswordInput(false);
            //}
            if (WULogin.UserNotWithEmail == true)
            {
                DisplayScreen(panels.error_login_pop_up);
                ErrorLoginText.text = "Er is geen gebruikersnaam die bij deze e-mail hoort";
                WULogin.UserNotWithEmail = false;
            }
            if (WULogin.UserNotFound == true)
            {
                DisplayScreen(panels.error_login_pop_up);
                ErrorLoginText.text = "Deze gebruikersnaam is niet gevonden";
                WULogin.UserNotFound = false;
            }

            if (WULogin.on_Login_Success == false)
            {
                ChangeLoginUIRed();
                DisplayScreen(panels.error_login_pop_up);
                ErrorLoginText.text = "Het wachtwoord of gebruikersnaam is incorrect";
            }
            else if (WULogin.on_Registration_Success == false)
            {
                ChangeRegistrationUIRed();
            }
            else if (WULogin.on_Registration == true)
            {
                DisplayScreen(panels.registration_pop_up);
                WULogin.on_Registration = false;
            }

            if (WULogin.ChangeAllToWhite == true)
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(210, 210, 210, 150);
                RegEmailField.color = new Color32(210, 210, 210, 150);
                RegPasswordField.color = new Color32(210, 210, 210, 150);
                RegRepeatPasswordField.color = new Color32(210, 210, 210, 150);

                WULogin.ChangeAllToWhite = false;
            }

            if (removeText == true)
            {
                fields.login_username.text = fields.register_username.text;
                fields.login_password.text = fields.register_password.text;
                fields.register_username.text = "";
                fields.register_email.text = "";
                fields.register_password.text = "";
                fields.register_verify.text = "";

                removeText = false;
            }
        }

        private void ChangeLoginUIRed()
        {
            LoginUsernameField = LoginUsernameField.GetComponent<Image>();
            LoginPasswordField = LoginPasswordField.GetComponent<Image>();

            LoginUsernameField.color = new Color32(255, 0, 0, 150);
            LoginPasswordField.color = new Color32(255, 0, 0, 150);

            WULogin.on_Login_Success = true;
        }

        private void ChangeRegistrationUIRed()
        {
            if (WULogin.EmailTheSame == true)
            {

                WULogin.ChangeAllToWhite = true;

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Dit e-mailadres is al in gebruik.";

                RegEmailField = RegEmailField.GetComponent<Image>();
                RegEmailField.color = new Color32(255, 0, 0, 150);
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegUsernameField.color = new Color32(210, 210, 210, 150);

                WULogin.EmailTheSame = false;
            }
            if (WULogin.UsernameTheSame == true)
            {
                WULogin.ChangeAllToWhite = true;

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Deze gebruikersnaam is al in gebruik.";

                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegUsernameField.color = new Color32(255, 0, 0, 150);
            }
            if (WULogin.UsernameNotValid == true)
            {
                WULogin.ChangeAllToWhite = true;

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Gebruikersnaam niet toegestaan. Gebruik geen speciaal tekens, accenten of streepjes in je gebruikernaam.";

                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegUsernameField.color = new Color32(255, 0, 0, 150);
            }

            WULogin.on_Registration_Success = true;
        }

        static WUUGLoginGUI instance;

        static public WUUGLoginGUI Instance
        {
            get
            {
                if (null == instance)
                {
                    WUUGLoginGUI[] objs = FindObjectsOfType<WUUGLoginGUI>();
                    if (null != objs && objs.Length > 0)
                    {
                        instance = objs[0];
                        for (int i = 1; i < objs.Length; i++)
                            Destroy(objs[i].gameObject);
                    }
                    else
                    {
                        GameObject newobject = new GameObject("WUUGLoginGUI");
                        instance = newobject.AddComponent<WUUGLoginGUI>();
                    }
                }
                return instance;
            }
        }

        [Header("GUI Prefab")]
        [SerializeField] WUInputFields fields = default(WUInputFields);
        [SerializeField] WUPanels panels = default(WUPanels);
        [SerializeField] Toggle auto_login_toggle = default(Toggle);
#if WUS
        [Header("Leaderboards")]
        [SerializeField] bool fetch_scores_on_showing_panel = true;
#endif

#if WUSKU
        [Header("Optional")]
        [SerializeField]
        string product_url = "";
#endif

        public bool attempt_auto_login { get { return auto_login_toggle.isOn; } set { auto_login_toggle.isOn = value; } }
        public eWULUGUIState active_state { get; private set; } = eWULUGUIState.Inactive;
        GameObject active_screen = null;

        void DisplayScreen(GameObject screen)
        {
            active_state = eWULUGUIState.Active;

            if (null != active_screen && screen != active_screen)
                HideActiveScreen();
            active_screen = screen;
            active_screen.SetActive(WPServer.ServerState == WPServerState.None);
        }

        public void ShowActiveScreen() => active_screen?.SetActive(true);
        public void HideActiveScreen() => active_screen?.SetActive(false);

        void OnServerStateChanged(WPServerState state)
        {
            if (state == WPServerState.Contacting)
                HideActiveScreen();
            else
                if (active_state == eWULUGUIState.Active)
                ShowActiveScreen();
        }

        void Start()
        {
            startTime = Time.time;

            if (GameObject.Find("VersionNumber") != null)
            {
                string _sing = "";
                if ((int)utcOffsetHours >= 0)
                    _sing = "+";

                string buildTime = buildTimestamp ? buildTimestamp.ToString(format, utcOffsetHours) : "";
                GameObject.Find("VersionNumber").GetComponent<Text>().text = "v" + Application.version + " " + buildTime + " UTC" + _sing + ((int)utcOffsetHours).ToString();
            }
            UpdateDebugOptionsPanelVis();

            PlayerPrefsManager.plusCoins = false;
            PlayerPrefsManager.plusDiamonds = false;
            PlayerPrefsManager.resetPurchases = false;
            PlayerPrefsManager.editCharacterOnStart = false;
            PlayerPrefsManager.tutorialOnStart = false;
            PlayerPrefsManager.firstStart = true;

            if (this == Instance)
            {
                InitWULoginGUI();
            }

            ErrorText.GetComponent<Text>();
            ErrorLoginText.GetComponent<Text>();

            uILangSwitch.UpdateAllLangButtons();
        }

        private void UpdateTextComponents()
        {
            foreach(UILocalization uil in GameObject.FindObjectsOfType<UILocalization>(true))
            {
                uil.UpdateText();
            }
        }

        //Debug options changed
        public void PlusCoins()
        {
            bool value = panels.debug_options.transform.Find("Panel/Plus300/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.plusCoins = value;
        }

        public void PlusDiamonds()
        {
            bool value = panels.debug_options.transform.Find("Panel/Plus5/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.plusDiamonds = value;
        }

        public void ResetPurchasesChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/ResetPurchases/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.resetPurchases = value;
        }

        public void BandleServerIPChanged()
        {
            string value = panels.debug_options.transform.Find("AssetBundleServer/InputField").GetComponent<InputField>().text;
            PlayerPrefsManager.TestBundleIPAddr = value;
        }


        public void UpdateDebugOptionsPanelVis()
        {
            panels.debug_options.SetActive(PlayerPrefsManager.GetDevMode());
        }
        public void UseDevBundleServerChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/DevBundleServer/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.UseDevBundleServer = value;
            if (!value)
                CAServer.BundleAddress = CAServer.ReleaseBundleAddress;
            else
                CAServer.BundleAddress = CAServer.DevBundleAddress;
        }

        public void UseTestDBServerChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/TestServer/Toggle").GetComponent<Toggle>().isOn;
            GameObject.FindObjectOfType<WPServer>().setUseOnlineURL(!value);
        }
        public void ShowUpdatesChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/ShowUpdatePanel/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.forceShowUpdatesPanel = value;
        }

        public void EditCharacterChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/EditCharacter/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.editCharacterOnStart = value;
            panels.debug_options.transform.Find("Panel/Tutorial/Toggle").GetComponent<Toggle>().interactable = value;
        }

        public void TutorialChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/Tutorial/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.tutorialOnStart = value;
        }

        public void AutoplayChanged()
        {
            bool value = panels.debug_options.transform.Find("Panel/AutoPlay/Toggle").GetComponent<Toggle>().isOn;
            PlayerPrefsManager.simulatePlayerActions = value;
        }

        virtual protected void InitWULoginGUI()
        {
            localization_change = PlayerPrefs.GetInt(localisation_pref_name, 0);
            int id = -1;
            foreach (MBSLocalisationBase local in MBSLocalisationList.AllLogin)
            {
                id++;
                if (null == local)
                    continue;
                WULocalizationButton lb = Instantiate<WULocalizationButton>(localisation_button_prefab);
                lb.transform.SetParent(localisation_grid.transform, false);
                lb.SetId(id, local.LocalisationGraphic);
            }
            DoLocalisation();

            WUCookie.LoadStoredCookie();
            if (PlayerPrefs.HasKey("Remember Me"))
            {
                attempt_auto_login = PlayerPrefs.GetInt("Remember Me", 0) > 0;
                fields.login_username.text = PlayerPrefs.GetString("username", string.Empty);
                fields.login_password.text = PlayerPrefs.GetString("password", string.Empty);
            }

            //if this script is loaded while already logged in, go to the post login menu or else show the login menu
            DisplayScreen(WULogin.logged_in ? panels.post_login_menu_screen : panels.login_menu);

            //setup all the actions that will take place when buttons are clicked
            SetupResponders();

            // fuck this
            //if "Remember me" was selected during the last login, try to log in automatically...
#if !UNITY_EDITOR

            if (attempt_auto_login && !WULogin.logged_in)
                WULogin.AttemptAutoLogin();
#endif

            //if (loginPasswordVisibility != null)
            //{
            //    loginPasswordVisibility.onClick.AddListener(OnTogglePasswordVisibility);
            //}
            //if (fields.login_password != null)
            //{
            //    fields.login_password.onValueChanged.AddListener(delegate { EnablePasswordVisibilityIcon(); });
            //}
        }

        void SetupResponders()
        {
            WULogin.onRegistered += OnRegistered;
            WULogin.onLoggedIn += OnLoggedIn;
            WULogin.onLoggedOut += OnLoggedOut;
            WULogin.onReset += OnReset;
            WULogin.onAccountInfoReceived += OnAccountInfoReceived;
            WULogin.onInfoUpdated += OnAccountInfoUpdated;
            WULogin.onPasswordChanged += OnPasswordChanged;

            WULogin.onAccountInfoFetchFailed += DisplayErrors;
            WULogin.onInfoUpdateFail += DisplayErrors;
            WULogin.onLoginFailed += DisplayErrors;
            WULogin.onLogoutFailed += DisplayErrors;
            WULogin.onPasswordChangeFail += DisplayErrors;
            WULogin.onRegistrationFailed += DisplayErrors;
            WULogin.onResetFailed += DisplayErrors;

            WPServer.OnServerStateChange += OnServerStateChanged;
        }

        void OnDestroy()
        {
            WULogin.onRegistered -= OnRegistered;
            WULogin.onLoggedIn -= OnLoggedIn;
            WULogin.onLoggedOut -= OnLoggedOut;
            WULogin.onReset -= OnReset;
            WULogin.onAccountInfoReceived -= OnAccountInfoReceived;
            WULogin.onInfoUpdated -= OnAccountInfoUpdated;
            WULogin.onPasswordChanged -= OnPasswordChanged;

            WULogin.onAccountInfoFetchFailed -= DisplayErrors;
            WULogin.onInfoUpdateFail -= DisplayErrors;
            WULogin.onLoginFailed -= DisplayErrors;
            WULogin.onLogoutFailed -= DisplayErrors;
            WULogin.onPasswordChangeFail -= DisplayErrors;
            WULogin.onRegistrationFailed -= DisplayErrors;
            WULogin.onResetFailed -= DisplayErrors;
            WPServer.OnServerStateChange -= OnServerStateChanged;
        }

        void DisplayErrors(CMLData error) => StatusMessage.Message = error.String("message");

        #region Server contact
        public void DoLogin()
        {
            WULogin.onLoginFailed += On_Login_Fail;
            WULogin.on_Login_Success = true;
            CMLData data = new CMLData();
            data.Set("username", fields.login_username.text.Trim());
            data.Set("password", fields.login_password.text.Trim());
            SmartlookUnity.Smartlook.SetUserIdentifier(fields.login_username.text.Trim());
            WULogin.AttemptToLogin(data);
            PlayerPrefs.SetInt("Remember Me", attempt_auto_login ? 1 : 0);
            DisplayScreen(panels.login_menu);
        }

        public void DoAutoLogin(string username, string password)
        {
            WULogin.onLoginFailed += On_Login_Fail;
            WULogin.on_Login_Success = true;
            CMLData data = new CMLData();
            data.Set("username", username);
            data.Set("password", password);
            WULogin.AttemptToLogin(data);
            PlayerPrefs.SetInt("Remember Me", attempt_auto_login ? 1 : 0);
            DisplayScreen(panels.login_menu);
        }


        void On_Login_Fail(CMLData response) => WULogin.on_Login_Success = false;

        public void DoTrustedLogin(string email)
        {
            email = email.Trim();
            if (!email.IsValidEmailFormat())
            {
                Debug.LogWarning($"{email} is geen valide e-mailadres!");
                return;
            }
            CMLData data = new CMLData();
            data.Set("email", email);
            WULogin.AttemptTrustedLogin(data);
            DisplayScreen(panels.login_menu);
        }

        public void DoResumeGame()
        {
#if WUSKU
            if (WULogin.RequireSerialForLogin && !WULogin.HasSerial)
            {
                DisplayScreen(panels.serialnumber_screen);
                StatusMessage.Message = "Product requires registration before you may continue";
                return;
            }
#endif

            active_state = eWULUGUIState.Inactive;
            active_screen.SetActive(false);
            WULogin.onResumeGame?.Invoke();
        }

        public void DoRegistration()
        {
            if (fields.register_email.text.Trim() == string.Empty || fields.register_password.text.Trim() == string.Empty || fields.register_username.text.Trim() == string.Empty)
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(255, 0, 0, 150);
                RegEmailField.color = new Color32(255, 0, 0, 150);
                RegPasswordField.color = new Color32(255, 0, 0, 150);
                RegRepeatPasswordField.color = new Color32(255, 0, 0, 150);

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Je moet alle velden invullen";
                return;
            }
            if (fields.register_verify.text.Trim() != fields.register_password.text.Trim())
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(210, 210, 210, 150);
                RegEmailField.color = new Color32(210, 210, 210, 150);
                RegPasswordField.color = new Color32(210, 210, 210, 150);
                RegRepeatPasswordField.color = new Color32(210, 210, 210, 150);

                RegPasswordField.color = new Color32(255, 0, 0, 150);
                RegRepeatPasswordField.color = new Color32(255, 0, 0, 150);

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "De wachtwoorden komen niet overeen.";
                return;
            }
            if (!fields.register_email.text.Trim().IsValidEmailFormat())
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(210, 210, 210, 150);
                RegEmailField.color = new Color32(210, 210, 210, 150);
                RegPasswordField.color = new Color32(210, 210, 210, 150);
                RegRepeatPasswordField.color = new Color32(210, 210, 210, 150);

                RegEmailField.color = new Color32(255, 0, 0, 150);

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Het ingevulde e-mailadres in niet geldig.";
                return;
            }

            WULogin.onRegistrationFailed += On_Registration_Fail;
            WULogin.onRegistered += On_Registration;
            WULogin.onRegistered += On_Registration_Succes;
            WULogin.onRegistered += RemoveText;
            WULogin.on_Registration_Success = true;
            CMLData data = new CMLData();
            data.Set("username", fields.register_username.text.Trim());
            data.Set("email", fields.register_email.text.Trim());
            data.Set("password", fields.register_password.text.Trim());
            WULogin.RegisterAccount(data);

            DisplayScreen(panels.registration_pop_up);
        }

        void On_Registration_Fail(CMLData response) => WULogin.on_Registration_Success = false;
        void On_Registration_Succes(CML response) => WULogin.on_Registration = true;
        void On_Registration(CML response) => WULogin.ChangeAllToWhite = true;
        void RemoveText(CML response) => removeText = true;

        public void DoFetchAccountInfo() => WULogin.FetchPersonalInfo();
        public void LogOut() => WULogin.LogOut();

        public void DoProductRegistration()
        {
#if WUSKU
            WUSerials.RegisterSerial(fields.serial_number.text.Trim(), OnRegistrationSucceeded, OnRegistrationFailed);
#endif
        }

#if WUSKU
        void OnRegistrationSucceeded(CML response)
        {
            Canvas c = GetComponent<Canvas>();
            if (null == c) c = GetComponentInParent<Canvas>();

            if (null != c)
                MBSNotification.SpawnInstance(c, new Vector2(270f, -30f), new Vector2(-20f, -30f), localisation.RegistrationSuccessHeader, localisation.RegistrationSuccessMessage);
            WULogin.HasSerial = true;
            WULogin.SerialNumber = response[0].String("serial");
            bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
        }

        void OnRegistrationFailed(CMLData response)
        {
            Debug.Log("Gefaald");
            StatusMessage.Message = response.String("message");
            DisplayScreen(panels.register_screen);
        }
#endif

        public void DoInfoUpdates()
        {
            CMLData data = new CMLData();

            if (fields.personal_email.text != string.Empty)
            {
                if (!fields.personal_email.text.Trim().IsValidEmailFormat())
                {
                    StatusMessage.Message = localisation.InvalidEmail;
                    return;
                }
                data.Set("email", fields.personal_email.text.Trim());
            }
            else
            {
                StatusMessage.Message = localisation.EmailRequired;
                return;
            }
            data.Set("website", fields.personal_url.text.Trim());
            data.Set("descr", fields.personal_bio.text.Trim());
            data.Set("yim", fields.personal_yim.text.Trim());
            data.Set("jabber", fields.personal_jabber.text.Trim());
            data.Set("aim", fields.personal_aol.text.Trim());
            data.Set("fname", fields.personal_name.text.Trim());
            data.Set("lname", fields.personal_surname.text.Trim());
            data.Set("nname", fields.personal_nickname.text.Trim());
            data.Set("dname", fields.personal_display_name.text.Trim());
            WULogin.UpdatePersonalInfo(data);
            DisplayScreen(panels.post_login_menu_screen);
        }

        public void DoPasswordChange()
        {
            if (fields.pass_change_old.text.Trim() == string.Empty)
            {
                StatusMessage.Message = localisation.ProvideCurrentPassword;
                return;
            }
            if (fields.pass_change_new.text.Trim() == string.Empty)
            {
                StatusMessage.Message = localisation.ProvideNewPassword;
                return;
            }
            if (fields.pass_change_new.text.Trim() != fields.pass_change_verify.text.Trim())
            {
                StatusMessage.Message = localisation.FailedVerification;
                return;
            }

            CMLData data = new CMLData();
            data.Set("password", fields.pass_change_old.text.Trim());
            data.Set("passnew", fields.pass_change_new.text.Trim());
            WULogin.ChangePassword(data);
            DisplayScreen(panels.post_login_menu_screen);
        }

        public void DoPasswordReset()
        {
            fields.pass_reset_email.text = fields.pass_reset_email.text.Trim();
            if (fields.pass_reset_email.text == string.Empty && fields.pass_reset_username.text.Trim() == string.Empty)
            {
                //StatusMessage.Message = localisation.NeedEmailOrUsername;
                DisplayScreen(panels.error_login_pop_up);
                ErrorLoginText.text = "Alle velden moeten worden ingevult";
                return;
            }
            string login = fields.pass_reset_email.text == string.Empty ? fields.pass_reset_username.text.Trim() : fields.pass_reset_email.text;
            if (fields.pass_reset_email.text != string.Empty && !fields.pass_reset_email.text.IsValidEmailFormat())
            {
                //StatusMessage.Message = localisation.InvalidEmail;
                DisplayScreen(panels.error_login_pop_up);
                ErrorLoginText.text = "Het ingevulde e-mailadres in niet geldig.";
                return;
            }
            CMLData data = new CMLData();
            data.Set("login", login);
            WULogin.ResetPassword(data);
        }
        #endregion

        #region Server response handlers
        //upon successful login, the fields you requested to be returned are stored in CMLData fetched_info
        //and are left available to you until logout.
        virtual public void OnLoggedIn(CML _data)
        {
            //remember the "Remember me" choice...
            PlayerPrefs.SetInt("Remember Me", attempt_auto_login ? 1 : 0);
            if (attempt_auto_login)
            {
                PlayerPrefs.SetString("username", fields.login_username.text);
                PlayerPrefs.SetString("password", fields.login_password.text);
            }

            //remove the password from the textfield
            //fields.login_password.text = "";

#if WUSKU
            //fields.serial_number.text = WULogin.SerialNumber;

            //return to main menu and set it out of view...
            //unless you require a serial first...
            if (WULogin.RequireSerialForLogin)
            {

                if (WULogin.HasSerial)
                {
                    DisplayScreen(panels.post_login_menu_screen);
                    active_state = eWULUGUIState.Inactive;
                    active_screen.SetActive(false);
                }
                //and don't have one...
                else
                {
                    DisplayScreen(panels.serialnumber_screen);
                }
            }
            else
            {
                DisplayScreen(panels.post_login_menu_screen);
                active_state = eWULUGUIState.Inactive;
                active_screen.SetActive(false);
            }
#else
            //return to main menu and set it out of view...
            DisplayScreen( panels.post_login_menu_screen );

            active_state = eWULUGUIState.Inactive;
            active_screen.SetActive( false );            
#endif
        }

        virtual public void OnLoggedOut(CML data)
        {
            StatusMessage.Message = $"{WULogin.display_name} Uitloggen is gelukt!";
            WULogin.logged_in = false;
            WULogin.nickname = WULogin.display_name = string.Empty;
            DisplayScreen(panels.login_menu);
        }

        virtual public void OnReset(CML data)
        {
            DisplayScreen(panels.registration_pop_up);
            SuccesText.text = "Er is een e-mail naar je verzonden waarin je je wachtwoord kunt veranderen.";
            fields.pass_reset_email.text = fields.pass_reset_username.text = string.Empty;
        }

        virtual public void OnAccountInfoReceived(CML data)
        {
            fields.personal_aol.text = data[0].String("aim");
            fields.personal_bio.text = data[0].String("descr");
            fields.personal_display_name.text = data[0].String("dname");
            fields.personal_email.text = data[0].String("email");
            fields.personal_jabber.text = data[0].String("jabber");
            fields.personal_name.text = data[0].String("fname");
            fields.personal_nickname.text = data[0].String("nname");
            fields.personal_surname.text = data[0].String("lname");
            fields.personal_url.text = data[0].String("website");
            fields.personal_yim.text = data[0].String("yim");
            ShowAccountDetailsScreen();
        }

        virtual public void OnPasswordChanged(CML data)
        {
            fields.pass_change_old.text = fields.pass_change_new.text = fields.pass_change_verify.text = string.Empty;
            OnLoggedOut(data);
            StatusMessage.Message = "Password successfully changed";
        }

        virtual public void OnRegistered(CML data)
        {
            StatusMessage.Message = "Registatie gelukt! Je account is aangemaakt.";
            DisplayScreen(panels.login_menu);
        }

        virtual public void OnAccountInfoUpdated(CML data)
        {
            WULogin.nickname = fields.personal_nickname.text.Trim();
            WULogin.display_name = fields.personal_display_name.text.Trim();
            WULogin.email = fields.personal_email.text.Trim();
            WULogin.website = fields.personal_url.text.Trim();

            DisplayScreen(panels.post_login_menu_screen);
        }
        #endregion

        #region ugui accessors
        override public void ShowLoginMenuScreen() => DisplayScreen(panels.start_menu);
        public void ShowPreLoginMenu() => DisplayScreen(panels.login_menu);
        public void ShowPostLoginMenu() => DisplayScreen(panels.post_login_menu_screen);
        public void ShowLoginScreen() => DisplayScreen(panels.login_screen);
        public void ShowRegisterScreen() => DisplayScreen(panels.register_screen);
        public void ShowPostLoginScreen() => DisplayScreen(panels.post_login_menu_screen);
        public void ShowPasswordResetScreen() //=> DisplayScreen( panels.password_reset_screen );
        {
            PlayerPrefsManager.OpenUrl_NewWindow("https://careup.online/culogin/?action=lostpassword");
            //Application.OpenURL("https://careup.online/culogin/?action=lostpassword&error=expiredkey");
        }
        public void ShowPasswordChangeScreen() => DisplayScreen(panels.password_change_screen);
        public void ShowAccountDetailsScreen() => DisplayScreen(panels.personal_info_screen);
        public void ShowTermsAndConditionScreen()
        {
            // email field
            if (fields.register_email.text.Trim() == string.Empty)
            {
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegEmailField.color = new Color32(255, 0, 0, 150);
                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Je moet alle velden invullen";
                return;
            }

            // username field
            if (fields.register_username.text.Trim() == string.Empty)
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegUsernameField.color = new Color32(255, 0, 0, 150);
                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Je moet alle velden invullen";
                return;
            }

            // password field
            if (fields.register_password.text.Trim() == string.Empty)
            {
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegPasswordField.color = new Color32(255, 0, 0, 150);
                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Je moet alle velden invullen";
                return;
            }

            // repeat password field
            if (fields.register_verify.text.Trim() != fields.register_password.text.Trim())
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(210, 210, 210, 150);
                RegEmailField.color = new Color32(210, 210, 210, 150);
                RegPasswordField.color = new Color32(210, 210, 210, 150);
                RegRepeatPasswordField.color = new Color32(210, 210, 210, 150);

                RegPasswordField.color = new Color32(255, 0, 0, 150);
                RegRepeatPasswordField.color = new Color32(255, 0, 0, 150);

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "De wachtwoorden komen niet overeen.";
                return;
            }

            // email format
            if (!fields.register_email.text.Trim().IsValidEmailFormat())
            {
                RegUsernameField = RegUsernameField.GetComponent<Image>();
                RegEmailField = RegEmailField.GetComponent<Image>();
                RegPasswordField = RegPasswordField.GetComponent<Image>();
                RegRepeatPasswordField = RegRepeatPasswordField.GetComponent<Image>();

                RegUsernameField.color = new Color32(210, 210, 210, 150);
                RegEmailField.color = new Color32(210, 210, 210, 150);
                RegPasswordField.color = new Color32(210, 210, 210, 150);
                RegRepeatPasswordField.color = new Color32(210, 210, 210, 150);

                RegEmailField.color = new Color32(255, 0, 0, 150);
                RegEmailField.color = new Color32(255, 0, 0, 150);

                DisplayScreen(panels.error_pop_up);
                ErrorText.text = "Het ingevulde e-mailadres in niet geldig.";
                return;
            }

            DisplayScreen(panels.termsandcondition_screen);
        }

        public void CloseHighScoresScreen() => DisplayScreen(panels.login_menu);
        public void ReturnFromSerialScreen() => LogOut();
        public void ShowTerms_condition_Screen() => panels.terms_condition_screen.SetActive(true);
        public void ShowVoorwaarden_Screen() => panels.voorwaarden_screen.SetActive(true);
        public void RemoveTerms_condition_Screen() => panels.terms_condition_screen.SetActive(false);
        public void RemoveVoorwaarden_Screen() => panels.voorwaarden_screen.SetActive(false);
        public void RemoveSession_Screen() => panels.SessionTimeOutPanel.SetActive(false);
        public void ShowLocalizationScreen()
        {
            DisplayScreen(panels.localization_screen);
            panels.localization_screen.BroadcastMessage("SelectALanguage", MBSLocalisationList.Login.Selected);
        }

        public void GoToWebsite()
        {
            Application.OpenURL(
#if WUSKU
            product_url.Trim() != string.Empty ? product_url.Trim() :
#endif
            WPServer.Instance.SelectedURL);
        }

        //in case you want to show your hgh scores on the menu page, this is a way for you to trigger that
        //it is entirely optional but I am including it since I needed it recently and, just in case you do also, here it is :)
        public void ShowHighScoresScreen()
        {
#if WUS
            if (fetch_scores_on_showing_panel)
                WUScoring.FetchScores();
#endif
            DisplayScreen(panels.high_score_screen);
        }

        //this only loads a screen where you can show your shop, nothing more
        //instead of showing this panel you can use this function to load a shop scene instead
        //whichever way you go, this function allows you to setup the shop open trigger via the inspector
        public void ShowShopScreen() => DisplayScreen(panels.shop_screen);

        public void ShowCustomScreen(int which = 0)
        {
            //in preparation of you adding more custom screens...
            switch (which)
            {
                case 0:
                    DisplayScreen(panels.custom_1);
                    break;
            }
        }

        public void ShowSerialScreen()
        {
#if WUSKU
            bool enable_fields = !WULogin.HasSerial;
            ltf.serial_buy.transform.parent.GetComponent<Button>().interactable = enable_fields;
            ltf.serial_register.transform.parent.GetComponent<Button>().interactable = enable_fields;
            ltf.serial_label.transform.parent.GetComponent<InputField>().interactable = enable_fields;
            DisplayScreen(panels.serialnumber_screen);
#endif
        }

        public void ShowOrderScreen()
        {
            panels.serialnumber_screen.SetActive(false);
            panels.order_screen.SetActive(true);
        }

        public void RemoveOrderScreen()
        {
            panels.serialnumber_screen.SetActive(true);
            panels.order_screen.SetActive(false);
        }

        public void onClickChangeColor(Image m_image)
        {
            m_image = m_image.GetComponent<Image>();

            m_image.color = new Color32(210, 210, 210, 150);
        }

        public void OnTogglePasswordVisibility()
        {
            passwordVisible = !passwordVisible;

            fields.login_password.contentType = passwordVisible ? InputField.ContentType.Standard : InputField.ContentType.Password;
            fields.popup_input.contentType = passwordVisible ? InputField.ContentType.Standard : InputField.ContentType.Password;
            loginPasswordVisibilityOn.SetActive(!passwordVisible);
            loginPasswordVisibilityOff.SetActive(passwordVisible);
            fields.login_password.ForceLabelUpdate();
            fields.popup_input.ForceLabelUpdate();
        }

        //public void EnablePasswordVisibilityIcon()
        //{
        //    if (!string.IsNullOrEmpty(fields.login_password.text))
        //        loginPasswordVisibility.gameObject.SetActive(true);
        //    else
        //        loginPasswordVisibility.gameObject.SetActive(false);
        //}

        #endregion
    }
}
