using UnityEngine;
using UnityEditor;
using System.IO;
using Multimorphic.P3App.GUI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

public class KeyMapGenerator
{

    [UnityEditor.MenuItem("Multimorphic/Generate Key Map")]
    static void GenerateKeyMap()
    {
        string configFilename = "./Configuration/AppConfig.json";
        string keyMapFilename = "./Configuration/KeyMap.html";
        P3ControllerInterfaceConfiguration config = P3ControllerInterfaceConfiguration.FromFile(configFilename);


        string[] opening = {    "<!DOCTYPE html>",
                                "<html>",
                                 "<head>",
                                 "<style>",
                                 "p.small { line-height: 0.3; }",
                                 "p.big { line-height: 1.8; }",
                                "</style>",
                                "</head>",
                                "<body>" };
        string[] closing = {    "</body>",
                                "</html>"};

        var lines = new List<string>();

        lines.AddRange(opening);

        string[,,] keys =
        {
            {
                { "F1", "",  },
                { "F2", "",  },
                { "F3", "",  },
                { "F4", "",  },
                { "F5", "",  },
                { "F6", "",  },
                { "F7", "",  },
                { "F8", "",  },
                { "F9", "",  },
                { "F10", "" },
                { "Ins", "Insert" },
                { "Del", "Delete" },
                { "", "" },
            },
            {
                { "1", "Alpha1" },
                { "2", "Alpha2" },
                { "3", "Alpha3" },
                { "4", "Alpha4" },
                { "5", "Alpha5" },
                { "6", "Alpha6" },
                { "7", "Alpha7" },
                { "8", "Alpha8" },
                { "9", "Alpha9" },
                { "0", "Alpha0" },
                { "-", "Minus" },
                { "=", "Equals" },
                { "<-", "Backspace" },

            },
            {
                { "Q", "" },
                { "W", "" },
                { "E", "" },
                { "R", "" },
                { "T", "" },
                { "Y", "" },
                { "U", "" },
                { "I", "" },
                { "O", "" },
                { "P", "" },
                { "[", "LeftBracket" },
                { "]", "RightBracket" },
                { "\\", "Backslash" },
            },
            {
                { "A", "" },
                { "S", "" },
                { "D", "" },
                { "F", "" },
                { "G", "" },
                { "H", "" },
                { "J", "" },
                { "K", "" },
                { "L", "" },
                { ";", "Semicolon" },
                { ",", "Comma" },
                { "Enter", "Enter" },
                { "", "" },
            }, 
            {
                { "Z", "" },
                { "X", "" },
                { "C", "" },
                { "V", "" },
                { "B", "" },
                { "N", "" },
                { "M", "" },
                { ",", "Comma" },
                { ".", "Period" },
                { "/", "Slash" },
                { "", "" },
                { "", "" },
                { "", "" },
            }
        };
  
        int indent = 0;
        for (int row = 0; row < 5; row++)
        {
            indent += 20;
            lines.Add("<table border = \"1\"  style = \"margin-left:" + indent.ToString() + "px\" >");
            lines.Add("<tr bgcolor = \"grey\" >");

            for (int key = 0; key < 13; key++)
            { 
                KeySwitchConfigFileEntry matchingMap = null;

                foreach (var map in config.KeySwitchMaps) {
                    if ((keys[row, key, 0] == map.Key) || (keys[row, key, 1] == map.Key))
                        matchingMap = map;
                }

                string keyLabel = keys[row, key, 0];

                string meaning = "";
                if (matchingMap != null)
                    meaning = matchingMap.Switch + matchingMap.GUIToModeEvent + matchingMap.ModeToGUIEvent + matchingMap.ModeToModeEvent;
                meaning = meaning.Replace("Evt_", "");

                string size = "2";
                if (meaning.Length > 8)
                    size = "1";

                if (keyLabel != "")
                    lines.Add("<th { text - align: center; width = 60px height = 50px }> <p class=\"small\"><font size = \"7\" color=\"black\">" + keyLabel + "  </font><font size = \"" + size + "\" color=\"white\"><br>" + meaning + "</font></p></th>");                
            }

            lines.Add("</table>");  
        }

        lines.AddRange(closing);
        System.IO.File.WriteAllLines(keyMapFilename, lines.ToArray());

        var processInfo = new ProcessStartInfo(Path.GetFullPath(keyMapFilename));
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = true;

        Process.Start(processInfo);
    }
}
