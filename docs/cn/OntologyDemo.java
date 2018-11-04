import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import com.github.ontio.OntSdk;
import com.github.ontio.common.Helper;
import com.github.ontio.core.DataSignature;
import com.github.ontio.crypto.SignatureScheme;
import com.github.ontio.sdk.wallet.Identity;

import java.util.*;

/**
 * @version 1.0
 * @date 2018/10/23
 */
public class OntologyDemo {


    public static void main(String[] args) {

        try {
            //ONTO identity encryptedprivatekey
            String ontoEncryptedPriKey = "rnC6WclHSS9tpHGp0QKQOM10NzeZt4lvPOOOQC8ht9N0x7dsjkjccP9Ay3qrmStT";
            //ONTO identity password
            String ontoPassword = "142669";
            //ONTO identity salt
            String ontoSalt = "UyDgCiZsuStSBkjTmynRJg==";
            //ONTO identity OntId
            String ontoOntId = "did:ont:AYMGcyxiEuY6o7qqMX87DCPbwmsFpNSoAx";
            //Scrypt-N
            int scryptN = 4096;
            //impoort identity
            Identity identity = importIdentityFromONTO(scryptN, ontoEncryptedPriKey , ontoPassword, ontoOntId,ontoSalt);

            String identityPwd = ontoPassword;


            String signerOntId = identity.ontid;
            //original data
            JSONObject obj = new JSONObject();
            obj.put("OntId", signerOntId);
            obj.put("Uid", "15397687481203634");
            String orignalData = obj.toJSONString();
            System.out.println("########original data: " + orignalData);
            //signature
            String signedData = signatureData(identity, identityPwd, orignalData);
            System.out.println("########The signed data: " + signedData);
            //verify signature
            Boolean verifyRs = verifySignature(signerOntId, signedData, orignalData);
            System.out.println("########verify signature result: " + verifyRs);


            //String claimStr = createClaim(identity, identityPwd);
            //Boolean verifyRs2 = verifyClaim(claimStr);
            //System.out.println("verify claim result: "+verifyRs2);

        } catch (Exception e) {
            e.printStackTrace();
        }

    }


    /**
     * import identity from information according to ONTO
     *
     * @param ontoEncryptedPriKey
     * @param salt
     * @param password
     * @param ontId
     * @return
     * @throws Exception
     */
    public static Identity  importIdentityFromONTO(int scryptN, String ontoEncryptedPriKey, String password, String ontId,String salt) throws Exception{

        System.out.println("#####################################");
        OntSdk ontSdk = getOntSdk();

        String prikey = com.github.ontio.account.Account.getGcmDecodedPrivateKey(ontoEncryptedPriKey, password,ontId.split(":")[2], Base64.getDecoder().decode(salt),scryptN, SignatureScheme.SHA256WITHECDSA);
        Identity identity = ontSdk.getWalletMgr().createIdentityFromPriKey(password, prikey);

        ontSdk.getWalletMgr().writeWallet();

        return identity;
    }

    /**
     * create asset account
     *
     *
     * @return
     * @throws Exception
     */
    public static com.github.ontio.account.Account createAssetAccount() throws Exception {

        System.out.println("#####################################");
        OntSdk ontSdk = getOntSdk();

        com.github.ontio.account.Account account = new com.github.ontio.account.Account(ontSdk.defaultSignScheme);
        //address
        String address = account.getAddressU160().toBase58();
        System.out.println("address: " + address);
        //privatekey
        String prikey = Helper.toHexString(account.serializePrivateKey());
        System.out.println("privatekey: " + prikey);
        //publickey
        String pubkey = Helper.toHexString(account.serializePublicKey());
        System.out.println("publickey: " + pubkey);
        //wif
        String wif = account.exportWif();
        System.out.println("wif: " + wif);

        return account;
    }

    /**
     * create identity
     *
     * @param account
     * @param identityPwd
     * @return
     * @throws Exception
     */
    public static Identity createIdentityAccount(com.github.ontio.account.Account account, String identityPwd) throws Exception {

        System.out.println("#####################################");
        OntSdk ontSdk = getOntSdk();

        Identity identity = ontSdk.getWalletMgr().createIdentity(identityPwd);
        //ontid
        String ontid = identity.ontid;
        //salt，security param
        String salt = identity.controls.get(0).salt;
        System.out.println("ontid: " + ontid);
        System.out.println("salt: " + salt);

        //send register ontid transaction to blockchain.
        //fee:0.01ONG ,paied by account01
        String txnhash = ontSdk.nativevm().ontId().sendRegister(identity, identityPwd, account, 20000, 500);
        System.out.println("register ontid txnhash: " + txnhash);
        //write identity into identity.json file
        ontSdk.getWalletMgr().writeWallet();
        return identity;
    }


