## 包含完整性证明的可信声明模板示例

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
	},
	"Proof":{
		"Type":"MerkleProof",
		"TxnHash":"c89e76ee58ae6ad99cfab829d3bf5bd7e5b9af3e5b38713c9d76ef2dcba2c8e0",
		"BlockHeight":10,
		"MerkleRoot":"bfc2ac895685fbb01e22c61462f15f2a6e3544835731a43ae0cba82255a9f904",
		"Nodes":[
			{
				"Direction":"Right",
				"TargetHash":"2fa49b6440104c2de900699d31506845d244cc0c8c36a2fffb019ee7c0c6e2f6"
			},
			{
				"Direction":"Left",
				"TargetHash":"fc4990f9758a310e054d166da842dab1ecd15ad9f8f0122ec71946f20ae964a4"
			}
		]
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
|    Proof|   Object |  完整性证明  |N|
|    Proof.Type|   String |  验证类型，MerkleProof  |N|
|    Proof.TxnHash|   String |  存证交易hash值  |N|
|    Proof.BlockHeight|   int|  区块高度  |N|
|    Proof.MerkleRoot|   String |  MerkleRoot，梅克尔树根  |N|
|    Proof.Nodes|   list |  验证节点数组  |N|
|    Proof.Nodes.Direction|   String |  验证节点方向  |N|
|    Proof.Nodes.TargetHash|   String |  验证节点hash值  |N|


详细说明：
- Id 即可信声明唯一标识，生成逻辑为对Context，Content，Metadata字段内容做Hash，保证了唯一性。
- Context即可信声明模板标识，该字段的值与Content字段包含的key值息息相关，同一种Context的可信声明里的Content内的key值应该一致。可用于分类可信声明。
- Content即可信声明具体内容，里面包含的具体字段与业务层最终已验证信息相关。
- Metadata即元数据，包含了签发时间，签发者和属主身份标识等基本信息。
- Signature即签名信息，是由声明签发者对Id，Context，Content，Metadata内容做的签名，可用于后续可信声明验证。
- Proof即完整性证明，在ontology中使用梅克尔树证明。主要包含了MerkleRoot，交易hash和证明路径，可用于验证可信声明的存在性和完整性。