using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// https://answers.unity.com/questions/956123/add-and-select-game-view-resolution.html
/// https://forum.unity.com/threads/add-game-view-resolution-programatically-old-solution-doesnt-work.860563/
/// https://github.com/Syy9/GameViewSizeChanger
/// 
/// How to use:
/// Simple Call UnityResolutionManager.UseCustomResolution() to set the Unity Editor GameViewMode
/// and remove automatically when application ends
/// Note that you should not remove or add new resolution while in Play Mode
/// 
/// Dev Note:
/// Took the code from unity forums and add Contains() function from syy9
/// </summary>
public static class UnityGameViewManager
{
    public enum GameViewSizeType
    {
        FixedResolution,
        AspectRatio
    }
    public struct GameViewSize//names of variable is important to match unity classes
    {
        public int width;
        public int height;
        public string baseText;
        public GameViewSizeType type;
    }
    private static GameViewSize _gameViewSize;

    public static void UseCustomResolution(int width, int height, string baseText = "Temp Resolution")
    {
#if UNITY_EDITOR
        object gameViewSizesinstance = GetInstance(Types.gameViewSizes);
        object group = GetGroup(gameViewSizesinstance);

        AddResolution(width, height, baseText);
        _gameViewSize = new GameViewSize
        {
            width = width,
            height = height,
            baseText = baseText,
            type = GameViewSizeType.FixedResolution
        };

        SetResolution(GetTotalCount(group) - 1);

        Application.quitting += OnApplicationQuit;
#endif
    }

    static void OnApplicationQuit()
    {
        object gameViewSizesinstance = GetInstance(Types.gameViewSizes);
        object group = GetGroup(gameViewSizesinstance);

        RemoveResolution(group,_gameViewSize);
        SetResolution(0);

        Application.quitting -= OnApplicationQuit;
    }


    static void AddResolution(int width, int height, string label)
    {
        Type gameViewSize = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        Type gameViewSizes = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");

        Type gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        Type generic = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizes);

        MethodInfo getGroup = gameViewSizes.GetMethod("GetGroup");
        object instance = generic.GetProperty("instance").GetValue(null, null);

