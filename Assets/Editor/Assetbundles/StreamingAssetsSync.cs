﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using Core;
using Core.Editor;

namespace Framework.Editor
{
    /// <summary>
    /// 同步资源到StreamingAssets的目录下
    /// </summary>
    public static class StreamingAssetsSync
    {
        /// <summary>
        /// 文件的类型
        /// </summary>
        public enum FileState
        {
            New,
            Modify,
            Delete,
            Same
        };
    
        public class FileVersion
        {
            /// <summary>
            /// 文件原始路径
            /// </summary>
            public string       ScrPath;
            /// <summary>
            /// 文件的目标路径
            /// </summary>
            public string       DistPath;
            /// <summary>
            /// 文件的状态
            /// </summary>
            public FileState    State;
        }
        
        /// <summary>
        /// 加载Manifest
        /// </summary>
        private static IEnumerator LoadManifest(string rManifestURL, Action<AssetBundleManifest> rLoadCompleted)
        {
            WWW www = new WWW(rManifestURL);
            yield return www;
    
            if (www == null || !string.IsNullOrEmpty(www.error))
            {
                Debug.Log("加载Manifest出错: " + www.error);
                UtilTool.SafeExecute(rLoadCompleted, null);
                yield break;
            }
            var rABManifest = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest") as AssetBundleManifest;
            WWWAssist.Dispose(ref www);
            UtilTool.SafeExecute(rLoadCompleted, rABManifest);
        }
    
        [MenuItem("Tools/同步StreamingAssets下的资源")]
        public static void SyncAssets()
        {
            EditorCoroutineManager.Start(SyncAssets_Async());
        }
    
        /// <summary>
        /// 开始同步资源
        /// </summary>
        public static IEnumerator SyncAssets_Async()
        {
            AssetBundleManifest rBuildManifest = null;
            AssetBundleManifest rStreamingManifest = null;
    
            string rManifestName = AssetbundleHelper.Instance.GetManifestName();
            string rBuildABDir = Path.GetFullPath(AssetbundleHelper.Instance.GetPathPrefix_Assetbundle()).Replace('\\', '/');
            string rStreamingDir = Path.GetFullPath("Assets/StreamingAssets/Assetbundles/" + rManifestName).Replace('\\', '/');
    
            string rBuildManifestURL = "file:///" + rBuildABDir + "/" + rManifestName;
            string rStreamingManifestURL = "file:///" + rStreamingDir + "/" + rManifestName;
    
            yield return EditorCoroutineManager.Start(LoadManifest(rBuildManifestURL, (rABManifest) => 
            {
                rBuildManifest = rABManifest;
            }));
    
            yield return EditorCoroutineManager.Start(LoadManifest(rStreamingManifestURL, (rABManifest) => 
            {
                rStreamingManifest = rABManifest;
            }));
            
            // 得到文件复制信息
            Dict<string, FileVersion> rFileVersionDict = new Dict<string, FileVersion>();
            if (rBuildManifest == null) yield break;
    
            List<string> rSrcFiles = new List<string>(rBuildManifest.GetAllAssetBundles());
            List<string> rDistFiles = new List<string>(rStreamingManifest != null ? rStreamingManifest.GetAllAssetBundles() : new string[] { });
            for (int i = 0; i < rSrcFiles.Count; i++)
            {
                FileVersion rFileVersion = new FileVersion();
                rFileVersion.ScrPath = rBuildABDir + "/" + rSrcFiles[i];
                rFileVersion.DistPath = rStreamingDir + "/" + rSrcFiles[i];
                if (rDistFiles.Contains(rSrcFiles[i]))
                {
                    string rSrcMD5 = rBuildManifest.GetAssetBundleHash(rSrcFiles[i]).ToString();
                    string rDistMD5 = rStreamingManifest.GetAssetBundleHash(rDistFiles[i]).ToString();
    
                    if (rSrcMD5.Equals(rDistMD5))
                        rFileVersion.State = FileState.Same;
                    else
                        rFileVersion.State = FileState.Modify;
                }
                else
                {
                    rFileVersion.State = FileState.New;
                }
                rFileVersionDict.Add(rSrcFiles[i], rFileVersion);
            }
            for (int i = 0; i < rDistFiles.Count; i++)
            {
                FileVersion rFileVersion = new FileVersion();
                rFileVersion.ScrPath = rBuildABDir + "/" + rDistFiles[i];
                rFileVersion.DistPath = rStreamingDir + "/" + rDistFiles[i];
                if (!rSrcFiles.Contains(rDistFiles[i]))
                {
                    rFileVersion.State = FileState.Delete;
                    rFileVersionDict.Add(rDistFiles[i], rFileVersion);
                }
            }
    
            // 开始复制文件，删除文件
            foreach (var rPair in rFileVersionDict)
            {
                FileVersion rFileVersion = rPair.Value;
    
                if (rFileVersion.State == FileState.New || rFileVersion.State == FileState.Modify)
                {
                    FileInfo rFileInfo = new FileInfo(rFileVersion.DistPath);
                    if (!Directory.Exists(rFileInfo.DirectoryName)) Directory.CreateDirectory(rFileInfo.DirectoryName);
    
                    File.Copy(rFileVersion.ScrPath, rFileVersion.DistPath, true);
                }
                else if (rFileVersion.State == FileState.Delete)
                {
                    if (File.Exists(rFileVersion.DistPath))
                        File.Delete(rFileVersion.DistPath);
                }
            }
            
            // 复制Manifest
            File.Copy(rBuildABDir + "/" + rManifestName, rStreamingDir + "/" + rManifestName, true);
    
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("提示", "复制完成!", "是");
        }
    }
}