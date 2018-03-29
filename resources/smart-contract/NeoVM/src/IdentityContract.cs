using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Ontology
{
    public class IdentityContract : SmartContract
    {
        public delegate void RegisterId(string op, byte[] ontId);
        [DisplayName("Register")]
        public static event RegisterId Registered;

        public delegate void PublicKey(string op, byte[] ontId, byte[] publicKey);
        [DisplayName("PublicKey")]
        public static event PublicKey PubKeyChanged;

        public delegate void Attribute(string op, byte[] ontId, byte[] attrName);
        [DisplayName("Attribute")]
        public static event Attribute AttrChanged;

        public delegate void DebugInfo(string func, byte[] info);
        [DisplayName("Debug")]
        public static event DebugInfo Debug;

        public static Object Main(string operation, params object[] args)
        {
            if (operation == "RegIdWithPublicKey")
            {
                if (args.Length != 2) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] publicKey = (byte[])args[1];
                return RegIdWithPublicKey(ontId, publicKey);
            }
            else if (operation == "RegIdWithAttributes")
            {
                if (args.Length != 3) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] publicKey = (byte[])args[1];
                byte[] attrs     = (byte[])args[2];
                return RegIdWithAttributes(ontId, publicKey, attrs);
            }
            else if (operation == "AddKey")
            {
                if (args.Length != 3) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] newPublicKey = (byte[])args[1];
                byte[] sender    = (byte[])args[2];
                return AddKey(ontId, newPublicKey, sender);
            }
            else if (operation == "RemoveKey")
            {
                if (args.Length != 3) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] oldPublicKey = (byte[])args[1];
                byte[] sender    = (byte[])args[2];
                return RemoveKey(ontId, oldPublicKey, sender);
            }
            else if (operation == "AddAttribute")
            {
                if (args.Length != 5) return false;
                byte[] id = (byte[])args[0];
                byte[] attrName = (byte[])args[1];
                byte[] type = (byte[])args[2];
                byte[] value = (byte[])args[3];
                byte[] publicKey = (byte[])args[4];
                return AddAttribute(id, attrName, type, value, publicKey);
            }
            else if (operation == "RemoveAttribute")
            {
                if (args.Length != 3) return false;
                byte[] ontId  = (byte[])args[0];
                byte[] path   = (byte[])args[1];
                byte[] pk     = (byte[])args[2];

                return RemoveAttribute(ontId, path, pk);
            }
            else if (operation == "AddRecovery")
            {
                if (args.Length != 3) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] recovery  = (byte[])args[1];
                byte[] publicKey = (byte[])args[2];
                return AddRecovery(ontId, recovery, publicKey);
            }
            else if (operation == "ChangeRecovery")
            {
                if (args.Length != 3) return false;
                byte[] ontId     = (byte[])args[0];
                byte[] newRecovery = (byte[])args[1];
                byte[] recovery  = (byte[])args[2];
                return ChangeRecovery(ontId, newRecovery, recovery);
            }
            else if (operation == "AddAttributeArray")
            {
                
            }

            if (operation == "GetPublicKeys")
            {
                if (args.Length != 1) return false;
                byte[] ontId = (byte[])args[0];
                return GetPublicKeys(ontId);
            }
            else if (operation == "GetAttributes")
            {
                if (args.Length != 1) return false;
                byte[] ontId = (byte[])args[0];
                return GetAttributes(ontId);
            }
            else if (operation == "GetDDO")
            {
                byte[] id = (byte[]) args[0];
                byte[] nonce = (byte[]) args[1];
                return GetDDO(id, nonce);
            }
            return false;
        }

        
        public static bool RegIdWithPublicKey(byte[] ontId, byte[] publicKey)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || Util.checkIdExistence(id) ) return false;
            if (!Runtime.CheckWitness(publicKey)) return false;

            if (Util.insertPk(id, publicKey))
            {
                Util.setExistenceFlag(id);
                Util.setNumOfAttr(id, 0);
                Util.setNumOfPk(id, 1);
                Registered("register", ontId);
                return true;
            }
            return false;
        }

        /*
         * every attribute consists of three parts: (attrname, type, value)
         * attr array is organized as n || tuple 1 || tuple 2 || ... || tuple n
         * 
         * and each tuple is organized as tuple_len || attr_len || attr || type_len || type || value_len || value
         */
        public static bool RegIdWithAttributes(byte[] ontId, byte[] publicKey, byte[] tuples)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || Util.checkIdExistence(id)) return false;
            if (!Runtime.CheckWitness(publicKey)) return false;
            
            int i, j, exit;
            int la, lb, lc;
            int n = (tuples.Length == 0) ? 0 : tuples[0];
            int length = tuples.Length - 1;
            object[] attrnames = new object[256];

            for (i=0,j=1,exit=0; i < n; i++)
            {
                if (length < 2) exit = 1 / exit; //abort this tx

                int len = (tuples[j] * 256) + tuples[j + 1];
                if (length < len + 2) exit = 1 / exit; //abort this tx
                
                byte[] attrs = Helper.Range(tuples, j + 2, len);
                if (attrs.Length <= 1) exit = 1 / exit; //abort this tx

                la = attrs[0];
                if (attrs.Length <= 1 + la) exit = 1 / exit; //abort this tx

                lb = attrs[1 + la];
                if (attrs.Length < 4 + la + lb) exit = 1 / exit; //abort this tx

                lc = (attrs[2 + la + lb] * 256) + attrs[3 + la + lb];
                if (la + lb + lc + 4 != len) exit = 1 / exit; //abort this tx

                byte[] attrname = Helper.Range(attrs, 1, la);
                byte[] type = Helper.Range(attrs, 2 + la, lb);
                byte[] value = Helper.Range(attrs, 4 + la + lb, lc);

                if (lb > 255) return false;
                
                if (Util.insertAttr(id, attrname))
                    Util.SetAttr(id, attrname, type, value); //two attributes with same attrname will be updated
                attrnames[i] = attrname;

                j = j + 2 + len;
                length = length - (2 + len);
            }
            Util.setExistenceFlag(id);
            Util.insertPk(id, publicKey);
            Util.setNumOfPk(id, 1);
            Util.setNumOfAttr(id, i);

            Registered("register", ontId);
            return true;
        }
        public static bool AddKey(byte[] ontId, byte[] newPublicKey, byte[] sender)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || ! Util.checkIdExistence(id)) return false;
            if (!Runtime.CheckWitness(sender) || !isOwner(id, sender)) return false;

            BigInteger n = Util.getNumOfPk(id);
            if (Util.insertPk(id, newPublicKey))
            {
                Util.setNumOfPk(id, n+1);
                PubKeyChanged("add", ontId, newPublicKey);
                return true;
            }
            return false;
        }

        public static bool RemoveKey(byte[] ontId, byte[] oldPublicKey, byte[] sender)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || !Util.checkIdExistence(id)) return false;
            if (!Runtime.CheckWitness(sender) || !isOwner(id, sender)) return false;

            BigInteger n = Util.getNumOfPk(id);
            if (Util.deletePk(id, oldPublicKey))
            {
                Util.setNumOfPk(id, n - 1);
                PubKeyChanged("remove", ontId, oldPublicKey);
                return true;
            }
            return false;
        }

        public static bool AddRecovery(byte[] ontId, byte[] recovery, byte[] publicKey)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || !Util.checkIdExistence(id)) return false;
            if (!Runtime.CheckWitness(publicKey) || !isOwner(id, publicKey)) return false;

            //this function can be called only once
            if (Util.getRecovery(id).Length != 0) return false;
            Util.setRecovery(id, recovery);
            return true;
        }

        public static bool ChangeRecovery(byte[] ontId, byte[] newRecovery, byte[] recovery)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || !Util.checkIdExistence(id)) return false;

            if (!Util.isEqual(recovery, Util.getRecovery(id)) || !Runtime.CheckWitness(recovery)) return false;
            Util.setRecovery(id, newRecovery);
            return true;
        }

        public static bool AddAttribute(byte[] ontId, byte[] path, byte[] type, byte[] value, byte[] publicKey)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || !Util.checkIdExistence(id)) return false;
            //msg.sender == pk && pk in pks
            if (!Runtime.CheckWitness(publicKey) || !isOwner(id, publicKey)) return false;
            
            if (Util.insertAttr(id, path))
            {
                BigInteger n = Util.getNumOfAttr(id);
                Util.setNumOfAttr(id, n + 1);
                Util.SetAttr(id, path, type, value);
                AttrChanged("add", ontId, path);
            }
            else
            {
                //attr exists, so update it
                Util.SetAttr(id, path, type, value);
                AttrChanged("update", ontId, path);
            }
            return true;
        }

        public static bool RemoveAttribute(byte[] ontId, byte[] path, byte[] publicKey)
        {
            byte[] id = Util.encodeId(ontId);
            if (id.Length < 1 || !Util.checkIdExistence(id)) return false;
            //msg.sender == pk && pk in pks
            if (!Runtime.CheckWitness(publicKey) || !isOwner(id, publicKey)) return false;

            if (Util.deleteAttr(id, path))
            {
                BigInteger n = Util.getNumOfAttr(id);
                Util.setNumOfAttr(id, n-1);
                Util.DelAttr(id, path);
                AttrChanged("remove", id, path);
                return true;
            }
            return false;
        }
        private static bool isOwner(byte[] id, byte[] pk)
        {
            if (Util.getListPointer(id.Concat(Util.PkField), pk).Length == 0) return false;
            return true;
        }

        /*
         * struct ListPointer 
         * {
         *      byte[] next, prev;
         * }
         * 
         */
        public static byte[] GetPublicKeys(byte[] ontId)
        {
            byte[] id = Util.encodeId(ontId);
            byte[] pks = { };
            int i = 0;

            byte[] pk_header = Storage.Get(Storage.CurrentContext, id.Concat(Util.PkField));
            if (pk_header.Length == 0) return null;
            byte[] p = pk_header;
            pks = Util.packBytes(p);

            while (true)
            {
                byte[] q = Storage.Get(Storage.CurrentContext, id.Concat(Util.PkField).Concat(p));
                p = Util.getNext(q);
                i++;
                if (p.Length == 0)
                    break;
                pks = pks.Concat(Util.packBytes(p));
            }
            byte[] num = Util.GetByte((byte)i);
            byte[] ret = num.Concat(pks);

            return ret;
        }
        public static byte[] GetAttributes(byte[] ontId)
        {
            byte[] id = Util.encodeId(ontId);
            byte[] attrs = { };
            int i = 0;

            byte[] attrname_header = Storage.Get(Storage.CurrentContext, id.Concat(Util.AttrnameField));
            if (attrname_header.Length == 0)
            {
                return null;
            }
            else
            {
                byte[] p = attrname_header;
                byte[] val = Storage.Get(Storage.CurrentContext, id.Concat(Util.AttrField).Concat(p));
                attrs = Util.packBytes(Util.packBytes(p).Concat(Util.packBytes(val)));

                while (true)
                {
                    byte[] q = Storage.Get(Storage.CurrentContext, id.Concat(Util.AttrnameField).Concat(p));
                    p = Util.getNext(q);
                    i++;
                    if (p.Length == 0)
                        break;

                    val = Storage.Get(Storage.CurrentContext, id.Concat(Util.AttrField).Concat(p));
                    attrs = attrs.Concat(Util.packBytes(Util.packBytes(p).Concat(Util.packBytes(val))));
                }
                return Util.GetByte((byte)i).Concat(attrs);
            }
        }
        public static byte[] GetDDO(byte[] id, byte[] nonce)
        {
            byte[] pubkeys = GetPublicKeys(id);
            byte[] attrs = GetAttributes(id);

            return Util.packBytes(pubkeys).Concat(Util.packBytes(attrs));
        }
        
    }
}
