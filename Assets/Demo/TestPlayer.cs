using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotGame;
using UnityEngine.UI;

public class TestPlayer : MonoBehaviour
{
    public List<Sprite> symbols = new List<Sprite>();
    public List<int> reelData = new List<int>();
    public SpriteMask bounds;

    public Toggle stopOrdinaryToggle;
    private Machine machine;
    
    void Start()
    {
        machine=Machine.CreateAndInit(new Vector2Int(5, 3), 0.01f, bounds.bounds);
        machine.OnReelRenderUpdated = OnReelRenderUpdated;
        //Reel Cursor Randomize();
        for (int i=0;i<machine.reels.Count;i++) {
            machine.reels[i].SetCursorAll(Random.Range(0,reelData.Count));
        }

        machine.RefreshAll();
    }

    public void Spin() {
        if (machine.isSpinReady) {
            machine.SpinAll();
        }
    }
    public void Stop() {
        if(!machine.isStopReady)
            return;

        if (stopOrdinaryToggle.isOn) {
            //Stop Ordinary
            StartCoroutine(CoStopOrdinary());
        } else {
            //Stop Instant
            int[] cursors = GetCursors();
            machine.StopAll(cursors);
        }
    }
    IEnumerator CoStopOrdinary() {
        int[] cursors = GetCursors();
        for(int i = 0; i < machine.reels.Count; i++) {
            machine.reels[i].Stop(cursors[i]);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnReelRenderUpdated(Reel reel,int reelIndex,SpriteRenderer renderer,int cursor) {
        cursor = cursor < 0 ? reelData.Count + cursor : cursor;
        var symbol = symbols[reelData[cursor % reelData.Count]];

        renderer.sprite = symbol;
    }

    private int[] GetCursors() {
        int[] cursors = new int[machine.reels.Count];
        for(int i = 0; i < machine.reels.Count; i++) {
            cursors[i] = Random.Range(0, reelData.Count);
        }
        return cursors;
    }

}
