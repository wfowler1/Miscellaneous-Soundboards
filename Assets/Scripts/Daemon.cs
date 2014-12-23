using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PlayMode : int {
	Queue = 0,
	Override = 1,
	AlwaysInstant = 2,
	Spam = 3
}

public enum SortMode : int {
	Alphabetic = 0,
	MostPlayed = 1,
	SearchRelevance = 2
}

public enum MenuState {
	ListingSets,
	ListingPhrases
}

public class Daemon : MonoBehaviour {

	public static Daemon main;
	
	private MenuState menuState = MenuState.ListingSets;
	private PlayMode mode = PlayMode.Queue;
	[System.NonSerialized] public SortMode sortMode = SortMode.Alphabetic;
	[System.NonSerialized] public SortMode lastMode = SortMode.Alphabetic;
	private Vector2 scrollPos = new Vector2();
	private float scrollFling = 0.0f;
	public float scrollDampening = 0.98f;
	private bool updateScrolling = false;
	private bool scrolling = false;
	public List<string> soundSets = new List<string>();
	[System.NonSerialized] public SoundSet currentSet = null;
	private Queue<AudioClip> clipQueue = new Queue<AudioClip>();
	[System.NonSerialized] public string searchString = "";
	[System.NonSerialized] public string previousSearchString = "";
	private AudioSource currentSound = null;
	[System.NonSerialized] public Set<Phrase> searchResults = new Set<Phrase>();

	public void Awake() {
		if(main == null) {
			main = this;
			GameObject.DontDestroyOnLoad(this.gameObject);
		} else {
			System.GC.Collect();
			GameObject.Destroy(this.gameObject);
		}
	}

	// Use this for initialization
	void Start() {
		if(PlayerPrefs.HasKey("lastSortMode")) {
			sortMode = (SortMode)PlayerPrefs.GetInt("lastSortMode");
		}
		if(sortMode == SortMode.SearchRelevance) {
			sortMode = SortMode.Alphabetic;
		}
		if(PlayerPrefs.HasKey("lastPlayMode")) {
			mode = (PlayMode)PlayerPrefs.GetInt("lastPlayMode");
		}
	}
	
	// Update is called once per frame
	void Update () {
		switch(menuState) {
			case MenuState.ListingSets:
				if(Input.touchCount > 0) {
					if(new Rect(Screen.width * 0.05f, Screen.height * 0.1f, Screen.width * 0.9f - 24, Screen.height * 0.85f).Contains(Input.touches[0].position) && Input.touches[0].phase == TouchPhase.Moved && Input.touches[0].deltaPosition.magnitude > 3) {
						updateScrolling = true;
					}
					if(updateScrolling && Input.touches[0].phase != TouchPhase.Ended) {
						scrollPos += Input.touches[0].deltaPosition * 6;
						scrollFling = Input.touches[0].deltaPosition.y * 6;
					}
				} else {
					updateScrolling = false;
					scrollFling = scrollFling * scrollDampening;
					scrollPos = new Vector2(scrollPos.x, scrollPos.y + scrollFling);
				}
				if(Input.GetKeyDown(KeyCode.Escape)) {
#if UNITY_EDITOR
					EditorApplication.isPlaying = false;
#else
					Application.Quit();
#endif
				}
				break;
			case MenuState.ListingPhrases:
				if(Input.touchCount > 0) {
					if(new Rect(Screen.width * 0.05f, Screen.height * 0.1f, Screen.width * 0.9f - 24, Screen.height * 0.75f).Contains(Input.touches[0].position) && Input.touches[0].phase == TouchPhase.Moved && Input.touches[0].deltaPosition.magnitude > 3) {
						updateScrolling = true;
					}
					if(updateScrolling && Input.touches[0].phase != TouchPhase.Ended) {
						scrollPos += Input.touches[0].deltaPosition * 6;
						scrollFling = Input.touches[0].deltaPosition.y * 6;
					}
				} else {
					updateScrolling = false;
					scrollFling = scrollFling * scrollDampening;
					scrollPos = new Vector2(scrollPos.x, scrollPos.y + scrollFling);
				}
				if(clipQueue.Count > 0) {
					if(currentSound == null) {
						PlaySound(clipQueue.Dequeue());
					}
				}
				if(Input.GetKeyDown(KeyCode.Escape)) {
					if(searchString != "") {
						searchString = "";
					} else {
						Application.LoadLevel("default");
						scrollPos = new Vector2();
						menuState = MenuState.ListingSets;
						searchString = "";
						scrollFling = 0.0f;
					}
				}
				break;
		}
	}

