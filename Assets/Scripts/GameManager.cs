﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// A singleton class used to keep track of gameover logic, load subsequent levels, and store persistent data

public class GameManager : MonoBehaviour {

	[System.Serializable] public class Songs {
		public AudioClip songFile;
		public float wubFrequency;
		public float wubDelay;
	}

	public GameObject deathParticles;
	public GameObject player;
	public GameObject ball;
	public string nextLevel;
	public float loadNextLevelDelay = 1.0f;
	public float reviveDelay = 1.0f;
	public AudioClip deathSound;
	public AudioClip gameOverSound;
	public Songs[] songs;
	public static GameManager instance = null;

	private int bricks;
	private int startingLives;
	private int currentClip;
	private float originalDestroyTime;
	private float gameOverTime = 6.0f;
	private GameObject playerClone;
	new private AudioSource audio = null;
	private PersistentData data = null;
	private Text livesText;
	private int[] songIndexList;
	private int currentSongIndex;
	
	public delegate void BallBounceListener();
	public event BallBounceListener onBallBounce;

	public delegate void BrickDestroyedListener( int bricksLeft );
	public event BrickDestroyedListener onBrickDestroyed;

	// expose bricks variable, but only a getter
	public int numBricks { get { return bricks; } }

	void ShuffleSongs()
	{
		if( songs.Length <= 0) return;
		for(int i = 0; i < songs.Length; i++ )
			songIndexList[i] = i;
		//Shuffle the index list
		for(int i = songs.Length-1; i > 0; i--)
		{
			int randomIndex = Random.Range(0, i);
			int temp = songIndexList[randomIndex];
			songIndexList[randomIndex] = songIndexList[i];
			songIndexList[i] = temp;
		}
		currentSongIndex = 0;
	}

	void PlaySong()
	{
		if( audio == null )
		{
			audio = GetComponent<AudioSource>();
		}
		if( currentSongIndex >= songs.Length )
		{
			ShuffleSongs();
		}
		
		currentClip = songIndexList[currentSongIndex++];
		audio.clip = songs[currentClip].songFile;
		audio.Play();
		//Play the next song after this one ends
		Invoke ( "PlaySong", audio.clip.length );
	}

	public float GetWubFrequency()
	{
		return songs[currentClip].wubFrequency;
	}

	public float GetWubDelay()
	{
		return songs[currentClip].wubDelay;
	}

	void Awake() 
	{
		if( instance == null )
		{
			instance = this;
            if (songs.Length > 0)
            {
                songIndexList = new int[songs.Length];
                ShuffleSongs();
                PlaySong();
            }
		}
		else if( instance != this )
		{
			Destroy(gameObject);
		}
	}

	void Setup()
	{
		//Count all the bricks in the scene
		GameObject[] brickObjects = GameObject.FindGameObjectsWithTag( "Brick" );
		bricks = 0;
		foreach( GameObject brickObject in brickObjects )
		{
			Brick brickComponent = brickObject.GetComponent<Brick>();
			if( brickComponent.canBreak )
			{
				bricks++;
			}
		}

		RevivePlayer();
		//livesText is part of the Canvas, which gets deleted between scenes
		livesText = GameObject.FindGameObjectWithTag( "LivesText" ).GetComponent<Text>();
		livesText.text = "Lives: " + data.lives;
	}

	void Start()
	{
		data = gameObject.GetComponent<PersistentData>();
		originalDestroyTime = deathParticles.GetComponent<DeleteAfterElapsedTime>().destroyTime;
		startingLives = data.lives;
		Setup();
	}

	void OnLevelWasLoaded( int Level)
	{
		if( instance == this )
		{
			Setup();
			switch( Level )
			{
			case 0:
				nextLevel = "levelTwo";
				break;
			case 1:
				nextLevel = "levelOne";
				break;
			default:
				Debug.LogError( "Loaded an unknown level." );
				break;
			}
		}
	}

	void LoadNextLevel()
	{
		Application.LoadLevel( nextLevel );
	}

	void LoadLevelOne()
	{
		deathParticles.GetComponent<DeleteAfterElapsedTime>().destroyTime = originalDestroyTime;
		Application.LoadLevel( "levelOne" );
		data.lives = startingLives;
	}

	void PlayGameOverSound()
	{
		audio.PlayOneShot(gameOverSound);
	}

	void RevivePlayer()
	{
		playerClone = Instantiate( player, transform.position, Quaternion.identity ) as GameObject;
		Vector3 ballSpawnPosition = new Vector3( transform.position.x, transform.position.y + 1.74f, 0f );
		GameObject BallClone = Instantiate( ball, ballSpawnPosition, Quaternion.identity ) as GameObject;
		BallClone.transform.parent = playerClone.transform; 
	}

	public AudioSource GetAudioSource()
	{
		return audio;
	}

	public void DestroyBrick()
	{
		bricks--;

		if (onBrickDestroyed != null) {
			onBrickDestroyed( bricks );
		}

		if( bricks < 1 )
		{
			Invoke ( "LoadNextLevel", loadNextLevelDelay );
		}
	}

	public void LoseLife()
	{
		data.lives--;
		livesText.text = "Lives: " + data.lives;
		if( data.lives < 1 )
		{
			Invoke ( "PlayGameOverSound", reviveDelay );
			//Make the playe wallow in defeat with a longer death animation
			deathParticles.GetComponent<DeleteAfterElapsedTime>().destroyTime = gameOverTime+reviveDelay;
			//Add the revive delay since we want to wait until the death sound ends
			Invoke ( "LoadLevelOne", gameOverTime+reviveDelay );
		}
		else
		{
			Invoke ( "RevivePlayer", reviveDelay );
			deathParticles.GetComponent<DeleteAfterElapsedTime>().destroyTime = originalDestroyTime;
		}
		Instantiate( deathParticles, playerClone.transform.position, Quaternion.identity );
		Destroy( playerClone );
		audio.PlayOneShot( deathSound );
	}

	public void BallBounced()
	{
		if ( onBallBounce != null ) onBallBounce();
	}
}
