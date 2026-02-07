using UnityEngine;

public class PauseMenuTester : MonoBehaviour
{
    public PauseMenuController menuController;

    private void Update()
    {
       // if (Input.GetKeyDown(KeyCode.M))
        //{
          //  if (menuController)
            //{
              //  if (menuController.IsPaused) menuController.ResumeGame();
              //  else menuController.PauseGame();
            //}
       // }
    }

    [ContextMenu("Open Menu")]
    /*public void Open()
    {
        if (menuController) menuController.PauseGame();
    }*/

    [ContextMenu("Close Menu")]
    public void Close()
    {
        if (menuController) menuController.ResumeGame();
    }
}