	public void LateUpdate() {
		scrolling = updateScrolling;
	}

	public void OnGUI() {
		GUI.skin.FontSizeFull(16);
		switch(menuState) {
			case MenuState.ListingSets:
				OnSetSelectGUI();
				break;
			case MenuState.ListingPhrases:
				if(currentSet != null) {
					OnPhraseSelectGUI();
				} else {
					GUI.Box(ScreenF.MiddleCenter(0.9f, 0.9f), "Loading...");
				}
				break;
		}
	}

	public void OnSetSelectGUI() {
#if !UNITY_WEBPLAYER
		GUIF.Button(new Rect(Screen.width * 0.01f, Screen.height * 0.01f, Screen.width * 0.4f, Screen.height * 0.075f), "Quit", () => {
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		});
#endif
		Rect scrollArea = new Rect(Screen.width * 0.01f, Screen.height * 0.1f, Screen.width * 0.98f, Screen.height * 0.85f);
		float lineHeight = GUI.skin.button.LineSize() * 1.33f;
		Rect brush = new Rect(0.0f, 0.0f, scrollArea.width - 24, lineHeight);
		scrollPos = GUI.BeginScrollView(scrollArea, scrollPos, new Rect(0, 0, Screen.width * 0.9f - 24, lineHeight * 1.05f * soundSets.Count)); {
			brush.y = (int)(scrollPos.y / (brush.height * 1.05f)) * (brush.height * 1.05f);
			for(int i = (int)Mathf.Max(scrollPos.y / (brush.height * 1.05f), 0); scrollPos.y >= ((i - 1) * brush.height * 1.05f) - scrollArea.height && i < soundSets.Count; i++) {
				string set = soundSets[i];
				GUIF.Button(brush, set, () => {
					if(!scrolling) {
						//currentSet = (Transform.Instantiate(Resources.Load<Transform>(set) as Transform) as Transform).GetComponent<SoundSetGO>();
						Application.LoadLevel(set);
						menuState = MenuState.ListingPhrases;
						scrollPos = new Vector2();
						scrollFling = 0;
						searchString = "";
						previousSearchString = "";
					}
				});
				brush = brush.Move(0.0f, 1.05f);
			}
		} GUI.EndScrollView();
		GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.95f, Screen.width * 0.9f, lineHeight), "Made by 005");
	}

	public void OnPhraseSelectGUI() {
		GUIF.Button(new Rect(Screen.width * 0.01f, Screen.height * 0.01f, Screen.width * 0.4f, Screen.height * 0.075f), "Back", () => {
			Application.LoadLevel("default");
			menuState = MenuState.ListingSets;
			searchString = "";
			scrollPos = new Vector2();
			scrollFling = 0.0f;
		});
		string str;
		switch(mode) {
			case PlayMode.AlwaysInstant:
				str = "Always Play";
				break;
			case PlayMode.Override:
				str = "Override Playing";
				break;
			case PlayMode.Queue:
				str = "Queue";
				break;
			case PlayMode.Spam:
				str = "Spam";
				break;
			default:
				str = "Error";
				break;
		}
		GUIF.Button(new Rect(Screen.width * 0.59f, Screen.height * 0.01f, Screen.width * 0.4f, Screen.height * 0.075f), str, () => {
			NextMode();
		});
		Rect scrollArea = new Rect(Screen.width * 0.01f, Screen.height * 0.2f, Screen.width * 0.98f, Screen.height * 0.73f);
		float lineHeight = GUI.skin.button.LineSize() * 1.33f;
		Rect brush = new Rect(Screen.width * 0.01f, Screen.height * 0.1f, Screen.width * 0.93f, lineHeight);
		GUI.Label(brush.Left(0.2f), "Search");
		searchString = GUI.TextField(brush.Right(0.8f), searchString);
		GUIF.Button(new Rect(Screen.width * 0.94f, Screen.height * 0.1f, Screen.width * 0.05f, lineHeight), "X", () => {
			searchString = "";
		});
		brush = brush.Move(0.0f, 1.0f);
		brush.width = Screen.width * 0.98f;
		switch(sortMode) {
			case SortMode.Alphabetic:
				str = "Alphabetic";
				break;
			case SortMode.MostPlayed:
				str = "Most Played";
				break;
			case SortMode.SearchRelevance:
				str = "Relevance";
				break;
			default:
				str = "Error";
				break;
		}
		GUIF.Button(brush, "Sort by: " + str, () => {
			NextSortMode();
			scrollPos = new Vector2();
			scrollFling = 0.0f;
			if(System.String.IsNullOrEmpty(SoundSet.Searchify(searchString))) {
				currentSet.Sort();
			} else {
				searchResults.Sort((Phrase phrase1, Phrase phrase2) => {
					if(sortMode == SortMode.Alphabetic) {
						return phrase1.name.ToUpper().CompareTo(phrase2.name.ToUpper());
					} else if(sortMode == SortMode.MostPlayed) {
						return phrase2.timesPlayed - phrase1.timesPlayed;
					} else if(sortMode == SortMode.SearchRelevance) {
						return SoundSet.ComparePhrasesBySearchRelevance(phrase1.name.ToUpper(), phrase2.name.ToUpper(), SoundSet.Searchify(searchString));
					} else {
						return 0;
					}
				});
			}
		});
		brush = new Rect(0.0f, 0.0f, scrollArea.width - 24, lineHeight);
		System.Func<Rect, string, System.Action, bool> buttonFunc;
		if(mode == PlayMode.Spam) {
			buttonFunc = GUIF.RepeatButton;
		} else {
			buttonFunc = GUIF.Button;
		}
		if(!System.String.IsNullOrEmpty(SoundSet.Searchify(searchString))) {
			scrollPos = GUI.BeginScrollView(scrollArea, scrollPos, new Rect(0, 0, Screen.width * 0.9f - 24, lineHeight * 1.05f * searchResults.Count)); {
				brush.y = (int)(scrollPos.y / (brush.height * 1.05f)) * (brush.height * 1.05f);
				for(int i = (int)Mathf.Max(scrollPos.y / (brush.height * 1.05f), 0); scrollPos.y >= ((i - 1) * brush.height * 1.05f) - scrollArea.height && i < searchResults.Count; i++) {
					Phrase phrase = searchResults[i];
					str = phrase.name;
					if(phrase.clips.Count > 1) {
						str += " (" + phrase.clips.Count + ")";
					}
					buttonFunc(brush, str, () => {
						if(!scrolling) {
							switch(mode) {
								case PlayMode.Queue:
									clipQueue.Enqueue(phrase.clips.Choose<AudioClip>());
									break;
								case PlayMode.AlwaysInstant:
								case PlayMode.Spam:
									PlaySound(phrase.clips.Choose<AudioClip>());
									break;
								case PlayMode.Override:
									if(currentSound != null) {
										currentSound.Stop();
									}
									PlaySound(phrase.clips.Choose<AudioClip>());
									break;
							}
						}
					});
					brush = brush.Move(0.0f, 1.05f);
				}
			} GUI.EndScrollView();
		} else {
			scrollPos = GUI.BeginScrollView(scrollArea, scrollPos, new Rect(0, 0, Screen.width * 0.9f - 24, lineHeight * 1.05f * currentSet.phrases.Count)); {
				brush.y = (int)(scrollPos.y / (brush.height * 1.05f)) * (brush.height * 1.05f);
				for(int i = (int)Mathf.Max(scrollPos.y / (brush.height * 1.05f), 0); scrollPos.y >= ((i - 1) * brush.height * 1.05f) - scrollArea.height && i < currentSet.phrases.Count; i++) {
					Phrase phrase = currentSet.phrases[i];
					str = phrase.name;
					if(phrase.clips.Count > 1) {
						str += " (" + phrase.clips.Count + ")";
					}
					buttonFunc(brush, str, () => {
						switch(mode) {
							case PlayMode.Queue:
								clipQueue.Enqueue(phrase.clips.Choose<AudioClip>());
								break;
							case PlayMode.AlwaysInstant:
							case PlayMode.Spam:
								PlaySound(phrase.clips.Choose<AudioClip>());
								break;
							case PlayMode.Override:
								if(currentSound != null) {
									currentSound.Stop();
								}
								PlaySound(phrase.clips.Choose<AudioClip>());
								break;
						}
						phrase.timesPlayed += 1;
					});
					brush = brush.Move(0.0f, 1.05f);
				}
			} GUI.EndScrollView();
		}
		buttonFunc(new Rect(scrollArea.x, Screen.height * 0.95f, scrollArea.width, lineHeight), "[Random]", () => {
			List<Phrase> set;
			if(System.String.IsNullOrEmpty(SoundSet.Searchify(searchString))) {
				set = currentSet.phrases;
			} else {
				set = searchResults;
			}
			if(set.Count > 0) {
				Phrase phrase = set.Choose<Phrase>();
				switch(mode) {
					case PlayMode.Queue:
						clipQueue.Enqueue(phrase.clips.Choose<AudioClip>());
						break;
					case PlayMode.AlwaysInstant:
					case PlayMode.Spam:
						PlaySound(phrase.clips.Choose<AudioClip>());
						break;
					case PlayMode.Override:
						if(currentSound != null) {
							currentSound.Stop();
						}
						PlaySound(phrase.clips.Choose<AudioClip>());
						break;
				}
				phrase.timesPlayed += 1;
			}
		});
	}

	public void NextMode() {
		int current = (int)mode;
		current++;
		if(current >= System.Enum.GetNames(typeof(PlayMode)).Length) {
			current = 0;
		}
		mode = (PlayMode)current;
		PlayerPrefs.SetInt("lastPlayMode", current);
	}

	public void NextSortMode() {
		int current = (int)sortMode;
		current++;
		if(current >= System.Enum.GetNames(typeof(SortMode)).Length) {
			current = 0;
		}
		if(current == (int)SortMode.SearchRelevance && System.String.IsNullOrEmpty(SoundSet.Searchify(searchString))) {
			current = 0;
		}
		sortMode = (SortMode)current;
		if(current < 2) {
			lastMode = sortMode;
		}
		PlayerPrefs.SetInt("lastSortMode", current);
	}

	public void PlaySound(AudioClip sound) {
		GameObject soundPlayer = new GameObject("Audio " + sound.name, typeof(AudioSource), typeof(AutodestructSound));
		soundPlayer.audio.clip = sound;
		soundPlayer.audio.pitch = 0.95f + (Random.value * 0.1f);
		soundPlayer.audio.Play();
		currentSound = soundPlayer.GetComponent<AudioSource>();
	}

}

[System.Serializable] public class Phrase {
	public string name = "New Phrase";
	public string searchName = "";
	public List<AudioClip> clips;

	public int timesPlayed {
		get {
			if(!PlayerPrefs.HasKey("phrase_" + name + "_amt")) {
				return 0;
			} else {
				return PlayerPrefs.GetInt("phrase_" + name + "_amt");
			}
		}

		set {
			PlayerPrefs.SetInt("phrase_" + name + "_amt", value);
		}
	}
}