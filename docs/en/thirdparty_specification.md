
<h1 align="center">KYC Requestor Access Standards </h1>
<p align="center" class="version">Version 0.8.0 </p>

## Overview

The Ontology collaborative trust system is able to provide authenticity verification services to KYC requestor for related people, events and object, by working with certified TrustAnchors from Ontology. 

![flow](https://github.com/ontio/ontology-DID/blob/master/images/thirdparty_flow.png)


## How it works

![](http://on-img.com/chart_image/5b62a85fe4b025cf4932deb2.png)


- A1：KYC requestor will register their basic information and recall function and then choose the certification template on the OntPass platform. OntPass platform will form a official QR code for KYC reqestor.
- A2，A3：Users will scan the official QR code provided by the KYC requestor within the ONTO App and trigger the verification request to Ontpass after scanning the QR code. OntPass platform will verify the QR code.
- A4：OntPass will feedback the basic information and verification requirement to ONTO App after the successful confirmation of the QR code.
- A5：Users will delegate the verification right on ONTO App. After choosing the correct verifiable claim for the verification, XX will send the encrypted statement to Ontpass.
- A6：Ontpass will deliver the encrypted verifiable claim to KYC requestor by using the registered information and the recall function. KYC requestor will then be able to obtain the verifiable claim by decrypting with his private key. 
- A7：The KYC requestor verifies the integrity and validity of the verifiable claims presented by the user through the blockchain.



## Access Procedures

### 1. Registeration on OntPass Platform

To receive the authenticity service in the Ontology trust system, a KYC requestor needs to register on the OntPass platform.

OntPass provides different verification templates for different verifiable claim from different TrustAnchors (Please refer to **OntPass verification template** section). A KYC requestor is able to choose from different templates and register by using the API of a KYC requestor. The registration requires basic information, label of the verification templates and recall function etc.


#### API /API for Registration of a KYC Requestor

```json
url：/api/v1/ontpass/thirdparty?version=0.8
method：POST
requestExample：
{
    "OntId":"did:ont:Assxxxxxxxxxxxxx",
	"NameCN":"COO",
	"NameEN":"COO",
	"DesCN":"COO Blockchain",
	"DesEN":"COO Blockchain",
	"Logo":"https://coo.chain/logo/coo.jpg",
	"Type":"Blockchain",
	"CallBackAddr":"https://coo.chain/user/authentication",
	"ReqContext":"authtemplate_kyc01",
	"Signature":"AXFqy6w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo14lHF0="
}

successResponse：
{
	"Version":"0.8",
	"Action":"RegisterThirdParty",
	"Error":0,
	"Desc":"SUCCESS",
	"Result":true
}
```

| UrlParam     |     Type |   Description   |
| :--------------: | :--------:| :------: |
|    version|   String | Currently 0.8 |


| RequestField     |     Type |   Description   |
| :--------------: | :--------:| :------: |
|    OntId|   String|  KYC requestor OntId  |
|    NameEN|   String|  Name of the KYC requestor in English  |
|    NameCN|   String|  Name of the KYC requestor in Chinese  |
|    DesEN|   String|  Description of the KYC requestor in English  |
|    DesCN|   String|  Description of the KYC requestor in Chinese  |
|    Logo|   String|  URL of the KYC requestor logo  |
|    CallBackAddr|   String|  Address of recall function in http format |
|    Type|   String|  Category of the KYC requestor|
|    ReqContext|   String|  Logo of the verifiable claim template. Template will be provided by OntPass。 |
|    Signature|   String|  Signature of the requsted information。KYC requestor will signify by using ECDSA algorithm with his private key |



| ResponseField     |     Type |   Description   |
| :--------------: | :--------:| :------: |
|    Result|   Boolean|  true：registration successed  false：registration failed |


> Note: For security purposes, KYC requestor must use http+ address as his recall inteface. In the meantime, KYC requestor needs to ensure the recall interface accepts post request from OntPass.


### 2. Generate QR Code

KYC Requestor will need to follow the OntPass standards in order to generate the official QR code. The OntID of the KYC requestor, language, expire date, verifiable claim template and signature needs to be embedded into the QR code. There is a 7% faulty tolerance when generating the QR code

The signature is used for verifying the identity of a KYC requestor. ONTO App will return the KYC requestor basic information back to user after the verifying the QR code.

Example for generating a QR code:

```
{
	"OntId":"did:ont:A17j42nDdZSyUBdYhWoxnnE5nUdLyiPoK3",
	"Exp":1534838857,
	"Sig":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}
```
or
```
{
	"OntId":"did:ont:A17j42nDdZSyUBdYhWoxnnE5nUdLyiPoK3",
	"Exp":1534838857,
	"ReqContext":"authtemplate_kyc02",
	"Sig":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}
```


| Field     |     Type |   Description   | 
| :--------------: | :--------:| :------: |
|    OntId|   String|  KYC Requestor OntId  |
|    Exp|   int|  Expired date，unix timestamp  |
|    ReqContext|   String|  Optional. verifiable claim template。  |
|    Sig|   String|   Signature of the KYC requestor by using the private key |



> Note: The OntID in the QR code must match the OntID registered in the OntPass platform. The signature must use standard ECDSA algorithm


### 3. Receive verifiable from user

User will delegate the verifying rights by scanning the QR code of the KYC requestor. If the delegation passes on ONTO App, the encrypted verifiable claim will be sent to OntPass, and then redirected to KYC requestor through the recall interface.

User can encrypt the verifiable claim with the KYC requestor's public. This will ensure security during the whole data transfer process.

Therefore the recall interface provided by KYC requestor must accept the POST request below

```json
url：recall interface from third party:
method：POST
requestExample：
{
	"Version":"0.8",
	"AuthId":"123123",
	"OntPassOntId":"did:ont:AMBaMGCzYfrV3NyroxwtTMfzubpFMCv55c",
	"UserOntId":"did:ont:AXZUn3r5yUk8o87wVm3tBZ31mp8FTaeqeZ",
	"EncryClaims": [
		"eyJraWQiOiJkaWQ6b250OkFScjZBcEsyNEVVN251Zk5ENHMxU1dwd1VMSEJlcnRwSmIja2V5cy0xIiwidHlwIjoiSldULVgiLCJhbGciOiJPTlQtRVMyNTYifQ==.eyJjbG0tcmV2Ijp7InR5cCI6IkF0dGVzdENvbnRyYWN0IiwiYWRkciI6IjM2YmI1YzA..."
	],
	"Signature":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}

```

| RequestField     |     Type |   Description   | 
| :--------------: | :--------:| :------: |
|    Version|   String|  Version No.Currently is 0.8  |
|    AuthId|   String|   OntPass platform authorization code  |
|    OntPassOntId|   String| OntID of the OntPass |
|    UserOntId|   String|  OntID of the user|
|    EncryClaims|   list|  list of verifiable claim after encryption |
|    Signature|   String|  signature of the requested information from Ontpass |


### 4.可信声明验证 Verify the Verifiable claim

After the KYC requestor receives the encrypted verifiable claim, the requestor is able to decrypt by using his private key of his OntID and verify the completeness ad effective of that verifiable claim on the blockchain. Detailed explation of verifiable claim can be found in the annex **Verifiable Claim Standards**. You can refer to the SDKs provided by Ontology.

[JAVA SDK verifiable claim verification](https://ontio.github.io/documentation/ontology_java_sdk_identity_claim_en.html#3-verify-verifiable-claim)


[TS SDK verifiable claim verification](https://ontio.github.io/documentation/ontology_ts_sdk_identity_claim_en.html#verifiable-claim-verification)



## OntPass平台认证模板

Based on some veriable claims issued by different TrustAnchors, the OntPass platform defines some basic authentication templates for common kyc  authentication applications.

KYC requestors can also freely combine various veriable claims and generate authorization logic rules according to their own business needs. In this way, the KYC requestor can flexibly select the authentication template when registering or generating the QR code on the OntPass platform.

** Verifiable Claim **

| veriable claim id     |     explanation |  issuer   | 
| :--------------: | :--------:| :------: |
|    claim:email_authentication|   Personal email authentication|  ONTO  |
|    claim:mobile_authentication|   Personal cell number authentication|  ONTO  |
|    claim:cfca_authentication|   Chinese citizen authentication|  CFCA |
|    claim:sensetime_authentication|   中国公民实名认证可信声明|  SenseTime |
|    claim:idm_passport_authentication|   Personal passport authentication|  IdentityMind |
|    claim:idm_idcard_authentication|    global personal ID card authentication|  IdentityMind |
|    claim:idm_dl_authentication|    global personal driver's license authentication|  IdentityMind |
|    claim:github_authentication|   Github social media authentication|  Github |
|    claim:twitter_authentication|   Twitter social media authentication|  Twitter |
|    claim:linkedin_authentication|   Linkedin social media authentication| Linkedin |
|    claim:facebook_authentication|   Facebook social media authentication|  Facebook |


**Authentication Template:**

Authentication template includes identification of templates, category, description, identification of corresponding verifiable claim template, authentication logic, unit price and etc.



| Type of verifiable claim | Identification of verifiable claim | Description of verifiable claim | Template with corresponding verifiable  | Authentication logic |
| :--------: | :--------:|:---------:|:--------: | :--------:|
|  Social media   |   authtemplate_social01 |  basic information verification from social media  | claim:github_authentication<br><br>claim:twitter_authentication<br><br>claim:facebook_auuthentication<br><br>claim:linkedin_authentication    |   Choose anyone |
| kyc verification   |   authtemplate_kyc01 |   Verification of information for global users  | claim:idm_passport_authentication<br><br>claim:idm_idcard_authentication<br><br>claim:idm_dl_authentication |  Choose anyone |
| kyc verification   |   authtemplate_kyc02 |   Verification of information for Chinese citizen  | claim:cfca_authentication |   Mandatory |




## Incentives

It's considered typical data exchange transaction when a user is using the obtained verifiable claim to authorize a verification from the KYC requestor. OntPass platform is able to guarantee the fairness and friendliness during the data exchange transaction


##  Standard of Verifying Identification

When a user logs in to a third party with OntID, the third party can rapidly verify the owner of the OntID with JSON format request. Even if the user is using the private key of the OntID to do the signature, the KYC requestor is able to verify the signature with the public key of that particular OntID. 

```
{
	"OntId":"did:ont:A17j42nDdZSyUBdYhWoxnnE5nUdLyiPoK3",
	"Nonce":45348391,
	"Sig":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}
```

| RequestField     |     Type |   Description   | 
| :--------------: | :--------:| :------: |
|    OntId|   String|  User'sOntId  |
|    Nonce|   int|    |
|    Signature|   String|  Signature of the requested information by a user|


## Annex:

### Verifiable Claim Standards

There are three parts to form a JSON Web Token extension file verifiable claim: Header, Payload and Signature. We will attach "Blockchain_Proof" to extend JWT format, a typical verifiable claim can be arranged as:

	Header.Payload.Signature.Blockchain_Proof


**Header**

Header defines the category of the verifiable claim, the signature methodology and the ID of the public key for verifying the signature

```
{
    "alg": "ES256",
    "typ": "JWT-X",
    "kid": "did:ont:TRAtosUZHNSiLhzBdHacyxMX4Bg3cjWy3r#keys-1"
}
```


**Payload**

Payload defines the basic information of a verifiable claim and the verified content by the TrustAnchor

```
{
    "ver": "0.7.0",
    "iss": "did:ont:TRAtosUZHNSiLhzBdHacyxMX4Bg3cjWy3r",
    "sub": "did:ont:SI59Js0zpNSiPOzBdB5cyxu80BO3cjGT70",
    "iat": 1525465044,
    "exp": 1530735444,
    "jti":"4d9546fdf2eb94a364208fa65a9996b03ba0ca4ab2f56d106dac92e891b6f7fc",
    "@context":"https://example.com/template/v1",
    "clm":{
        "Name": "Bob Dylan",
        "Age": "22"
    },
    "clm-rev":{ 
        "typ": "AttestContract",
        "addr": "8055b362904715fd84536e754868f4c8d27ca3f6"
    }
}
```

- **ver**  Version of the verifiable claim
-  **iss** ONT ID of the verifiable claim issuer
-  **sub** ONT ID of the verifiable claim receiver
-  **iat** creation time of the unix timestamp
-  **exp** expire time of the unix timestamp
-  **jti** the identification of verifiable claim
-  **@context** the identification of verifiable claim  template
-  **clm** indicates the verifiable claim contents verified by TrustAnchor
-  **clm-rev** indicates the revoke method for a verifiable claim


**Signature**

After constructing "Header" and "Payload", the signature if calcuated based on JWS standards


- The input of the signature if based on serialization of "Header" and "Payload"
	
	sig := sign(Base64URL(header) || . || Base64URL(payload))


- Calcuate the signature based on specific signing method of the "Header"

- Coding towards signature
	
	signature := Base64URL(sig).


**Blockchain Proof**

Claim issuer will save the verifiable claim in the blockchain, and obtain the merkleproof from the transaction

```
{
    "Type":"MerkleProof",
    "TxnHash":"c89e76ee58ae6ad99cfab829d3bf5bd7e5b9af3e5b38713c9d76ef2dcba2c8e0",
    "ContractAddr": "8055b362904715fd84536e754868f4c8d27ca3f6",
    "BlockHeight":10,
    "MerkleRoot":"bfc2ac895685fbb01e22c61462f15f2a6e3544835731a43ae0cba82255a9f904",
    "Nodes":[{
    	"Direction":"Right",
        "TargetHash":"2fa49b6440104c2de900699d31506845d244cc0c8c36a2fffb019ee7c0c6e2f6"
    }, {
        "Direction":"Left",
        "TargetHash":"fc4990f9758a310e054d166da842dab1ecd15ad9f8f0122ec71946f20ae964a4"
    }]
}
```

- **Type** constant number"MerkleProof"
- **TxnHash** Hash value of saving the verifiable claim ID in the smart contract
- **ContractAddr**  Contract address
- **BlockHeight** Block Height
- **MerkleRoot** Merkle root of the corresponding blockheight
- **Nodes**  Nodes path of the Merkletree


**Verifiable Claim Transmission**


Verifiable claim received by KYC requestor will be arranged as the following:

	BASE64URL(Header) || '.' || BASE64URL(Payload) || '.' || BASE64URL(Signature)  '.' || BASE64URL(blockchain_proof) 


For more information, please vist (https://ontio.github.io/documentation/claim_spec_en.html)

