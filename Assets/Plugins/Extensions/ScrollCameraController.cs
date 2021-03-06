﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extensions
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class ScrollCameraController : MonoBehaviour
    {
        private Camera _camera;
        private Vector3? _focusObjectPosition;

        private bool _isScrolling;
        private Vector2 _startScrollingPoint;
        private Vector3 _startScrollingCameraPosition;
        private Vector3 _scrollAcceleration;

        private bool _isAutoScrolling;
        private float _autoscrollDuration;

        private bool _isZooming;
        private Vector2 _startZoomFingerSize;
        private float _startZoomPercent;

        private Vector2 _screenSize;
        private Vector2 _hScreenSize;

        private Rect _scrollRect;

        private float _minCamera;
        private float _maxCamera;

        private float _minFov;
        private float _maxFov;

#pragma warning disable 649
        [Header("Lockers"), SerializeField] private bool _lockZoom;
        [SerializeField] private bool _lockScrolling;
        [SerializeField] private bool _lockAutoScrolling;
        [SerializeField] private bool _lockAutoScrollingToNearestSector = false;


        [Header("Focus object"), SerializeField]
        private MeshRenderer _focusObject;

        [SerializeField] private float _leftBorder;
        [SerializeField] private float _topBorder;
        [SerializeField] private float _rightBorder;
        [SerializeField] private float _bottomBorder;
        [SerializeField] private bool _fitInScreen;


        [Header("Orthographic settings")]
        [SerializeField]
        private float _minCameraSize;

        [SerializeField] private float _maxCameraSize;

        [Header("Perspective settings")]
        [SerializeField]
        private float _minFieldOfView;

        [SerializeField] private float _maxFieldOfView;

        private Camera Camera => _camera ? _camera : _camera = GetComponent<Camera>();

        public bool IsScrolling => _isScrolling || _isAutoScrolling || _isZooming;

        private float _autoScrollTimeLenght;
        [Header("Autoscroll constansts")]
        [SerializeField] private float _autoScrollAccelerationFactor = 5f;
        [SerializeField] private float _auvtoScrollTimeFactor = 25f;
        [SerializeField] private float _autoScrollMaxLenghTime = 3f;
        [SerializeField] private float _autoScrollDeltaLevel = 1500f; // минимальная амплитуда, при которой начнется автоскроллинг. Запускается только при залоченном скроллинге и разлоченном автоскроллинге. 
        [Header("AutoScroll curve")]
        [SerializeField] private AnimationCurve _curve;
#pragma warning restore 649

        private Vector3 _autoScrollStartPosition;
        private Vector3 _autoScrollEndPosition;

        private List<Vector3> _sectorCenters = new List<Vector3>();
        private Vector3 _currentSector = Vector3.zero;
        public Vector3 CurrentCenter => _currentSector;

        /// <summary>
        /// Установить список точек, к которым будет центроватся камера
        /// </summary>
        /// <param name="centers"></param>
        public void SetSectorCenters(List<Vector3> centers)
        {
            List<Vector3> correctCenters = centers.Select(center => new Vector3(center.x, center.y, transform.position.z)).ToList();

            _sectorCenters = correctCenters;
        }

        public void AutoScrollTo(Vector3 target, bool autoScrollingToNearestSector = true)
        {
            Vector3 autoScrollEndPosition = target;
            if (autoScrollingToNearestSector)
                autoScrollEndPosition = GetNearestSectorCenterFrom(autoScrollEndPosition);

            autoScrollEndPosition.z = transform.position.z;

            if (autoScrollEndPosition != _currentSector)
            {
                _autoScrollEndPosition = autoScrollEndPosition;

                _isScrolling = false;
                _isAutoScrolling = true;

                _autoscrollDuration = 0;
                _autoScrollStartPosition = transform.position;

                _autoScrollTimeLenght = (_autoScrollEndPosition - _autoScrollStartPosition).sqrMagnitude / _auvtoScrollTimeFactor;
                _autoScrollTimeLenght = _autoScrollTimeLenght < _autoScrollMaxLenghTime ? _autoScrollTimeLenght : _autoScrollMaxLenghTime;
            }
        }

        public Vector3 GetNearestSectorCenterFrom(Vector3 point)
        {
            Vector3 result;

            if (_sectorCenters != null && _sectorCenters.Count > 0)
            {
                Func<Vector3, float> orderPredicate = sectorPoint => (sectorPoint - point).sqrMagnitude;
                result = _sectorCenters.OrderBy(orderPredicate).First();
            }
            else
            {
                result = point;
            }

            return result;
        }

        /// <summary>
        /// Блокировка скроллинга.
        /// </summary>
        public bool LockScrolling
        {
            set
            {
                _lockScrolling = value;
                if (_lockScrolling)
                {
                    _isScrolling = false;
                    _isAutoScrolling = false;
                }
            }
            get => _lockScrolling;
        }

        /// <summary>
        /// Блокировка автоскроллинга.
        /// </summary>
        public bool LockAutoScrolling
        {
            set
            {
                _lockAutoScrolling = value;
                if (_lockAutoScrolling)
                {
                    _isAutoScrolling = false;
                    _lockAutoScrolling = false;
                }
            }
            get => _lockAutoScrolling;
        }

        public bool LockAutoScrollingToNearestSector
        {
            set
            {
                _lockAutoScrollingToNearestSector = value;
            }
            get
            {
                return _lockAutoScrollingToNearestSector;
            }

        }

        /// <summary>
        /// Блокировка зума.
        /// </summary>
        public bool LockZoom
        {
            set
            {
                _lockZoom = value;
                if (_lockZoom)
                {
                    _isZooming = false;
                }
            }
            get => _lockZoom;
        }

        /// <summary>
        /// Задать новые границы скроллинга.
        /// </summary>
        /// <param name="bounds">Границы скроллинга в абсолютных координатах.</param>
        /// <param name="leftBorder">Смещение для левой границы скроллинга.</param>
        /// <param name="topBorder">Смещение для верхней границы скроллинга.</param>
        /// <param name="rightBorder">Смещение для правой границы скроллинга.</param>
        /// <param name="bottomBorder">Смещение для нижней границы скроллинга.</param>
        public void SetFocusObjectBounds(Bounds bounds, float? leftBorder = null,
                    float? topBorder = null, float? rightBorder = null, float? bottomBorder = null)
        {
            _focusObjectPosition = bounds.center;
            _scrollRect = new Rect(bounds.min, bounds.size);
            _scrollRect.xMin -= leftBorder ?? _leftBorder;
            _scrollRect.xMax += rightBorder ?? _rightBorder;
            _scrollRect.yMax += topBorder ?? _topBorder;
            _scrollRect.yMin -= bottomBorder ?? _bottomBorder;
            if (Camera.orthographic)
            {
                Camera.orthographicSize = _maxCameraSize;
                _screenSize = GetCameraRect(Camera).size;
                _hScreenSize = _screenSize * 0.5f;
            }
            else
            {
                Camera.fieldOfView = _maxFieldOfView;
                _screenSize = GetCameraRect(Camera, _focusObjectPosition.Value).size;
                _hScreenSize = _screenSize * 0.5f;
            }
            if (_scrollRect.width < _screenSize.x || _scrollRect.height < _screenSize.y)
            {
                // Calc _maxCamera
                if (_screenSize.x - _scrollRect.width > _screenSize.y - _scrollRect.height)
                {
                    // Подогнать к горизонтальному размру
                    _maxCamera = _scrollRect.width * _screenSize.y / _screenSize.x * 0.5f;
                }
                else
                {
                    // Подогнать к вертикальному размеру
                    _maxCamera = _scrollRect.height * 0.5f;
                }
                // \_maxCamera
                // Calc _maxFOV
                var v = _focusObjectPosition.Value - Camera.transform.position;
                if (_scrollRect.width / _scrollRect.height > _screenSize.x / _screenSize.y)
                {
                    _maxFov = Mathf.Atan2(_scrollRect.height * 0.5f, v.z) * 2f * Mathf.Rad2Deg;
                }
                else
                {
                    _maxFov = Mathf.Atan2(_scrollRect.width * _screenSize.y / _screenSize.x * 0.5f, v.z)
                              * 2f * Mathf.Rad2Deg;
                }
                // \_maxFOV
            }
            else
            {
                _maxCamera = _maxCameraSize;
                _maxFov = _maxFieldOfView;
            }
            if (_fitInScreen)
            {
                if (Camera.orthographic && !_maxCamera.Equals(_maxCameraSize))
                {
                    Camera.orthographicSize = _maxCamera;
                    _screenSize = GetCameraRect(Camera).size;
                    _hScreenSize = _screenSize * 0.5f;
                }
                else if (!Camera.orthographic && !_maxFov.Equals(_maxFieldOfView))
                {
                    Camera.fieldOfView = _maxFov;
                    _screenSize = GetCameraRect(Camera, _focusObjectPosition.Value).size;
                    _hScreenSize = _screenSize * 0.5f;
                }
            }
            else
            {
                _maxCamera = Mathf.Max(_maxCamera, _maxCameraSize);
                _maxFov = Mathf.Max(_maxFov, _maxFieldOfView);
            }
            // Calc _minCamera
            _minCamera = _minCameraSize > _maxCamera ? _maxCamera : _minCameraSize;
            // \_minCamera
            // Calc_minFOV
            _minFov = _minFieldOfView > _maxFov ? _maxFov : _minFieldOfView;
            // \_minFOV
            transform.position = FitIntoScrollrect(transform.position);
        }
        public static Rect GetCameraRect(Camera camera, Vector3? plane = null)
        {
            Vector3 min, max;
            if (!plane.HasValue)
            {
                var nearClipPlane = camera.nearClipPlane;
                min = new Vector3(0f, 0f, nearClipPlane);
                max = new Vector3(1f, 1f, nearClipPlane);
            }
            else
            {
                var v = plane.Value - camera.transform.position;
                min = new Vector3(0f, 0f, v.z);
                max = new Vector3(1f, 1f, v.z);
            }

            var bottomLeft = camera.ViewportToWorldPoint(min);
            var topRight = camera.ViewportToWorldPoint(max);
            return new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        }

        private void Start()
        {
            if (_focusObject && _focusObjectPosition == null)
                SetFocusObjectBounds(_focusObject.bounds);
        }

        private float _autoScrollDeltaLevelWithDpi { get => _autoScrollDeltaLevel / Screen.dpi; }

        private void OnDestroy()
        {
            _focusObjectPosition = null;
        }

        private bool _previousIsPointerOverUiObject;
        private void Update()
        {
            var touches = TouchHelper.GetTouches();

            if (_focusObjectPosition != null && touches.Length > 0)
            {
                if (!LockZoom && touches.Length > 1)
                {
                    // zoom
                    bool isZoom;
                    if (Camera.orthographic)
                        isZoom = _maxCamera > _minCamera;
                    else
                        isZoom = _minFov > 0 && _maxFov > _minFov;

                    if (isZoom)
                    {
                        var touch1 = touches[0];
                        var touch2 = touches[1];

                        switch (touch2.phase)
                        {
                            case TouchPhase.Began:
                                _isScrolling = false;
                                _isAutoScrolling = false;
                                _isZooming = true;
                                _startZoomFingerSize = touch1.position - touch2.position;
                                if (Camera.orthographic)
                                {
                                    _startZoomPercent = 1f - Mathf.Clamp01((Camera.orthographicSize - _minCamera) / (_maxCamera - _minCamera));
                                }
                                else
                                {
                                    _startZoomPercent = 1f - Mathf.Clamp01(Camera.fieldOfView - _minFov) / (_maxFov - _minFov);
                                }

                                break;
                            case TouchPhase.Ended:
                            case TouchPhase.Canceled:
                                _isZooming = false;
                                break;
                            case TouchPhase.Moved:
                                DoZoom(touch1.position, touch2.position);
                                break;
                        }
                    }
                }
                else
                {
                    // scroll
                    var touch = touches[0];
                    var currentIsPointerOverUiObject = TouchHelper.IsPointerOverUiObject();
                    if (!currentIsPointerOverUiObject)
                    {
                        //Debug.Log(touch.phase);
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                                if (!LockScrolling)
                                {
                                    _isScrolling = true;
                                    _isAutoScrolling = false;
                                    _isZooming = false;
                                }

                                _scrollAcceleration = Vector3.zero;
                                _startScrollingPoint = touch.position;
                                _startScrollingCameraPosition = transform.position;
                                _previousIsPointerOverUiObject = currentIsPointerOverUiObject;

                                break;
                            case TouchPhase.Ended:
                            case TouchPhase.Canceled:
                                if (_isAutoScrolling == false && !_previousIsPointerOverUiObject)
                                {
                                    _previousIsPointerOverUiObject = currentIsPointerOverUiObject;
                                    //Debug.Log(_scrollAcceleration.sqrMagnitude);
                                    _isScrolling = false;
                                    if (LockAutoScrolling == false && _scrollAcceleration.sqrMagnitude > _autoScrollDeltaLevelWithDpi)
                                    {
                                        //Debug.Log("true");
                                        _autoScrollEndPosition = transform.position + (_scrollAcceleration * _autoScrollAccelerationFactor);

                                        _isAutoScrolling = true;
                                        _autoscrollDuration = 0;
                                        _autoScrollStartPosition = transform.position;

                                        if (LockAutoScrollingToNearestSector == false)
                                            _autoScrollEndPosition = GetNearestSectorCenterFrom(_autoScrollEndPosition);

                                        _autoScrollTimeLenght = (_autoScrollEndPosition - _autoScrollStartPosition).sqrMagnitude / _auvtoScrollTimeFactor;
                                        _autoScrollTimeLenght = _autoScrollTimeLenght < _autoScrollMaxLenghTime ? _autoScrollTimeLenght : _autoScrollMaxLenghTime;
                                    }
                                    else
                                    {
                                        //Debug.Log("false");
                                        _isAutoScrolling = false;
                                    }
                                }
                                break;
                            case TouchPhase.Moved:
                                if (_isScrolling)
                                {
                                    _scrollAcceleration = touch.deltaPosition * (-1f) / UnityEngine.Screen.dpi;
                                    DoScroll(touch.position);
                                }
                                else
                                {
                                    _scrollAcceleration = ((touch.position - _startScrollingPoint) * (-1f) / UnityEngine.Screen.dpi);

                                    if (LockScrolling == true && _isAutoScrolling == false && _scrollAcceleration.sqrMagnitude > _autoScrollDeltaLevelWithDpi)
                                    {
                                        //Debug.Log("!");

                                        _isScrolling = false;
                                        _isAutoScrolling = true;
                                        _autoscrollDuration = 0;
                                        _autoScrollStartPosition = transform.position;

                                        _autoScrollEndPosition = transform.position + (_scrollAcceleration * _autoScrollAccelerationFactor);
                                        if (LockAutoScrollingToNearestSector == false)
                                            _autoScrollEndPosition = GetNearestSectorCenterFrom(_autoScrollEndPosition);

                                        _autoScrollTimeLenght = (_autoScrollEndPosition - _autoScrollStartPosition).sqrMagnitude / _auvtoScrollTimeFactor;
                                        _autoScrollTimeLenght = _autoScrollTimeLenght < _autoScrollMaxLenghTime ? _autoScrollTimeLenght : _autoScrollMaxLenghTime;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            if (_isAutoScrolling)
            {
                DoAutoScroll();
            }
        }


        private void DoZoom(Vector2 p1, Vector2 p2)
        {
            //TODO: позже разобратся, почему ассерт не проходит иногда
            //Assert.IsTrue(_isZooming);
            if (_isZooming)
            {
                var zoomDistance = p1 - p2;
                var delta = new Vector2(Mathf.Abs(zoomDistance.x) - Mathf.Abs(_startZoomFingerSize.x),
                    Mathf.Abs(zoomDistance.y) - Mathf.Abs(_startZoomFingerSize.y));
                var zoomPercent = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? delta.x / Screen.width
                    : delta.y / Screen.height;
                var zoom = 1f - Mathf.Clamp01(_startZoomPercent + zoomPercent);
                if (Camera.orthographic)
                {
                    Camera.orthographicSize = Mathf.Lerp(_minCamera, _maxCamera, zoom);
                }
                else
                {
                    Camera.fieldOfView = Mathf.Lerp(_minFov, _maxFov, zoom);
                }

                CalcScreenSize();
            }
        }

        private void DoScroll(Vector2 p)
        {
            Assert.IsTrue(_isScrolling);
            var offset = _startScrollingPoint - p;
            var delta = new Vector3(offset.x / Screen.width * _screenSize.x,
                offset.y / Screen.height * _screenSize.y);
            var newPosition = FitIntoScrollrect(_startScrollingCameraPosition + delta);
            transform.position = newPosition;
        }

        private void DoAutoScroll()
        {
            _autoscrollDuration += Time.deltaTime;

            Vector3 currentPosition;
            float lerpValue = _autoscrollDuration / _autoScrollTimeLenght;

            if (lerpValue > 1)
            {
                currentPosition = _autoScrollEndPosition;
                _isAutoScrolling = false;
            }
            else
            {
                float curve = _curve.Evaluate(lerpValue);
                currentPosition = Vector3.Lerp(_autoScrollStartPosition, _autoScrollEndPosition, curve);
            }

            bool boundsReached;
            transform.position = FitIntoScrollrect(currentPosition, out boundsReached);
            //if (boundsReached) // TODO: взять у Яши последнюю версию метода FitIntoScrollrect и переделать!
            //{
            //    _isAutoScrolling = false;
            //}

            if (_isAutoScrolling == false)
            {
                _currentSector = _autoScrollEndPosition;
            }
        }

        private void CalcScreenSize()
        {
            if (Camera.orthographic)
            {
                _screenSize = GetCameraRect(Camera).size;
            }
            else
            {
                Assert.IsTrue(_focusObjectPosition.HasValue);
                _screenSize = GetCameraRect(Camera, _focusObjectPosition.Value).size;
            }

            _hScreenSize = _screenSize * 0.5f;
            transform.position = FitIntoScrollrect(transform.position);
        }

        private Vector3 FitIntoScrollrect(Vector3 pos)
        {
            return FitIntoScrollrect(pos, out _);
        }

        private Vector3 FitIntoScrollrect(Vector3 pos, out bool boundsReached)
        {
            boundsReached = false;
            if (_focusObjectPosition == null) return pos;

            var x = pos.x;
            var y = pos.y;
            if (_scrollRect.width < _screenSize.x)
            {
                x = _scrollRect.center.x;
            }
            else if (x - _hScreenSize.x < _scrollRect.xMin)
            {
                x = _scrollRect.xMin + _hScreenSize.x;
                boundsReached = true;
            }
            else if (x + _hScreenSize.x > _scrollRect.xMax)
            {
                x = _scrollRect.xMax - _hScreenSize.x;
                boundsReached = true;
            }

            if (_scrollRect.height < _screenSize.y)
            {
                y = _scrollRect.center.y;
            }
            else if (y - _hScreenSize.y < _scrollRect.yMin)
            {
                y = _scrollRect.yMin + _hScreenSize.y;
                boundsReached = true;
            }
            else if (y + _hScreenSize.y > _scrollRect.yMax)
            {
                y = _scrollRect.yMax - _hScreenSize.y;
                boundsReached = true;
            }

            return new Vector3(x, y, pos.z);
        }
    }
}