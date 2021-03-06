using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class SaveSystem
{
    // Save button activates this method. Takes in the player's stats, as well as their position, and saves it.
    public static void SavePlayer(PlayerStatManager playerStats, PlayerMovement playerMovement,
        PlayerManager playerManager, SaveMethods saveMethods)
    {
        if (saveMethods.saveNums.Count != 10)
        {
            // Adds to the save number variable, allowing for a new save afterwards.
            var saveNumString = saveMethods.FloattoString(saveMethods.saveNum);
            var formatter = new BinaryFormatter();
            // Saves the least used path in windows plus save(number).sv to the path variable.
            var path = Application.persistentDataPath + "/save" + saveNumString + ".sv";
            // Goes through every button in the button array!
            foreach (var button in saveMethods.buttons)
            {
                // Counts down from how many Save Numbers are in the saveNums list.
                for (var i = 0; i < saveMethods.saveNums.Count; i++)
                {
                    // Turns the button's name into a char array with a length of one, getting the saveNum!
                    var buttonNums = button.name.ToCharArray(button.name.Length - 1, 1);
                    var buttonNum = float.Parse(buttonNums[0].ToString());
                    if (saveMethods.saveNum != buttonNum) continue;
                    var buttonText = button.transform.Find("Text").GetComponent<Text>();
                    buttonText.text = "Save Occupied!";
                    break;
                }
            }

            var stream = new FileStream(path, FileMode.Create);
            var data = new PlayerData(playerStats, playerMovement, playerManager);

            formatter.Serialize(stream, data);
            stream.Close();
            
            saveMethods.saveNums.Add(saveMethods.saveNum);
            saveMethods.saveNum++;
        }
        else
        {
            Debug.LogError("Save Limit Reached!");
        }
    }

    public static string FindPath(float saveNum)
    {
        // Finds the path
        var saveMethods = SaveMethods.instance;
        var saveNumString = saveNum.ToString(CultureInfo.InvariantCulture);
        // Finds the file path that actually exists and contains the saveNumString and returns it.
        return saveMethods.filePaths.FirstOrDefault(path => File.Exists(path) && path.Contains(saveNumString));
    }

    public static PlayerData LoadPlayer(string path)
    {

        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            var data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        Debug.LogError("Save File not found in: " + path + "!");
        return null;
    }

    public static void DeleteSave(string path)
    {
        if (!File.Exists(path)) return;
        foreach (var buttonText in SaveMethods.instance.buttons.Select(button => button.transform.Find("Text").gameObject.GetComponent<Text>()))
        {
            if (path.Contains(buttonText.text.Substring(5, 1)))
            {
                // Changes the text of the save button to Empty Save
                buttonText.text = "Empty Save";
                // Removes the save number that's being deleted from the saveNums list
                SaveMethods.instance.saveNums.Remove(SaveMethods.instance.saveNum);
                // Gets the current unsaved saveNum by subtracting savNum by one
                SaveMethods.instance.saveNum--;
                SaveMethods.instance.filePaths[SaveMethods.instance.filePaths.Count - 1] = null;
                File.Delete(path);
                break;
            }
        }
    }
}
