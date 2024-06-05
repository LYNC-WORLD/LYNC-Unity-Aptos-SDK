# LYNC-Unity-Aptos-SDK

LYNC Unity Aptos SDK is a no-code Modular Unity SDK supporting PC (MacOS and Windows) and Mobile (Android and iOS) on Aptos. 

### **Platform Supported:** PC (Windows and MacOs) and Mobile (Android and iOS)

### **Network Supported:** Aptos Testnet and Mainnet

This release includes the following:
- Social Logins
- Keyless Login
- Pontem Wallet Login
- Custom Transactions in Social Login and Web3 Wallet like Pontem
- Paymaster inbuilt to sponsor transactions for your users.

## Get your API Key
Please get your API key before downloading the SDK from [here](https://lync.world/form.html)

## Installation
Download the LYNC Unity Aptos SDK from [Here​](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/releases)

**Example Project:** https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK​

Import the SDK .unitypackage file to your project. or simply drag and drop .unitypackage file to your project.

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/39f2c3a3-5b36-4456-8db9-69022fc25bb6)

Once the LYNC Aptos SDK package has finished importing into your Unity project, you can begin integrating it into your game.
The Folder structure looks like this

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/cda6370f-3b43-49af-b9ed-bdd4bddb99fe)

## Integrating LYNC Aptos SDK in Unity

There is 1 Example Projects present in the SDK: 

Assets/LYNC-APTOS-SDK/Example/APTOSExample.unity

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/e348486e-45b5-4447-9247-83944d0e7f45)

You can find the example scene in the folders. Simply pass the API key in LyncManager GameObject.

To test, Build and Run after adding this scene in (Scene in Build).

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/4120d692-eb5e-49f2-8a87-3f688442d195)

## Setup the Project
To use LYNC Aptos SDK. Attach LYNC Manager Prefab(Assets/LYNC-APTOS-SDK/LYNC Manager.prefab), on the starting scene.
This will serve as the starting point for your project. In LYNC Manager Prefab, be sure to provide the following details:

- LYNC API Key ([The API Key can be generated from here](https://lync.world/form.html))
- Choose Network -> Testnet / Mainnet
- Sponsor Transactions -> If you want to sponsor transactions for users ([Please contact LYNC](https://calendly.com/shanu-lync))
- Login Options -> Allowing users to choose which login method to login from

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/6a954c5c-2225-46fe-9cd4-9aa61df66e92)

- Pass a deep link name (example: lyncaptos/gameName etc.)

## Integrating Login or Transaction Layer via LYNC Aptos SDK in Unity

The Sample Code for Login can be found at APTOSExample.cs.
```
Make sure to Import LYNC.
```

### Example (Event Trigger):
LYNC ready Should be a function which has an argument of type "LyncManager"

```
LyncManager.onLyncReady += LyncReady;

private void LyncReady(LyncManager Lync)
    {
        // Once LYNC is ready, you can do any steps like Login, Logout, Transactions etc.
        
        //To Login:
        Lync.WalletAuth.ConnectWallet((wallet) =>
        {
            OnWalletConnected(wallet);
        });
        
        //To Logout:
        Lync.WalletAuth.Logout();
        
    }
```

To Check if the user is logged in or not:

```
using LYNC;

private AuthBase authBase;

    authBase = await AuthBase.LoadSavedAuth();
        if (authBase.WalletConnected)
        {
            // User is Already Login
            OnWalletConnected(authBase);
        }
        else{
             // Ask user to login
        }
```

On Wallet Connected (TypeOfLoginMethod)

```
//To OnWalletConnected(TypeOfLoginMethod):
        
        private void OnWalletConnected(AuthBase _authBase)
        {
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
        }
        

        public void Populate(FirebaseAuth firebaseAuth = null)
        {
            WalletAddressText.text = (firebaseAuth == null ? "Disconnected" : AbbreviateWalletAddressHex(firebaseAuth.AptosFirebaseAuthData.publicKey));
            balance.text = (firebaseAuth == null ? "0" : firebaseAuth.AptosFirebaseAuthData.balance) + " APT";
        }
```

To Logout directly:
```
LyncManager.Instance.WalletAuth.Logout();
```

## Transaction Flow 
There are two methods for proceeding with a transaction:

- **Gasless Transaction -** Game Developer/ Game Studio will be sponsoring the transaction fee.
- **User Paid Transaction-** The Gamer/ User will be responsible for paying the gas fee and cost if any, required to do the transaction.

### Gasless Transaction
To Enable Gasless Transactions or to Sponsor Transactions for your users, 
Set Sponsor Transactions as true, and [contact LYNC team to setup your paymaster](https://calendly.com/shanu-lync)

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/8c340731-61d9-4187-8d12-d18db882bfff)

To do transactions, APTOSExample.cs can be taken as a reference.

Pass in the Contract Address, Contract Name, Function Name and Network.

Arguments are not compulsory parameters, but if the function accepts any argument, make sure to pass them.

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/1ac484a3-d4b7-4fac-a50b-6b46449a7b9a)

```
LyncManager.Instance.TransactionsManager.SendTransaction(Transaction);
```

You can create a public Transaction Object, pass in the parameters and hit the function call where you want to do the transactions

```
public Transaction mintTxn;

TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(mintTxn);

if (txData.success)
    SuccessfulTransaction(txData.hash, "MINT");
else
    ErrorTransaction(txData.error);

```

Or You can create a Transaction Object, 

```
public Transaction mintTxn;

//LyncManager.Instance.TransactionsManager.SendTransaction( new Transaction(ContractAddress, ContractName, FunctionName,ListOfArguments));

TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(
new Transaction("0x55db3f109405348dd4ce271dc92a39a6e1cbc3d78cf71f6bf128b1c8a9dfac33","tst_unity","set_data_bytes",arguments));

if (txData.success)
    SuccessfulTransaction(txData.hash, "MINT");
else
    ErrorTransaction(txData.error);

```

List of Arguments: 

```
//List<TransactionArgument>{
//    new TransactionArgument{ argument = value, type = ARGUMENT_TYPE.STRING }
//};

List<TransactionArgument> arguments = new List<TransactionArgument>{
    new TransactionArgument{ argument = "0xb66b180422a4886dac85b8f68cc42ec1c6bafc824e196d437fdfd176192c25fccfc10e47777699420eec0c54a0176861a353a43dd45b338385e1b975709f2000", type = ARGUMENT_TYPE.STRING }
};
```

## Some common bugs and their resolutions

**Problem**: Newtonsoft JSON is missing.
**Solution**: Please, Add this as a git URL in adding package

```
com.unity.nuget.newtonsoft-json
```

![image](https://github.com/LYNC-WORLD/LYNC-Unity-Aptos-SDK/assets/42548654/f55fe529-951c-4eca-a80a-843d9ae2c30e)