    /**
     * use identity privatekey to sinature data
     *
     * @param identity
     * @param identityPwd
     * @param origData
     * @return
     * @throws Exception
     */
    public static String signatureData(Identity identity, String identityPwd, String origData) throws Exception {

        System.out.println("#####################################");
        OntSdk ontSdk = getOntSdk();

        //ontid
        String ontid = identity.ontid;
        //salt，security param
        String salt = identity.controls.get(0).salt;
        System.out.println("salt:"+salt);


        //根据创建的ontid身份获取ontid的公钥，私钥信息
        com.github.ontio.account.Account ontidAcct = ontSdk.getWalletMgr().getAccount(ontid, identityPwd, Base64.getDecoder().decode(salt));
        String ontidPubkey = Helper.toHexString(ontidAcct.serializePublicKey());
        System.out.println("identity Publickey: " + ontidPubkey);
        String ontidPrikey = Helper.toHexString(ontidAcct.serializePrivateKey());
        System.out.println("identity Privatekey: " + ontidPrikey);

        //use privatekey of the identity to signature
        DataSignature sign = new DataSignature(ontSdk.defaultSignScheme, ontidAcct, origData.getBytes());
        String signedData = Base64.getEncoder().encodeToString(sign.signature());
        return signedData;
    }

    /**
     * verify signature
     *
     * @param signerOntId
     * @return
     */
    public static Boolean verifySignature(String signerOntId, String signedData, String origData) throws Exception {

        System.out.println("#####################################");
        OntSdk ontSdk = getOntSdk();
        //verify signature
        String issuerDdo = ontSdk.nativevm().ontId().sendGetDDO(signerOntId);
        String pubkeyStr = JSON.parseObject(issuerDdo).getJSONArray("Owners").getJSONObject(0).getString("Value");

        com.github.ontio.account.Account account = new com.github.ontio.account.Account(false, Helper.hexToBytes(pubkeyStr));
        DataSignature sign = new DataSignature();
        Boolean rs = sign.verifySignature(account, origData.getBytes(), Base64.getDecoder().decode(signedData));
        return rs;
    }


    /**
     * create claim
     *
     * @param identity
     * @return
     */
    public static String createClaim(Identity identity, String identityPwd) throws Exception{

        System.out.println("#####################################");
        OntSdk ontSdk = OntSdk.getInstance();

        String ontid =identity.ontid;
        String ontidSalt = identity.controls.get(0).salt;

        Map metaData = new HashMap();
        metaData.put("Issuer", ontid);
        metaData.put("Subject", "did:ont:AU1oLpK14EB7nu7ND4s12WpwUQHBOrt1Nh");

        Map claimInfo = new HashMap();
        claimInfo.put("Email", "182test@hotmail.com");
        claimInfo.put("IssuerName", "hotmail");

        Map<String, Object> clvMap = new HashMap<String, Object>();
        clvMap.put("typ", "AttestContract");
        clvMap.put("addr", "36bb5c053b6b839c8f6b923fe852f91239b9fccc");

        Calendar c = Calendar.getInstance();
        c.setTime(new Date());
        c.add(Calendar.YEAR, 1);
        long expires = c.getTimeInMillis() / 1000L;

        String claimContext = "claim:email_authentication";
        String claimStr = ontSdk.nativevm().ontId().createOntIdClaim(ontid, identityPwd, Base64.getDecoder().decode(ontidSalt), claimContext, claimInfo, metaData, clvMap, expires);
        System.out.println("claimStr: " + claimStr);

        String[] varArray = claimStr.split("\\.");
        String header = new String(Base64.getDecoder().decode(varArray[0]), "utf-8");
        System.out.println("header: " + header);

        String payload = new String(Base64.getDecoder().decode(varArray[1]), "utf-8");
        System.out.println("payload: " + payload);

        String signature = varArray[2];
        System.out.println("signature: " + signature);

        return claimStr;
    }

    /**
     * verify claim
     *
     * @param claimStr
     * @return
     */
    public static Boolean verifyClaim(String claimStr)throws Exception {

        OntSdk ontSdk = OntSdk.getInstance();
        Boolean flag = ontSdk.nativevm().ontId().verifyOntIdClaim(claimStr);
        return flag;
    }


    public static OntSdk getOntSdk() throws Exception {

        OntSdk wm = OntSdk.getInstance();
        //testnet
       // wm.setRestful("http://polaris1.ont.io:20334");
        //mainnet
        wm.setRestful("http://dappnode1.ont.io:20334");
        wm.setDefaultConnect(wm.getRestful());
        wm.openWalletFile("identity01.json");
        return wm;
    }

}
