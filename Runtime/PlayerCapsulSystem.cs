using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace LocalPoliceDepartment
{
    public class PlayerCapsulSystem : UdonSharpBehaviour
    {
        //need enough capsuls to track every player in the world
        [SerializeField] private CapsuleCollider[] playerCapsuls;
        private VRCPlayerApi[] players = new VRCPlayerApi[0];

        public override void PostLateUpdate()
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                
                Vector3 playerPos = players[i].GetPosition();
                Quaternion playerRot = players[i].GetRotation();
                Vector3 headPos = players[i].GetBonePosition(HumanBodyBones.Head);

                if (headPos == Vector3.zero)
                {
                    playerCapsuls[i].height = 1.8f;
                    playerCapsuls[i].transform.position = playerPos + new Vector3(0f, .9f, 0f);
                    playerCapsuls[i].transform.rotation = playerRot;
                }
                else
                {
                    headPos += new Vector3(0f, .2f, 0f); //bit extra so its not resting on thier neck
                    float distanceToHead = Vector3.Distance(playerPos,headPos);
                    playerCapsuls[i].height = distanceToHead;
                    playerCapsuls[i].transform.position = Vector3.Lerp(playerPos, headPos, .5f);
                    playerCapsuls[i].transform.rotation = playerRot;
                }
            }
        }

        public VRCPlayerApi GetPlayerFromCapsul(CapsuleCollider collider)
        {
            //check if the collider is in the array
            for (int i = 0; i < playerCapsuls.Length; i++)
            {
                if (playerCapsuls[i] == collider)
                {
                    //check that the index isnt out of bounds in the player array
                    if (i < players.Length)
                    {
                        return players[i];
                    }
                }
            }
            return null;
        }

        public CapsuleCollider GetCapsuleFromPlayer(VRCPlayerApi player)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == player)
                {
                    return playerCapsuls[i];
                }
            }

            return null;
        }

        public void TogglePlayersCapsul(VRCPlayerApi player, bool Enabled)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == player)
                {
                    playerCapsuls[i].gameObject.SetActive(Enabled);
                    return;
                }
            }
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            //add player to the array
            VRCPlayerApi[] temp2 = new VRCPlayerApi[players.Length + 1];
            for (int i = 0; i < players.Length; i++)
            {
                temp2[i] = players[i];
            }
            temp2[players.Length] = player;
            players = temp2;
            Debug.Log("CapsulSystem is now tracking " + player.displayName);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            //find and remove that player from the array
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == player)
                {
                    VRCPlayerApi[] temp2 = new VRCPlayerApi[players.Length - 1];
                    for (int j = 0; j < players.Length; j++)
                    {
                        if (j < i) temp2[j] = players[j];
                        else if (j > i) temp2[j - 1] = players[j];
                    }
                    players = temp2;
                    return;
                }
            }
        }
    }
}