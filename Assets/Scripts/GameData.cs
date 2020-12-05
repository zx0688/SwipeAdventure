using System;
using System.Collections.Generic;
using Managers;

[Serializable]
public class GameData {
    public List<CardData> cards;
    public List<ResourceData> resources;
    public List<QuestData> quests;

    public List<ActionData> actions;
    public List<BuffData> buffs;

    public PlayerData[] profiles;

    public ConfigData config;

    public int timestamp;
}

[Serializable]
public class BuffData {
    public int id;
    public string[] tags;
    public string name;
    public string description;
    public List<ConditionData> conditions;
    public List<RewardData> cost;
    public List<RewardData> reward;
    public int trigger;
    public bool swipe;
    public bool disable;
    public int factor;

}

[Serializable]
public class ConfigData {
    public List<ConditionData> win;
    public List<ConditionData> fail;
    public List<int> tutorial;
    public List<int> tutstep;
}

[Serializable]
public class QuestData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public List<TriggerData> triggers;
    public List<ConditionData> conditions;
    public List<RewardData> rewards;
    public List<RewardData> fines;

    public int duration;
    public int startTime;
    public int endTime;

}

[Serializable]
public class CardData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public int priority;
    public List<TriggerData> triggers;
    public List<ConditionData> conditions;
    public List<RewardData> drops;
    public string[] locations;

    public string[] sound;

    public int duration;
    public float chance;
    public int locked;

    public bool off;
    public bool validate;
    public string image;
    public string icon;

    public string anim;

    public string header;
    public string description;

    public ChoiceData left;
    public ChoiceData right;
    public ChoiceData eRight;
    public ChoiceData eLeft;

}

[Serializable]
public class ChoiceData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public string anim;
    public string text;
    public List<RewardData> cost;
    public List<RewardData> reward;
    public List<ConditionData> conditions;

    public string[] sound;

}

[Serializable]
public class ActionData {
    public int id;
    public string[] tags;
    public string name;
    public string description;
    public bool enemy;
    public List<RewardData> cost;
    public List<RewardData> reward;
    public List<ConditionData> conditions;
}

[Serializable]
public class RewardData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public float chance;
    public int count;
    public bool enemy;
    public List<ConditionData> conditions;
}

[Serializable]
public class ResourceData {

    public int id;
    public int category;
    public string[] tags;
    public float inapp;
    public int priority;
    public bool image2enemy;

    public bool hide;
    public string image;
    public string icon;
    public string description;
    public string name;

    public int maxValue;

    public string[] locations;

}

[Serializable]
public class TriggerData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public int choise;
    public int count;
}

[Serializable]
public class ConditionData {
    //0 - card, 1 - res, 2 - tag
    public int id;
    public int category;
    public string[] tags;
    public bool invert;
    public string sign;
    public int choise;
    public int count;
    public string location;
    public bool enemy;

}