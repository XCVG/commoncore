using CommonCore.State;
using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that makes the player camera fall and fade to black when they die
    /// </summary>
    public class PlayerDeathComponent : MonoBehaviour
    {
        [SerializeField]
        private Transform CameraNode = null;
        [SerializeField]
        private Graphic FadeGraphic = null;
        [SerializeField]
        private float FallDistance = 1.2f;
        [SerializeField]
        private float FadeoutTime = 5f;
        [SerializeField]
        private float HoldTime = 3f;                

        private bool IsDying = false;

        private void Update()
        {
            if (IsDying || LockPauseModule.IsPaused())
                return;

            if(GameState.Instance.PlayerRpgState.Health <= 0)
            {
                IsDying = true;
                StartCoroutine(DeathSequenceCoroutine());
            }
        }

        private IEnumerator DeathSequenceCoroutine()
        {
            yield return null;

            FadeGraphic.transform.parent.gameObject.SetActive(true); //hacky but will work for now

            float cameraOriginalY = CameraNode.position.y;
            for(float elapsed = 0; elapsed < FadeoutTime;)
            {
                //Debug.Log(elapsed);
                float ratio = elapsed / FadeoutTime;
                FadeGraphic.color = new Color(0, 0, 0, ratio);

                CameraNode.position = new Vector3(CameraNode.position.x, cameraOriginalY - ratio * FallDistance, CameraNode.position.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            FadeGraphic.color = Color.black;

            yield return new WaitForSeconds(HoldTime);

            EndGame();
        }


        private void EndGame()
        {
            MetaState.Instance.NextScene = SceneManager.GetActiveScene().name; //in case we need it...
            SceneManager.LoadScene("GameOverScene");
        }
    }
}