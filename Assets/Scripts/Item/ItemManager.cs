﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Circk{
	
	public class ItemManager : MonoBehaviour {

		public enum ItemType{
			BALL,
			LION,
			GUN
		}

		[Header("GameManager")]
		private GameManager gameManager; 

		[Header("Itens")]
		public GameObject itemBall;
		public GameObject itemLion;
		public GameObject itemGun;

		[Header("Sprite Itens")]
		public Sprite itemBallSprite;
		public Sprite itemLionSprite;
		public Sprite itemGunSprite;

		[Header("UsingItens")]
		public GameObject[] itemUseBallSprite;
		public GameObject itemUseLionSprite;
		public GameObject itemUseGunSprite;

		[Header("Item Values")]
		public int maxUsesItemBall;
		public int maxUsesItemLion;
		public int maxUsesItemGun;

		[Header("Values")]
		public float timeBetweenSpawn;
		public int maxItens;

		[Header("Spawn Time")]
		private float spawnCounter;

		private void Awake(){
			gameManager = GetComponent<GameManager>();
			spawnCounter = timeBetweenSpawn*.5f;
		}

		private void Update(){
			//Only works if in the game
			if (gameManager.CurrentGameState == GameManager.GameState.GAME) {
				spawnCounter -= Time.deltaTime;
				if (spawnCounter <= 0 && (ItemObject.items == null || ItemObject.items.Count < maxItens)) {
					spawnCounter = timeBetweenSpawn;
					/*
					switch ((int)(Random.value*3)) {
						case 0: SpawnItem(itemBall); break;
						case 1: SpawnItem(itemLion); break;
						default: SpawnItem(itemGun); break;
					}
					//*/
					SpawnItem((Random.value >= .5f) ? itemBall : itemLion);
				}
			}
		}

		private void SpawnItem(GameObject itemGameObject){
			Vector2 pos;
			if (!RandomPosition(out pos)) return;
			Instantiate (itemGameObject, new Vector3(pos.x,pos.y,.1f), Quaternion.identity);
		}

		public Sprite GetItemSprite(ItemType itemType){
			if(itemType == ItemType.BALL){
				return itemBallSprite;
			}
			if(itemType == ItemType.GUN){
				return itemGunSprite;
			}
			if(itemType == ItemType.LION){
				return itemLionSprite;
			}
			return null;
		}
		public int GetMaxUsesItem(ItemType itemType){
			if(itemType == ItemType.BALL){
				return maxUsesItemBall;
			}
			if(itemType == ItemType.GUN){
				return maxUsesItemGun;
			}
			if(itemType == ItemType.LION){
				return maxUsesItemLion;
			}
			return 0;
		}
	
		public void UseItem(ItemType itemType, Transform origin){
			switch (itemType) {
				case ItemType.BALL: UseItemBall(origin); break;
				case ItemType.GUN: UseItemGun(origin); break;
				case ItemType.LION: UseItemLion(origin); break;
			}
			GameManager.Instance.ResetEnergyBar();
		}
		private void UseItemBall(Transform origin){
			int n = PlayerController.me.currentUseOfItem;

			//Instantiate the ball on the player origin
			GameObject ball = (GameObject)Instantiate(itemUseBallSprite[n], origin.transform.position, Quaternion.identity);

			//Set the speed and direction
			var comp = ball.GetComponent<Ball>();
			comp.orientation.rotation = origin.rotation;
			comp.SetSpeed((gameManager.energyBarCurrentPoints/100f)*7+2);
		}
		private void UseItemLion(Transform origin){
			//Instantiate the Lion on the player origin
			GameObject lion = (GameObject)Instantiate(itemUseLionSprite, origin.transform.position, Quaternion.identity);

			//Set the speed and direction
			var comp = lion.GetComponent<Ball>();
			comp.orientation.rotation = origin.rotation;
			comp.SetSpeed((gameManager.energyBarCurrentPoints/100f)*7+2);
		}
		private void UseItemGun(Transform origin){
			//Instantiate the Gun on the player origin
			//GameObject gun = (GameObject)Instantiate(itemUseGunSprite, origin.transform.position, Quaternion.identity);
		}

		HashSet<int> set = null;
		List<int> list = null;
		const int stageWidth = 23;
		const int stageHeight = 8;
		const float stageProp = (float)stageWidth/stageHeight;
		const float stagePropInv = (float)stageHeight/stageWidth;
		const int stageRadius = stageWidth/2;
		const int stageSqrRadius = stageRadius*stageRadius;
		private bool RandomPosition(out Vector2 pos) {
			if (set == null) {
				set = new HashSet<int>();
				list = new List<int>();
			}
			for (int x = 0; x < stageWidth; x++) {
				for (int y = 0; y < stageWidth; y++) {
					int xx = x-stageRadius;
					int yy = y-stageRadius;
					if (xx*xx+yy*yy < stageSqrRadius) {
						set.Add(x+y*stageWidth);
						list.Add(x+y*stageWidth);
					}
				}
			}
			if (PlayerController.me != null) {
				RandomPositionCheckTransform(PlayerController.me.transform.position,true);
			}
			if (EnemyBase.enemies != null) {
				for (int a = 0; a < EnemyBase.enemies.Count; a++) {
					RandomPositionCheckTransform(EnemyBase.enemies[a].transform.position,false);
				}
			}
			if (ItemObject.items != null) {
				for (int a = 0; a < ItemObject.items.Count; a++) {
					RandomPositionCheckTransform(ItemObject.items[a].transform.position,false);
				}
			}
			if (set.Count > 0) {
				foreach (var i in set) {
					list.Add(i);
				}
				set.Clear();
			}
			if (list.Count == 0) {
				pos = Vector2.zero;
				return false;
			}
			int p = list[(int)(Random.value*list.Count)%list.Count];
			pos = new Vector2(p%stageWidth-stageRadius+.5f,(p/stageWidth-stageRadius)*stagePropInv+.5f);
			return true;
		}
		private void RandomPositionCheckTransform(Vector2 pos,bool isPlayer) {
			int x = (int)(pos.x+stageRadius);
			if (x < 0 || x >= stageWidth) return;
			int y = (int)((pos.y-.7f)*stageProp+stageRadius);
			if (y < 0 || y >= stageWidth) return;
			set.Remove(x+y*stageWidth);
			if (isPlayer) {
				list.Remove(x+y*stageWidth);
				for (int ox = x-2; ox <= x+2; ox++) {
					for (int oy = y-2; oy <= y+2; oy++) {
						RandomPositionRemoveFromList(ox,oy);
					}
				}
			}
		}
		private void RandomPositionRemoveFromList(int x,int y) {
			if (x >= 0 && x < stageWidth && y >= 0 && y < stageWidth) {
				list.Remove(x+y*stageWidth);
				set.Remove(x+y*stageWidth);
			}
		}
	}
}