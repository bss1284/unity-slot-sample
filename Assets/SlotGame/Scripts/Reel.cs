using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace SlotGame {
    public enum RotateDirection {
        Down, Up
    }

    internal class CursorRequest {
        public CursorRequest(int _waitCount, int _settingCursor) {
            waitCount = _waitCount;
            settingCursor = _settingCursor;
        }
        public int waitCount;
        public int settingCursor;
    }

    /// <summary>
    /// 슬롯의 릴의 움직임을 조정하는 클래스
    /// </summary>
    public class Reel : MonoBehaviour {
        public const int EXTRA_COUNT = 2;

        public enum ReelState { Idle = 0, Moving = 1, Stopping = 2 }
        [SerializeField]
        private ReelState state = ReelState.Idle;

        public int displayCount { get; private set; }
        public int realRenderCount => displayCount + 4;
        public Rect rect { get; private set; }
        public Vector3 firstPosition { get; private set; }
        public float distanceY { get; private set; }

        private List<SpriteRenderer> renders = new List<SpriteRenderer>();
        [SerializeField]
        private List<int> cursors = new List<int>();
        [SerializeField]
        public float moveValue;
        public int cursor => cursors[2];

        public float speed;
        public float initSpeed { get; private set; }

        //릴 상태값
        public bool isReady => state == ReelState.Idle;
        public bool isMoving => state == ReelState.Moving || state == ReelState.Stopping;
        public RotateDirection dir => speed >= 0 ? RotateDirection.Down : RotateDirection.Up;

        /// <summary>
        /// 릴 스핀 시작 콜백함수
        /// </summary>
        public event Action OnSpinStarted;
        /// <summary>
        /// 릴 스핀 스탑 콜백함수 
        /// </summary>
        public event Action OnStoped;
        /// <summary>
        /// 릴 렌더러 업데이트 콜백함수 (인자 : 렌더러,커서값)
        /// </summary>
        public event Action<SpriteRenderer, int> OnRenderUpdated;
        /// <summary>
        /// 릴 커서 변경 콜백함수 (인자 : 커서값)
        /// </summary>
        public event Action<int> OnCursorChanged;
        /// <summary>
        /// 릴 보이기 콜백함수
        /// </summary>
        public event Action OnShowed;
        /// <summary>
        /// 릴 숨기기 콜백함수
        /// </summary>
        public event Action OnHided;

        private long spinCount;
        private int stopRequest = -1;
        private CursorRequest cursorRequest;


        private bool isInit;
        public Reel Initialize(int _displayCount, Rect _rect, float _speed) {
            if(isInit)
                return this;
            isInit = true;
            displayCount = _displayCount;
            rect = _rect;
            distanceY = rect.size.y / (float)displayCount;
            firstPosition = rect.min + new Vector2(rect.size.x * 0.5f, -distanceY * 0.5f);
            transform.position = rect.center;
            initSpeed = _speed;
            speed = _speed;

            for(int i = 0; i < displayCount + EXTRA_COUNT * 2; i++) {
                var renderGO = new GameObject("Render");
                renderGO.transform.SetParent(transform);
                var render = renderGO.AddComponent<SpriteRenderer>();
                render.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                renders.Add(render);
                cursors.Add(i - EXTRA_COUNT);
            }
            ApplyMoveValue();
            return this;
        }

        private void Update() {
            if(!isInit)
                return;
            long preSpinCount = spinCount;
            if(isMoving) {
                moveValue += speed * 0.1f;
            }
            while(moveValue >= 1f) {
                moveValue -= 1f;
                CursorUp();
            }
            while(moveValue <= -1f) {
                moveValue += 1f;
                CursorDown();
            }
            if(preSpinCount != spinCount) {//커서가 변경되었을 때
                //다음 커서값 설정하기
                if(cursorRequest != null) {
                    if(dir == RotateDirection.Down) {
                        cursors[cursors.Count - 1] = cursorRequest.settingCursor - cursorRequest.waitCount;
                    } else {
                        cursors[0] = cursorRequest.settingCursor + cursorRequest.waitCount;
                    }
                    cursorRequest.waitCount -= 1;
                    cursorRequest = null;
                }
                if(stopRequest > 0) {
                    stopRequest--;
                }
                if(stopRequest == 0) {
                    moveValue = 0f;
                    ApplyMoveValue();
                    state = ReelState.Idle;
                    OnStoped?.Invoke();
                }

                UpdateRenderAll();
                OnCursorChanged?.Invoke(cursor);
            }
        }

        private void LateUpdate() {
            if(!isInit)
                return;
            if(!isReady) {
                ApplyMoveValue();
            }
        }

        public void SetCursorAll(int val) {
            for(int i = 0; i < cursors.Count; i++) {
                cursors[i] = val - EXTRA_COUNT + i;
            }
        }
        public void UpdateRenderAll() {
            for(int i = 0; i < renders.Count; i++) {
                UpdateRender(i);
            }
        }

        public void Show() {
            for(int i = 0; i < renders.Count; i++) {
                renders[i].gameObject.SetActive(true);
            }
            OnShowed?.Invoke();
        }
        public void Hide() {
            for(int i = 0; i < renders.Count; i++) {
                renders[i].gameObject.SetActive(false);
            }
            OnHided?.Invoke();
        }
        public void Spin() {
            if(!isReady)
                return;
            UpdateRenderAll();
            state = ReelState.Moving;
            stopRequest = -1;
            OnSpinStarted?.Invoke();
        }


        public void Stop(int waitCount, int setCursor) {
            if(isReady || state == ReelState.Stopping)
                return;
            SetCursorRequest(waitCount, setCursor);
            if(dir == RotateDirection.Down) {
                StopInstant(displayCount + EXTRA_COUNT + waitCount);
            } else {
                StopInstant(EXTRA_COUNT + 1 + waitCount);
            }
        }
        public void Stop(int setCursor) {
            Stop(EXTRA_COUNT, setCursor);
        }

        public void StopInstant(int waitCount = 0) {
            if(isReady || state == ReelState.Stopping)
                return;
            state = ReelState.Stopping;
            stopRequest = waitCount;
        }
        public void SetCursorRequest(int waitCount, int setCursor) {
            cursorRequest = new CursorRequest(waitCount, setCursor);
        }

        public void ResetSpeed() {
            speed = initSpeed;
        }
        public List<SpriteRenderer> GetRenderAll() {
            return renders;
        }

        /// <summary>
        /// 스프라이트 렌더러를 반환합니다.
        /// [Order:보이는 렌더러가 0부터 시작 (최소값:-2)]
        /// </summary>
        public SpriteRenderer GetRender(int order) {
            return renders[order + EXTRA_COUNT];
        }
        /// <summary>
        /// 스프라이트 렌더러가 위치하는 커서값을 반환합니다.
        /// [Order:보이는 렌더러가 0부터 시작 (최소값:-2)]
        /// </summary>
        public int GetCursor(int order) {
            return cursors[order + EXTRA_COUNT];
        }
        /// <summary>
        /// 스프라이트 렌더러가 위치하는 기본 월드 위치를 반환합니다.
        /// [Order:보이는 렌더러가 0부터 시작 (최소값:-2)]
        /// </summary>
        public Vector3 GetInitPosition(int order) {
            return firstPosition + new Vector3(0f, (order + 1) * distanceY);
        }

        private void UpdateRender(int renderIndex) {
            OnRenderUpdated?.Invoke(renders[renderIndex], cursors[renderIndex]);
        }

        private void CursorUp() {
            for(int i = 0; i < cursors.Count - 1; i++) {
                cursors[i] = cursors[i + 1];
            }
            cursors[cursors.Count - 1] = cursors[cursors.Count - 2] + 1;
            spinCount++;
        }

        private void CursorDown() {
            for(int i = cursors.Count - 1; i > 0; i--) {
                cursors[i] = cursors[i - 1];
            }
            cursors[0] = cursors[1] - 1;
            spinCount++;
        }

        private void ApplyMoveValue() {
            float dis = distanceY * moveValue;
            for(int i = 0; i < renders.Count; i++) {
                renders[i].transform.position = GetInitPosition(i - EXTRA_COUNT) - new Vector3(0f, dis);
            }
        }
    }

}
