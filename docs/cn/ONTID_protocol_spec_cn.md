[English Version](./ONTID_protocol_spec.md)

<h1 align="center">ONT ID 身份标识协议及智能合约实现说明 </h1>
<h4 align="center">版本 V0.6.0 </h4>


实体是指现实世界中的个人、组织（组织机构、企事业单位等）、物品（手机、汽车、IOT设备等）、内容（文章、版权等），而身份是指实体在网络上的对应标识。本体使用本体⾝份标识（ONT ID）来标识和管理实体的网络身份。在本体上，⼀个实体可以对应到多个身份标识，且多个身份标识之间没有任何关联。

ONT ID是⼀个去中心化的身份标识协议，ONT ID具有去中心化、自主管理、隐私保护、安全易用等特点。每⼀个ONT ID都会对应到⼀个ONT ID描述对象（ONT DDO）。

> ONT ID协议已被本体区块链智能合约完整实现，作为协议层，和区块链是解耦设计，并不仅限于本体区块链，同样可以基于其他区块链。

## ONT ID协议

### 1.1 ONT ID生成

ONT ID是一种URI，由每个实体自己生成成。其生成算法需要保证了两个ONT，同时在向本体注册时，共识节点会检查该ID是否已被注册。

ONT ID 生成算法：

为了防止用户错误输入Ont ID，我们定义一个合法的Ont ID必须包含4个字节的校验数据。我们详细描述一下如何生成一个合法的Ont ID。
 1. 生成32字节临时随机数nonce，计算h = Hash160(nonce），data = VER || h；
 2. 计算出data的一个4字节校验，即checksum = SHA256(SHA256(data))[0:3]；
 3. 令idString = data || checksum；
 4. 将"did:ont:"与data级联，即 ontId = "did:ont:" || idString；
 5. 输出ontId。

其中，"did:ont:"是8个字节的数据，idString是25字节，因而ontId是一个33字节的数据。


### 1.2 自主管理
本体利用数字签名技术保障实体对自己身份标识的管理权。ONT ID在注册时即与实体的公钥绑定，从而表明其所有权。对ONT ID的使用及其属性的修改需要提供所有者的数字签名。实体可以自主决定ONT ID的使用范围，设置ONT ID绑定的公钥，以及管理ONT ID的属性。

### 1.3 多密钥绑定
本体支持多种国内、国际标准化的数字签名算法，如RSA、ECDSA、SM2等。ONT ID绑定的密钥需指定所使用的算法，同时一个ONT ID可以绑定多个不同的密钥，以满足实体在不同的应用场景的使用需求。

### 1.4 身份丢失恢复
ONT ID的所有者可以设置恢复人代替本人行使对ONT ID的管理权，如修改ONT ID对应的属性信息，在密钥丢失时替换密钥。恢复人可以实现多种访问控制逻辑，如“与”、“或”、“(m, n)-门限”。

### 1.5 身份描述对象DDO规范

ONT ID对应的身份描述对象DDO存储在本体区块链，由DDO的控制人写入到区块链，并向所有用户开放读取。

DDO规范包含如下信息：
- 公钥列表`PublicKeys`：用户用于身份认证的公钥信息，包括公钥id、公钥类型、公钥数据；
- 属性对象`Attributes`：所有的属性构成一个JSON对象；
- 恢复人地址`Recovery`：恢复人可帮助重置用户公钥列表。

举个例子
```json
{
	"OntId": "did:ont:xxxxxxxx",
	"PublicKeys": [
		{
			"PubKeyId": "did:ont:xxxxxxxx#keys-1",
			"Type": "ECDSA",
			"Curve": "nistp256",
			"Value": "02yyyyyyyy"
		}, 
		{
			"PubKeyId": "did:ont:xxxxxxxx#keys-2",
			"Type": "SM2",
			"Curve": "sm2p256",
			"Value": "02zzzzzzzz"
		}
	],
	"Attributes": {
		"SocialCredential": {
			"service": "weibo",
			"username": "alice",
			"proof": "https://weibo.com/status/ttttttt"
		},
		"OfficialCredential": {
			"service": "eID",
			"eId": "dddddddd",
			"proof": "03xz4f....."
		}
	},
	"Recovery": "AKDVzYGLczmykdtRaejgvWeZrvdkVEvQ1X"
}
```

## 智能合约实现说明

IdContract是在Ontology区块链平台上ONT ID协议的智能合约实现。借助ONT IdContract合约，用户可以管理自己的公钥列表、修改自己的个人资料、添加账户恢复人。

###  2.1 如何调用
IdContract合约对外的接口只有一个主函数，其参数包括要调用的子函数名（称之为操作码`op`），然后是传入该子函数的参数列表`params`。
```json
public static Object Main(string op, object[] params);
```
大部分子函数的返回值是布尔类型，代表执行操作的成功与否。正确执行之后，会推送事件消息通知调用者，具体消息类型请参考“**API描述**”小节。

#### Ontology智能合约调用
通过发送类型为*InvocationTransaction*的交易，并将需要调用的合约地址及调用传入参数作为交易的Payload。更为详尽的信息请参考[合约调用]()。

### 2.2 IdContract接口定义
####  a. 身份登记
用户在登记身份时，必须要提交一个公钥，并且**这次操作必须是由该公钥发起**。

```json
bool RegIdWIthPublicKey(byte[] ontId, byte[] publicKey); 
```
 参数：
  - ontId，用户标识，byte[] 类型；
  - publicKey，公钥， byte[] 类型。
   
 
####  b. 增加控制密钥

用户在自己的公钥列表中添加一个新公钥。
```json
bool AddKey(byte[] ontId, byte[] newPublicKey, byte[] sender); 
```
参数
- ontId：用户ont Id；
- newPublicKey：欲添加的新公钥；
- sender：交易发起者，账户现有公钥，或者恢复人。
	
#### c. 删减控制密钥	
从用户的公钥列表中，移除一个公钥。
```json
bool RemoveKey(byte[] ontId, byte[] oldPublicKey, byte[] sender);
```
参数
- ontId：用户Ont ID；
- oldPublicKey：欲添加的新公钥；
- sender：交易发起者，账户现有公钥，或者恢复人。	
	
#### d. 密钥恢复机制

添加与修改账户的恢复人。
```json
bool AddRecovery(byte[] ontId, byte[20] recovery, byte[] publicKey);
```
在函数`AddRecovery`中，当且仅当`publicKey`是账户现有公钥，且恢复人没有被设置过，`recovery`才能被添加进去。

参数
- ontId：用户Ont ID；
- recovery：恢复人地址，即脚本哈希[ScriptHash]()；
- publicKey：用户公钥

```json
bool ChangeRecovery(byte[] ontId, byte[] newRecovery, byte[20] oldRecovery);
```
这次合约调用必须是由oldRecovery发起。
参数
- ontId：用户Ont ID；
- newRecovery：新恢复人
- oldRecovery：现有恢复人
	
#### e. 属性管理
用户个人属性的增删改，均需要得到用户的授权。
```json
bool AddAttribute(byte[] ontId, byte[] path, byte[] type, byte[] value, byte[] publicKey);

```
- ontId：用户Ont ID； path：属性名路径；
- type：属性类型； value：属性值；
- publicKey： 用户公钥。
	
必须由一个合法公钥调用，且`publicKey`在用户公钥列表中。若该属性不存在时，则插入该属性，否则更新原先的属性值。	

```json
bool AddAttributeArray(byte[] ontId, byte[] tuples, byte[] publicKey);
```

#### f. 查询身份信息
```json
byte[] GetDDO(byte[] ontId);
```
返回用户的所有信息，是一个JSON对象的序列化。

```json
byte[] GetPublicKeys(byte[] ontId);
```	
返回用户的所有公钥。

#### g. 事件推送

IdContract中包含三种事件消息，分别是
- `Register`:  会推送与身份登记有关的信息。

	| Field | Type | Description |
	| :--- | :--- | :--- |
	|op| string | 消息类型 |
	| ontId | byte[] | 注册的Ont Id |

- `PublicKey`: 会推送与公钥操作有关的消息。

	| Field | Type | Description |
	| :--- | :--- | :--- |
	|op| string | 消息类型："add"或"remove" |
	| ontId | byte[] | 用户的Ont Id |
	| publicKey | byte[] | 公钥数据 |

- `Attribute`: 会推送与属性操作有关的消息。

	| Field | Type | Description |
	| :--- | :--- | :--- |
	|op| string | 消息类型："add"、"update"、"remove"  |
	| ontId | byte[] | 用户的Ont Id |
	| attrName | byte[] | 属性名 |
	






