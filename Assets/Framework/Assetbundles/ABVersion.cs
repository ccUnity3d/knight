﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using Core;
using Core.Serializer;
using System.IO;
using System.Collections;
using Core.WindJson;

namespace UnityEngine.AssetBundles
{
    [System.Serializable]
    public partial class ABVersionEntry : SerializerBinary
    {
        public string                       N;
        public int                          V;
        public string                       M;
        public long                         S;
        public string[]                     D;
        
        [SBIgnore][JsonIgnore]
        public string                       Name         { get { return N; } set { N = value; } }
        [SBIgnore][JsonIgnore]
        public int                          Version      { get { return V; } set { V = value; } }
        [SBIgnore][JsonIgnore]
        public string                       MD5          { get { return M; } set { M = value; } }
        [SBIgnore][JsonIgnore]
        public long                         Size         { get { return S; } set { S = value; } }
        [SBIgnore][JsonIgnore]
        public string[]                     Dependencies { get { return D; } set { D = value; } }
    }

    public partial class ABVersion : SerializerBinary
    {
        public class LoaderRequest : CoroutineRequest<LoaderRequest>
        {
            public ABVersion                Version;
            public string                   Url;
            
            public LoaderRequest(string rURL)
            {
                this.Url = rURL;
            }
        }

        [SBIgnore]
        public static string                ABVersion_File_Json    = "ABVersion.Json";
        [SBIgnore]
        public static string                ABVersion_File_Bin     = "ABVersion.Bin";
        [SBIgnore]
        public static string                ABVersion_File_MD5     = "ABVersion_MD5.Bin";

        public Dict<string, ABVersionEntry> Entries;

        public ABVersionEntry GetEntry(string rABName)
        {
            if (this.Entries == null) return null;
            ABVersionEntry rEntry = null;
            this.Entries.TryGetValue(rABName, out rEntry);
            return rEntry;
        }

        public static LoaderRequest Load(string rURL)
        {
            LoaderRequest rRequest = new LoaderRequest(rURL);
            rRequest.Start(Load_Async(rRequest));
            return rRequest;
        }

        public static LoaderRequest Download(string rURL)
        {
            LoaderRequest rRequest = new LoaderRequest(rURL);
            rRequest.Start(Download_Async(rRequest));
            return rRequest;
        }

        public void Save(string rPath)
        {
            using (var fs = new FileStream(rPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    this.Serialize(bw);
                }
            }
        }

        private static IEnumerator Load_Async(LoaderRequest rRequest)
        {
            string rVersionURL = rRequest.Url;
            WWWAssist.LoaderRequest rWWWVersionRequest = WWWAssist.LoadFile(rVersionURL);
            yield return rWWWVersionRequest;

            if (rWWWVersionRequest.Bytes == null || rWWWVersionRequest.Bytes.Length == 0)
            {
                yield break;
            }

            ABVersion rVersion = new ABVersion();
            using (var ms = new MemoryStream(rWWWVersionRequest.Bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    rVersion.Deserialize(br);
                }
            }
            rRequest.Version = rVersion;
        }

        private static IEnumerator Download_Async(LoaderRequest rRequest)
        {
            string rVersionURL = rRequest.Url;
            WebRequestAssist.LoaderRequest rWebVersionRequest = WebRequestAssist.DownloadFile(rVersionURL);
            yield return rWebVersionRequest;

            if (rWebVersionRequest.Bytes == null || rWebVersionRequest.Bytes.Length == 0)
            {
                yield break;
            }

            ABVersion rVersion = new ABVersion();
            using (var ms = new MemoryStream(rWebVersionRequest.Bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    rVersion.Deserialize(br);
                }
            }
            rRequest.Version = rVersion;
        }
    }
}