        Type[] types = new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) };
        ConstructorInfo constructorInfo = gameViewSize.GetConstructor(types);
        object entry = constructorInfo.Invoke(new object[] { 1, width, height, label });
        MethodInfo addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");

        object group = getGroup.Invoke(instance, new object[] { (int)GameViewSizeGroupType.Standalone });
        addCustomSize.Invoke(group, new object[] { entry });
    }

    public static void SetResolution(int index)
    {
        Type gameView = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        PropertyInfo selectedSizeIndex = gameView.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        EditorWindow window = EditorWindow.GetWindow(gameView);
        selectedSizeIndex.SetValue(window, index, null);
    }


    /// <summary>
    /// find and remove a particular gameviewsize, of a particular width,height,label etc..
    /// </summary>
    static void RemoveResolution(object gameViewSizeGroupinstance, GameViewSize gameviewsize)
    {
        int gameViewSizeLength = GetCustomCount(gameViewSizeGroupinstance);
        int totalCount = GetTotalCount(gameViewSizeGroupinstance);
        for (int i = totalCount - gameViewSizeLength; i < totalCount; i++)
        {
            object other_gameViewSize = GetGameViewSize(gameViewSizeGroupinstance, i);
            if (GameViewSize_Equals(gameviewsize, other_gameViewSize))
            {
                RemoveCustomSize(gameViewSizeGroupinstance, i);
            }
        }
    }

    /// <summary>
    /// find and see if we currently have an existing gameviewsize
    /// </summary>
    static bool Contains(object gameViewSizeGroupinstance, GameViewSize gameviewsize)
    {
        int gameViewSizeLength = GetCustomCount(gameViewSizeGroupinstance);
        int totalCount = GetTotalCount(gameViewSizeGroupinstance);
        for (int i = totalCount - gameViewSizeLength; i < totalCount; i++)
        {
            if (GameViewSize_Equals(gameviewsize, GetGameViewSize(gameViewSizeGroupinstance, i)))
            {
                return true;
            }
        }
        return false;
    }


    static bool GameViewSize_Equals(GameViewSize a, object b)
    {
        int b_width = (int)GetGameSizeProperty(b, "width");
        int b_height = (int)GetGameSizeProperty(b, "height");
        string b_baseText = (string)GetGameSizeProperty(b, "baseText");
        GameViewSizeType b_sizeType = (GameViewSizeType)Enum.Parse(typeof(GameViewSizeType), GetGameSizeProperty(b, "sizeType").ToString());

        return a.type == b_sizeType && a.width == b_width && a.height == b_height && a.baseText == b_baseText;
    }
    static object GetGameSizeProperty(object instance, string name)
    {
        return instance.GetType().GetProperty(name).GetValue(instance, new object[0]);
    }



    static object GetGroup(object gameViewSizesinstance)
    {
        GameViewSizeGroupType groupType = GetCurrentGameViewSizeGroupType(gameViewSizesinstance);
        MethodInfo getGroupMethod = gameViewSizesinstance.GetType().GetMethod("GetGroup");
        return getGroupMethod.Invoke(gameViewSizesinstance, new object[] { (int)groupType });
    }
    static object GetGameViewSize(object gameViewSizeGroupinstance, int i)
    {
        MethodInfo getGameViewSizeMethod = gameViewSizeGroupinstance.GetType().GetMethod("GetGameViewSize");
        object[] parameters = new object[] { i };
        return getGameViewSizeMethod.Invoke(gameViewSizeGroupinstance, parameters);
    }
    static void RemoveCustomSize(object gameViewSizeGroupinstance, int index)
    {
        MethodInfo getGameViewSizeMethod = gameViewSizeGroupinstance.GetType().GetMethod("RemoveCustomSize");
        object[] parameters = new object[] { index };
        getGameViewSizeMethod.Invoke(gameViewSizeGroupinstance, parameters);
    }
    static int GetBuiltinCount(object gameViewSizeGroupinstance)
    {
        MethodInfo getCustomCountMethod = gameViewSizeGroupinstance.GetType().GetMethod("GetBuiltinCount");
        return (int)getCustomCountMethod.Invoke(gameViewSizeGroupinstance, null);
    }
    static int GetCustomCount(object gameViewSizeGroupinstance)
    {
        MethodInfo getCustomCountMethod = gameViewSizeGroupinstance.GetType().GetMethod("GetCustomCount");
        return (int)getCustomCountMethod.Invoke(gameViewSizeGroupinstance, null);
    }
    static int GetTotalCount(object gameViewSizeGroupinstance)
    {
        MethodInfo getCustomCountMethod = gameViewSizeGroupinstance.GetType().GetMethod("GetTotalCount");
        return (int)getCustomCountMethod.Invoke(gameViewSizeGroupinstance, null);
    }


    public static GameViewSizeGroupType GetCurrentGameViewSizeGroupType(object gameViewSizesInstance)
    {
        PropertyInfo currentGroupType = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
        GameViewSizeGroupType groupType = (GameViewSizeGroupType)(int)currentGroupType.GetValue(gameViewSizesInstance, null);

        return groupType;
    }








    static object GetInstance(Type gameViewSizes)
    {
        return gameViewSizes.GetProperty("instance").GetValue(null, null);
    }
    private static class Types
    {
        private static Assembly assembly = typeof(Editor).Assembly;
        public static Type gameView = assembly.GetType("UnityEditor.GameView");

        public static Type gameViewSizeType = assembly.GetType("UnityEditor.GameViewSizeType");
        public static Type gameViewSize = assembly.GetType("UnityEditor.GameViewSize");
        public static Type gameViewSizes = typeof(ScriptableSingleton<>).MakeGenericType(assembly.GetType("UnityEditor.GameViewSizes"));
    }
}