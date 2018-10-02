﻿using UnityEngine;
using UnityEngine.UI;

namespace MBS
{
    public class WUUGLoginGUI : WUUGLoginLocalisation
    {

        public enum eWULUGUIState { Inactive, Active }

        [System.Serializable]
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
            termsandcondition_screen,
            custom_1;
        }

        [System.Serializable]
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
                serial_number;
        }

        static WUUGLoginGUI _instance;
        static public WUUGLoginGUI Instance
        {
            get
            {
                if ( null == _instance )
                {
                    WUUGLoginGUI [] objs = FindObjectsOfType<WUUGLoginGUI>();
                    if ( null != objs && objs.Length > 0 )
                    {
                        _instance = objs [0];
                        for ( int i = 1; i < objs.Length; i++ )
                            Destroy( objs [i].gameObject );
                    }
                    else
                    {
                        GameObject newobject = new GameObject( "WUUGLoginGUI" );
                        _instance = newobject.AddComponent<WUUGLoginGUI>();
                    }
                }
                return _instance;
            }
        }

        [Header("GUI Prefab")]
        [SerializeField] WUInputFields fields;
        [SerializeField] WUPanels panels;
        [SerializeField] Toggle auto_login_toggle;
#if WUS
        [Header("Leaderboards")]
        [SerializeField] bool fetch_scores_on_showing_panel = true;
#endif

#if WUSKU
        [Header("Optional")]
        [SerializeField]
        string product_url;
