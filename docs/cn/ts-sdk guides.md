# ONT SDK Guides

## 开始使用

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

## 创建身份

用户可以通过使用ONT SDK生成自己的**ONT ID**。这是一个去中心化的身份标识，能管理自己的各种数字身份认证。首先我们需要生成一个身份，该身份中包含了用户的ONT ID。

以下的代码都以Node环境为例。

创建身份需要用户提供私钥，身份的名称和用来加密私钥的密码。

````
//通过core模块提供的方法为用户生成私钥
let privateKey = core.generatePrivateKeyStr()
var identity = new Identity()
//创建身份
identity.create( privateKey, 'password', 'name' )
console.log(identity.ontid)
````

身份创建完成后，用户还需要将ONT ID 发送到区块链上，使之真正地成为去中心化的身份标识。

发送ONT ID上链是需要发送交易的过程。可以通过调用SDK提供的方法构造交易对象。

通过传递刚刚创建的身份的ONT ID和用户的私钥来构造交易对象。

````
var param = utils.buildRegisterOntidTx(identity.ontid, privateKey)
````

该方法返回的是交易对象序列化好的参数，接下来是发送该参数。可以通过websocket或者http请求的方式发送。这里我们以websocket为例，能够监听链上推送回来的消息，来确认ONT ID是否上链成功。

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

## 获取身份声明

用户可能会有多种不同的身份。比如拥有公安部颁发的身份证的用户，都拥有中国公民这种身份，用户可以在生活中的某些场景中，出示自己的身份证，来声明自己的这种身份；身份证就是公安部对我们公民身份的认证。

再比如某所大学毕业的学生，可以获得该大学的毕业生的身份。这个身份可以通过学校给学生颁发的毕业证来证明。现在，还有一种新的方式来认证这种某大学毕业生的身份。这就是通过区块链技术，将某种身份声明同用户的ONT ID绑定起来。同样地，用户可以向多个不同的机构或平台获取不同的身份声明。关于认证服务提供商相关的文档，请参见[验证服务提供商Verification Provider接入标准](https://github.com/ONTIO-Community/ONT-ID/blob/master/docs/verification_provider_specification.md)。

获取身份声明就是对某种身份认证的过程，该过程需要将用户的声明，通过用户的私钥签名并构建交易对象，发送到链上。

常见的身份认证是各种社交平台上身份的认证。我们以Facebook的身份认证来举例说明如何获取身份声明。

 首先用户需要登录Facebook来获取Facebook的授权，Facebook会返回给用户一些个人信息。用户通过SDK将这些信息构造成一份身份的声明。

````
var claim = SDK.signSelfClaim(context, claimData, ontid, encryptedPrivateKey, password)
````

该方法的参数说明如下：

**context** 是表示认证的模块标识，比如在使用Facebook的认证时，该值可以为‘claim:facebook_authentication’。

**claimData** 是用户声明的具体内容，这里就是Facebook上用户的个人信息。

**ontid** 用户的ONT ID。将该声明绑定到用户的ONT ID上。

**encryptedPrivateKey** 用户加密后的私钥。为了保证用户的信息安全，在使用过程中一般要求参数是加密后的私钥。私钥是用来对声明对象进行签名。

**password** 用户加密私钥的密码。用来解密私钥。

该方法返回的claim对象中的signedData就是声明对象签名后的值。

接下来需要发送交易到链上。

````
var param =  SDK.buildClaimTx( content, encryptedPrivateKey, password)
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

当得知上链成功后，用户就成功获得了第三方认证的身份声明。

## 使用身份声明