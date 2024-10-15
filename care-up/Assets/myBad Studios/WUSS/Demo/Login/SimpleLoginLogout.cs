using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MBS
{
    /// <summary>
    /// This is a very, very simple script to demonstrate how to log in and out of your site without the use of a GUI.
    /// The goal is to demonstrate that the included prefab is only one example. Just derive from WULogin like this
    /// scrpt does and you are free to design your own GUI using NGUI or any other system you want. 
    /// Everything you need pre-and post login is all contained inside of WULogin.cs
    /// 
    /// To use this demonstration code:
    /// 1. Drop this script on a Button on a Canvas
    /// 2. Add the WPServer component to the scene.
    /// 3. Enter your username and password into the fields below 
    /// </summary>
    public class SimpleLoginLogout : MonoBehaviour, IPointerClickHandler
    {
        public string user_name, password;

        //make sure the is a text component on your button!!!
        Text button_text;

        //stored data to improve performance a tad
        CMLData login_credentials;
        Button self;

        void Start()
        {
            //first, decide what functions to call when something happens
            WULogin.onLoggedIn += OnLoggedIn;
            WULogin.onLoggedOut += OnLoggedOut;
            WULogin.onLoginFailed += HandleErrors;
            WULogin.onLogoutFailed += HandleErrors;

            //optional but a nice touch. Disable the button while waiting for a response from the server
            WPServer.OnServerStateChange += OnServerStateChange;

            //get and store refrences now so I don't have to keep doing it later
            self = GetComponent<Button>();
            button_text = GetComponentInChildren<Text>();

            //create the demo login details from the fields you provided in the inspector
            login_credentials = new CMLData();
            login_credentials.Set( "username", user_name );
            login_credentials.Set( "password", password );

            //and let's see if we can login immediately...
#if !UNITY_EDITOR

            WULogin.AttemptAutoLogin();
#endif
        }

        //upon login or logout, change the text on the button to show what will happen when you click it
        //in the event that you didn't provide a valid url or login details, print a message when the server fails
        //and while you are contacting the server, disable teh button to prevent duplicate server instructions
        void OnLoggedIn( CML response ) => button_text.text = "Log out";
        void OnLoggedOut( CML response ) => button_text.text = "Log In";
        void OnServerStateChange( WPServerState state ) => self.interactable = state == WPServerState.None;
        void HandleErrors( CMLData response ) => Debug.LogWarning( response.String( "message" ) );

        //When you click on this button you will either log in or out so first check whether
        //you ARE currently logged IN or OUT then call the function to do the opposite...
        public void OnPointerClick( PointerEventData data )
        {
            if ( WULogin.logged_in )
                WULogin.LogOut();
            else
                WULogin.AttemptToLogin(login_credentials);
        }
    }
}