#endif

        public bool attempt_auto_login { get { return auto_login_toggle.isOn; } set { auto_login_toggle.isOn = value; } }

        public eWULUGUIState active_state { get; private set; } = eWULUGUIState.Inactive;

        GameObject active_screen = null;

        void DisplayScreen( GameObject screen )
        {
            active_state = eWULUGUIState.Active;

            if ( null != active_screen && screen != active_screen )
                HideActiveScreen();
            active_screen = screen;
            active_screen.SetActive( WPServer.ServerState == WPServerState.None );
        }

        public void ShowActiveScreen() => active_screen?.SetActive( true );
        public void HideActiveScreen() => active_screen?.SetActive( false );

        void OnServerStateChanged( WPServerState state )
        {
            if ( state == WPServerState.Contacting )
                HideActiveScreen();
            else
                if ( active_state == eWULUGUIState.Active )
                ShowActiveScreen();
        }

        void Start()
        {
            if ( this == Instance )
            {
                InitWULoginGUI();
            }
        }

        virtual protected void InitWULoginGUI()
        {
            localization_change = PlayerPrefs.GetInt( localisation_pref_name, 0 );
            int id = -1;
            foreach ( MBSLocalisationBase local in MBSLocalisationList.AllLogin )
            {
                id++;
                if ( null == local )
                    continue;
                WULocalizationButton lb = Instantiate<WULocalizationButton>( localisation_button_prefab );
                lb.transform.SetParent( localisation_grid.transform, false );
                lb.SetId( id, local.LocalisationGraphic );
            }
            DoLocalisation();

            WUCookie.LoadStoredCookie();
            if ( PlayerPrefs.HasKey( "Remember Me" ) )
            {
                attempt_auto_login = PlayerPrefs.GetInt( "Remember Me", 0 ) > 0;
                fields.login_username.text = PlayerPrefs.GetString( "username", string.Empty );
                fields.login_password.text = PlayerPrefs.GetString( "password", string.Empty );
            }

            //if this script is loaded while already logged in, go to the post login menu or else show the login menu
            DisplayScreen( WULogin.logged_in ? panels.post_login_menu_screen : panels.login_menu );

            //setup all the actions that will take place when buttons are clicked
            SetupResponders();

            //if "Remember me" was selected during the last login, try to log in automatically...
            if ( attempt_auto_login && !WULogin.logged_in )
                WULogin.AttemptAutoLogin();
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

        void DisplayErrors( CMLData error ) => StatusMessage.Message = error.String( "message" );

        #region Server contact
        public void DoLogin()
        {
            PlayerPrefs.SetInt( "Remember Me", attempt_auto_login ? 1 : 0 );
            CMLData data = new CMLData();
            data.Set( "username", fields.login_username.text.Trim() );
            data.Set( "password", fields.login_password.text.Trim() );
            WULogin.AttemptToLogin( data );
            DisplayScreen( panels.login_menu );
        }

        public void DoTrustedLogin( string email )
        {
            email = email.Trim();
            if ( !email.IsValidEmailFormat() )
            {
                Debug.LogWarning( $"{email} is not a valid email address" );
                return;
            }
            CMLData data = new CMLData();
            data.Set( "email", email );
            WULogin.AttemptTrustedLogin( data );
            DisplayScreen( panels.login_menu );
        }

        public void DoResumeGame()
        {
#if WUSKU
            if ( WULogin.RequireSerialForLogin && !WULogin.HasSerial )
            {
                DisplayScreen( panels.serialnumber_screen );
                StatusMessage.Message = "Product requires registration before you may continue";
                return;
            }
#endif

            active_state = eWULUGUIState.Inactive;
            active_screen.SetActive( false );
            WULogin.onResumeGame?.Invoke();
        }

        public void DoRegistration()
        {
            if ( fields.register_email.text.Trim() == string.Empty || fields.register_password.text.Trim() == string.Empty || fields.register_username.text.Trim() == string.Empty )
            {
                StatusMessage.Message = localisation.AllFieldsRequired;
                DisplayScreen (panels.login_menu);
                return;
            }
            if ( fields.register_verify.text.Trim() != fields.register_password.text.Trim() )
            {
                StatusMessage.Message = localisation.FailedVerification;
                DisplayScreen (panels.login_menu);
                return;
            }
            if ( !fields.register_email.text.Trim().IsValidEmailFormat() )
            {
                StatusMessage.Message = localisation.InvalidEmail;
                DisplayScreen (panels.login_menu);
                return;
            }

            CMLData data = new CMLData();
            data.Set( "username", fields.register_username.text.Trim() );
            data.Set( "email", fields.register_email.text.Trim() );
            data.Set( "password", fields.register_password.text.Trim() );
            WULogin.RegisterAccount( data );
            DisplayScreen( panels.login_menu );
        }

        public void DoFetchAccountInfo() => WULogin.FetchPersonalInfo();
        public void LogOut() => WULogin.LogOut();

        public void DoProductRegistration()
        {
#if WUSKU
            WUSerials.RegisterSerial( fields.serial_number.text.Trim(), OnRegistrationSucceeded, OnRegistrationFailed );
#endif
        }

#if WUSKU
        void OnRegistrationSucceeded( CML response )
        {
            Canvas c = GetComponent<Canvas>();
            if (null == c) c = GetComponentInParent<Canvas>();

            if (null != c)
                MBSNotification.SpawnInstance(c , new Vector2( 270f, -30f ), new Vector2( -20f, -30f ), localisation.RegistrationSuccessHeader, localisation.RegistrationSuccessMessage );
            WULogin.HasSerial = true;
            WULogin.SerialNumber = response [0].String( "serial" );
            bl_SceneLoaderUtils.GetLoader.LoadLevel("UMenuPro");
        }

        void OnRegistrationFailed( CMLData response )
        {
            StatusMessage.Message = response.String( "message" );
            DisplayScreen( panels.serialnumber_screen );
        }
#endif

        public void DoInfoUpdates()
        {
            CMLData data = new CMLData();

            if ( fields.personal_email.text != string.Empty )
            {
                if ( !fields.personal_email.text.Trim().IsValidEmailFormat() )
                {
                    StatusMessage.Message = localisation.InvalidEmail;
                    return;
                }
                data.Set( "email", fields.personal_email.text.Trim() );
            }
            else
            {
                StatusMessage.Message = localisation.EmailRequired;
                return;
            }
            data.Set( "website", fields.personal_url.text.Trim() );
            data.Set( "descr", fields.personal_bio.text.Trim() );
            data.Set( "yim", fields.personal_yim.text.Trim() );
            data.Set( "jabber", fields.personal_jabber.text.Trim() );
            data.Set( "aim", fields.personal_aol.text.Trim() );
            data.Set( "fname", fields.personal_name.text.Trim() );
            data.Set( "lname", fields.personal_surname.text.Trim() );
            data.Set( "nname", fields.personal_nickname.text.Trim() );
            data.Set( "dname", fields.personal_display_name.text.Trim() );
            WULogin.UpdatePersonalInfo( data );
            DisplayScreen( panels.post_login_menu_screen );
        }

        public void DoPasswordChange()
        {
            if ( fields.pass_change_old.text.Trim() == string.Empty )
            {
                StatusMessage.Message = localisation.ProvideCurrentPassword;
                return;
            }
            if ( fields.pass_change_new.text.Trim() == string.Empty )
            {
                StatusMessage.Message = localisation.ProvideNewPassword;
                return;
            }
            if ( fields.pass_change_new.text.Trim() != fields.pass_change_verify.text.Trim() )
            {
                StatusMessage.Message = localisation.FailedVerification;
                return;
            }

            CMLData data = new CMLData();
            data.Set( "password", fields.pass_change_old.text.Trim() );
            data.Set( "passnew", fields.pass_change_new.text.Trim() );
            WULogin.ChangePassword( data );
            DisplayScreen( panels.post_login_menu_screen );
        }

        public void DoPasswordReset()
        {
            fields.pass_reset_email.text = fields.pass_reset_email.text.Trim();
            if ( fields.pass_reset_email.text == string.Empty && fields.pass_reset_username.text.Trim() == string.Empty )
            {
                StatusMessage.Message = localisation.NeedEmailOrUsername;
                return;
            }
            string login = fields.pass_reset_email.text == string.Empty ? fields.pass_reset_username.text.Trim() : fields.pass_reset_email.text;
            if ( fields.pass_reset_email.text != string.Empty && !fields.pass_reset_email.text.IsValidEmailFormat() )
            {
                StatusMessage.Message = localisation.InvalidEmail;
                return;
            }
            CMLData data = new CMLData();
            data.Set( "login", login );
            WULogin.ResetPassword( data );
        }
#endregion

#region Server response handlers
        //upon successful login, the fields you requested to be returned are stored in CMLData fetched_info
        //and are left available to you until logout.
        virtual public void OnLoggedIn( CML _data )
        {
            //remember the "Remember me" choice...
            PlayerPrefs.SetInt( "Remember Me", attempt_auto_login ? 1 : 0 );
            if (attempt_auto_login)
            {
                PlayerPrefs.SetString("username", fields.login_username.text);
                PlayerPrefs.SetString("password", fields.login_password.text);
            }

            //remove the password from the textfield
            //fields.login_password.text = "";


#if WUSKU
            fields.serial_number.text = WULogin.SerialNumber;

            //return to main menu and set it out of view...
            //unless you require a serial first...
            if ( WULogin.RequireSerialForLogin )
            {
                if ( WULogin.HasSerial )
                {
                    DisplayScreen( panels.post_login_menu_screen );
                    active_state = eWULUGUIState.Inactive;
                    active_screen.SetActive( false );
                }
                //and don't have one...
                else
                {
                    DisplayScreen( panels.serialnumber_screen );
                }
            }
            else
            {
                DisplayScreen( panels.post_login_menu_screen );
                active_state = eWULUGUIState.Inactive;
                active_screen.SetActive( false );
            }
#else
            //return to main menu and set it out of view...
            DisplayScreen( panels.post_login_menu_screen );

            active_state = eWULUGUIState.Inactive;
            active_screen.SetActive( false );            
#endif
        }

        virtual public void OnLoggedOut( CML data )
        {
            StatusMessage.Message = $"{WULogin.display_name} logged out successfully";
            WULogin.logged_in = false;
            WULogin.nickname = WULogin.display_name = string.Empty;
            DisplayScreen( panels.login_menu );
        }

        virtual public void OnReset( CML data )
        {
            StatusMessage.Message = "Password reset emailed to your registered email address";
            DisplayScreen( panels.login_menu );
            fields.pass_reset_email.text = fields.pass_reset_username.text = string.Empty;
        }

        virtual public void OnAccountInfoReceived( CML data )
        {
            fields.personal_aol.text = data [0].String( "aim" );
            fields.personal_bio.text = data [0].String( "descr" );
            fields.personal_display_name.text = data [0].String( "dname" );
            fields.personal_email.text = data [0].String( "email" );
            fields.personal_jabber.text = data [0].String( "jabber" );
            fields.personal_name.text = data [0].String( "fname" );
            fields.personal_nickname.text = data [0].String( "nname" );
            fields.personal_surname.text = data [0].String( "lname" );
            fields.personal_url.text = data [0].String( "website" );
            fields.personal_yim.text = data [0].String( "yim" );
            ShowAccountDetailsScreen();
        }

        virtual public void OnPasswordChanged( CML data )
        {
            fields.pass_change_old.text = fields.pass_change_new.text = fields.pass_change_verify.text = string.Empty;
            OnLoggedOut( data );
            StatusMessage.Message = "Password successfully changed";
        }

        virtual public void OnRegistered( CML data )
        {
            StatusMessage.Message = "Registration successful...";
            DisplayScreen( panels.login_menu );
        }

        virtual public void OnAccountInfoUpdated( CML data )
        {
            WULogin.nickname = fields.personal_nickname.text.Trim();
            WULogin.display_name = fields.personal_display_name.text.Trim();
            WULogin.email = fields.personal_email.text.Trim();
            WULogin.website = fields.personal_url.text.Trim();

            DisplayScreen( panels.post_login_menu_screen );
        }
#endregion

#region ugui accessors
        override public void ShowLoginMenuScreen() => DisplayScreen( panels.login_menu );
        public void ShowPreLoginMenu() => DisplayScreen( panels.login_menu );
        public void ShowPostLoginMenu() => DisplayScreen( panels.post_login_menu_screen );
        public void ShowLoginScreen() => DisplayScreen( panels.login_screen );
        public void ShowRegisterScreen() => DisplayScreen( panels.register_screen );
        public void ShowPostLoginScreen() => DisplayScreen( panels.post_login_menu_screen );
        public void ShowPasswordResetScreen() => DisplayScreen( panels.password_reset_screen );
        public void ShowPasswordChangeScreen() => DisplayScreen( panels.password_change_screen );
        public void ShowAccountDetailsScreen() => DisplayScreen( panels.personal_info_screen );
        public void ShowTermsAndConditionScreen () => DisplayScreen (panels.termsandcondition_screen );
        public void CloseHighScoresScreen() => DisplayScreen( panels.login_menu );
        public void ReturnFromSerialScreen() => DisplayScreen( WULogin.logged_in ? panels.post_login_menu_screen : panels.login_menu );
        public void ShowLocalizationScreen()
        {
            DisplayScreen( panels.localization_screen );
            panels.localization_screen.BroadcastMessage( "SelectALanguage", MBSLocalisationList.Login.Selected );
        }


        public void GoToWebsite()
        {
            Application.OpenURL(
#if WUSKU
            product_url.Trim() != string.Empty ? product_url.Trim() : 
#endif
            WPServer.Instance.SelectedURL );
        }

        //in case you want to show your hgh scores on the menu page, this is a way for you to trigger that
        //it is entirely optional but I am including it since I needed it recently and, just in case you do also, here it is :)
        public void ShowHighScoresScreen()
        {
#if WUS
            if (fetch_scores_on_showing_panel)
                WUScoring.FetchScores();
#endif
            DisplayScreen( panels.high_score_screen );
        }

        //this only loads a screen where you can show your shop, nothing more
        //instead of showing this panel you can use this function to load a shop scene instead
        //whichever way you go, this function allows you to setup the shop open trigger via the inspector
        public void ShowShopScreen() => DisplayScreen( panels.shop_screen );
        
        public void ShowCustomScreen( int which = 0 )
        {
            //in preparation of you adding more custom screens...
            switch ( which )
            {
                case 0:
                    DisplayScreen( panels.custom_1 );
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
            DisplayScreen( panels.serialnumber_screen );
#endif
        }

#endregion
    }
}
