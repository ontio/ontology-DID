using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Ontology
{
    internal class Util
    {
        internal static readonly byte[] fTrue = { 0x01 };

        //storage-related keys
        internal static readonly byte[] NumofpkField = { 0x01 }; //int
        internal static readonly byte[] PkField = { 0x02 }; //doubly-linked list
        internal static readonly byte[] NumofattrField = { 0x04 }; //int
        internal static readonly byte[] AttrnameField = { 0x05 }; //doubly-linked list
        internal static readonly byte[] AttrField = { 0x06 }; //tree
        internal static readonly byte[] RecoveryField = { 0x07 };

        public delegate void DebugInfo(string func, BigInteger trace);
        [DisplayName("Debug")]
        public static event DebugInfo Debug;

        internal Util()
        {
        }

        internal static bool checkIdExistence(byte[] id)
        {
            byte[] fExists = Storage.Get(Storage.CurrentContext, id);
            return fExists.Length != 0 && ((fExists[0] == fTrue[0]) ? true : false);
        }
        internal static void setExistenceFlag(byte[] id)
        {
            Storage.Put(Storage.CurrentContext, id, fTrue);
        }

        internal static BigInteger getNumOfAttr(byte[] id)
        {
            return Storage.Get(Storage.CurrentContext, id.Concat(NumofattrField)).AsBigInteger();
        }

        internal static void setNumOfAttr(byte[] id, BigInteger no)
        {
            Storage.Put(Storage.CurrentContext, id.Concat(NumofattrField), no);
        }

        internal static BigInteger getNumOfPk(byte[] id)
        {
            return Storage.Get(Storage.CurrentContext, id.Concat(NumofpkField)).AsBigInteger();
        }

        internal static void setNumOfPk(byte[] id, BigInteger no)
        {
            Storage.Put(Storage.CurrentContext, id.Concat(NumofpkField), no);
        }

        internal static byte[] getRecovery(byte[] id)
        {
            return Storage.Get(Storage.CurrentContext, id.Concat(RecoveryField));
        }

        internal static void setRecovery(byte[] id, byte[] recovery)
        {
            Storage.Put(Storage.CurrentContext, id.Concat(RecoveryField), recovery);
        }

        internal static byte[] encodeId(byte[] id)
        {
            if (id.Length == 0 || id.Length > 255) return null;
            return GetByte((byte)id.Length).Concat(id);
        }

        internal static byte[] decodeId(byte[] data)
        {
            if (data.Length < 1 || data.Length != data[0] + 1) return null;
            else return data.Range(1, data[0]);
        }

        internal static bool insertPk(byte[] id, byte[] pk)
        {
            return Insert(id.Concat(PkField), pk);
        }
        internal static bool deletePk(byte[] id, byte[] pk)
        {
            return Delete(id.Concat(PkField), pk);
        }

        internal static bool insertAttr(byte[] id, byte[] path)
        {
            return Insert(id.Concat(AttrnameField), path);
        }
        internal static bool deleteAttr(byte[] id, byte[] path)
        {
            return Delete(id.Concat(AttrnameField), path);
        }

        internal static byte[] getHeader(byte[] index)
        {
            return Storage.Get(Storage.CurrentContext, index);
        }

        internal static void setHeader(byte[] index, byte[] header)
        {
            Storage.Put(Storage.CurrentContext, index, header);
        }

        internal static byte[] getNext(byte[] v)
        {
            int next_idx = (int) asBigInteger( v.Range(0, 4) );
            return v.Range(4,  next_idx);
        }

        // llen(4) || ll || 1 | rlen(4) | rr
        internal static byte[] getPrev(byte[] v)
        {
            int llen = (int) asBigInteger(v.Range(0, 4));
            int rlen = (int) asBigInteger(v.Range(llen+5, 4));
            return v.Range(9 + llen, rlen);
        }

        internal static byte[] makeListPointer(byte[] next, byte[] prev)
        {
            return packBytes(next).Concat(new byte[1] { 0x01 }).Concat(packBytes(prev));
        }

        internal static byte[] getListPointer(byte[] index, byte[] item)
        {
            return Storage.Get(Storage.CurrentContext, index.Concat(item));
        }

        internal static BigInteger asBigInteger(byte[] b)
        {
            BigInteger ret = 0;
            int i = 0;
            for (; i < b.Length; i++)
            {
                ret *= 256;
                ret += b[i];
            }
            return ret;
        }

        internal static byte[] packBytes(byte[] b)
        {
            int l = b.Length;

            byte l0 = (byte)(l % 256); l = (l - l0) / 256;
            byte l1 = (byte)(l % 256); l = (l - l1) / 256;
            byte l2 = (byte)(l % 256); l = (l - l2) / 256;
            byte l3 = (byte)(l % 256);

            return GetByte(l3).Concat(GetByte(l2)).Concat(GetByte(l1)).Concat(GetByte(l0)).Concat(b);
        }

        internal static byte[] GetAttr(byte[] id, byte[] attrname)
        {
            return Storage.Get(Storage.CurrentContext, id.Concat(AttrField).Concat(attrname));
        }

        internal static void SetAttr(byte[] id, byte[] attrname, byte[] type, byte[] value)
        {
            Storage.Put(Storage.CurrentContext,
                id.Concat(AttrField.Concat(attrname)),
                GetByte((byte)type.Length).Concat(type.Concat(value)));
        }
        internal static void DelAttr(byte[] id, byte[] attrname)
        {
            Storage.Delete(Storage.CurrentContext, id.Concat(AttrField.Concat(attrname)));
        }

        internal static bool isEqual(byte[] a, byte[] b)
        {
            return ((a.Length == b.Length) &&
                    Helper.AsBigInteger(a) == Helper.AsBigInteger(b));
        }

        internal static bool Insert(byte[] index, byte[] item)
        {
            byte[] header = getHeader(index); //header is an item
            byte[] q = getListPointer(index, item);
            byte[] qheader = getListPointer(index, header);
            if (q != null) return false;

            if (header == null)
            {
                Storage.Put(Storage.CurrentContext, index.Concat(item),
                        makeListPointer(null, null));
                setHeader(index, item);
            }
            else
            {
                //item.next = header,item.prev = null,
                Storage.Put(Storage.CurrentContext, index.Concat(item),
                            makeListPointer(header, null));
                // header.next = header.next, header.prev = item
                Storage.Put(Storage.CurrentContext, index.Concat(header),
                            makeListPointer(getNext(qheader), item));
                // header = item
                setHeader(index, item);
            }
            return true;
        }

        internal static bool Delete(byte[] index, byte[] item)
        {
            //if item does not exist, return false
            //else return true
            byte[] q = getListPointer(index, item);
            if (q.Length == 0) return false;

            byte[] nil = null;
            //item is the head & tail
            byte[] prev = getPrev(q), next = getNext(q);
            if (prev.Length == 0)
            {
                if (next.Length == 0)
                {
                    setHeader(index, nil);
                } else
                {
                    //change the next node
                    byte[] qnext = getListPointer(index, next);
                    Storage.Put(Storage.CurrentContext, index.Concat(next),
                                makeListPointer(getNext(qnext), null)); //
                    setHeader(index, next);
                }
            }
            else
            {
                //prev != null
                if (next.Length == 0)
                {
                    byte[] qprev = getListPointer(index, prev);
                    Storage.Put(Storage.CurrentContext, index.Concat(prev),
                        makeListPointer(null, getPrev(qprev)));
                }
                else
                {
                    //prev != null & next != null, item is in the middle
                    byte[] qnext = getListPointer(index, next);
                    byte[] qprev = getListPointer(index, prev);
                    Storage.Put(Storage.CurrentContext, index.Concat(prev), makeListPointer(next, getPrev(qprev)));
                    Storage.Put(Storage.CurrentContext, index.Concat(next), makeListPointer(getNext(qnext), prev));
                }
            }
            Storage.Put(Storage.CurrentContext, index.Concat(item), nil);
            return true;
        }
        internal static byte[] GetByte(byte i)
        {
            byte[] bytes = new byte[256] {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
                31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
                41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
                51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
                61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
                71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
                81, 82, 83, 84, 85, 86, 87, 88, 89, 90,
                91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
                101, 102, 103, 104, 105, 106, 107, 108,
                109, 110, 111, 112, 113, 114, 115, 116,
                117, 118, 119, 120, 121, 122, 123, 124,
                125, 126, 127, 128, 129, 130, 131, 132,
                133, 134, 135, 136, 137, 138, 139, 140,
                141, 142, 143, 144, 145, 146, 147, 148, 149, 150,
                151, 152, 153, 154, 155, 156, 157, 158, 159, 160,
                161, 162, 163, 164, 165, 166, 167, 168, 169, 170,
                171, 172, 173, 174, 175, 176, 177, 178, 179, 180,
                181, 182, 183, 184, 185, 186, 187, 188, 189, 190,
                191, 192, 193, 194, 195, 196, 197, 198, 199, 200,
                201, 202, 203, 204, 205, 206, 207, 208, 209, 210,
                211, 212, 213, 214, 215, 216, 217, 218, 219, 220,
                221, 222, 223, 224, 225, 226, 227, 228, 229, 230,
                231, 232, 233, 234, 235, 236, 237, 238, 239, 240,
                241, 242, 243, 244, 245, 246, 247, 248, 249, 250,
                251, 252, 253, 254, 255 };

            return Helper.Range(bytes, i, 1);
        }
    }
}

