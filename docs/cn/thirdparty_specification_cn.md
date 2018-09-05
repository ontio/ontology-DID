
<h1 align="center">场景方接入标准 </h1>
<p align="center" class="version">Version 0.8.0 </p>

## 概述

场景方作为本体信任生态中使用认证服务的一方，结合本体信任生态中提供认证服务的TrustAnchor，可基于可信声明、OntId、区块链来完成对现实中有关人，物，事的认证。

![flow](https://github.com/ontio/ontology-DID/blob/master/images/thirdparty_flow.png)


## 交互流程说明

![交互流程说明](http://on-img.com/chart_image/5b62a85fe4b025cf4932deb2.png)


- A1：场景方到OntPass平台注册相关基本信息及回调接口，并选择认证模板。然后按照OntPass平台二维码规范生成场景方的认证二维码。
- A2，A3：用户使用ONTO App扫描场景方出示的二维码，获取到二维码信息后向OntPass发起认证请求。OntPass平台进行二维码校验。
- A4：二维码校验成功，OntPass返回场景方注册时的基本信息和认证需求到ONTO App。
- A5：用户在ONTO App上进行授权决策。选择场景方所需的可信声明做授权确认，将加密后的可信声明发送到OntPass。OntPass触发智能合约进行资产交割。
- A6：OntPass通过场景方注册的回调接口，将用户加密后的的可信声明透传到场景方，场景方可使用自己OntId对应的私钥进行解密，获取用户可信声明。
- A7：场景方可通过区块链验证用户出示的可信声明的完整性和有效性。



## 接入步骤

### 1. OntPass平台注册

作为本体信任生态中认证服务的使用方，场景方首先需要到OntPass平台进行注册。

OntPass根据本体生态中各种认证服务提供商TrustAnchor可签发的可信声明，提供了不同类型的标准认证模板（可参考**OntPass认证模板**章节）。场景方注册时可根据自身业务场景选择所需要的认证模板，然后调用场景方注册API进行登记注册，主要包括场景方基本信息、认证模板标识及回调接口等信息。


#### 场景方注册API

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
|    OntId|   String|  场景方OntId  |
|    NameEN|   String|  场景方名称，英文  |
|    NameCN|   String|  场景方名称，中文  |
|    DesEN|   String|  场景方描述，英文  |
|    DesCN|   String|  场景方描述，中文  |
|    Logo|   String|  场景方Logo的url链接  |
|    CallBackAddr|   String|  回调地址。满足https+域名，接收post回调请求 |
|    Type|   String|  场景方所属类型|
|    ReqContext|   String|  场景方选择的标准认证模板标识。该认证模板由OntPass提供。 |
|    Signature|   String|  请求信息的签名。由场景方使用自己OntId的私钥按照标准的ECDSA算法签名。 |



| ResponseField     |     Type |   Description   |
| :--------------: | :--------:| :------: |
|    Result|   Boolean|  true：注册成功  false：注册失败|


	注意：为保证数据传输安全性，场景方注册的回调接口必须是https+域名形式，同时场景方需保证注册的回调接口高可用性且接受OntPass标准的https的post请求


### 2.生成二维码

场景方需要按照OntPass平台的规范生成标准二维码，供ONTO App扫码并进行授权决策。二维码需要嵌入场景方的OntId、语言版本标识、过期时间、认证模板（扩展项，若不填写则默认是场景方注册时登记的认证模板）以其签名。并使用7%低容错率标准生成二维码。

签名用于OntPass对场景方进行身份验证，二维码验证成功后返回给ONTO App用户场景方在OntPass平台注册时的相关信息。

标准二维码示例：

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
|    OntId|   String|  场景方的OntId  |
|    Exp|   int|  过期时间，unix时间戳  |
|    ReqContext|   String|  认证模板。扩展项，可没有该key |
|    Sig|   String|  场景方使用OntId私钥对二维码信息的签名 |



	注意：二维码里的OntId必须是场景方在OntPass平台登记的OntId，签名也需要使用该OntId对应的私钥按照标准ECDSA算法，对二维码信息进行签名


### 3.接收用户可信声明

用户使用ONTO App扫描场景方二维码后可进行授权决策，若确认授权则会将用户ONTO App上的可信声明加密传输到OntPass，再由OntPass通过场景方注册的回调接口透传到场景方。

    用户出示的可信声明使用场景方OntId绑定的公钥进行加密，保证数据传输过程中的隐私性和安全性，即只有场景方可进行解密获取原文信息。

所以场景方提供的回调地址需要接收以下POST请求。

```json
url：第三方回调地址
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
|    Version|   String|  版本号。目前是0.8  |
|    AuthId|   String|  OntPass平台授权编码  |
|    OntPassOntId|   String|  OntPass平台的OntId  |
|    UserOntId|   String|  用户OntId  |
|    EncryClaims|   list|  加密后的用户可信声明列表 |
|    Signature|   String|  OntPass对请求信息的签名 |


### 4.可信声明验证

场景方收到用户的加密可信声明后，可使用自己在OntPass平台登记时的OntId对应的私钥进行解密，并到链上验证该可信声明的完整性和有效性。具体的可信声明说明可参考附录**可信声明规范**，验证方法可参考官方提供的各种SDK。

[JAVA SDK验证可信声明](https://ontio.github.io/documentation/ontology_java_sdk_identity_claim_en.html#3-verify-verifiable-claim)


[TS SDK验证可信声明](https://ontio.github.io/documentation/ontology_ts_sdk_identity_claim_en.html#verifiable-claim-verification)



## OntPass平台认证模板

基于不同TrustAnchor机构签发的各种可信声明，OntPass平台定义了一些基本认证模板，适用于通用的kyc或社媒认证等应用场景。

场景方也可以根据自己的业务需求，自由组合各种可信声明并制定基本的授权逻辑规则，生成自定义的认证模板。这样场景方在OntPass平台注册或生成二维码时可灵活选择认证模板。


**可信声明：**

| 可信声明标识     |     说明 |   签发者   | 
| :--------------: | :--------:| :------: |
|    claim:email_authentication|   个人邮箱认证可信声明|  ONTO  |
|    claim:mobile_authentication|   个人手机认证可信声明|  ONTO  |
|    claim:cfca_authentication|   中国公民实名认证可信声明|  CFCA |
|    claim:sensetime_authentication|   中国公民实名认证可信声明|  SenseTime |
|    claim:idm_passport_authentication|   全球用户个人护照认证可信声明|  IdentityMind |
|    claim:idm_idcard_authentication|    全球用户个人身份证件认证可信声明|  IdentityMind |
|    claim:idm_dl_authentication|    全球用户个人驾照认证可信声明|  IdentityMind |
|    claim:github_authentication|   Github社媒认证可信声明|  Github |
|    claim:twitter_authentication|   Twitter社媒认证可信声明|  Twitter |
|    claim:linkedin_authentication|   Linkedin社媒认证可信声明| Linkedin |
|    claim:facebook_authentication|   Facebook社媒认证可信声明|  Facebook |


**认证模板：**

认证模板包括认证模板标识、类型、描述，对应的可信声明模板标识，授权逻辑规则，单价等信息。



| 认证模板类型 | 认证模板标识 | 认证模板描述 | 认证模板对应的可信声明模板标识 | 授权逻辑规则 |
| :--------: | :--------:|:---------:|:--------: | :--------:|
| 社交媒体认证    |   authtemplate_social01 |  有关用户各种社交媒体的基本信息认证  | claim:github_authentication<br><br>claim:twitter_authentication<br><br>claim:facebook_auuthentication<br><br>claim:linkedin_authentication    |   任选其一 |
| kyc认证    |   authtemplate_kyc01 |  有关全球用户基本个人信息的认证  | claim:idm_passport_authentication<br><br>claim:idm_idcard_authentication<br><br>claim:idm_dl_authentication |   任选其一 |
| kyc认证    |   authtemplate_kyc02 |  有关中国用户的实名信息认证  | claim:cfca_authentication |   必选 |




## 经济激励

用户使用自己已获取到的可信声明在场景方进行扫码授权认证，属于一种标准数据交易模式。可由OntPass平台和智能合约体系来解决数据交易过程中资金分配的公平公正性及用户友好性。


## Login场景身份验证规范

在用户使用OntId在第三方login的场景中，第三方可按照以下标准JSON格式的登录请求，来简单快速验证用户是否是某个OntId的属主，完成平台身份验证。即用户使用自己的OntId绑定的私钥对请求信息进去签名，场景方收到请求信息后从链上获取该OntId用户的公钥，进行验签。根据验签结果即可验证该用户是否是某个OntId的属主，完成身份验证。

```
{
	"OntId":"did:ont:A17j42nDdZSyUBdYhWoxnnE5nUdLyiPoK3",
	"Nonce":45348391,
	"Sig":"AXFqt7w/xg+IFQBRZvucKXvTuIZaIxOS0pesuBj1IKHvw56DaFwWogIcr1B9zQ13nUM0w5g30KHNNVCTo04lHF0="
}
```

| RequestField     |     Type |   Description   | 
| :--------------: | :--------:| :------: |
|    OntId|   String|  用户的OntId  |
|    Nonce|   int|  随机数  |
|    Signature|   String|  用户对请求信息的签名 |


## 附录：

### 可信声明规范

可信声明使用JSON Web Token的扩展格式来表示，基本结构由三部分组成：Header，Payload，Signature。我们通过在最后附加区块链证明Blockchain_Proof来扩展JWT格式，一个典型的完整可信声明被组织为

	Header.Payload.Signature.Blockchain_Proof


**Header**
Header部分定义了该可信声明的格式类型，使用的签名方案以及用于验证签名的公钥id

```
{
    "alg": "ES256",
    "typ": "JWT-X",
    "kid": "did:ont:TRAtosUZHNSiLhzBdHacyxMX4Bg3cjWy3r#keys-1"
}
```


**Payload**

Payload部分定义了可信声明的基本信息及TrustAnchor认证的内容

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

- **ver** 指明可信声明版本
-  **iss** 可信声明签发者的ONT ID
-  **sub** 可信声明接收者的ONT ID
-  **iat** unix时间戳格式的创建时间
-  **exp** unix时间戳格式的过期时间
-  **jti** 可信声明的唯一标识符
-  **@context** 可信声明模板标识
-  **clm** 指明了TrustAnchor认证的可信声明内容
-  **clm-rev** 指明了可信声明吊销方式。


**Signature**

在构造完Header和Payload部分后，签名根据JWS标准来计算。

- 根据JWS规范对Header和Payload部分进行序列化，作为签名的输入
	
	sig := sign(Base64URL(header) || . || Base64URL(payload))


- 根据Header部分指定的特定签名方案来计算签名。

- 对签名进行编码
	
	signature := Base64URL(sig).


**Blockchain Proof**

签发者会将该可信声明进行区块链存证，并根据存证交易获取到区块链证明merkleproof

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

- **Type** 固定值"MerkleProof"
- **TxnHash** 将可信声明id存证在合约里的交易hash值
- **ContractAddr** 存证合约的地址
- **BlockHeight** 存证交易对应的区块高度
- **MerkleRoot** 该区块高度的区块对应的Merkle树根
- **Nodes** Merkle树证明的证明路径


**可信声明传输**

传输时参考JWT规范使用base64编码后的格式。场景方获取到的可信声明也按照如下格式组织：

	BASE64URL(Header) || '.' || BASE64URL(Payload) || '.' || BASE64URL(Signature)  '.' || BASE64URL(blockchain_proof) 


有关可信声明的详细定义及规范可参考：[可信声明协议规范](https://ontio.github.io/documentation/claim_spec_en.html)


  [1]: 123123
