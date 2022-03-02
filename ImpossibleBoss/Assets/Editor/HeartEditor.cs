using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal enum EColor
{
    Red,
    Yellow,
    Green,
    Blue,
    Purple
}

[CustomEditor(typeof(ChessBoss))]
public class HeartEditor : Editor
{
    ChessBoss boss;
    string format;
    string bitFormat;
    bool[] forderIndex = new bool[5];
    private void OnEnable()
    {
        boss = target as ChessBoss;
        if (boss.DeadZoneInfo == null)
        {            
            boss.DeadZoneInfo = new List<QuizDeadZone>(64);
            for (int index = 0; index < 64; index++)
            {
                boss.DeadZoneInfo.Add(new QuizDeadZone(/*7 - */Mathf.FloorToInt(index / 8), index % 8, 5, false));
            }
            boss.QuizPatternLoad();
        }
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        format = @"({0}, {1})";

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("패턴 저장", GUILayout.Width(200), GUILayout.Height(40)))
        {
            QuizPatternSave();
        }
        if (GUILayout.Button("패턴 불러오기", GUILayout.Width(200), GUILayout.Height(40)))
        {
            boss.QuizPatternLoad();
        }
        GUILayout.EndHorizontal();

        for (int index = 0; index < 5; index++)
        {
            forderIndex[index] = EditorGUILayout.BeginFoldoutHeaderGroup(forderIndex[index], (index + 1).ToString());
            if (forderIndex[index])
            {
                for (int row = 0; row < 8; row++)
                {
                    GUILayout.BeginHorizontal();
                    for (int col = 0; col < 8; col++)
                    {
                        //버튼이 눌렸을때 그 버튼이 해당되는 index색으로 바꿈
                        if (boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone)
                        {
                            GUI.backgroundColor = GetColor((EColor)index);
                        }

                        //Order와 index가 같지 않으면서 눌렸으면 Box 아니면 Button
                        if (boss.DeadZoneInfo[(row * 8 + col)].Order != index && boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone)
                        {
                            if (boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone)
                            {
                                GUI.backgroundColor = GetColor((EColor)boss.DeadZoneInfo[(row * 8 + col)].Order);
                            }
                            GUILayout.Box(string.Format("{0}", boss.DeadZoneInfo[(row * 8 + col)].Order + 1), GUILayout.Width(40), GUILayout.Height(40));
                        }
                        else
                        {
                            if (GUILayout.Button(string.Format(format, 7 - row, col), GUILayout.Width(40), GUILayout.Height(40)))
                            {
                                if (boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone)
                                {
                                    boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone = false;
                                }
                                else
                                {
                                    boss.DeadZoneInfo[(row * 8 + col)].IsDeadZone = true;
                                    boss.DeadZoneInfo[(row * 8 + col)].Order = index;
                                }
                            }
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    void QuizPatternSave()
    {
        bitFormat = @"{0}{1}{2}{3}";
        string bitString = "";
        for (int index = 0; index < 64; index++)
        {
            QuizDeadZone info = boss.DeadZoneInfo[index];
            bitString += string.Format(bitFormat, info.IsDeadZone ? 1 : 0, Convert.ToString(info.Coord[0], 2).PadLeft(3, '0'), Convert.ToString(info.Coord[1], 2).PadLeft(3, '0'), Convert.ToString(info.Order, 2).PadLeft(3, '0'));

        }

        System.IO.File.WriteAllText(@"Assets/BossStageAssets/Chess/ChessQuiz/QuizPattern.txt", bitString);

        //byte[] bitResult = new byte[80];
        //for (int index = 0; index < 80; index++)
        //{
        //    bitResult[index] = 0;
        //}


        //for (int index = 0; index < 64; index++)
        //{
        //    QuizDeadZone info = boss.DeadZoneInfo[index];
        //    bitString += string.Format(bitFormat, info.IsDeadZone ? 1 : 0, Convert.ToString(info.Coord[0], 2).PadLeft(3, '0'), Convert.ToString(info.Coord[1], 2).PadLeft(3, '0'), Convert.ToString(info.Order, 2).PadLeft(3, '0'));
        //}

        //uint one = 0b_1;
        //uint zero = 0b_0;



        //for (int index = 0; index < 640; index++)
        //{



        //}

        //System.IO.File.WriteAllText(@"Assets/BossStageAssets/Chess/ChessQuiz/QuizPattern.txt", bitString);
    }    

    Color GetColor(EColor color)
    {
        switch (color)
        {
            case EColor.Red: return Color.red;
            case EColor.Yellow: return Color.yellow;
            case EColor.Green: return Color.green;
            case EColor.Blue: return Color.blue;
            case EColor.Purple: return new Color(0.749f, 0.0f, 1.0f);
            default: throw new System.ArgumentOutOfRangeException(nameof(color));
        }
    }
}
