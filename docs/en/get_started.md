## 快速入手

### 1. 浏览器环境

将构建后的单个js文件 **browser.js** 放到你的html文件中，然后就可以直接使用SDK暴露出的全局变量**Ont**。

````
<html>
<head>
    <script src="browser.js"></script>
</head>
<body>
	<script>
		//开始使用
		var wallet = Ont.SDK.createWallet('name','password')
	</script>
</body>
````

### 2. Node环境

通过npm下载sdk，然后通过模块引入的方式使用SDK。

````
//安装
npm install Ont-ts --save
//使用
var Ont = require('Ont')
````

## 创建ONT ID

### 1 生成ONT ID

描述ONT ID 是什么，有什么用。 描述生成一个随机生成的ONT ID的过程。

ONT ID是一个去中心化的身份标识，能够管理用户的各种数字身份认证。用户可以通关ONT SDK生成自己的ONT ID。它实际上是一个唯一的id。生成ONT ID的代码如下：

````
var nonce = core.generateRandomHex(32)
var ontid = core.generateOntid(nonce)
````

关于ONT ID 的规范参见[ONT身份的智能合约实现方案中2.1ONT身份标识生成算法](http://doc.ont.network:9001/chapter2/OntId%E6%99%BA%E8%83%BD%E5%90%88%E7%BA%A6.html)

本文档中以下的示例代码都以Node环境为例。

### 2 登记ONT ID

ONT ID创建完成后，用户还需要将ONT ID 发送到区块链上，使之真正地成为去中心化的身份标识。

发送ONT ID上链是需要发送交易的过程。可以通过调用SDK提供的方法构造交易对象。

一种比较典型的场景是通过传递刚刚创建的ONT ID和用户的私钥来构造交易对象。

这里传递的私钥有两个作用：

1.对构造的交易进行签名；

2.将用户的ONT ID绑定到用户的私钥对应的公钥上。用户之后还可以在ONT ID上添加其它的公钥。

````
var param = buildRegisterOntidTx(ontid, privateKey)
````

该方法返回的是交易对象序列化好的参数，接下来是发送该参数。可以通过websocket或者http请求的方式发送。这里我们以websocket为例，这样能够监听链上推送回来的消息，来确认ONT ID是否上链成功。

````
//构造发送交易的工具类，这里连接的是测试节点。
var txSender = new TxSender(ONT_NETWORK.TEST)

//构造回调函数，处理接收到的消息
const callback = function(res, socket) {
    if(res.Action === 'Notify' && res.Result == 0 ) {
    	//确认上链成功，关闭socket
        socket.close()
    }
}

//发送交易
txSender.sendTxWithSocket( param, callback )
````

当我们定义的回调函数里处理得到上链成功的推送消息时，ONT ID创建过程才真正完成。接下来就可以通过ONT ID来管理用户的各项身份声明认证了。

关于链上推送返回的具体信息，可以参见[ONT ID智能合约的设计与调用相关文档](http://42.159.224.87:8080/browse/ONTID-3?nsukey=uvv4OCzAVcvMGUodSthJA9t4EYUSkIQqgnTj9a8jAAfysOAMX%2Bu1yjYeth2KKNim4MxIpSN07vXLBhZy8V7SHy6vVSx4J%2BOA1LeFqATn9QvBRyqx%2F53iqcWwzCYOmaGtSvyFexy9YNeZJ42u8oc5mqGX7ehJpQPH5Y76CD1wP68raYBDJjdmCX8nOYD2xIlVI3FcDp4hoh7GXpgh7aQqcA%3D%3D)。

## 获取身份声明

用户可能会有多种不同的身份。比如拥有公安部颁发的身份证的用户，都拥有中国公民这种身份，用户可以在生活中的某些场景中，出示自己的身份证，来声明自己的这种身份；身份证就是公安部对我们公民身份的认证。

再比如某所大学毕业的学生，可以获得该大学的毕业生的身份。这个身份可以通过学校给学生颁发的毕业证来证明。现在，还有一种新的方式来认证这种某大学毕业生的身份。这就是通过区块链技术，将某种身份声明同用户的ONT ID绑定起来。同样地，用户可以向多个不同的机构或平台获取不同的身份声明。关于认证服务提供商相关的文档，请参见[验证服务提供商Verification Provider接入标准](https://github.com/ONTIO-Community/ONT-ID/blob/master/docs/verification_provider_specification.md)。

我们以MIT大学颁发数字毕业证书来举例说明如何获取第三方授予用户的的身份声明。

假设Alice是MIT的学生，向学校申请毕业证的数字证明。学校验证确认了Alice的身份后，通过调用SDK的api生成一份可信声明，该声明包含Alice的毕业信息，和用学校的私钥对声明做的签名。

````
var claim = SDK.signClaim(context, claimData, issuer, subject, privateKey)
````

该方法的参数说明如下：

**context** 是一种声明模板的标识。

**claimData** 是用户声明的具体内容，该值是JSON对象。在这里就是Alice毕业证上的信息，如：

````
{
    "degree" : "bachelor",
    "year" : "2017",
    ......
}
````

**issuer** 是声明的签发者（这里是MIT大学）的ONT ID。

**subject** 是声明接收者（这里是Alice）的ONT ID。表示将该声明绑定到Alice的ONT ID上。

**privateKey** 声明签发者的私钥。用来对声明做签名。

该方法返回的声明对象，内容类似于：

````
{
    .....
}
````

关于声明对象的具体规范，详见[claim的规范]()。

接下来需要发送交易到链上用于存证。上链成功后，会返回该声明上链的完整性证明。该证明的具体格式参见

[claim完整性证明]()。

首先需要构造要发送的交易。需要传递的参数

**path** 是声明信息存储在链上的键名。该值为声明对象中的Id。这是一个对声明对象内容做hash运算后的值。

**value** 是需要存储在链上的声明信息。该值为如下的JSON结构：

````
{
    Context : string, //声明模板的标识，
    Ontid : string //声明签发者的ONT ID
}
````

**ontid** 交易发送者ONT ID，即声明签发者的ONT ID。

**privateKey** 交易发送者私钥，即声明签发者的私钥。

````
var param = SDK.buildClaimTx(path, value, ontid, privateKey)
````

接下来构建发送交易的工具类和监听消息的回调方法。

在回调方法中，声明上链成功后会返回声明的完整性证明。将该完整性证明添加到之前构建的声明对象中，用户就得到完整的第三方认证的声明对象。之后，用户就可以在有需要的场景中，提供该声明。

````
//这里以测试节点为例
var txSender = new TxSender(ONT_NETWORK.TEST)
const callback = function(res, socket) {
    let res 
    if(typeof event.data === 'string') {
    res = JSON.parse(event.data)
    //解析后台推送的Event通知
    //通过简单的判断区块高度，得知上链成功，
    if(res.Result.BlockHeight) {
      socket.close()
    }
}
txSender.sendTxWithSocket(param, callback)
````

证明的内容类似如下：

````
{
    "Proof" : {
        "Type" : "MerkleProof",
        "TxnHash" : "aaa",
        "BlockHeight" : "1000",
        "MerkleRoot" : "aaaaaaa",
        "Nodes" : [
            {"Direction" : "Right", "TargetHash" : "aaaa"},
            {"Direction" : "Left", "TargetHash" : "bbbbb"}
        ]
    }
}
````

## 使用身份声明