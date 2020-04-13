using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace SlotGame {
    /// <summary>
    /// 릴을 자식으로 가지고 있는 머신 클래스 
    /// </summary>
    public class Machine : MonoBehaviour {
        public Vector2Int slotSize { get; private set; }
        public List<Reel> reels { get; private set; }
        public Rect rect { get; private set; }
        public Vector3 firstPosition { get; private set; }
        public Vector2 distance { get; private set; }

        public bool isSpinReady => isStopAll;
        public bool isStopReady => !isStopAll;

        /// <summary>
        /// 릴이 모두 정지해있는지 bool값 반환
        /// </summary>
        public bool isStopAll => reels.TrueForAll(reel => reel.isReady);

        /// <summary>
        /// 머신 스핀 콜백함수 
        /// </summary>
        public Action OnSpinStarted;

        /// <summary>
        /// 머신 스핀 스탑 콜백함수 
        /// </summary>
        public Action OnSpinEnded;

        /// <summary>
        /// 머신 보이기 콜백함수
        /// </summary>
        public event Action OnShowed;
        /// <summary>
        /// 머신 숨기기 콜백함수
        /// </summary>
        public event Action OnHided;

        /// <summary>
        /// 릴 스핀 콜백함수 (인자 : Reel,ReelIndex)
        /// </summary>
        public Action<Reel, int> OnReelSpined;

        /// <summary>
        /// 릴 스탑 콜백함수 (인자 : Reel,ReelIndex)
        /// </summary>
        public Action<Reel, int> OnReelStoped;

        /// <summary>
        /// 릴 렌더러 업데이트 콜백함수 (인자 :Reel,ReelIndex,렌더러,커서값)
        /// </summary>
        public Action<Reel, int, SpriteRenderer, int> OnReelRenderUpdated;



        private GameObject reelParent;


        public static Machine CreateAndInit(Vector2Int _slotSize, float reelSpeed, Bounds slotBounds) {
            var machineObj=new GameObject("SlotMachine");
            var machine = machineObj.AddComponent<Machine>();
            return machine.Initialize(_slotSize, reelSpeed, slotBounds);
        }

        public void SpinAll() {
            if(!isSpinReady)
                return;
            foreach(var reel in reels) {
                reel.Spin();
            };
            OnSpinStarted?.Invoke();
        }
        public void StopAll(int[] cursors) {
            if(!isStopReady)
                return;
            for(int i = 0; i < cursors.Length; i++) {
                reels[i].Stop(cursors[i]);
            }
        }
        public void StopAll(List<int> cursors) {
            if(!isStopReady)
                return;
            for(int i = 0; i < cursors.Count; i++) {
                reels[i].Stop(cursors[i]);
            }
        }

        public void ShowAll() {
            foreach(var reel in reels) {
                reel.Show();
            }
            OnShowed?.Invoke();
        }
        public void HideAll() {
            foreach(var reel in reels) {
                reel.Hide();
            }
            OnHided?.Invoke();
        }

        public void RefreshAll() {
            foreach(var reel in reels) {
                reel.UpdateRenderAll();
            }
        }

        public SpriteRenderer GetRender(Vector2Int coords) {
            return reels[coords.x].GetRender(coords.y);
        }
        public int GetCursor(Vector2Int coords) {
            return reels[coords.x].GetCursor(coords.y);
        }
        public Vector3 GetPosition(Vector2Int coords) {
            return firstPosition + Vector3.Scale(distance, (Vector2)coords);
        }



        private bool isInit;
        public Machine Initialize(Vector2Int _slotSize, float reelSpeed, Rect slotRect) {
            if(isInit)
                return this;
            isInit = true;
            slotSize = _slotSize;
            rect = slotRect;
            distance = CustomUtility.InverseScale(rect.size, slotSize);
            firstPosition = rect.min + Vector2.Scale(distance, new Vector2(0.5f, 0.5f));
            transform.position = rect.center;
            var _reels = new List<Reel>();
            //릴 생성하기
            reelParent = new GameObject("Reels");
            reelParent.transform.SetParent(transform, false);
            for(int i = 0; i < slotSize.x; i++) {
                int index = i;
                var reelRect = rect.Split(slotSize.x, 1, i, 0);
                var reelGO = new GameObject($"Reel{i}");
                reelGO.transform.SetParent(reelParent.transform);
                var reel = reelGO.AddComponent<Reel>();
                reel.Initialize(slotSize.y, reelRect, reelSpeed);
                reel.OnRenderUpdated += (render, cursor) => {
                    OnReelRenderUpdated?.Invoke(reel, index, render, cursor);
                };
                reel.OnSpinStarted += () => {
                    OnReelSpined?.Invoke(reel, index);
                };
                reel.OnStoped += () => {
                    OnReelStoped?.Invoke(reel, index);
                    if(isStopAll) {
                        OnSpinEnded?.Invoke();
                    }
                };
                _reels.Add(reel);
            }
            reels = _reels;
            return this;
        }

        public Machine Initialize(Vector2Int _slotSize, float reelSpeed, Bounds slotBounds) {
            return Initialize(_slotSize, reelSpeed, slotBounds.ToRect());
        }
    }


}
