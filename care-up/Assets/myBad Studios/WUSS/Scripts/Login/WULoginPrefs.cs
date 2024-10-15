using System.Collections;
using UnityEngine;

namespace MBS
{
    public class WULoginPrefs : MonoBehaviour
    {
        static WULoginPrefs _;
        static public WULoginPrefs Instance
        {
            get
            {
                if ( null == _ )
                {
                    WULoginPrefs [] all = FindObjectsOfType<WULoginPrefs>();
                    if ( all.Length > 1 )
                    {
                        for ( int i = 1; i < all.Length; i++ )
                            Destroy( all [i] );
                    }
                    if ( all.Length > 0 )
                        _ = all [0];
                }
                return _;
            }
        }

        [Header("Fields to fetch")]
        [SerializeField] bool fetch_username = false;
        [SerializeField] bool fetch_display_name = false;
        [SerializeField] bool fetch_url = false;
        [SerializeField] bool fetch_registration = false;
        [SerializeField] bool fetch_roles = false;
        [SerializeField] bool fetch_gravatar = true; 


        [SerializeField] string[]   fetch_meta_info = new string[]{"nickname"};
        [SerializeField] int avatar_size = 128;
        [SerializeField] WULGravatarTypes gravatar_type = WULGravatarTypes.Wavatar;

        public bool FetchUsername => fetch_username;
        public bool FetchDisplayName => fetch_display_name;
        public bool FetchURL => fetch_url;
        public bool FetchRegistration => fetch_registration;
        public bool FetchRoles => fetch_roles;
        public bool FetchGravatar => fetch_gravatar;
        public string [] MetaInfo => fetch_meta_info;
        public int AvatarSize => avatar_size;
        public WULGravatarTypes GravatarType => gravatar_type;


#if WUM
        [SerializeField] string[]   currencies = new string[]{"points"};
        public string [] Currencies => currencies;
#endif

#if WUS
        [SerializeField] bool fetch_highscore = false;
        public bool FetchHighscore => fetch_highscore;
#endif

#if WUSKU
        [Header("Product registration")]
        [SerializeField] bool check_for_serial = true; //check if the game is registered during login or will you do it manually later on?
        [SerializeField] bool require_serial_for_login = true; //when true you should never allow gameplay to start unless HasSerial is true
        [SerializeField] bool fetch_serial = false; //only needed if you want to display the serial in game somewhere

        public bool RequireSerial => require_serial_for_login;
        public bool FetchSerial => fetch_serial;
        public bool CheckForSerial => check_for_serial;
#endif

        public void __internalOnProfileImageFetched( string gravatar, WULGravatarTypes type ) => ContactGravatar( gravatar, WULogin.__InternalOnProfileImageReceived, type );

        public void ContactGravatar( string gravatar, System.Action<Texture2D> response, WULGravatarTypes gravatar_type = WULGravatarTypes.Identicon ) =>
            StartCoroutine( __ContactGravatar( gravatar, response, gravatar_type ) );
        IEnumerator __ContactGravatar( string gravatar, System.Action<Texture2D> response, WULGravatarTypes gravatar_type )
        {
            if ( avatar_size < 32 )
                avatar_size = 32;
            if ( avatar_size > 512 )
                avatar_size = 512;
            string URL = $"https://www.gravatar.com/avatar/{gravatar}?s={avatar_size}&d={gravatar_type.ToString().ToLower()}";
#pragma warning disable
            WWW w = new WWW( URL );
#pragma warning restore
            yield return w;
            Texture2D avatar = null;
            if ( !string.IsNullOrEmpty( w.error ) )
            {
                avatar = new Texture2D( 1, 1 );
                avatar.SetPixel( 0, 0, Color.white );
                avatar.Apply();
            }
            else
            {
                avatar = w.texture;
            }
            response( avatar );
        }
    }
}