
<h1 align="center">场景方接入标准KYC Requestor Access Standards </h1>
<p align="center" class="version">Version 0.8.0 </p>

## 概述Overview

场景方作为本体信任生态中使用认证服务的一方，结合本体信任生态中提供认证服务的TrustAnchor，可基于可信声明、OntId、区块链来完成对现实中有关人，物，事的认证。The Ontology collaborative trust system is able to provide authenticity verification services to KYC requestor for related people, events and object, by working with certified TrustAnchors from Ontology. 

![flow](https://github.com/ontio/ontology-DID/blob/master/images/thirdparty_flow.png)


## 交互流程说明 Exchange Process

![Exchange Process](http://on-img.com/chart_image/5b62a85fe4b025cf4932deb2.png)


- A1：场景方到OntPass平台注册相关基本信息及回调接口，并选择认证模板。然后按照OntPass平台二维码规范生成场景方的认证二维码。KYC requestor will register their basic information and recall function and then choose the certification template on the OntPass platform. OntPass platform will form a official QR code for KYC reqestor.
- A2，A3：用户使用ONTO App扫描场景方出示的二维码，获取到二维码信息后向OntPass发起认证请求。OntPass平台进行二维码校验。Users will scan the official QR code provided by the KYC requestor within the ONTO App and trigger the verification request to Ontpass after scanning the QR code. OntPass platform will verify the QR code.
- A4：二维码校验成功，OntPass返回场景方注册时的基本信息和认证需求到ONTO App。OntPass will feedback the basic information and verification requirement to ONTO App after the successful confirmation of the QR code.
- A5：用户在ONTO App上进行授权决策。选择场景方所需的可信声明做授权确认，将加密后的可信声明发送到OntPass。OntPass触发智能合约进行资产交割。Users will delegate the verification right on ONTO App. After choosing the correct verifiable claim for the verification, XX will send the encrypted statement to Ontpass.
- A6：OntPass通过场景方注册的回调接口，将用户加密后的的可信声明透传到场景方，场景方可使用自己OntId对应的私钥进行解密，获取用户可信声明。Ontpass will deliver the encrypted verifiable claim to KYC requestor by using the registered information and the recall function. KYC requestor will then be able to obtain the verifiable claim by decrypting with his private key. 
- A7：场景方可通过区块链验证用户出示的可信声明的完整性和有效性。



## 接入步骤 Detail Access Procedures

### 1. OntPass平台注册 Registeration on OntPass Platform

作为本体信任生态中认证服务的使用方，场景方首先需要到OntPass平台进行注册。To receive the authenticity service in the Ontology trust system, a KYC requestor needs to register on the OntPass platform.

OntPass根据本体生态中各种认证服务提供商TrustAnchor可签发的可信声明，提供了不同类型的标准认证模板（可参考**OntPass认证模板**章节）。场景方注册时可根据自身业务场景选择所需要的认证模板，然后调用场景方注册API进行登记注册，主要包括场景方基本信息、认证模板标识及回调接口等信息。
OntPass provides different verification templates for different verifiable claim from different TrustAnchors (Please refer to **OntPass verification template** section). A KYC requestor is able to choose from different templates and register by using the API of a KYC requestor. The registration requires basic information, label of the verification templates and recall function etc.


#### 场景方注册API /API for Registration of a KYC Requestor

```json
url：/api/v1/ontpass/thirdparty?version=0.8
method：POST
requestExample：
{
    "OntId":"did:ont:Assxxxxxxxxxxxxx",
	"NameCN":"COO",
	"NameEN":"COO",
	"DesCN":"COO 区块链",
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
|    version|   String | 版本信息。目前是0.8 |


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
|    Signature|   String|  Signature of the requsted information。由场景方使用自己OntId的私钥按照标准的ECDSA算法签名。KYC requestor will signify by using ECDSA algorithm with his private key |



| ResponseField     |     Type |   Description   |
| :--------------: | :--------:| :------: |
|    Result|   Boolean|  true：registration successed  false：registration failed |


	注意：为保证数据传输安全性，场景方注册的回调接口必须是https+域名形式，同时场景方需保证注册的回调接口高可用性且接受OntPass标准的https的post请求 Remark: For security purposes, KYC requestor must use http+ address as his recall inteface. In the meantime, KYC requestor needs to ensure the recall interface accepts post request from OntPass.


### 2.生成二维码 Generate QR Code

场景方需要按照OntPass平台的规范生成标准二维码，供ONTO App扫码并进行授权决策。二维码需要嵌入场景方的OntId、语言版本标识、过期时间、认证模板（扩展项，若不填写则默认是场景方注册时登记的认证模板）以其签名。并使用7%低容错率标准生成二维码。KYC Requestor will need to follow the OntPass standards in order to generate the official QR code. The OntID of the KYC requestor, language, expire date, verifiable claim template and signature needs to be embedded into the QR code. There is a 7% faulty tolerance when generating the QR code

签名用于OntPass对场景方进行身份验证，二维码验证成功后, ONTO将场景方注册信息显示给用户。
The signature is used for verifying the identity of a KYC requestor. ONTO App will return the KYC requestor basic information back to user after the verifying the QR code.

标准二维码示例：
Example for generating a QR code:

```
{
	"OntId":"did:ont:A17j42nDdZSyUBdYhWoxnnE5nUdLyiPoK3",
	"Exp":1534838857,
	"Sig":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}
```
或
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
|    ReqContext|   String|  verifiable claim template。扩展项，可没有该key Extension file, key is optional |
|    Sig|   String|  场景方使用OntId私钥对二维码信息的签名 Signature of the KYC requestor by using the private key |



	注意：二维码里的OntId必须是场景方在OntPass平台登记的OntId，签名也需要使用该OntId对应的私钥按照标准ECDSA算法，对二维码信息进行签名
	Remark: The OntID in the QR code must match the OntID registered in the OntPass platform. The signature must use standard ECDSA algorithm


### 3.接收用户可信声明 Receive verifiable from user

用户使用ONTO App扫描场景方二维码后可进行授权决策，若确认授权则会将用户ONTO App上的可信声明加密传输到OntPass，再由OntPass通过场景方注册的回调接口透传到场景方。User will delegate the verifying rights by scanning the QR code of the KYC requestor. If the delegation passes on ONTO App, the encrypted verifiable claim will be sent to OntPass, and then redirected to KYC requestor through the recall interface.

    用户出示的可信声明使用场景方OntId绑定的公钥进行加密，保证数据传输过程中的隐私性和安全性，即只有场景方可进行解密获取原文信息。
    User can encrypt the verifiable claim with the KYC requestor's public. This will ensure security during the whole data transfer process.

所以场景方提供的回调地址需要接收以下POST请求。
Therefore the recall interface provided by KYC requestor must accept the POST request below

```json
url：第三方回调地址 recall interface from third party:
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
|    AuthId|   String|  OntPass平台授权编码 OntPass platform authorization code  |
|    OntPassOntId|   String|  OntPass平台的OntId OntID of the OntPass |
|    UserOntId|   String|  用户OntId  OntID of the user|
|    EncryClaims|   list|  加密后的用户可信声明列表 list of verifiable claim after encryption |
|    Signature|   String|  OntPass对请求信息的签名 signature of the requested information from Ontpass |


### 4.可信声明验证 Verify the Verifiable claim

场景方收到用户的加密可信声明后，可使用自己在OntPass平台登记时的OntId对应的私钥进行解密，并到链上验证该可信声明的完整性和有效性。具体的可信声明说明可参考附录**可信声明规范**，验证方法可参考官方提供的各种SDK。After the KYC requestor receives the encrypted verifiable claim, the requestor is able to decrypt by using his private key of his OntID and verify the completeness ad effective of that verifiable claim on the blockchain. Detailed explation of verifiable claim can be found in the annex **Verifiable Claim Standards**. You can refer to the SDKs provided by Ontology.

[JAVA SDK验证可信声明 JAVA SDK verifiable claim verification](https://ontio.github.io/documentation/ontology_java_sdk_identity_claim_en.html#3-verify-verifiable-claim)


[TS SDK验证可信声明 TS SDK verifiable claim verification](https://ontio.github.io/documentation/ontology_ts_sdk_identity_claim_en.html#verifiable-claim-verification)



## OntPass平台认证模板

基于不同TrustAnchor机构签发的各种可信声明，OntPass平台定义了一些基本认证模板，适用于通用的kyc或社媒认证等应用场景。

场景方也可以根据自己的业务需求，自由组合各种可信声明并制定基本的授权逻辑规则，生成自定义的认证模板。这样场景方在OntPass平台注册或生成二维码时可灵活选择认证模板。


**可信声明：** ** Verifiable Claim**

| 可信声明标识     |     explanation |  issuer   | 
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


**认证模板：** **Authentication Template:**

认证模板包括认证模板标识、类型、描述，对应的可信声明模板标识，授权逻辑规则，单价等信息。
Authentication template includes identification of templates, category, description, identification of corresponding verifiable claim template, authentication logic, unit price and etc.



| Type of verifiable claim | Identification of verifiable claim | Description of verifiable claim | Template with corresponding verifiable 认证模板对应的可信声明模板标识 | Authentication logic |
| :--------: | :--------:|:---------:|:--------: | :--------:|
|  Social media   |   authtemplate_social01 |  basic information verification from social media  | claim:github_authentication<br><br>claim:twitter_authentication<br><br>claim:facebook_auuthentication<br><br>claim:linkedin_authentication    |   Choose anyone |
| kyc verification   |   authtemplate_kyc01 |  有关全球用户基本个人信息的认证 Verification of information for global users  | claim:idm_passport_authentication<br><br>claim:idm_idcard_authentication<br><br>claim:idm_dl_authentication |  Choose anyone |
| kyc verification   |   authtemplate_kyc02 |  有关中国用户的实名信息认证 Verification of information for Chinese citizen  | claim:cfca_authentication |   Mandatory |




## 经济激励 Incentives

用户使用自己已获取到的可信声明在场景方进行扫码授权认证，属于一种标准数据交易模式。可由OntPass平台和智能合约体系来解决数据交易过程中资金分配的公平公正性及用户友好性。It's considered typical data exchange transaction when a user is using the obtained verifiable claim to authorize a verification from the KYC requestor. OntPass platform is able to guarantee the fairness and friendliness during the data exchange transaction


## Login场景身份验证规范 Standard of Verifying Identification

在用户使用OntId在第三方login的场景中，第三方可按照以下标准JSON格式的登录请求，来简单快速验证用户是否是某个OntId的属主，完成平台身份验证。即用户使用自己的OntId绑定的私钥对请求信息进去签名，场景方收到请求信息后从链上获取该OntId用户的公钥，进行验签。根据验签结果即可验证该用户是否是某个OntId的属主，完成身份验证。 When a user logs in to a third party with OntID, the third party can rapidly verify the owner of the OntID with JSON format request. Even if the user is using the private key of the OntID to do the signature, the KYC requestor is able to verify the signature with the public key of that particular OntID. 

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
|    Signature|   String|  用户对请求信息的签名 Signature of the requested information by a user|


## 附录：## Annex:

### Verifiable Claim Standards

可信声明使用JSON Web Token的扩展格式来表示，基本结构由三部分组成：Header，Payload，Signature。我们通过在最后附加区块链证明Blockchain_Proof来扩展JWT格式，一个典型的完整可信声明被组织为
There are three parts to form a JSON Web Token extension file verifiable claim: Header, Payload and Signature. We will attach "Blockchain_Proof" to extend JWT format, a typical verifiable claim can be arranged as:

	Header.Payload.Signature.Blockchain_Proof


**Header**
Header部分定义了该可信声明的格式类型，使用的签名方案以及用于验证签名的公钥id
Header defines the category of the verifiable claim, the signature methodology and the ID of the public key for verifying the signature

```
{
    "alg": "ES256",
    "typ": "JWT-X",
    "kid": "did:ont:TRAtosUZHNSiLhzBdHacyxMX4Bg3cjWy3r#keys-1"
}
```


**Payload**

Payload部分定义了可信声明的基本信息及TrustAnchor认证的内容
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

- **ver** 指明可信声明版本 Version of the verifiable claim
-  **iss** 可信声明签发者的ONT ID / ONT ID of the verifiable claim issuer
-  **sub** 可信声明接收者的ONT ID / ONT ID of the verifiable claim receiver
-  **iat** unix时间戳格式的创建时间 creation time of the unix timestamp
-  **exp** unix时间戳格式的过期时间 expire time of the unix timestamp
-  **jti** 可信声明的唯一标识符 the identification of verifiable claim
-  **@context** 可信声明模板标识 the identification of verifiable claim  template
-  **clm** 指明了TrustAnchor认证的可信声明内容 indicates the verifiable claim contents verified by TrustAnchor
-  **clm-rev** 指明了可信声明吊销方式。indicates the revoke method for a verifiable claim


**Signature**

在构造完Header和Payload部分后，签名根据JWS标准来计算。
After constructing "Header" and "Payload", the signature if calcuated based on JWS standards

- 根据JWS规范对Header和Payload部分进行序列化，作为签名的输入
- The input of the signature if based on serialization of "Header" and "Payload"
	
	sig := sign(Base64URL(header) || . || Base64URL(payload))


- 根据Header部分指定的特定签名方案来计算签名。
- Calcuate the signature based on specific signing method of the "Header"

- 对签名进行编码
- Coding towards signature
	
	signature := Base64URL(sig).


**Blockchain Proof**

签发者会将该可信声明进行区块链存证，并根据存证交易获取到区块链证明merkleproof
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
- **TxnHash** 将可信声明id存证在合约里的交易hash值 Hash value of saving the verifiable claim ID in the smart contract
- **ContractAddr** 存证合约的地址  Contract address
- **BlockHeight** 存证交易对应的区块高度 Block Height
- **MerkleRoot** 该区块高度的区块对应的Merkle树根 Merkle root of the corresponding blockheight
- **Nodes** Merkle树证明的证明路径 Nodes path of the Merkletree


**可信声明传输** **Verifiable Claim Transmission**

传输时参考JWT规范使用base64编码后的格式。场景方获取到的可信声明也按照如下格式组织：
Verifiable claim received by KYC requestor will be arranged as the following:

	BASE64URL(Header) || '.' || BASE64URL(Payload) || '.' || BASE64URL(Signature)  '.' || BASE64URL(blockchain_proof) 


有关可信声明的详细定义及规范可参考：[可信声明协议规范](https://ontio.github.io/documentation/claim_spec_en.html)
For more information, please vist (https://ontio.github.io/documentation/claim_spec_en.html)


  [1]: 123123
