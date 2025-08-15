using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;

namespace SmartFoxServer.Unity.Examples
{
	/**
     * Script attached to the Controller object in the Game scene.
     */
	public class GameSceneController : BaseSceneController
	{
		//----------------------------------------------------------
		// UI elements
		//----------------------------------------------------------

		public SettingsPanel settingsPanel;

        //----------------------------------------------------------
        // Public properties
        //----------------------------------------------------------
        [SerializeField] public InputActionAsset inputActionAsset;

        public GameObject[] playerModels;
		public Material[] playerMaterials;
		public Collider terrainCollider;
		public GameObject aoiPrefab;

		//----------------------------------------------------------
		// Private properties
		//----------------------------------------------------------

		private SmartFox sfs;
		private GameObject localPlayer;
		private PlayerController localPlayerController;
		private Dictionary<User, GameObject> remotePlayers = new Dictionary<User, GameObject>();
		private GameObject aoi;

		//----------------------------------------------------------
		// Unity callback methods
		//----------------------------------------------------------

		private void Start()
		{
			// Set a reference to the SmartFox client instance
			sfs = gm.GetSfsClient();

			// Hide modal panels
			HideModals();

			// Add event listeners
			AddSmartFoxListeners();

			// Set random model and material and spawn player model
			int numModel = UnityEngine.Random.Range(0, playerModels.Length);
			int numMaterial = UnityEngine.Random.Range(0, playerMaterials.Length);
			SpawnLocalPlayer(numModel, numMaterial);

			// Instantiate and set scale and position of game object representing the Area of Interest
			aoi = GameObject.Instantiate(aoiPrefab) as GameObject;
			Vec3D aoiSize = ((MMORoom)sfs.LastJoinedRoom).DefaultAOI;
			aoi.transform.localScale = new Vector3(aoiSize.FloatX * 2, 10, aoiSize.FloatZ * 2);
			aoi.transform.position = new Vector3(localPlayer.transform.position.x, -3, localPlayer.transform.position.z);

			// Update settings panel with the selected model and material
			settingsPanel.SetModelSelection(numModel);
			settingsPanel.SetMaterialSelection(numMaterial);
		}

		override protected void Update()
		{
			base.Update();

			// If the player model was already spawned, set its position by means of User Variables (if movement is dirty only)
			if (localPlayer != null && localPlayerController != null && localPlayerController.MovementDirty)
			{
				List<UserVariable> userVariables = new List<UserVariable>();
				userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
				userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
				userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
				userVariables.Add(new SFSUserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));

				sfs.Send(new SetUserVariablesRequest(userVariables));

				/*
				 * NOTE
				 * On the server side the User Variable Update event is captured and the coordinates are
				 * passed to the MMOApi.SetUserPosition() method to update the player position in the MMORoom.
				 * This in turn will keep this client in synch with all the other players within the current player's Area of Interest (AoI).
				 * Check the server-side Extension code.
				 */

				localPlayerController.MovementDirty = false;
			}

			// Make AoI game object follow player
			if (localPlayer != null)
				aoi.transform.position = localPlayer.transform.position;
		}

		//----------------------------------------------------------
		// UI event listeners
		//----------------------------------------------------------
		#region
		/**
		 * On Logout button click, disconnect from SmartFoxServer.
		 * This causes the SmartFox listeners added by this scene to be removed (see BaseSceneController.OnDestroy method)
		 * and the Login scene to be loaded (see GlobalManager.OnConnectionLost method).
		 */
		public void OnLogoutButtonClick()
		{
			// Disconnect from SmartFoxServer
			sfs.Disconnect();
		}

		/**
		 * On Settings button click, show Settings Panel prefab instance.
		 */
		public void OnSettingsButtonClick()
		{
			settingsPanel.Show();
		}

		/**
		 * On AoI visibility changed in Settings panel, show/hide game object representing it.
		 */
		public void OnAoiVisibilityChange(bool showAoi)
		{
			aoi.SetActive(showAoi);
		}

