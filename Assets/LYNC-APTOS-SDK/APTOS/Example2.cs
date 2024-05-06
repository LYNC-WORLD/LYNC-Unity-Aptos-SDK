using LYNC;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Example2 : MonoBehaviour
{
    public Button login, logout, sendTransaction;
    public TMP_Text publicAddress, loginDateTxt;
    [Space]
    public Transaction transaction;

    private void OnEnable()
    {
        login.interactable = false;
        logout.interactable = false;
        sendTransaction.interactable = false;
        LyncManager.onLyncReady += LyncReady;
    }

    private async void LyncReady(LyncManager Lync)
    {
        AuthBase authBase;
        try
        {
            authBase = await AuthBase.LoadSavedAuth(OnSessionExpired);
            if (authBase.WalletConnected)
            {
                OnWalletConnected(authBase);
            }
            else
            {
                login.interactable = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            logout.interactable = true;
        }

        login.onClick.AddListener(() => Lync.WalletAuth.ConnectWallet((wallet) => OnWalletConnected(wallet)));

        logout.onClick.AddListener(OnLogout);

        sendTransaction.onClick.AddListener(async () =>
        {
            Debug.Log("Transaction started");
            TransactionResult txResult = await LyncManager.Instance.TransactionsManager.SendTransaction(transaction);
            if (txResult.success)
            {
                var newGO = new GameObject();
                newGO.transform.parent = publicAddress.transform.parent.parent;
                newGO.AddComponent<TextMeshProUGUI>().text = "SUCCESS - Click to check on explorer";
                EventTrigger trigger = newGO.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((eventData) => { Application.OpenURL("https://explorer.aptoslabs.com/txn/" + txResult.hash + "?network=" + LyncManager.Instance.Network.ToString()); });
                trigger.triggers.Add(entry);
            }
            else
            {
                var newGO = new GameObject();
                newGO.transform.parent = publicAddress.transform.parent.parent;
                newGO.AddComponent<TextMeshProUGUI>().text = "ERROR: " + txResult.response;
                newGO.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        });

    }

    private void OnWalletConnected(AuthBase _authBase)
    {
        publicAddress.text = _authBase.PublicAddress;
        loginDateTxt.text = _authBase.LoginDate.ToString();

        login.interactable = false;
        logout.interactable = true;
        sendTransaction.interactable = true;
    }
    private void OnLogout()
    {
        LyncManager.Instance.WalletAuth.Logout();
        login.interactable = true;
        logout.interactable = false;
        sendTransaction.interactable = false;
        publicAddress.text = "";
        loginDateTxt.text = "";
    }
    private void OnSessionExpired()
    {
        Debug.Log("Session expired.");
    }

}
