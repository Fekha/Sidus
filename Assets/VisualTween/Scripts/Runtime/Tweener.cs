using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tweener : MonoBehaviour {
	public List<Sequence> sequences;
	
	private int position = 0;
	private bool closed = true;


	private void Start(){
		for (int i=0; i< sequences.Count; i++) {
			Sequence sequence=sequences[i];
			if(sequence.playAutomatically){
				sequence.Play();
			}
		}
	}

	private void Update(){
		for (int i=0; i< sequences.Count; i++) {
			sequences[i].Update(gameObject);
		}
	}

	public void Play(string name){
		for (int i=0; i< sequences.Count; i++) {
			Sequence sequence=sequences[i];
			if(sequence.name == name){
				sequence.Play();
			}
		}
	}

	public void Stop(){
		for (int i=0; i< sequences.Count; i++) {
			Sequence sequence=sequences[i];
			sequence.Stop();
		}
	}

	public void Stop(string name){
		for (int i=0; i< sequences.Count; i++) {
			Sequence sequence=sequences[i];
			if(sequence.name == name){
				sequence.Stop();
			}
		}
	}

	public bool IsPlaying(string name){
		for (int i=0; i< sequences.Count; i++) {
			Sequence sequence=sequences[i];
			if(sequence.name == name){
				return !sequence.stop;
			}
		}
		return false;
	}

	public void Progress(){

		Play("Complete");
		/* Support for multiple research progressions
		Play($"Progress.{position}");
		Debug.Log($"Progress.{position}");
		Debug.Log(sequences.Count);
		if (position < sequences.Count){
			position++;
		}
		*/
	}

	public void ToggleState(){
		// need to re-write this so we can toggle any animation. Not just whatever this was opening and closing. Oh yeah hamburger. 
		// ie: there is a ("ReadyUp" and a "ReadyDown" that should be here too) Turn Order Prefab. 
		if (closed)
        {
            Play("Open");
        }
        else
        {
            Play("Close");
        }
               
        closed = !closed;
	}
}