		/**
		 * On model selected in Settings panel, spawn new player model.
		 */
		public void OnSelectedModelChange(int numModel)
		{
			SpawnLocalPlayer(numModel, sfs.MySelf.GetVariable("mat").GetIntValue());
		}

		/**
		 * On material selected in Settings panel, change player material.
		 */
		public void OnSelectedMaterialChange(int numMaterial)
		{
			localPlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];

			List<UserVariable> userVariables = new List<UserVariable>();
			userVariables.Add(new SFSUserVariable("mat", numMaterial));

			sfs.Send(new SetUserVariablesRequest(userVariables));
		}
		#endregion

		//----------------------------------------------------------
		// Helper methods
		//----------------------------------------------------------
		#region
		/**
		 * Add all SmartFoxServer-related event listeners required by the scene.
		 */
		private void AddSmartFoxListeners()
		{
			sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
			sfs.AddEventListener(SFSEvent.PROXIMITY_LIST_UPDATE, OnProximityListUpdate);
		}

		/**
		 * Remove all SmartFoxServer-related event listeners added by the scene.
		 * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
		 */
		override protected void RemoveSmartFoxListeners()
		{
			sfs.RemoveEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
			sfs.RemoveEventListener(SFSEvent.PROXIMITY_LIST_UPDATE, OnProximityListUpdate);
		}

		/**
		 * Hide all modal panels.
		 */
		override protected void HideModals()
		{
			settingsPanel.Hide();
		}

		/**
		 * Add the game object representing the current player to the scene.
		 */
		private void SpawnLocalPlayer(int numModel, int numMaterial)
		{
			Vector3 pos;
			Quaternion rot;

			// In case a model already exists, get its position and rotation before spawning a new one
			// This occurs in case the current player selects a new model in the Settings panel
			if (localPlayer != null)
			{
				pos = localPlayer.transform.position;
				rot = localPlayer.transform.rotation;

				Camera.main.transform.parent = null;

				Destroy(localPlayer);
			}
			else
			{
				pos = new Vector3(0, 0, 0);
				rot = Quaternion.identity;

				pos.y = GetTerrainHeight(pos);
			}

			// Spawn local player model
			localPlayer = GameObject.Instantiate(playerModels[numModel]) as GameObject;
			localPlayer.transform.SetPositionAndRotation(pos, rot);

			// Assign starting material
			localPlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];

			// Since this is the local player, lets add a controller and set the camera
			localPlayerController = localPlayer.AddComponent<PlayerController>();
			localPlayer.GetComponentInChildren<TMP_Text>().text = sfs.MySelf.Name;
			Camera.main.transform.parent = localPlayer.transform;

			// Set movement limits based on map limits set for the MMORoom
			Vec3D lowerMapLimits = ((MMORoom)sfs.LastJoinedRoom).LowerMapLimit;
			Vec3D higherMapLimits = ((MMORoom)sfs.LastJoinedRoom).HigherMapLimit;
			localPlayerController.SetLimits(lowerMapLimits.FloatX, lowerMapLimits.FloatZ, higherMapLimits.FloatX, higherMapLimits.FloatZ);

			// Save model, material and position in User Variables, causing other players
			// to be notified about the current player presence (see server-side Extension)
			List<UserVariable> userVariables = new List<UserVariable>();

			userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
			userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
			userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
			userVariables.Add(new SFSUserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
			userVariables.Add(new SFSUserVariable("model", numModel));
			userVariables.Add(new SFSUserVariable("mat", numMaterial));

			// Send request
			sfs.Send(new SetUserVariablesRequest(userVariables));
		}

		/**
		 * Add the game object representing another player to the scene.
		 */
		private void SpawnRemotePlayer(User user, Vector3 pos, Quaternion rot)
		{
			// Check if there already exists a model, and destroy it first
			// This occurs in case the player selects a new model in their Settings panel
			if (remotePlayers.ContainsKey(user) && remotePlayers[user] != null)
			{
				Destroy(remotePlayers[user]);
				remotePlayers.Remove(user);
			}

			// Get model and material from User Variables
			int numModel = user.GetVariable("model").GetIntValue();
			int numMaterial = user.GetVariable("mat").GetIntValue();

			// Spawn remote player model
			GameObject remotePlayer = GameObject.Instantiate(playerModels[numModel]) as GameObject;
			remotePlayer.AddComponent<SimpleRemoteInterpolation>();
			remotePlayer.GetComponent<SimpleRemoteInterpolation>().SetTransform(pos, rot, false);

			// Set material and name
			remotePlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];
			remotePlayer.GetComponentInChildren<TMP_Text>().text = user.Name;

			// Add the object to the list of remote players
			remotePlayers.Add(user, remotePlayer);
		}

		/**
		 * Evaluate terrain height at given position.
		 */
		private float GetTerrainHeight(Vector3 pos)
		{
			int maxHeight = 10;
			float currPosY = pos.y;
			pos.y = maxHeight;

			Ray ray = new Ray(pos, Vector3.down);
			if (terrainCollider.Raycast(ray, out RaycastHit hit, 2.0f * maxHeight))
				return hit.point.y;
			else
				return currPosY;
		}
		#endregion

		//----------------------------------------------------------
		// SmartFoxServer event listeners
		//----------------------------------------------------------
		#region
		/**
		 * This is where we receive events about users in proximity (inside the Area of Interest) of the current player.
		 * We get two lists, one of new users that have entered the AoI and one with users that have left the proximity area.
		 */
		public void OnProximityListUpdate(BaseEvent evt)
		{
			var addedUsers = (List<User>)evt.Params["addedUsers"];
			var removedUsers = (List<User>)evt.Params["removedUsers"];

			// Handle new users
			foreach (User user in addedUsers)
			{
				// Get vertical position
				float h = GetTerrainHeight(new Vector3(user.AOIEntryPoint.FloatX, user.AOIEntryPoint.FloatY, user.AOIEntryPoint.FloatZ));

				// Spawn model representing remote player
				SpawnRemotePlayer(user,
					new Vector3(user.AOIEntryPoint.FloatX, h, user.AOIEntryPoint.FloatZ),
					Quaternion.Euler(0, (float)user.GetVariable("rot").GetDoubleValue(), 0)
				);
			}

			// Handle removed users
			foreach (User user in removedUsers)
			{
				if (remotePlayers.ContainsKey(user))
				{
					Destroy(remotePlayers[user]);
					remotePlayers.Remove(user);
				}
			}
		}

		/**
		 * When a User Variable is updated on any client within the current player's AoI, this event is received.
		 * This is where most of the game logic for this example is contained.
		 */
		public void OnUserVariableUpdate(BaseEvent evt)
		{
			List<string> changedVars = (List<string>)evt.Params["changedVars"];
			SFSUser user = (SFSUser)evt.Params["user"];

			// Ignore all updates for the current player
			if (user == sfs.MySelf)
				return;

			// Check if the remote user changed their position or rotation
			if (changedVars.Contains("x") || changedVars.Contains("y") || changedVars.Contains("z") || changedVars.Contains("rot"))
			{
				if (remotePlayers.ContainsKey(user))
				{
					// Get vertical position
					float h = GetTerrainHeight(new Vector3((float)user.GetVariable("x").GetDoubleValue(), 1, (float)user.GetVariable("z").GetDoubleValue()));

					// Move the character to the new position using a simple interpolation
					remotePlayers[user].GetComponent<SimpleRemoteInterpolation>().SetTransform(
						new Vector3((float)user.GetVariable("x").GetDoubleValue(), h, (float)user.GetVariable("z").GetDoubleValue()),
						Quaternion.Euler(0, (float)user.GetVariable("rot").GetDoubleValue(), 0),
						true
					);
				}
			}

			// Check if the remote player selected a new model
			if (changedVars.Contains("model"))
			{
				// Spawn a new remote player model
				SpawnRemotePlayer(user, remotePlayers[user].transform.position, remotePlayers[user].transform.rotation);
			}

			// Check if the remote player selected a new material
			if (changedVars.Contains("mat"))
			{
				// Change material
				remotePlayers[user].GetComponentInChildren<Renderer>().material = playerMaterials[user.GetVariable("mat").GetIntValue()];
			}
		}
		#endregion
	}
}
