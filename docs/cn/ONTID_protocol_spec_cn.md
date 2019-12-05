[English](../en/ONTID_protocol_spec.md) / 中文

<h1 align="center">Ontology DID身份标识协议规范 </h1>
<p align="center" class="version">Version 1.3.8 </p>


实体是指现实世界中的个人、组织（组织机构、企事业单位等）、物品（手机、汽车、IOT设备等）、内容（文章、版权等），而身份是指实体在网络上的对应标识。本体使用本体⾝份标识（ONT ID）来标识和管理实体的网络身份。在本体上，⼀个实体可以对应到多个身份标识，且多个身份标识之间没有任何关联。

ONT ID是⼀个去中心化的身份标识协议，ONT ID具有去中心化、自主管理、隐私保护、安全易用等特点。ONT ID在遵循[DID规范](https://www.w3.org/TR/did-core/)的基础上增加了一些扩展性功能。

> ONT ID协议已被本体区块链智能合约完整实现，作为协议层，和区块链是解耦设计，并不仅限于本体区块链，同样可以基于其他区块链。


## ONT ID生成及注册

ONT ID是一种URI，由每个实体自己生成成，生成算法需要保证碰撞概率非常低。

一个合法的ONT ID生成过程如下：

1. 生成32字节的随机nonce，计算h = Hash160(nonce），data = VER || h；
2. 对data计算两次SHA256，并取结果哈希的前4字节作为校验，即checksum = SHA256(SHA256(data))[0:4]；
3. 令idString = base58(data || checksum)；
4. 将"did:ont:"与idString拼接，即ID = "did:ont:" || idString；
5. 输出ID。

上述过程中，|| 表示连接前后两个字节串，VER 是1个字节的标签位。

ONT ID需要在本体区块链上注册之后才能生效，可以在应用中使用。同一个ONT ID不能重复进行注册。

## ONT ID Document

ONT ID Document是一种以[JSON-LD](https://www.w3.org/TR/json-ld/)形式对ONT ID的相关信息进行序列化的方法，如[DID Documents](https://www.w3.org/TR/did-core/#did-documents)中的定义。

一个简单的ONT ID Document示例如下：

```json
{
  "@context": "https://w3id.org/did/v1",
  "id": "did:ont:some-ont-id",
  "authentication": [
    "did:ont:some-ont-id#keys-1"
  ],
  "publicKey": [
    {
      "id": "did:ont:some-ont-id#keys-1",
      "type": "EcdsaSecp256r1VerificationKey2019",
      "controller": "did:ont:some-ont-id",
      "publicKeyHex": "02a545599850544b4c0a222d594be5d59cf298f5a3fd90bff1c8caa095205901f2"
    }
  ]
}
```


## 管理权

ONT ID有两种管理方式：自主管理和代理控制。

### 自主管理

自主管理即ONT ID的所有者自己控制ID，进行注册、修改和注销等操作。在注册到链上时，ONT ID需要绑定所有者的公钥，而所有者自己持有对应的私钥。

本体支持多种国内、国际标准化的数字签名算法，如RSA、ECDSA、SM2等。ONT ID绑定的密钥需指定所使用的算法，同时一个ONT ID可以绑定多个不同的密钥，以满足实体在不同的应用场景的使用需求。各公钥按照绑定顺序依次从1开始编号。每个公钥的id形式为

```
did:ont:some-ont-id#keys-{index}
```

其中`{index}`即为公钥的编号。

绑定的公钥可以被废除。已废除的公钥不可被再次启用，但仍占有原编号。

在使用ID时，所有者使用私钥进行签名，并给出对应公钥的编号。验证端根据编号查找到公钥，验证签名。

### 代理控制

一个ONT ID可以被其他ONT ID代理控制。这种情况下被控制的ONT ID可以不绑定公钥。控制者拥有被控制ID的注册、修改和注销权限，但不能操作恢复人。在操作时被控制ID时，控制者需提供有效的数字签名。

在ONT ID Document中，控制者记录在"controller"字段。

控制者可以是一个ONT ID，也可以是若干ONT ID组成的管理组。管理组能够设置复杂的门限控制逻辑，以满足不同的安全需求。如设置n个ONT ID，最少m个ID共同签名才能进行操作(m <= n)，以如下JSON形式表示：

```
{
  "threshold": m,
  "members": [ID1, ID2, ... , IDn]
}
```

进一步的，可以定义递归组合的控制逻辑，即组成员可以是ONT ID，也可以是嵌套的管理组，如下所示：


```
{
  "threshold": m1,
  "members": [
    ID1,
    {
      "threshold": m2,
      "members": [ID2, ...]
    },
    ...
  ]
}
```

控制者的ONT ID必须是自主管理的。

控制者可以为被控制的ID绑定公钥，将其转换为自主管理模式。但是自主管理的ID无法转换成代理控制的模式。

## 恢复人

自主管理的ONT ID允许所有者设置其他ONT ID为恢复人。在所有者意外丢失密钥的情况下，恢复人可以帮助其重置密钥。

在ONT ID Document中，恢复人记录在"recovery"字段。

恢复人使用组管理的方式，规则同代理控制的管理组相同。

恢复人能够为ID添加、废除公钥，以及更新恢复人设置。操作时需提供符合控制逻辑的有效的数字签名。

## 附加属性

ONT ID的所有者或代理控制者可以为其添加、修改或删除附加属性。

每条属性包含"key", "value", "type"三个部分。其中"key"作为属性的标识，"type"指示属性的类型，"value"则为属性的内容。

在ONT ID Document中，以"attribute"字段记录各属性，其值为一个列表。每个属性的"key"将转换为一个URI，作为该ID的片段标识。

如did:ont:some-ont-id包含一个属性

```
key: "some-attribute"
type: "some-type"
value: "some-value"
```

在DID Document中将表示为

```json
{
  "attribute": [
    {
      "id": "did:ont:some-ont-id#some-attribute",
      "type": "some-type",
      "value": "some-value"
    },
  ]
}
```

除了服务入口类属性，其他的属性类型及其具体内容不在本规范的范畴内，由应用层自行定义。

### 服务入口

服务入口用于指示该ONT ID相关的某项服务的访问入口。具体参见[DID规范](https://www.w3.org/TR/did-core/#services)中的说明。

作为一类特殊的属性，服务入口的"type"固定为"service"。属性的"value"为一个json对象，应符合[Service Endpoints](https://www.w3.org/TR/did-core/#service-endpoints)中定义的除"id"以外的其他内容。而属性的"key"则作为"id"中'#'后的部分。

在ONT ID Document中，服务入口不会出现在"attribute"中，而是记录在"service"字段中。

例如did:ont:some-ont-id有一个服务入口属性

```
key: "some-service"
type: "service"
value: {"type": "SomeServiceType", "serviceEndpint": "Some URL"}
```

在ONT ID Document中该属性将以如下形式出现在"service"中

```json
{
  "service": [
    {
      "id": "did:ont:some-ont-id#some-service",
      "type": "SomeServiceType",
      "serviceEndpint": "Some URL"
    }
  ]
}
```

## 注销ONT ID

ONT ID的所有者或代理控制人可以将其注销。执行注销操作后，该ONT ID关联的密钥、控制人、属性及恢复人等一切数据将被删除，仅保留ID本身。已注销的ONT ID无法继续使用，也不能再次注册启用。




