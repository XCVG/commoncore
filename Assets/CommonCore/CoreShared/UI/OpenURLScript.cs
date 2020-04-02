using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Simple script that provides a callable method that opens a URL
    /// </summary>
    public class OpenURLScript : MonoBehaviour
    {
        public string URL;
        
        public void OpenURL()
        {
            Application.OpenURL(URL);
        }
    }
}