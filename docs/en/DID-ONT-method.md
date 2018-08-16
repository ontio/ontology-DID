# Ontology DID Method Specification
This specification defines how Ontology blockchain[1] stores DIDs and DID documents, and how 
to do CRUD operations on DID documents. More importantly, this specification confirms to 
the requirements specified in the DID specification[2] currently published by 
the W3C Credentials Community Group. 

Ontology provides high-performance public blockchains that include a series of complete distributed ledgers and smart contract systems.
Ontology blockchain framework supports public blockchain systems and is able to customize different public blockchains for different applications. 

## 1. Ontology DID Method Name
The namestring that shall identify this DID method is: `ont`.

A DID that uses this method **MUST** begin with the following prefix: `did:ont`. Per this DID specification, this string MUST be in lowercase.

## 2. Ontology DID Format
The decentralized identifiers(DID) on Ontology blockchain is of the following format:
```
ont-did   = "did:ont:" id-string 
id-string = 1* idchar
idchar    = 1-9 / A-H / J-N / P-Z / a-k / m-z 
```
`idchar` consists of all the characters in the base58 char set which is first defined by 
Bitcoin. The Ontology DIDs are encoded using the base58 encoding method.

### Example
Example Ontology DIDs: 
```
did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt
```
## 3. CRUD Operations
DID and associated DID documents on Ontology blockchain is managed by a native smart contract
. This smart contract is written in `go` language. This contract provides CRUD interfaces for 
controlling an identity holder's DID document, and this contract has a fixed address, namely, `0x0000000000000000000000000000000000000003`(20 bytes).
 

### 3.1 Create (Register)

To create a DID, invoke the `RegIdWithPublicKey` function of the native smart contract. 
And the interface for register a DID is defined as follows:
```go
func RegIdWIthPublicKey(ontId string, publicKey []byte) bool 
```
And the identity holder must include two parameters, i.e. the new DID to be registered and a cryptographic 
public key to act as the first management key. 
This function will return true if the new DID is not registered before.

### 3.2 Read (Resolve)
Ontology DID's associated DID document can be looked up by invoking the `GetDDO` function of the native smart contract. 
To make sure the result returned by invoking smart contract is trustworthy, the client could ask sufficient number of nodes 
and compare these return values.

The interface for resolving DID document is defined as follows:
```go
func GetDDO(ontId string) DDO
```
And DDO is a JSON-object which contains the `publicKey`, `authentication` elements.
Every public key in the array of `publicKey` can be used to authenticate the identity holder.

Note: the list of supported public key signature schemes is listed in [Appendix A](#appendix-a-public-key-algorithm).

#### DID Document Example 
```json
{
    "id": "did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt",
    "publicKey": [{
        "id": "did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt#keys-1",
        "type": ["ECDSA", "secp256r1"],
        "publicKeyHex": "02ba8ba8e8ef1f313ecc095ba0bf0d1a921a138346d6817812714f4a9e4cca8ccf"
    }, {
        "id": "did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt#keys-2",
        "type": ["ECDSA", "secp256r1"],
        "publicKeyHex": "02b9bd513110b2a7822326280f3fdff9fa667649d3302428a0ab6fb796c6f3201b"
    }],
    "authentication": [{
        "type": "Secp256r1SignatureAuthentication2018",
        "publicKey": "did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt#keys-1"
    }, {
        "type": "Secp256r1SignatureAuthentication2018",
        "publicKey": "did:ont:AGsL32ZMvAwxYRN9Sv4mrgu3DgBSvTm5vt#keys-2"
    }],
}
```

### 3.3 Update (Replace)

To update the DID document, just invoke two relevant functions, namely, 
```go
func AddKey(ontId string, newPublicKey []byte, sender []byte) bool
```
```go
func RemoveKey(ontId string, oldPublicKey []byte, sender []byte) bool
```
Note that `sender` param must be a currently-in-use public key of this DID.
If a public key is removed, then it **cannot** be added again.

### 3.4 Delete (Revoke)

To delete or deactivate a DID, it suffices to remove all public keys from its associated 
DID document. In this case, there is no public key that can be used to authenticate the holder's 
identity. 

More importantly, Deletion of Ontology DID means that this DID cannot be reactivated again. 

## 4. Security Considerations
TODO

## 5. Privacy Consideration
TODO

## 6. Reference Implementations
The code at https://github.com/ontio/ontology gives a reference implementation of Ontology DID scheme.

## Appendix A. Public Key Algorithm 
There are three public key algorithms supported in this document. 
1. ECDSA
2. SM2
3. EdDSA

### ECDSA 
The curves that can be used are: 

- secp224r1 -- same as nistp224
- secp256r1 -- same as nistp256 
- secp384r1 -- same as nistp384 
- secp521r1 -- same as nistp521

More curves may be supported in future versions of this document. 

### SM2 

There is only one curve that can be used, namely, `sm2p256v1` as defined in [SM2 Digital Signature Algorithm](https://tools.ietf.org/html/draft-shen-sm2-ecdsa-02#appendix-D). 

### EdDSA
There is only one curve that can be used, namely, `ed25519`. 

## References
[1]. Ontology blockchain, https://ont.io.

[2]. W3C Decentralized Identifiers (DIDs) v0.11, https://w3c-ccg.github.io/did-spec/.

[3]. IETF Internet draft, SM2 Digital Signature Algorithm, https://tools.ietf.org/html/draft-shen-sm2-ecdsa-02.
