## 标准可信声明模板示例


```json
{
	"Id":"ca4ab2f56d106dac92e891b6f7fc4d9546fdf2eb94a364208fa65a9996b03ba0",
	"Context":"claim:degree_authentication",
	"Content":{
	    "degree" : "bachelor",
	    "year" : "2017",
	    "name":"Alice",
	    "studentid":"1701021223"
	},
	"Metadata":{
		"CreateTime":"2017-03-01T12:01:20Z",
		"Issuer":"did:ont:TRAtosUZHNSiLhzBdHacyxMX4Bg3cjWy3r",
		"Subject":"did:ont:SI59Js0zpNSiPOzBdB5cyxu80BO3cjGT70",
		"IssuerName":"Fudan University",
		"Expires":"2018-03-01",
		"Revocation":"RevocationList",
		"Crl":"http://fudan.com/claim/rev/crl"
	},
	"Signature":{
		"Format":"pgp",
		"Algorithm":"ECDSAwithSHA256",
		"Value": "rsjaenrxJm8qDmhtOHNBNOCOlvz/GC1c6CMnUb7KOb1jmHbMNGB63VXhtKflwSggyu1cVBK14/0t7qELqIrNmQ=="
	}
}
```

字段描述：

| Field     |     Type |   Description   | Necessary|
| :--------------: | :--------:| :------: |:------: |
|    Id|   String|  可信声明唯一标识  |Y|
|    Context|   String|  可信声明模板标识  |Y|
|    Content|   Object|  可信声明具体内容，key-value形式，自定义  |Y|
|    Metadata|   Object|  可信声明元数据  |Y|
|    Metadata.CreateTime|   String|  创建时间,格式：yyyy-MM-dd'T'HH:mm:ss'Z'  |Y|
|    Metadata.Issuer|   String|  可信声明颁发者ONTID  |Y|
|    Metadata.Subject|   String|  可信声明属主ONTID  |Y|
|    Metadata.IssuerName|   String|  可信声明颁发者名称  |N|
|    Metadata.Expires|   String|  过期时间，格式：yyyy-MM-dd  |N|
|    Metadata.Revocation|   String|  声明注销类型，注销列表RevocationList或注销实时查询接口RevocationUrl  |N|
|    Metadata.Crl|   String|  注销查询API|N|
|    Signature|   Object |  签名信息  |Y|
|    Signature.Format|   String |  签名格式  |Y|
|    Signature.Algorithm|   String |  签名算法  |Y|
|    Signature.Value|   String |  签名值  |Y|

详细说明：
- Id 即可信声明唯一标识，生成逻辑为对Context，Content，Metadata字段内容做Hash，保证了唯一性。
- Context即可信声明模板标识，该字段的值与Content字段包含的key值息息相关，同一种Context的可信声明里的Content内的key值应该一致。可用于分类可信声明。
- Content即可信声明具体内容，里面包含的具体字段与业务层最终已验证信息相关。
- Metadata即元数据，包含了签发时间，签发者和属主身份标识等基本信息。
- Signature即签名信息，是由声明签发者对Id，Context，Content，Metadata内容做的签名，可用于后续可信声明验证。

