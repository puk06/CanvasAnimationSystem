using net.puk06.CanvasAnimation.Models;
using net.puk06.CanvasAnimation.Utils;
using net.puk06.Utils;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace net.puk06.CanvasAnimation
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CanvasAnimationSystem : UdonSharpBehaviour
    {
        [Header("同時実行できるアニメーション数（多すぎると重くなります）")]
        [Tooltip("一度に再生できるアニメーションの最大数を設定します。\nおすすめは「これまでの同時実行アニメーション最大数」か「その+1」です。")]
        [SerializeField] private int maxConcurrentAnimations = 256;

        #region 確認用
        [Header("現在実行中のアニメーション数（確認用）")]
        [Tooltip("現在進行中のアニメーション数を表示します。終了したアニメーションはカウントされません。")]
        [SerializeField] private int runningAnimations = 0;

        [Header("これまでの同時実行アニメーション最大数（確認用）")]
        [Tooltip("今まで同時に再生されたアニメーション数の最大値を記録します。\nMaxConcurrentAnimationsを決める際の目安にしてください。")]
        [SerializeField] private int peakConcurrentAnimations = 0;
        #endregion

        private readonly string LogPrefix = $"[{UdonUtils.ColorizeString("Canvas Animation System", "#4eb3ee")}]";

        private const int OBJECT_INDEX = 0;
        private const int DURATION_INDEX = 1;
        private const int START_TIME_INDEX = 2;
        private const int PIXEL_OFFSET_INDEX = 3;
        private const int TIME_OUT_INDEX = 4;

        private const int START_POSITION_INDEX = 5;
        private const int START_ROTATION_INDEX = 6;
        private const int START_SCALE_INDEX = 7;
        private const int START_COLOR_INDEX = 8;

        private const int TRANSITION_TYPE_INDEX = 9;
        private const int ANIMATION_MODE_INDEX = 10;
        private const int ELEMENT_TYPE_INDEX = 11;

        private const int TARGET_POSITION_INDEX = 12;
        private const int TARGET_ROTATION_INDEX = 13;
        private const int TARGET_SCALE_INDEX = 14;
        private const int TARGET_COLOR_INDEX = 15;

        private const int USE_DEFINED_POSITION_INDEX = 16;
        private const int USE_DEFINED_ROTATION_INDEX = 17;
        private const int USE_DEFINED_SCALE_INDEX = 18;
        private const int USE_DEFINED_COLOR_INDEX = 19;

        private const int DEFINITION_OBJECT_INDEX = 0;
        private const int DEFINITION_POSITION_INDEX = 1;
        private const int DEFINITION_ROTATION_INDEX = 2;
        private const int DEFINITION_SCALE_INDEX = 3;
        private const int DEFINITION_COLOR_INDEX = 4;

        private string[] m_currentTasks;

        private Component[] m_targetObjects;
        private float[] m_durations;
        private int[] m_pixelOffsets;
        private float[] m_startTimes;
        private float[] m_timeoutTimes;

        private Vector3[] m_startPositions;
        private Vector3[] m_startRotations;
        private Vector3[] m_startScales;
        private Color[] m_startColors;

        private TransitionType[] m_transitionTypes;
        private ElementType[] m_elementTypes;
        private AnimationMode[] m_animationModes;

        private Vector3[] m_targetPositions;
        private Vector3[] m_targetScales;
        private Vector3[] m_targetRotations;
        private Color[] m_targetColors;

        private string[] m_definedStats;
        private Component[] m_definedObjects;
        private Vector3[] m_definedPosition;
        private Vector3[] m_definedRotations;
        private Vector3[] m_definedScales;
        private Color[] m_definedColors;

        private bool _initialized = false;
        public bool Initialized => _initialized;

        void Start()
            => SetupArrays();

        void Update()
            => ProcessAllAnimations();

        #region Initialize
        private void SetupArrays()
        {
            m_currentTasks = new string[maxConcurrentAnimations];

            m_targetObjects = new Component[maxConcurrentAnimations];
            m_durations = new float[maxConcurrentAnimations];
            m_pixelOffsets = new int[maxConcurrentAnimations];
            m_startTimes = new float[maxConcurrentAnimations];
            m_timeoutTimes = new float[maxConcurrentAnimations];

            m_startPositions = new Vector3[maxConcurrentAnimations];
            m_startRotations = new Vector3[maxConcurrentAnimations];
            m_startScales = new Vector3[maxConcurrentAnimations];
            m_startColors = new Color[maxConcurrentAnimations];

            m_transitionTypes = new TransitionType[maxConcurrentAnimations];
            m_elementTypes = new ElementType[maxConcurrentAnimations];
            m_animationModes = new AnimationMode[maxConcurrentAnimations];

            m_targetPositions = new Vector3[maxConcurrentAnimations];
            m_targetScales = new Vector3[maxConcurrentAnimations];
            m_targetRotations = new Vector3[maxConcurrentAnimations];
            m_targetColors = new Color[maxConcurrentAnimations];

            m_definedStats = new string[maxConcurrentAnimations];
            m_definedObjects = new Component[maxConcurrentAnimations];
            m_definedPosition = new Vector3[maxConcurrentAnimations];
            m_definedScales = new Vector3[maxConcurrentAnimations];
            m_definedRotations = new Vector3[maxConcurrentAnimations];
            m_definedColors = new Color[maxConcurrentAnimations];

            ArrayUtils.InitializeValues(m_durations);
            ArrayUtils.InitializeValues(m_pixelOffsets);
            ArrayUtils.InitializeValues(m_startTimes);
            ArrayUtils.InitializeValues(m_timeoutTimes);

            ArrayUtils.InitializeValues(m_startPositions);
            ArrayUtils.InitializeValues(m_startRotations);
            ArrayUtils.InitializeValues(m_startScales);
            ArrayUtils.InitializeValues(m_startColors);

            ArrayUtils.InitializeValues(m_targetPositions);
            ArrayUtils.InitializeValues(m_targetScales);
            ArrayUtils.InitializeValues(m_targetRotations);
            ArrayUtils.InitializeValues(m_targetColors);

            ArrayUtils.InitializeValues(m_definedPosition);
            ArrayUtils.InitializeValues(m_definedScales);
            ArrayUtils.InitializeValues(m_definedRotations);
            ArrayUtils.InitializeValues(m_definedColors);

            _initialized = true;
        }
        #endregion

        #region Show
        /// <summary>
        /// Displays the provided UI element using an alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Show(Text element)
            => FadeInternal(element, ElementType.Text, 0f, 0f, FadeType.In, TransitionType.None);

        /// <summary>
        /// Displays the provided UI element using an alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Show(Button element)
            => FadeInternal(element, ElementType.Button, 0f, 0f, FadeType.In, TransitionType.None);

        /// <summary>
        /// Displays the provided UI element using an alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Show(Image element)
            => FadeInternal(element, ElementType.Image, 0f, 0f, FadeType.In, TransitionType.None);

        /// <summary>
        /// Displays the provided UI element using an alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Show(RawImage element)
            => FadeInternal(element, ElementType.RawImage, 0f, 0f, FadeType.In, TransitionType.None);

        /// <summary>
        /// Displays the provided UI element using an alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Show(TMP_Text element)
            => FadeInternal(element, ElementType.TMP_Text, 0f, 0f, FadeType.In, TransitionType.None);
        #endregion

        #region Hide
        /// <summary>
        /// Hide the provided UI element using its alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Hide(Text element)
            => FadeInternal(element, ElementType.Text, 0f, 0f, FadeType.Out, TransitionType.None);

        /// <summary>
        /// Hide the provided UI element using its alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Hide(Button element)
            => FadeInternal(element, ElementType.Button, 0f, 0f, FadeType.Out, TransitionType.None);

        /// <summary>
        /// Hide the provided UI element using its alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Hide(Image element)
            => FadeInternal(element, ElementType.Image, 0f, 0f, FadeType.Out, TransitionType.None);

        /// <summary>
        /// Hide the provided UI element using its alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Hide(RawImage element)
            => FadeInternal(element, ElementType.RawImage, 0f, 0f, FadeType.Out, TransitionType.None);

        /// <summary>
        /// Hide the provided UI element using its alpha value.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Hide(TMP_Text element)
            => FadeInternal(element, ElementType.TMP_Text, 0f, 0f, FadeType.Out, TransitionType.None);
        #endregion

        #region Fade
        /// <summary>
        /// Gradually displays or hides the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="fadeType"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Fade(Text element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Text, duration, after, fadeType, transitionType);

        /// <summary>
        /// Gradually displays or hides the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="fadeType"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Fade(Button element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Button, duration, after, fadeType, transitionType);

        /// <summary>
        /// Gradually displays or hides the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="fadeType"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Fade(Image element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Image, duration, after, fadeType, transitionType);

        /// <summary>
        /// Gradually displays or hides the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="fadeType"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Fade(RawImage element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.RawImage, duration, after, fadeType, transitionType);

        /// <summary>
        /// Gradually displays or hides the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="fadeType"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Fade(TMP_Text element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.TMP_Text, duration, after, fadeType, transitionType);
        private CanvasAnimationSystem FadeInternal(Component element, ElementType elementType, float duration, float after, FadeType fadeType, TransitionType transitionType)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (fadeType)
            {
                case FadeType.In: animationMode = AnimationMode.FadeIn; break;
                case FadeType.Out: animationMode = AnimationMode.FadeOut; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, true, true, true);
            return this;
        }
        #endregion

        #region Move
        /// <summary>
        /// The UI element is moved from its current position, shifted by the number of pixels provided, back to its original location.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="pixelOffset"></param>
        /// <param name="moveDirection"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Move(Component element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType, bool useDefinedPosition = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (moveDirection)
            {
                case MoveDirection.Up: animationMode = AnimationMode.MoveUp; break;
                case MoveDirection.Down: animationMode = AnimationMode.MoveDown; break;
                case MoveDirection.Right: animationMode = AnimationMode.MoveRight; break;
                case MoveDirection.Left: animationMode = AnimationMode.MoveLeft; break;
            }

            AddTask(element, duration, after, pixelOffset, transitionType, animationMode, ElementType.None, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), useDefinedPosition, true, true, true);
            return this;
        }

        /// <summary>
        /// The UI elements are moved from its current position, shifted by the number of pixels provided, back to its original location.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="pixelOffset"></param>
        /// <param name="moveDirection"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Move(Component[] elements, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType, bool useDefinedPosition = true)
        {
            foreach (Component element in elements)
            {
                Move(element, duration, after, pixelOffset, moveDirection, transitionType, useDefinedPosition);
            }

            return this;
        }
        #endregion

        #region MoveTo
        /// <summary>
        /// The UI element is moved from its current position by the number of pixels provided.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="pixelOffset"></param>
        /// <param name="moveDirection"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MoveTo(Component element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType, bool useDefinedPosition = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (moveDirection)
            {
                case MoveDirection.Up: animationMode = AnimationMode.MoveToUp; break;
                case MoveDirection.Down: animationMode = AnimationMode.MoveToDown; break;
                case MoveDirection.Right: animationMode = AnimationMode.MoveToRight; break;
                case MoveDirection.Left: animationMode = AnimationMode.MoveToLeft; break;
            }

            AddTask(element, duration, after, pixelOffset, transitionType, animationMode, ElementType.None, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), useDefinedPosition, true, true, true);
            return this;
        }

        /// <summary>
        /// The UI elements are moved from its current position by the number of pixels provided.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="pixelOffset"></param>
        /// <param name="moveDirection"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MoveTo(Component[] elements, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType, bool useDefinedPosition = true)
        {
            foreach (Component element in elements)
            {
                MoveTo(element, duration, after, pixelOffset, moveDirection, transitionType, useDefinedPosition);
            }

            return this;
        }
        #endregion

        #region MovePosition
        /// <summary>
        /// Move the UI element to the specified coordinates, or move from that point to the current position.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetPosition"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MovePosition(Component element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPosition, TransitionType transitionType, bool useDefinedPosition = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.MoveTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.MoveFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, ElementType.None, targetPosition, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), useDefinedPosition, true, true, true);
            return this;
        }

        /// <summary>
        /// Move the UI elements to the specified coordinates, or move from that point to the current position.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetPosition"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedPosition"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MovePosition(Component[] elements, float duration, float after, AnimationDirection animationDirection, Vector3 targetPosition, TransitionType transitionType, bool useDefinedPosition = true)
        {
            foreach (Component element in elements)
            {
                MovePosition(element, duration, after, animationDirection, targetPosition, transitionType, useDefinedPosition);
            }

            return this;
        }
        #endregion

        #region MoveFromTo
        /// <summary>
        /// Moves the provided UI Element from the provided coordinates to another set of provided coordinates.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MoveFromTo(Component element, float duration, float after, Vector3 startPosition, Vector3 targetPosition, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.MoveTo, ElementType.None, targetPosition, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), startPosition, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, true, true, true);
            return this;
        }

        /// <summary>
        /// Moves the provided UI Elements from the provided coordinates to another set of provided coordinates.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem MoveFromTo(Component[] elements, float duration, float after, Vector3 startPosition, Vector3 targetPosition, TransitionType transitionType)
        {
            foreach (Component element in elements)
            {
                MoveFromTo(element, duration, after, startPosition, targetPosition, transitionType);
            }

            return this;
        }
        #endregion

        #region Rotate
        /// <summary>
        /// Rotates the provided UI Element to the specified orientation, or rotates it from the specified orientation to its current orientation.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetRotation"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedRotation"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Rotate(Component element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType, bool useDefinedRotation = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.RotateTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.RotateFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, ElementType.None, Vector3.positiveInfinity, targetRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, useDefinedRotation, true, true);
            return this;
        }

        /// <summary>
        /// Rotates the provided UI Elements to the specified orientation, or rotates it from the specified orientation to its current orientation.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetRotation"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedRotation"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Rotate(Component[] elements, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType, bool useDefinedRotation = true)
        {
            foreach (Component element in elements)
            {
                Rotate(element, duration, after, animationDirection, targetRotation, transitionType, useDefinedRotation);
            }

            return this;
        }
        #endregion

        #region RotateFromTo
        /// <summary>
        /// Rotates the provided UI element from the provided orientation to another provided orientation.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startRotation"></param>
        /// <param name="targetRotation"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem RotateFromTo(Component element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.RotateTo, ElementType.None, Vector3.positiveInfinity, targetRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, startRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, true, true, true);
            return this;
        }

        /// <summary>
        /// Rotates the provided UI elements from the provided orientation to another provided orientation.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startRotation"></param>
        /// <param name="targetRotation"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem RotateFromTo(Component[] elements, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
        {
            foreach (Component element in elements)
            {
                RotateFromTo(element, duration, after, startRotation, targetRotation, transitionType);
            }

            return this;
        }
        #endregion

        #region Flip
        /// <summary>
        /// Rotates the provided UI element 180 degrees around the specified axis.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="rotationAxis"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedRotation"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Flip(Component element, float duration, float after, RotationAxis rotationAxis, TransitionType transitionType, bool useDefinedRotation = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (rotationAxis)
            {
                case RotationAxis.X: animationMode = AnimationMode.FlipX; break;
                case RotationAxis.Y: animationMode = AnimationMode.FlipY; break;
                case RotationAxis.Z: animationMode = AnimationMode.FlipZ; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, ElementType.None, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, useDefinedRotation, true, true);
            return this;
        }

        /// <summary>
        /// Rotates the provided UI elements 180 degrees around the specified axis.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="rotationAxis"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedRotation"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Flip(Component[] elements, float duration, float after, RotationAxis rotationAxis, TransitionType transitionType, bool useDefinedRotation = true)
        {
            foreach (Component element in elements)
            {
                Flip(element, duration, after, rotationAxis, transitionType, useDefinedRotation);
            }

            return this;
        }
        #endregion

        #region Scale
        /// <summary>
        /// Scales the provided UI element to the specified scale, or scales it from the specified scale to the current scale.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetScale"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedScale"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Scale(Component element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType, bool useDefinedScale = true)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.ScaleTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.ScaleFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, ElementType.None, Vector3.positiveInfinity, Vector3.positiveInfinity, targetScale, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, true, useDefinedScale, true);
            return this;
        }

        /// <summary>
        /// Scales the provided UI elements to the specified scale, or scales it from the specified scale to the current scale.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetScale"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedScale"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Scale(Component[] elements, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType, bool useDefinedScale = true)
        {
            foreach (Component element in elements)
            {
                Scale(element, duration, after, animationDirection, targetScale, transitionType, useDefinedScale);
            }

            return this;
        }
        #endregion

        #region ScaleFromTo
        /// <summary>
        /// Changes the UI Element from the provided scale to another provided scale.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startScale"></param>
        /// <param name="targetScale"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ScaleFromTo(Component element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.ScaleTo, ElementType.None, Vector3.positiveInfinity, Vector3.positiveInfinity, targetScale, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, startScale, ColorUtils.GetInvalidColor(), true, true, true, true);
            return this;
        }

        /// <summary>
        /// Changes the UI Elements from the provided scale to another provided scale.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startScale"></param>
        /// <param name="targetScale"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ScaleFromTo(Component[] elements, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
        {
            foreach (Component element in elements)
            {
                ScaleFromTo(element, duration, after, startScale, targetScale, transitionType);
            }

            return this;
        }
        #endregion

        #region Color
        /// <summary>
        /// Changes the UI element to the specified color, or changes it from the specified color to the current color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedColor"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Color(Text element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor = true)
            => ColorInternal(element, ElementType.Text, duration, after, animationDirection, targetColor, transitionType, useDefinedColor);

        /// <summary>
        /// Changes the UI element to the specified color, or changes it from the specified color to the current color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedColor"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Color(Button element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor = true)
            => ColorInternal(element, ElementType.Button, duration, after, animationDirection, targetColor, transitionType, useDefinedColor);

        /// <summary>
        /// Changes the UI element to the specified color, or changes it from the specified color to the current color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedColor"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Color(Image element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor = true)
            => ColorInternal(element, ElementType.Image, duration, after, animationDirection, targetColor, transitionType, useDefinedColor);

        /// <summary>
        /// Changes the UI element to the specified color, or changes it from the specified color to the current color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedColor"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Color(RawImage element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor = true)
            => ColorInternal(element, ElementType.RawImage, duration, after, animationDirection, targetColor, transitionType, useDefinedColor);

        /// <summary>
        /// Changes the UI element to the specified color, or changes it from the specified color to the current color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="animationDirection"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <param name="useDefinedColor"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Color(TMP_Text element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor = true)
            => ColorInternal(element, ElementType.TMP_Text, duration, after, animationDirection, targetColor, transitionType, useDefinedColor);
        private CanvasAnimationSystem ColorInternal(Component element, ElementType elementType, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType, bool useDefinedColor)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.ColorTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.ColorFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, targetColor, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), true, true, true, useDefinedColor);
            return this;
        }
        #endregion

        #region ColorFromTo
        /// <summary>
        /// Changes the color of the provided UI element from the first provided color to the second provided color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startColor"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ColorFromTo(Text element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Text, duration, after, startColor, targetColor, transitionType);

        /// <summary>
        /// Changes the color of the provided UI element from the first provided color to the second provided color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startColor"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ColorFromTo(Button element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Button, duration, after, startColor, targetColor, transitionType);

        /// <summary>
        /// Changes the color of the provided UI element from the first provided color to the second provided color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startColor"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ColorFromTo(Image element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Image, duration, after, startColor, targetColor, transitionType);

        /// <summary>
        /// Changes the color of the provided UI element from the first provided color to the second provided color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startColor"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ColorFromTo(RawImage element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.RawImage, duration, after, startColor, targetColor, transitionType);

        /// <summary>
        /// Changes the color of the provided UI element from the first provided color to the second provided color.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="duration"></param>
        /// <param name="after"></param>
        /// <param name="startColor"></param>
        /// <param name="targetColor"></param>
        /// <param name="transitionType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ColorFromTo(TMP_Text element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.TMP_Text, duration, after, startColor, targetColor, transitionType);
        private CanvasAnimationSystem ColorFromToInternal(Component element, ElementType elementType, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.ColorTo, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, targetColor, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, startColor, true, true, true, true);
            return this;
        }
        #endregion

        #region Save & Define & Reset Transform
        /// <summary>
        /// The specified type of Transform for the UI Element is saved in advance. If it exists, it is used as the initial Transform for this UI Element. It will not be removed unless RemoveDefine is called. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transformTypes"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveTransform(Component element, TransformType[] transformTypes)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            RectTransform currentObjectRectTransform = RectTransformUtils.GetRectTransform(element);
            if (currentObjectRectTransform == null)
            {
                Debug.LogError($"{LogPrefix} Couldn't Get RectTransform - Object: {element.name}");
                return this;
            }

            Vector3 currentPosition = currentObjectRectTransform.localPosition;
            Vector3 currentRotation = currentObjectRectTransform.localEulerAngles;
            Vector3 currentScale = currentObjectRectTransform.localScale;

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Position) != -1 && int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) != -1)
                {
                    m_definedPosition[int.Parse(parsedStrData[DEFINITION_POSITION_INDEX])] = Vector3.positiveInfinity;
                }

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Rotation) != -1 && int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) != -1)
                {
                    m_definedRotations[int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX])] = Vector3.positiveInfinity;
                }

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Scale) != -1 && int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) != -1)
                {
                    m_definedScales[int.Parse(parsedStrData[DEFINITION_SCALE_INDEX])] = Vector3.positiveInfinity;
                }

                int positionIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Position) == -1 ? int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) : ArrayUtils.AssignArrayValue(m_definedPosition, currentPosition);
                int rotationIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Rotation) == -1 ? int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) : ArrayUtils.AssignArrayValue(m_definedRotations, currentRotation);
                int scaleIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Scale) == -1 ? int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) : ArrayUtils.AssignArrayValue(m_definedScales, currentScale);
                int colorIndex = int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                m_definedStats[definedStatsIndex] = stats;
                Debug.Log($"{LogPrefix} Object Definition Updated - Object: {element.name} - Definition: {definedStatsIndex}");
            }
            else
            {
                objectIndex = ArrayUtils.AssignArrayValue(m_definedObjects, element);

                int positionIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Position) == -1 ? -1 : ArrayUtils.AssignArrayValue(m_definedPosition, currentPosition);
                int rotationIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Rotation) == -1 ? -1 : ArrayUtils.AssignArrayValue(m_definedRotations, currentRotation);
                int scaleIndex = UdonUtils.ArrayContains(transformTypes, TransformType.Scale) == -1 ? -1 : ArrayUtils.AssignArrayValue(m_definedScales, currentScale);
                int colorIndex = -1;

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                int statsIndex = ArrayUtils.AssignArrayValue(m_definedStats, stats);

                if (statsIndex == -1) Debug.LogError($"{LogPrefix} Failed to Define Object");
                else Debug.Log($"{LogPrefix} Object Definition Created - Object: {element.name} - Definition: {statsIndex}");
            }

            return this;
        }

        /// <summary>
        /// The specified type of Transform for the UI Elements is saved in advance. If they exist, they are used as the initial Transforms for these UI Elements. They will not be removed unless RemoveDefine is called. Overwriting is possible.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="transformTypes"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveTransform(Component[] elements, TransformType[] transformTypes)
        {
            foreach (Component element in elements)
            {
                SaveTransform(element, transformTypes);
            }

            return this;
        }

        /// <summary>
        /// Predefine the specified type of Transform for the UI element passed to you using the given value. If it exists, it will be used as the initial Transform for this UI element. It will not be removed unless you use RemoveDefine. Overwriting is possible.
        /// <param name="element"></param>
        /// <param name="transform"></param>
        /// <param name="transformType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem DefineTransform(Component element, Vector3 transform, TransformType transformType)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            RectTransform currentObjectRectTransform = RectTransformUtils.GetRectTransform(element);
            if (currentObjectRectTransform == null)
            {
                Debug.LogError($"{LogPrefix} Couldn't Get RectTransform from Object: {element.name}");
                return this;
            }

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (transformType == TransformType.Position && int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) != -1)
                {
                    m_definedPosition[int.Parse(parsedStrData[DEFINITION_POSITION_INDEX])] = Vector3.positiveInfinity;
                }

                if (transformType == TransformType.Rotation && int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) != -1)
                {
                    m_definedRotations[int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX])] = Vector3.positiveInfinity;
                }

                if (transformType == TransformType.Scale && int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) != -1)
                {
                    m_definedScales[int.Parse(parsedStrData[DEFINITION_SCALE_INDEX])] = Vector3.positiveInfinity;
                }

                int positionIndex = transformType != TransformType.Position ? int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) : ArrayUtils.AssignArrayValue(m_definedPosition, transform);
                int rotationIndex = transformType != TransformType.Rotation ? int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) : ArrayUtils.AssignArrayValue(m_definedRotations, transform);
                int scaleIndex = transformType != TransformType.Scale ? int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) : ArrayUtils.AssignArrayValue(m_definedScales, transform);
                int colorIndex = int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                m_definedStats[definedStatsIndex] = stats;
                Debug.Log($"{LogPrefix} Object Definition Updated - Object: {element.name} - Definition: {definedStatsIndex}");
            }
            else
            {
                objectIndex = ArrayUtils.AssignArrayValue(m_definedObjects, element);

                int positionIndex = transformType != TransformType.Position ? -1 : ArrayUtils.AssignArrayValue(m_definedPosition, transform);
                int rotationIndex = transformType != TransformType.Rotation ? -1 : ArrayUtils.AssignArrayValue(m_definedRotations, transform);
                int scaleIndex = transformType != TransformType.Scale ? -1 : ArrayUtils.AssignArrayValue(m_definedScales, transform);
                int colorIndex = -1;

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                int statsIndex = ArrayUtils.AssignArrayValue(m_definedStats, stats);

                if (statsIndex == -1) Debug.LogError($"{LogPrefix} Failed to Define Object");
                else Debug.Log($"{LogPrefix} Object Define Created - Object: {element.name} - Definition: {statsIndex}");
            }

            return this;
        }

        /// <summary>
        /// Predefine the specified type of Transform for the UI elements passed to you using the given value. If it exists, it will be used as the initial Transforms for these UI elements. It will not be removed unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="transform"></param>
        /// <param name="transformType"></param>
        /// <returns></returns>
        public CanvasAnimationSystem DefineTransform(Component[] elements, Vector3 transform, TransformType transformType)
        {
            foreach (Component element in elements)
            {
                DefineTransform(element, transform, transformType);
            }

            return this;
        }

        /// <summary>
        /// Restores the Transform of the provided UI Element based on the definition information. Only values present in the definition are applied.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transformTypes"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetTransform(Component element, TransformType[] transformTypes)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                RectTransform currentObjectRectTransform = RectTransformUtils.GetRectTransform(element);
                if (currentObjectRectTransform == null)
                {
                    Debug.LogError($"{LogPrefix} Couldn't Get RectTransform - Object: {element.name}");
                    return this;
                }

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Position) != -1 && int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) != -1)
                {
                    currentObjectRectTransform.localPosition = m_definedPosition[int.Parse(parsedStrData[DEFINITION_POSITION_INDEX])];
                }

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Position) != -1 && int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) != -1)
                {
                    currentObjectRectTransform.localEulerAngles = m_definedRotations[int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX])];
                }

                if (UdonUtils.ArrayContains(transformTypes, TransformType.Position) != -1 && int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) != -1)
                {
                    currentObjectRectTransform.localScale = m_definedScales[int.Parse(parsedStrData[DEFINITION_SCALE_INDEX])];
                }
            }
            else
            {
                Debug.LogError($"{LogPrefix} Object Definition not found - Object: {element.name}");
            }

            return this;
        }

        /// <summary>
        /// Restores the Transform of the provided UI Elements based on the definition information. Only values present in the definition are applied.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="transformTypes"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetTransform(Component[] elements, TransformType[] transformTypes)
        {
            foreach (Component element in elements)
            {
                ResetTransform(element, transformTypes);
            }

            return this;
        }
        #endregion

        #region Save & Define & Reset Color
        /// <summary>
        /// The color of the UI element passed to you is saved beforehand. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveColor(Text element)
            => SaveColorInternal(element, ElementType.Text);

        /// <summary>
        /// The color of the UI element passed to you is saved beforehand. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveColor(Button element)
            => SaveColorInternal(element, ElementType.Button);

        /// <summary>
        /// The color of the UI element passed to you is saved beforehand. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveColor(Image element)
            => SaveColorInternal(element, ElementType.Image);

        /// <summary>
        /// The color of the UI element passed to you is saved beforehand. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveColor(RawImage element)
            => SaveColorInternal(element, ElementType.RawImage);

        /// <summary>
        /// The color of the UI element passed to you is saved beforehand. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem SaveColor(TMP_Text element)
            => SaveColorInternal(element, ElementType.TMP_Text);
        public CanvasAnimationSystem SaveColorInternal(Component element, ElementType elementType)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            Color currentObjectColor = ColorUtils.GetColor(element, elementType);

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]) != -1)
                {
                    m_definedColors[int.Parse(parsedStrData[DEFINITION_COLOR_INDEX])] = ColorUtils.GetInvalidColor();
                }

                int positionIndex = int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]);
                int rotationIndex = int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]);
                int scaleIndex = int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]);
                int colorIndex = ArrayUtils.AssignArrayValue(m_definedColors, currentObjectColor);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                m_definedStats[definedStatsIndex] = stats;
                Debug.Log($"{LogPrefix} Object Definition Updated - Object: {element.name} - Definition: {definedStatsIndex}");
            }
            else
            {
                objectIndex = ArrayUtils.AssignArrayValue(m_definedObjects, element);

                int positionIndex = -1;
                int rotationIndex = -1;
                int scaleIndex = -1;
                int colorIndex = ArrayUtils.AssignArrayValue(m_definedColors, currentObjectColor);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                int statsIndex = ArrayUtils.AssignArrayValue(m_definedStats, stats);

                if (statsIndex == -1) Debug.LogError($"{LogPrefix} Failed to Define Object");
                else Debug.Log($"{LogPrefix} Object Definition Created - Object: {element.name} - Definition: {statsIndex}");
            }

            return this;
        }

        /// <summary>
        /// Predefine the color for the UI element passed to you using the specified color. If it exists, it will be used as the initial color for this UI element. It will not disappear unless you use RemoveDefine. Overwriting is possible.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public CanvasAnimationSystem DefineColor(Component element, Color color)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]) != -1)
                {
                    m_definedColors[int.Parse(parsedStrData[DEFINITION_COLOR_INDEX])] = ColorUtils.GetInvalidColor();
                }

                int positionIndex = int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]);
                int rotationIndex = int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]);
                int scaleIndex = int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]);
                int colorIndex = ArrayUtils.AssignArrayValue(m_definedColors, color);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                m_definedStats[definedStatsIndex] = stats;
                Debug.Log($"{LogPrefix} Object Definition Updated - Object: {element.name} - Definition: {definedStatsIndex}");
            }
            else
            {
                objectIndex = ArrayUtils.AssignArrayValue(m_definedObjects, element);

                int positionIndex = -1;
                int rotationIndex = -1;
                int scaleIndex = -1;
                int colorIndex = ArrayUtils.AssignArrayValue(m_definedColors, color);

                string stats = $"{objectIndex},,,{positionIndex},,,{rotationIndex},,,{scaleIndex},,,{colorIndex}";
                int statsIndex = ArrayUtils.AssignArrayValue(m_definedStats, stats);

                if (statsIndex == -1) Debug.LogError($"{LogPrefix} Failed to Define Object");
                else Debug.Log($"{LogPrefix} Object Definition Created - Object: {element.name} - Definition: {statsIndex}");
            }

            return this;
        }

        /// <summary>
        /// Restores the color of the provided UI element based on the definition information. This applies only if the definition exists.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetColor(Text element)
            => ResetColorInternal(element, ElementType.Text);

        /// <summary>
        /// Restores the color of the provided UI element based on the definition information. This applies only if the definition exists.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetColor(Button element)
            => ResetColorInternal(element, ElementType.Button);

        /// <summary>
        /// Restores the color of the provided UI element based on the definition information. This applies only if the definition exists.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetColor(Image element)
            => ResetColorInternal(element, ElementType.Image);

        /// <summary>
        /// Restores the color of the provided UI element based on the definition information. This applies only if the definition exists.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetColor(RawImage element)
            => ResetColorInternal(element, ElementType.RawImage);

        /// <summary>
        /// Restores the color of the provided UI element based on the definition information. This applies only if the definition exists.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem ResetColor(TMP_Text element)
            => ResetColorInternal(element, ElementType.TMP_Text);
        public CanvasAnimationSystem ResetColorInternal(Component element, ElementType elementType)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]) != -1)
                {
                    ColorUtils.SetColor(element, elementType, m_definedColors[int.Parse(parsedStrData[DEFINITION_COLOR_INDEX])]);
                }
            }
            else
            {
                Debug.LogError($"{LogPrefix} Object Definition not found - Object: {element.name}");
            }

            return this;
        }
        #endregion

        #region Define Database Check
        /// <summary>
        /// Checks whether the specified UI Element is registered in the Define Database.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsRegisteredInDefineDatabase(Component element)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return false;
            }

            return UdonUtils.ArrayContains(m_definedObjects, element) != -1;
        }
        #endregion

        #region Remove Define
        /// <summary>
        /// Delete all definition information for the UI element that was passed.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem RemoveDefine(Component element)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            int objectIndex = UdonUtils.ArrayContains(m_definedObjects, element);

            if (objectIndex != -1)
            {
                int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, objectIndex.ToString());
                string[] parsedStrData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);

                if (int.Parse(parsedStrData[DEFINITION_OBJECT_INDEX]) != -1)
                {
                    m_definedObjects[int.Parse(parsedStrData[DEFINITION_OBJECT_INDEX])] = null;
                }

                if (int.Parse(parsedStrData[DEFINITION_POSITION_INDEX]) != -1)
                {
                    m_definedPosition[int.Parse(parsedStrData[DEFINITION_POSITION_INDEX])] = Vector3.positiveInfinity;
                }

                if (int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX]) != -1)
                {
                    m_definedRotations[int.Parse(parsedStrData[DEFINITION_ROTATION_INDEX])] = Vector3.positiveInfinity;
                }

                if (int.Parse(parsedStrData[DEFINITION_SCALE_INDEX]) != -1)
                {
                    m_definedScales[int.Parse(parsedStrData[DEFINITION_SCALE_INDEX])] = Vector3.positiveInfinity;
                }

                if (int.Parse(parsedStrData[DEFINITION_COLOR_INDEX]) != -1)
                {
                    m_definedColors[int.Parse(parsedStrData[DEFINITION_COLOR_INDEX])] = ColorUtils.GetInvalidColor();
                }

                m_definedStats[definedStatsIndex] = null;
                Debug.Log($"{LogPrefix} Object Definition Removed - Object: {element.name} - Definition: {definedStatsIndex}");
            }
            else
            {
                Debug.LogError($"{LogPrefix} Object Definition not found - Object: {element.name}");
            }

            return this;
        }

        /// <summary>
        /// Delete all definition information for the UI elements that was passed.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public CanvasAnimationSystem RemoveDefine(Component[] elements)
        {
            foreach (Component element in elements)
            {
                RemoveDefine(element);
            }

            return this;
        }
        #endregion

        #region Cancel
        /// <summary>
        /// Cancels the currently running animation for the provided UI element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Cancel(Component element)
        {
            if (element == null)
            {
                Debug.LogError($"{LogPrefix} Element is null. Processing was skipped.");
                return this;
            }

            for (int i = 0; i < m_currentTasks.Length; i++)
            {
                if (m_currentTasks[i] == null || m_currentTasks[i] == "") continue;

                string[] parsedStrData = UdonUtils.ParseDataString(m_currentTasks[i]);

                int objectIndex = int.Parse(parsedStrData[OBJECT_INDEX]);
                if (objectIndex == -1) continue;

                Component targetObj = m_targetObjects[objectIndex];
                if (targetObj == null || targetObj != element) continue;

                Debug.Log($"{LogPrefix} Animation Canceled - Object: {targetObj.name} - Task: {i}");
                RemoveTask(parsedStrData, i);
            }

            return this;
        }

        /// <summary>
        /// Cancels the currently running animation for the provided UI elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public CanvasAnimationSystem Cancel(Component[] elements)
        {
            foreach (Component element in elements)
            {
                Cancel(element);
            }

            return this;
        }
        #endregion

        #region CancelAll
        /// <summary>
        /// Cancel all animations. We do not recommend using this, as it will stop animations even if other tools are using them.
        /// </summary>
        /// <returns></returns>
        public CanvasAnimationSystem CancelAll()
        {
            for (int i = 0; i < m_currentTasks.Length; i++)
            {
                if (m_currentTasks[i] == null || m_currentTasks[i] == "") continue;

                string[] parsedStrData = UdonUtils.ParseDataString(m_currentTasks[i]);

                Debug.Log($"{LogPrefix} Animation Canceled - Task: {i}");
                RemoveTask(parsedStrData, i);
            }

            return this;
        }
        #endregion

        #region Exit
        private bool isExitCancelScheduled = false;

        /// <summary>
        /// Disables the component after a specified delay.
        /// </summary>
        /// <param name="after">
        /// The number of seconds to wait before disabling the component.
        /// Pass 0 or use <see cref="ExitImmediate"/> to disable it right away.
        /// </param>
        /// <remarks>
        /// You can cancel the next scheduled exit exactly once by calling <see cref="CancelExit"/> before the delay ends.
        /// </remarks>
        public void Exit(float after = 0f)
        {
            if (after <= 0f)
            {
                Debug.Log($"{LogPrefix} Exit triggered immediately.");
                ExitImmediate();
                return;
            }

            Debug.Log($"{LogPrefix} Exit scheduled — the component will be disabled in {after} second{(after == 1f ? "" : "s")}.");
            SendCustomEventDelayedSeconds(nameof(ExitImmediate), after);
        }

        /// <summary>
        /// Cancels the next scheduled exit process once.
        /// </summary>
        /// <remarks>
        /// Sets a one-time flag that prevents the next call to <see cref="ExitImmediate"/> from disabling the component.
        /// The flag is automatically cleared after one use.
        /// </remarks>
        public void CancelExit()
        {
            isExitCancelScheduled = true;
            Debug.Log($"{LogPrefix} Exit cancellation scheduled — the next exit attempt will be ignored.");
        }

        /// <summary>
        /// Immediately disables the component, unless the cancel flag is active.
        /// </summary>
        /// <remarks>
        /// If <see cref="CancelExit"/> was called beforehand, this exit will be canceled and the flag cleared automatically.
        /// </remarks>
        public void ExitImmediate()
        {
            if (isExitCancelScheduled)
            {
                isExitCancelScheduled = false;
                Debug.Log($"{LogPrefix} Exit process canceled successfully — the component remains active.");
                return;
            }

            Debug.Log($"{LogPrefix} Executing Exit — the component is now being disabled.");
            if (runningAnimations > 0)
                Debug.LogWarning($"{LogPrefix} Warning: {runningAnimations} animation(s) are still playing, but the component is being disabled.");

            enabled = false;
        }
        #endregion

        #region Task Management
        private void AddTask(
            Component element,
            float time, float after,
            int pixelOffset,
            TransitionType transitionType,
            AnimationMode mode,
            ElementType elementType,
            Vector3 targetPosition, Vector3 targetRotation, Vector3 targetScale, Color targetColor,
            Vector3 startPosition, Vector3 startRotation, Vector3 startScale, Color startColor,
            bool useDefinedPosition, bool useDefinedRotation, bool useDefinedScale, bool useDefinedColor
        )
        {
            if (!_initialized)
            {
                Debug.LogError($"{LogPrefix} Task creation was canceled because you attempted to create a task before initialization.");
                return;
            }

            int objectIndex = ArrayUtils.AssignArrayValue(m_targetObjects, element);
            int durationIndex = ArrayUtils.AssignArrayValue(m_durations, time);
            int startTimeIndex = ArrayUtils.AssignArrayValue(m_startTimes, Time.time);
            int pixelOffsetIndex = ArrayUtils.AssignArrayValue(m_pixelOffsets, pixelOffset);
            int timeoutTimesIndex = ArrayUtils.AssignArrayValue(m_timeoutTimes, after);

            int startPositionIndex = MathUtils.IsPositiveInfinity(startPosition) ? -1 : ArrayUtils.AssignArrayValue(m_startPositions, startPosition);
            int startRotationIndex = MathUtils.IsPositiveInfinity(startRotation) ? -1 : ArrayUtils.AssignArrayValue(m_startRotations, startRotation);
            int startScaleIndex = MathUtils.IsPositiveInfinity(startScale) ? -1 : ArrayUtils.AssignArrayValue(m_startScales, startScale);
            int startColorIndex = ColorUtils.IsInvalidColor(startColor) ? -1 : ArrayUtils.AssignArrayValue(m_startColors, startColor);

            int transitionTypeIndex = ArrayUtils.AssignArrayValue(m_transitionTypes, transitionType);
            int elementTypeIndex = ArrayUtils.AssignArrayValue(m_elementTypes, elementType);
            int modesIndex = ArrayUtils.AssignArrayValue(m_animationModes, mode);

            int targetPositionIndex = ArrayUtils.AssignArrayValue(m_targetPositions, targetPosition);
            int targetRotationIndex = ArrayUtils.AssignArrayValue(m_targetRotations, targetRotation);
            int targetScaleIndex = ArrayUtils.AssignArrayValue(m_targetScales, targetScale);
            int targetColorIndex = ArrayUtils.AssignArrayValue(m_targetColors, targetColor);

            string stats = $"{objectIndex},,,{durationIndex},,,{startTimeIndex},,,{pixelOffsetIndex},,,{timeoutTimesIndex},,,{startPositionIndex},,,{startRotationIndex},,,{startScaleIndex},,,{startColorIndex},,,{transitionTypeIndex},,,{modesIndex},,,{elementTypeIndex},,,{targetPositionIndex},,,{targetRotationIndex},,,{targetScaleIndex},,,{targetColorIndex},,,{TypeUtils.BoolToInt(useDefinedPosition)},,,{TypeUtils.BoolToInt(useDefinedRotation)},,,{TypeUtils.BoolToInt(useDefinedScale)},,,{TypeUtils.BoolToInt(useDefinedColor)}";
            int statsIndex = ArrayUtils.AssignArrayValue(m_currentTasks, stats);

            if (statsIndex == -1) Debug.LogError($"{LogPrefix} Failed to create Animation Task. You have likely exceeded the maximum number of simultaneous animations.");
            else Debug.Log($"{LogPrefix} Animation Task Created - Object: {element.name} - Task: {statsIndex}");
        }

        private void RemoveTask(string[] parsedTaskDataString, int taskIndex)
        {
            int objectIndex = int.Parse(parsedTaskDataString[OBJECT_INDEX]);
            if (objectIndex == -1) return;

            m_targetObjects[objectIndex] = null;

            if (int.Parse(parsedTaskDataString[DURATION_INDEX]) != -1) m_durations[int.Parse(parsedTaskDataString[DURATION_INDEX])] = float.PositiveInfinity;
            if (int.Parse(parsedTaskDataString[PIXEL_OFFSET_INDEX]) != -1) m_pixelOffsets[int.Parse(parsedTaskDataString[PIXEL_OFFSET_INDEX])] = -1;
            if (int.Parse(parsedTaskDataString[START_TIME_INDEX]) != -1) m_startTimes[int.Parse(parsedTaskDataString[START_TIME_INDEX])] = float.PositiveInfinity;
            if (int.Parse(parsedTaskDataString[TIME_OUT_INDEX]) != -1) m_timeoutTimes[int.Parse(parsedTaskDataString[TIME_OUT_INDEX])] = float.PositiveInfinity;

            if (int.Parse(parsedTaskDataString[START_POSITION_INDEX]) != -1) m_startPositions[int.Parse(parsedTaskDataString[START_POSITION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_ROTATION_INDEX]) != -1) m_startRotations[int.Parse(parsedTaskDataString[START_ROTATION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_SCALE_INDEX]) != -1) m_startScales[int.Parse(parsedTaskDataString[START_SCALE_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_COLOR_INDEX]) != -1) m_startColors[int.Parse(parsedTaskDataString[START_COLOR_INDEX])] = ColorUtils.GetInvalidColor();

            if (int.Parse(parsedTaskDataString[TRANSITION_TYPE_INDEX]) != -1) m_transitionTypes[int.Parse(parsedTaskDataString[TRANSITION_TYPE_INDEX])] = TransitionType.None;
            if (int.Parse(parsedTaskDataString[ANIMATION_MODE_INDEX]) != -1) m_animationModes[int.Parse(parsedTaskDataString[ANIMATION_MODE_INDEX])] = AnimationMode.None;
            if (int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX]) != -1) m_elementTypes[int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX])] = ElementType.None;

            if (int.Parse(parsedTaskDataString[TARGET_POSITION_INDEX]) != -1) m_targetPositions[int.Parse(parsedTaskDataString[TARGET_POSITION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_ROTATION_INDEX]) != -1) m_targetRotations[int.Parse(parsedTaskDataString[TARGET_ROTATION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_SCALE_INDEX]) != -1) m_targetScales[int.Parse(parsedTaskDataString[TARGET_SCALE_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_COLOR_INDEX]) != -1) m_targetColors[int.Parse(parsedTaskDataString[TARGET_COLOR_INDEX])] = ColorUtils.GetInvalidColor();

            m_currentTasks[taskIndex] = null;
        }
        #endregion


        #region Process All Animations
        private void ProcessAllAnimations()
        {
            int currentWorkingTasks = 0;
            for (int i = 0; i < m_currentTasks.Length; i++)
            {
                if (m_currentTasks[i] == null || m_currentTasks[i] == "") continue;
                currentWorkingTasks++;

                string[] parsedStrData = UdonUtils.ParseDataString(m_currentTasks[i]);

                int objectIndex = int.Parse(parsedStrData[OBJECT_INDEX]);
                float duration = int.Parse(parsedStrData[DURATION_INDEX]) == -1 ? 0f : m_durations[int.Parse(parsedStrData[DURATION_INDEX])];
                float startTime = int.Parse(parsedStrData[START_TIME_INDEX]) == -1 ? 0f : m_startTimes[int.Parse(parsedStrData[START_TIME_INDEX])];
                int pixelOffset = int.Parse(parsedStrData[PIXEL_OFFSET_INDEX]) == -1 ? 0 : m_pixelOffsets[int.Parse(parsedStrData[PIXEL_OFFSET_INDEX])];
                float timeoutTime = int.Parse(parsedStrData[TIME_OUT_INDEX]) == -1 ? 0f : m_timeoutTimes[int.Parse(parsedStrData[TIME_OUT_INDEX])];

                Vector3 startPosition = int.Parse(parsedStrData[START_POSITION_INDEX]) == -1 ? Vector3.positiveInfinity : m_startPositions[int.Parse(parsedStrData[START_POSITION_INDEX])];
                Vector3 startRotation = int.Parse(parsedStrData[START_ROTATION_INDEX]) == -1 ? Vector3.positiveInfinity : m_startRotations[int.Parse(parsedStrData[START_ROTATION_INDEX])];
                Vector3 startScale = int.Parse(parsedStrData[START_SCALE_INDEX]) == -1 ? Vector3.positiveInfinity : m_startScales[int.Parse(parsedStrData[START_SCALE_INDEX])];
                Color startColor = int.Parse(parsedStrData[START_COLOR_INDEX]) == -1 ? ColorUtils.GetInvalidColor() : m_startColors[int.Parse(parsedStrData[START_COLOR_INDEX])];

                TransitionType transitionType = int.Parse(parsedStrData[TRANSITION_TYPE_INDEX]) == -1 ? TransitionType.None : m_transitionTypes[int.Parse(parsedStrData[TRANSITION_TYPE_INDEX])];
                AnimationMode animationMode = int.Parse(parsedStrData[ANIMATION_MODE_INDEX]) == -1 ? AnimationMode.None : m_animationModes[int.Parse(parsedStrData[ANIMATION_MODE_INDEX])];
                ElementType elementType = int.Parse(parsedStrData[ELEMENT_TYPE_INDEX]) == -1 ? ElementType.None : m_elementTypes[int.Parse(parsedStrData[ELEMENT_TYPE_INDEX])];

                Vector3 targetPosition = int.Parse(parsedStrData[TARGET_POSITION_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetPositions[int.Parse(parsedStrData[TARGET_POSITION_INDEX])];
                Vector3 targetRotation = int.Parse(parsedStrData[TARGET_ROTATION_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetRotations[int.Parse(parsedStrData[TARGET_ROTATION_INDEX])];
                Vector3 targetScale = int.Parse(parsedStrData[TARGET_SCALE_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetScales[int.Parse(parsedStrData[TARGET_SCALE_INDEX])];
                Color targetColor = int.Parse(parsedStrData[TARGET_COLOR_INDEX]) == -1 ? ColorUtils.GetInvalidColor() : m_targetColors[int.Parse(parsedStrData[TARGET_COLOR_INDEX])];

                if (objectIndex == -1) continue;

                Component targetObj = m_targetObjects[objectIndex];
                if (targetObj == null) continue;

                if ((Time.time - (startTime + timeoutTime)) <= 0f) continue;

                if (int.Parse(parsedStrData[START_POSITION_INDEX]) == -1)
                {
                    Vector3 startPositionLocal = RectTransformUtils.GetPosition(targetObj);

                    if (TypeUtils.IntToBool(int.Parse(parsedStrData[USE_DEFINED_POSITION_INDEX])))
                    {
                        int definedObjectIndex = UdonUtils.ArrayContains(m_definedObjects, targetObj);
                        if (definedObjectIndex != -1)
                        {
                            int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, definedObjectIndex.ToString());
                            if (definedStatsIndex != -1)
                            {
                                string[] parsedStrDefinedData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);
                                int defLocIndex = int.Parse(parsedStrDefinedData[DEFINITION_POSITION_INDEX]);

                                if (defLocIndex != -1)
                                {
                                    startPositionLocal = m_definedPosition[defLocIndex];
                                }
                            }
                        }
                    }

                    int startPositionIndex = ArrayUtils.AssignArrayValue(m_startPositions, startPositionLocal);
                    parsedStrData[START_POSITION_INDEX] = startPositionIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startPosition = startPositionLocal;
                }

                if (int.Parse(parsedStrData[START_ROTATION_INDEX]) == -1)
                {
                    Vector3 startRotationLocal = RectTransformUtils.GetRotation(targetObj);

                    if (TypeUtils.IntToBool(int.Parse(parsedStrData[USE_DEFINED_ROTATION_INDEX])))
                    {
                        int definedObjectIndex = UdonUtils.ArrayContains(m_definedObjects, targetObj);
                        if (definedObjectIndex != -1)
                        {
                            int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, definedObjectIndex.ToString());
                            if (definedStatsIndex != -1)
                            {
                                string[] parsedStrDefinedData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);
                                int definedRotationIndex = int.Parse(parsedStrDefinedData[DEFINITION_ROTATION_INDEX]);

                                if (definedRotationIndex != -1)
                                {
                                    startRotationLocal = m_definedRotations[definedRotationIndex];
                                }
                            }
                        }
                    }

                    int startPositionIndex = ArrayUtils.AssignArrayValue(m_startRotations, startRotationLocal);
                    parsedStrData[START_ROTATION_INDEX] = startPositionIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startRotation = startRotationLocal;
                }

                if (int.Parse(parsedStrData[START_SCALE_INDEX]) == -1)
                {
                    Vector3 startScaleLocal = RectTransformUtils.GetScale(targetObj);

                    if (TypeUtils.IntToBool(int.Parse(parsedStrData[USE_DEFINED_SCALE_INDEX])))
                    {
                        int definedObjectIndex = UdonUtils.ArrayContains(m_definedObjects, targetObj);
                        if (definedObjectIndex != -1)
                        {
                            int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, definedObjectIndex.ToString());
                            if (definedStatsIndex != -1)
                            {
                                string[] parsedStrDefinedData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);
                                int definedScaleIndex = int.Parse(parsedStrDefinedData[DEFINITION_SCALE_INDEX]);

                                if (definedScaleIndex != -1)
                                {
                                    startScaleLocal = m_definedScales[definedScaleIndex];
                                }
                            }
                        }
                    }

                    int startPositionIndex = ArrayUtils.AssignArrayValue(m_startScales, startScaleLocal);
                    parsedStrData[START_SCALE_INDEX] = startPositionIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startScale = startScaleLocal;
                }

                if (int.Parse(parsedStrData[START_COLOR_INDEX]) == -1)
                {
                    Color startColorLocal = ColorUtils.GetColor(targetObj, elementType);

                    if (TypeUtils.IntToBool(int.Parse(parsedStrData[USE_DEFINED_COLOR_INDEX])))
                    {
                        int definedObjectIndex = UdonUtils.ArrayContains(m_definedObjects, targetObj);
                        if (definedObjectIndex != -1)
                        {
                            int definedStatsIndex = UdonUtils.DataArrayContains(m_definedStats, DEFINITION_OBJECT_INDEX, definedObjectIndex.ToString());
                            if (definedStatsIndex != -1)
                            {
                                string[] parsedStrDefinedData = UdonUtils.ParseDataString(m_definedStats[definedStatsIndex]);
                                int definedColorIndex = int.Parse(parsedStrDefinedData[DEFINITION_COLOR_INDEX]);

                                if (definedColorIndex != -1)
                                {
                                    startColorLocal = m_definedColors[definedColorIndex];
                                }
                            }
                        }
                    }

                    int startColorIndex = ArrayUtils.AssignArrayValue(m_startColors, startColorLocal);
                    parsedStrData[START_COLOR_INDEX] = startColorIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startColor = startColorLocal;
                }

                float t = 1f;
                if (duration > 0f) t = UdonUtils.Clamp((Time.time - (startTime + timeoutTime)) / duration, 0f, 1f);

                float eased = MathUtils.ApplyEasing(t, transitionType);

                switch (animationMode)
                {
                    case AnimationMode.FadeIn:
                        {
                            Color objectColor = ColorUtils.GetColor(targetObj, elementType);
                            objectColor.a = eased;
                            ColorUtils.SetColor(targetObj, elementType, objectColor);
                            break;
                        }
                    case AnimationMode.FadeOut:
                        {
                            Color objectColor = ColorUtils.GetColor(targetObj, elementType);
                            objectColor.a = 1f - eased;
                            ColorUtils.SetColor(targetObj, elementType, objectColor);
                            break;
                        }
                    case AnimationMode.MoveUp:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.y -= pixelOffset * (1f - eased);
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveDown:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.y += pixelOffset * (1f - eased);
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveRight:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.x -= pixelOffset * (1f - eased);
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveLeft:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.x += pixelOffset * (1f - eased);
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveToUp:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.y += pixelOffset * eased;
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveToDown:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.y -= pixelOffset * eased;
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveToRight:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.x += pixelOffset * eased;
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveToLeft:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = startPosition;
                                newPosition.x -= pixelOffset * eased;
                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveTo:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = new Vector3(
                                    targetPosition.x == float.PositiveInfinity ? startPosition.x : startPosition.x + ((targetPosition.x - startPosition.x) * eased),
                                    targetPosition.y == float.PositiveInfinity ? startPosition.y : startPosition.y + ((targetPosition.y - startPosition.y) * eased),
                                    targetPosition.z == float.PositiveInfinity ? startPosition.z : startPosition.z + ((targetPosition.z - startPosition.z) * eased)
                                );

                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.MoveFrom:
                        {
                            if (!MathUtils.IsPositiveInfinity(startPosition))
                            {
                                Vector3 newPosition = new Vector3(
                                    targetPosition.x == float.PositiveInfinity ? startPosition.x : targetPosition.x + ((startPosition.x - targetPosition.x) * eased),
                                    targetPosition.y == float.PositiveInfinity ? startPosition.y : targetPosition.y + ((startPosition.y - targetPosition.y) * eased),
                                    targetPosition.z == float.PositiveInfinity ? startPosition.z : targetPosition.z + ((startPosition.z - targetPosition.z) * eased)
                                );

                                RectTransformUtils.SetPosition(targetObj, newPosition);
                            }
                            break;
                        }
                    case AnimationMode.ScaleTo:
                        {
                            if (!MathUtils.IsPositiveInfinity(startScale))
                            {
                                Vector3 newScale = new Vector3(
                                    targetScale.x == float.PositiveInfinity ? startScale.x : startScale.x + ((targetScale.x - startScale.x) * eased),
                                    targetScale.y == float.PositiveInfinity ? startScale.y : startScale.y + ((targetScale.y - startScale.y) * eased),
                                    targetScale.z == float.PositiveInfinity ? startScale.z : startScale.z + ((targetScale.z - startScale.z) * eased)
                                );

                                RectTransformUtils.SetScale(targetObj, newScale);
                            }
                            break;
                        }
                    case AnimationMode.ScaleFrom:
                        {
                            if (!MathUtils.IsPositiveInfinity(startScale))
                            {
                                Vector3 newScale = new Vector3(
                                    targetScale.x == float.PositiveInfinity ? startScale.x : targetScale.x + ((startScale.x - targetScale.x) * eased),
                                    targetScale.y == float.PositiveInfinity ? startScale.y : targetScale.y + ((startScale.y - targetScale.y) * eased),
                                    targetScale.z == float.PositiveInfinity ? startScale.z : targetScale.z + ((startScale.z - targetScale.z) * eased)
                                );

                                RectTransformUtils.SetScale(targetObj, newScale);
                            }
                            break;
                        }
                    case AnimationMode.RotateTo:
                        {
                            if (!MathUtils.IsPositiveInfinity(startRotation))
                            {
                                Vector3 newEulerAngles = new Vector3(
                                    targetRotation.x == float.PositiveInfinity ? startRotation.x : startRotation.x + ((targetRotation.x - startRotation.x) * eased),
                                    targetRotation.y == float.PositiveInfinity ? startRotation.y : startRotation.y + ((targetRotation.y - startRotation.y) * eased),
                                    targetRotation.z == float.PositiveInfinity ? startRotation.z : startRotation.z + ((targetRotation.z - startRotation.z) * eased)
                                );

                                RectTransformUtils.SetRotation(targetObj, newEulerAngles);
                            }
                            break;
                        }
                    case AnimationMode.RotateFrom:
                        {
                            if (!MathUtils.IsPositiveInfinity(startRotation))
                            {
                                Vector3 newEulerAngles = new Vector3(
                                    targetRotation.x == float.PositiveInfinity ? startRotation.x : targetRotation.x + ((startRotation.x - targetRotation.x) * eased),
                                    targetRotation.y == float.PositiveInfinity ? startRotation.y : targetRotation.y + ((startRotation.y - targetRotation.y) * eased),
                                    targetRotation.z == float.PositiveInfinity ? startRotation.z : targetRotation.z + ((startRotation.z - targetRotation.z) * eased)
                                );

                                RectTransformUtils.SetRotation(targetObj, newEulerAngles);
                            }
                            break;
                        }
                    case AnimationMode.ColorTo:
                        {
                            if (!MathUtils.IsPositiveInfinity(startColor))
                            {
                                Color newColor = new Color(
                                    float.IsPositiveInfinity(targetColor.r) ? startColor.r : startColor.r + ((targetColor.r - startColor.r) * eased),
                                    float.IsPositiveInfinity(targetColor.g) ? startColor.g : startColor.g + ((targetColor.g - startColor.g) * eased),
                                    float.IsPositiveInfinity(targetColor.b) ? startColor.b : startColor.b + ((targetColor.b - startColor.b) * eased),
                                    float.IsPositiveInfinity(targetColor.a) ? startColor.a : startColor.a + ((targetColor.a - startColor.a) * eased)
                                );

                                ColorUtils.SetColor(targetObj, elementType, newColor);
                            }
                            break;
                        }
                    case AnimationMode.ColorFrom:
                        {
                            if (!MathUtils.IsPositiveInfinity(startColor))
                            {
                                Color newColor = new Color(
                                    float.IsPositiveInfinity(targetColor.r) ? startColor.r : targetColor.r + ((startColor.r - targetColor.r) * eased),
                                    float.IsPositiveInfinity(targetColor.g) ? startColor.g : targetColor.g + ((startColor.g - targetColor.g) * eased),
                                    float.IsPositiveInfinity(targetColor.b) ? startColor.b : targetColor.b + ((startColor.b - targetColor.b) * eased),
                                    float.IsPositiveInfinity(targetColor.a) ? startColor.a : targetColor.a + ((startColor.a - targetColor.a) * eased)
                                );

                                ColorUtils.SetColor(targetObj, elementType, newColor);
                            }
                            break;
                        }
                    case AnimationMode.FlipX:
                        {
                            if (!MathUtils.IsPositiveInfinity(startRotation))
                            {
                                Vector3 newEulerAngles = new Vector3(
                                    Mathf.LerpAngle(startRotation.x, startRotation.x + 180f, eased),
                                    startRotation.y,
                                    startRotation.z
                                );

                                RectTransformUtils.SetRotation(targetObj, newEulerAngles);
                            }
                            break;
                        }
                    case AnimationMode.FlipY:
                        {
                            if (!MathUtils.IsPositiveInfinity(startRotation))
                            {
                                Vector3 newEulerAngles = new Vector3(
                                    startRotation.x,
                                    Mathf.LerpAngle(startRotation.y, startRotation.y + 180f, eased),
                                    startRotation.z
                                );

                                RectTransformUtils.SetRotation(targetObj, newEulerAngles);
                            }
                            break;
                        }
                    case AnimationMode.FlipZ:
                        {
                            if (!MathUtils.IsPositiveInfinity(startRotation))
                            {
                                Vector3 newEulerAngles = new Vector3(
                                    startRotation.x,
                                    startRotation.y,
                                    Mathf.LerpAngle(startRotation.z, startRotation.z + 180f, eased)
                                );

                                RectTransformUtils.SetRotation(targetObj, newEulerAngles);
                            }
                            break;
                        }
                }

                if (t >= 1f)
                {
                    Debug.Log($"{LogPrefix} Animation Finished - Object: {targetObj.name} - Task: {i}");
                    RemoveTask(parsedStrData, i);
                }
            }

            if (peakConcurrentAnimations < currentWorkingTasks) peakConcurrentAnimations = currentWorkingTasks;
            runningAnimations = currentWorkingTasks;
        }
        #endregion
    }
}
