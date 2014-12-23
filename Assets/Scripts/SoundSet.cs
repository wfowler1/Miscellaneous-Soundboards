using UnityEngine;
using System.Collections.Generic;

public class SoundSet : MonoBehaviour {

	public string name = "";
	public List<Phrase> phrases = new List<Phrase>();

	void Awake() {
		Sort();
		foreach(Phrase phrase in phrases) {
			if(System.String.IsNullOrEmpty(phrase.searchName)) {
				phrase.searchName = Searchify(phrase.name);
			}
#if UNITY_EDITOR
			foreach(AudioClip clip in phrase.clips) {
				if(clip == null) {
					Debug.LogWarning("Null clip in phrase " + phrase.name);
				}
			}
#endif
		}
		Daemon.main.currentSet = this;

	}

	void Update() {
		Daemon.main.currentSet = this;
		string searchifiedSearchString = Searchify(Daemon.main.searchString);
		if(searchifiedSearchString != Daemon.main.previousSearchString) {
			if(System.String.IsNullOrEmpty(searchifiedSearchString)) {
				Daemon.main.searchResults = new Set<Phrase>();
				Daemon.main.sortMode = Daemon.main.lastMode;
				Sort();
				Daemon.main.previousSearchString = "";
			} else {
				//string searchifiedSearchString = Searchify(searchString);
				List<Phrase> searchSet;
				if(System.String.IsNullOrEmpty(Daemon.main.previousSearchString)) {
					Daemon.main.sortMode = SortMode.SearchRelevance;
				}
				if(searchifiedSearchString.Contains(Daemon.main.previousSearchString) && !System.String.IsNullOrEmpty(Daemon.main.previousSearchString)) {
					searchSet = Daemon.main.searchResults;
				} else {
					searchSet = phrases;
				}
				Daemon.main.searchResults = new Set<Phrase>();
				foreach(Phrase phrase in searchSet) {
					if(phrase.searchName.Length >= searchifiedSearchString.Length && phrase.searchName.Substring(0, searchifiedSearchString.Length).Equals(searchifiedSearchString, System.StringComparison.InvariantCultureIgnoreCase)) {
						Daemon.main.searchResults.Add(phrase);
					}
				}
				foreach(Phrase phrase in searchSet) {
					if(phrase.searchName.Contains(searchifiedSearchString)) {
						Daemon.main.searchResults.Add(phrase);
					}
				}
				Daemon.main.previousSearchString = searchifiedSearchString;
			}
		}
	}

	public void Sort() {
		if(System.String.IsNullOrEmpty(Searchify(Daemon.main.searchString))) {
			phrases.Sort((Phrase phrase1, Phrase phrase2) => {
				if(Daemon.main.sortMode == SortMode.Alphabetic) {
					return phrase1.searchName.CompareTo(phrase2.searchName);
				} else if(Daemon.main.sortMode == SortMode.MostPlayed) {
					return phrase2.timesPlayed - phrase1.timesPlayed;
				} else {
					return 0;
				}
			});
		} else {
			Daemon.main.searchResults.Sort((Phrase phrase1, Phrase phrase2) => {
				if(Daemon.main.sortMode == SortMode.Alphabetic) {
					return phrase1.searchName.CompareTo(phrase2.searchName);
				} else if(Daemon.main.sortMode == SortMode.MostPlayed) {
					return phrase2.timesPlayed - phrase1.timesPlayed;
				} else if(Daemon.main.sortMode == SortMode.SearchRelevance) {
					return ComparePhrasesBySearchRelevance(phrase1.searchName, phrase2.searchName, Searchify(Daemon.main.searchString));
				} else {
					return 0;
				}
			});
		}
	}

	public static string Searchify(string input) {
		if(System.String.IsNullOrEmpty(input)) { return ""; } else {
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			foreach(char c in input.ToUpper()) {
				if((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || c == '?') {
					output.Append(c);
				}
			}
			return output.ToString();
		}
	}

	public static int ComparePhrasesBySearchRelevance(string x, string y, string searchString) {
		if(x == searchString) { return -1; } else if(y == searchString) { return 1; } else if(x.Substring(0, searchString.Length) == searchString && y.Substring(0, searchString.Length) == searchString) { return x.CompareTo(y); } else if(x.Substring(0, searchString.Length) == searchString && y.Substring(0, searchString.Length) != searchString) { return -1; } else if(x.Substring(0, searchString.Length) != searchString && y.Substring(0, searchString.Length) == searchString) { return 1; } else { return x.CompareTo(y); }
	}

	public void OnDestroy() {
		foreach(Phrase phrase in phrases) {
			for(int i = 0; i < phrase.clips.Count; i++) {
				phrase.clips[i] = null;
			}
			/*foreach(AudioClip clip in phrase.clips) {
				clip = null;
			}*/
		}
	}

}
