﻿using System;
using AncibleCoreCommon.CommonData.Traits;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientObjectData
    {
        public string Name;
        public string Subtitle;
        public string ObjectId;
        public string Sprite;
        public bool ShowName;
        public Vector2IntData Position;
        public CombatAlignment Alignment;
        public int Health;
        public int MaxHealth;
        public InteractionType[] Interactions;
        public ClientStatusEffectData[] StatusEffects;
        public ClientObjectIconData[] Icons;
    }
}