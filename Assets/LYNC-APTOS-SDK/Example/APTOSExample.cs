using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class APTOSExample : MonoBehaviour
{
    [Header("General settings")]
    public Button login, logout, mint;

    [Space]
    [Header("Aptos")]
    public Transform aptosContainer;
    public TMP_Text WalletAddressText, loginDateTxt, balance;

    [Space]
    [Header("Pontem")]
    public Transform pontemContainer;
    public TMP_Text pontemPublicAddress;

    [Space]
    [Header("Transactions")]
    public Transform transactionResultsParent;
    public GameObject transactionResultHolder;
    public Transaction mintTxn;

    private AuthBase authBase;
    public static APTOSExample Instance;

    private void OnEnable()
    {
        LyncManager.onLyncReady += LyncReady;
    }

    private void Awake()
    {
        Instance = this;

        login.interactable = false;
        logout.interactable = false;
        mint.interactable = false;
        Application.targetFrameRate = 30;
    }

    private async void LyncReady(LyncManager Lync)
    {
        try
        {
            authBase = await AuthBase.LoadSavedAuth();
            if (authBase.WalletConnected)
            {
                Debug.Log("Saved wallet successfully loaded");
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

        login.onClick.AddListener(() =>
        {
            Lync.WalletAuth.ConnectWallet((wallet) =>
            {
                Debug.Log(wallet.WalletConnected);
                Debug.Log(wallet.PublicAddress);
                OnWalletConnected(wallet);
            });
        });

        logout.onClick.AddListener(() =>
        {
            Lync.WalletAuth.Logout();
            login.interactable = true;
            logout.interactable = false;
            mint.interactable = false;
            Populate();
        });

        List<TransactionArgument> arguments = new List<TransactionArgument>{
            new TransactionArgument{ argument = "0xb66b180422a4886dac85b8f68cc42ec1c6bafc824e196d437fdfd176192c25fccfc10e47777699420eec0c54a0176861a353a43dd45b338385e1b975709f2000", type = ARGUMENT_TYPE.STRING }
        };

        mint.onClick.AddListener(async () =>
        {
            // mint.interactable = false;

            TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(
                mintTxn
            );
            if (txData.success)
                SuccessfulTransaction(txData.hash, "MINT");
            else
                ErrorTransaction(txData.error);

            mint.interactable = true;
        });

    }

    private void OnWalletConnected(AuthBase _authBase)
    {
        EnableAppropriateComponents(AuthBase.AuthType);

        if (AuthBase.AuthType == AUTH_TYPE.FIREBASE)
        {
            Populate(_authBase as FirebaseAuth);
        }

        if (AuthBase.AuthType == AUTH_TYPE.PONTEM)
        {
            WalletAddressText.text = AbbreviateWalletAddressHex(_authBase.PublicAddress);
            StartCoroutine(API.CoroutineGetBalance(_authBase.PublicAddress, res =>
            {
                balance.text = res.ToString();
                Debug.Log("BALANCE"+balance);
            }, err =>
            {
                Debug.Log("Error");
            }));
        }

        login.interactable = false;
        logout.interactable = true;
        mint.interactable = true;
    }

    private void EnableAppropriateComponents(AUTH_TYPE authType)
    {
        if (authType == AUTH_TYPE.FIREBASE)
        {
            aptosContainer.gameObject.SetActive(true);
            pontemContainer.gameObject.SetActive(false);
            Debug.Log("FIREBASE auth");
        }
        if (authType == AUTH_TYPE.PONTEM)
        {
            pontemContainer.gameObject.SetActive(true);
            aptosContainer.gameObject.SetActive(false);
            Debug.Log("PONTEM auth");
        }
    }

    private void SuccessfulTransaction(string hash, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);

        if (!string.IsNullOrEmpty(hash))
        {
            go.transform.GetComponentInChildren<TMP_Text>().text = (txnTitle != "" ? ("(" + txnTitle + ")") : "") + " Success, hash = " + hash.Substring(0, 5) + "..." + hash.Substring(hash.Length - 5) + "<color=\"green\"> Check on APTOS EXPLORER<color=\"green\">";
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((eventData) => { Application.OpenURL("https://explorer.aptoslabs.com/txn/" + hash + "?network=testnet"); });
            trigger.triggers.Add(entry);
        }
        else
        {
            // Pontem mobile transactions doesnt contain a hash
            go.transform.GetComponentInChildren<TMP_Text>().text = (txnTitle != "" ? ("(" + txnTitle + ")") : "") + " Successfull transaction";
        }
    }

    private void ErrorTransaction(string error, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);
        go.transform.GetComponentInChildren<TMP_Text>().text = txnTitle + " <color=\"red\">TXN ERROR:</color=\"red\"> " + error;
    }

    public void Populate(FirebaseAuth firebaseAuth = null)
    {
        WalletAddressText.text = (firebaseAuth == null ? "Disconnected" : AbbreviateWalletAddressHex(firebaseAuth.AptosFirebaseAuthData.publicKey));
        loginDateTxt.text = "Login Date = " + (firebaseAuth == null ? "" : firebaseAuth.LoginDate.ToString());
        balance.text = (firebaseAuth == null ? "0" : firebaseAuth.AptosFirebaseAuthData.balance) + " APT";
    }

    public string AbbreviateWalletAddressHex(string hexString, int prefixLength = 4, int suffixLength = 3)
    {
        if (hexString.Length <= prefixLength + suffixLength)
        {
            return hexString; // No need for abbreviation
        }
        
        string prefix = hexString.Substring(0, prefixLength);
        string suffix = hexString.Substring(hexString.Length - suffixLength);
        
        return prefix + "..." + suffix;
    }
}
