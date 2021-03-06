﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using Core;
using Game.Knight;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Hotfix
{
    public class HotfixMBContainer : MonoBehaviour
    {
        [HideInInspector][SerializeField]
        protected string                        mHotfixName;
        [HideInInspector][SerializeField]
        protected List<UnityObject>             mObjects;
        [HideInInspector][SerializeField]
        protected List<BaseDataDisplayObject>   mBaseDatas;
        
        private HotfixMB                        mMBHotfixObj;
        public  HotfixMB                        MBHotfixObject { get { return this.mMBHotfixObj; } }

        public  string                          HotfixName
        {
            get { return mHotfixName;  }
            set { mHotfixName = value; }
        }

        protected virtual void Awake()
        {
            this.InitHotfixMB();

            if (this.mMBHotfixObj != null)
                mMBHotfixObj.Awake();
        }

        protected virtual void Start()
        {
            if (mMBHotfixObj == null) return;
            mMBHotfixObj.Start();
        }

        protected virtual void Update()
        {
            if (mMBHotfixObj == null) return;
            mMBHotfixObj.Update();
        }

        protected virtual void OnDestroy()
        {
            if (mMBHotfixObj != null)
                mMBHotfixObj.OnDestroy();

            if (mObjects != null)
                mObjects.Clear();

            if (mBaseDatas != null)
                mBaseDatas.Clear();

            mMBHotfixObj = null;
            mObjects = null;
            mBaseDatas = null;
        }

        protected virtual void OnEnable()
        {
            if (mMBHotfixObj == null) return;
            mMBHotfixObj.OnEnable();
        }

        protected virtual void OnDisable()
        {
            if (mMBHotfixObj == null) return;
            mMBHotfixObj.OnDisable();
        }

        public virtual void OnUnityEvent(Object rTarget)
        {
            if (mMBHotfixObj == null) return;
            mMBHotfixObj.OnUnityEvent(rTarget);
        }

        protected List<BaseDataObject> ToBaseDataObjects(List<BaseDataDisplayObject> rBaseDatas)
        {
            if (rBaseDatas == null) return new List<BaseDataObject>();

            List<BaseDataObject> rBaseDataObjects = new List<BaseDataObject>();
            for (int i = 0; i < rBaseDatas.Count; i++)
            {
                BaseDataObject rObj = new BaseDataObject();
                rObj.Type = rBaseDatas[i].Type;
                rObj.Name = rBaseDatas[i].Name;
                if (rBaseDatas[i].Type == "Int")
                    rObj.Object = rBaseDatas[i].IntObject;
                else if (rBaseDatas[i].Type == "Long")
                    rObj.Object = rBaseDatas[i].LongObject;
                else if (rBaseDatas[i].Type == "Float")
                    rObj.Object = rBaseDatas[i].FloatObject;
                else if (rBaseDatas[i].Type == "Double")
                    rObj.Object = rBaseDatas[i].DoubleObject;
                else if (rBaseDatas[i].Type == "String")
                    rObj.Object = rBaseDatas[i].StringObject;
                rBaseDataObjects.Add(rObj);
            }
            return rBaseDataObjects;
        }

        public void InitHotfixMB()
        {
            if (mMBHotfixObj == null && !string.IsNullOrEmpty(mHotfixName))
            {
                mMBHotfixObj = HotfixApp.Instance.Instantiate<HotfixMB>(mHotfixName);
                mMBHotfixObj.SetHotfix(mHotfixName, this.gameObject);

                if (this.mObjects == null) this.mObjects = new List<UnityObject>();
                mMBHotfixObj.Initialize(this.mObjects, this.ToBaseDataObjects(mBaseDatas));
            }
        }
    }
}
