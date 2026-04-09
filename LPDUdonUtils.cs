using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;

namespace LocalPoliceDepartment.Utilities
{
    public class LPDUdonUtils
    {
        /// <summary>
        /// Detects if Creator economy features are enabled in this world.
        /// CE is out of beta now so mostly obsolete function.
        /// </summary>
        /// <param name="product">Any UdonProduct</param>
        /// <returns></returns>
        public static bool IsCeEnabled(UdonProduct product)
        {
            return VRC.SDKBase.Utilities.IsValid(product);
        }
        
        /// <summary>
        /// Takes an array of buttons and returns the index of first one that is marked as NOT interactable.
        /// Commonly used for runtime generated lists of buttons where you need to figure out which one was pressed.
        /// Returns -1 if all buttons are interactable.
        /// </summary>
        /// <param name="buttons">The array of Button Components to search through</param>
        /// <returns></returns>
        public static int GetButtonIndex(Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;
                if (!buttons[i].interactable)
                {
                    buttons[i].interactable = true;
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns a VrcPlayerApi object from the displayname. Returns null if that player isnt found in the instance.
        /// </summary>
        /// <param name="displayname">The name of the player to search for</param>
        /// <returns></returns>
        public static VRCPlayerApi GetPlayerApi(string displayname)
        {
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                if (players[i].displayName == displayname) return players[i];
            }
            return null;
        }

        /// <summary>
        /// Returns the first instance of a component found in the players PlayerObjects
        /// </summary>
        /// <param name="player">The player who's PlayerObjects will be searched for the component</param>
        /// <typeparam name="T">The type of the component to find</typeparam>
        /// <returns>Object of the provided Type T</returns>
        public static T GetPlayerObjectOfType<T>(VRCPlayerApi player)
        {
            GameObject[] playerObjects = Networking.GetPlayerObjects(player);
            T component = default;
            
            for (int i = 0; i < playerObjects.Length; i++)
            {
                if (playerObjects[i] == null) continue;
                component = playerObjects[i].GetComponent<T>();
                if (component != null) break;
            }
            
            return component;
        }
        
        /// <summary>
        /// Returns an array of a component found in the players PlayerObjects
        /// </summary>
        /// <param name="player">The player who's PlayerObjects will be searched for the components</param>
        /// <typeparam name="T">The type of the component to find</typeparam>
        /// <returns>Object[] of the provided Type T</returns>
        public static T[] GetPlayerObjectsOfType<T>(VRCPlayerApi player)
        {
            GameObject[] playerObjects = Networking.GetPlayerObjects(player);
            T[] components = new T[playerObjects.Length];
            int compsFound = 0;
            for (int i = 0; i < playerObjects.Length; i++)
            {
                if  (playerObjects[i] == null) continue;
                T component = playerObjects[i].GetComponent<T>();
                if (component == null) continue;
                components[compsFound] = component;
                compsFound++;
            }
            
            T[] results = new T[compsFound];
            Array.Copy(components, results, compsFound);
            Debug.Log($"Found {compsFound} player objects of type {nameof(T)}");
            return results;
        }
        
        /// <summary>
        /// Compresses a bool array into a byte array, with 1 bit representing each bool.
        /// Usefull for saving toggles and settings with limited persistance space.
        /// </summary>
        /// <param name="boolArray">The array to compress</param>
        /// <returns></returns>
        public static byte[] ToByteArray(bool[] boolArray)
        {
            if (boolArray == null) return null;

            // Calculate the number of bytes needed (1 byte per 8 bits, rounded up)
            int byteCount = (boolArray.Length + 7) / 8;
            byte[] byteArray = new byte[byteCount];

            // Pack each bool into a bit
            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    // Set the corresponding bit: byteArray[i / 8] |= (1 << (i % 8))
                    byteArray[i >> 3] |= (byte)(1 << (i & 7));
                }
            }

            return byteArray;
        }

        /// <summary>
        /// Retrieves a bool array from a byte array that was compressed with the ToByteArray function.
        /// Requires the original length of the bool array to be provided.
        /// </summary>
        /// <param name="byteArray">The array to decompress</param>
        /// <param name="originalLength">The expected length of the array</param>
        /// <returns></returns>
        public static bool[] FromByteArray(byte[] byteArray, int originalLength)
        {
            if (byteArray == null) return null;

            bool[] boolArray = new bool[originalLength];

            // Unpack each bit into a bool
            for (int i = 0; i < originalLength && (i >> 3) < byteArray.Length; i++)
            {
                boolArray[i] = (byteArray[i >> 3] & (1 << (i & 7))) != 0;
            }

            return boolArray;
        }

        /// <summary>
        /// Takes a DataDictionary and sorts the number values
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public static DataDictionary DataDictionarySort(DataDictionary dictionary, bool descending = false) {
            DataDictionary sortedDic = new DataDictionary();

            DataList sortedValues = dictionary.GetValues().ShallowClone(); // Without cloning, this won't work
            if (sortedValues.Count == 0) return null;

            sortedValues.Sort();

            DataList dicKeys = dictionary.GetKeys();
            DataList dicValues = dictionary.GetValues();

            if (descending) {
                for (int j = sortedValues.Count - 1; j >= 0; j--) {
                    for (int i = 0; i < dictionary.Count; i++) {
                        if (dicValues[i] == sortedValues[j] && !sortedDic.GetKeys().Contains(dicKeys[i])) {
                            sortedDic.Add(dicKeys[i], dicValues[i]);
                            break;
                        }
                    }
                }
            } else {
                for (int j = 0; j < sortedValues.Count; j++) {
                    for (int i = 0; i < dictionary.Count; i++) {
                        if (dicValues[i] == sortedValues[j] && !sortedDic.GetKeys().Contains(dicKeys[i])) {
                            sortedDic.Add(dicKeys[i], dicValues[i]);
                            break;
                        }
                    }
                }
            }

            return sortedDic;
        }
        
        // Convert Color to string: "r,g,b,a" (values 0-1)
        public static string ColorToString(Color color)
        {
            return string.Format("{0},{1},{2},{3}", 
                color.r.ToString("F4"), 
                color.g.ToString("F4"), 
                color.b.ToString("F4"), 
                color.a.ToString("F4"));
        }

        // Convert string back to Color: Expects "r,g,b,a" format
        public static Color StringToColor(string colorString)
        {
            string[] parts = colorString.Split(',');
            if (parts.Length == 4)
            {
                float r = float.Parse(parts[0]);
                float g = float.Parse(parts[1]);
                float b = float.Parse(parts[2]);
                float a = float.Parse(parts[3]);
                return new Color(r, g, b, a);
            }
            else
            {
                Debug.LogError("Invalid color string format!");
                return Color.white; // Fallback
            }
        }
    }
}