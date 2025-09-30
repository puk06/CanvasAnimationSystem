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
        [SerializeField] private int maxConcurrentAnimations = 32;

        #region 確認用
        [Header("現在実行中のアニメーション数（確認用）")]
        [Tooltip("現在進行中のアニメーション数を表示します。終了したアニメーションはカウントされません。")]
        [SerializeField] private int runningAnimations = 0;

        [Header("これまでの同時実行アニメーション最大数（確認用）")]
        [Tooltip("今まで同時に再生されたアニメーション数の最大値を記録します。\nMaxConcurrentAnimationsを決める際の目安にしてください。")]
        [SerializeField] private int peakConcurrentAnimations = 0;
        #endregion

        private const string LogTag = "[{0}]";
        private readonly string ColoredTag = UdonUtils.ColorizeString("Canvas Animation System", "#4eb3ee");

        private const int OBJECT_INDEX = 0;
        private const int DURATION_INDEX = 1;
        private const int START_TIME_INDEX = 2;
        private const int PIXEL_OFFSET_INDEX = 3;
        private const int TIME_OUT_INDEX = 4;
        private const int START_LOCATION_INDEX = 5;
        private const int START_ROTATION_INDEX = 6;
        private const int START_SCALE_INDEX = 7;
        private const int START_COLOR_INDEX = 8;
        private const int TRANSITION_TYPE_INDEX = 9;
        private const int ANIMATION_MODE_INDEX = 10;
        private const int ELEMENT_TYPE_INDEX = 11;
        private const int TARGET_POINT_INDEX = 12;
        private const int TARGET_ROTATION_INDEX = 13;
        private const int TARGET_SCALE_INDEX = 14;
        private const int TARGET_COLOR_INDEX = 15;

        private string[] m_currentTasks;

        private Text[] m_targetTexts;
        private Button[] m_targetButtons;
        // private Dropdown[] targetDropDowns;
        // private InputField[] targetInputFields;
        private Image[] m_targetImages;
        private RawImage[] m_targetRawImages;
        // private Toggle[] targetToggles;
        // private Slider[] targetSliders;
        // private Scrollbar[] targetScrollbars;

        private TMP_Text[] m_targetTMPTexts;
        // private TMP_Dropdown[] targetTMPDropDowns;
        // private TMP_InputField[] targetTMPInputFields;

        private float[] m_durations;
        private int[] m_pixelOffsets;
        private float[] m_startTimes;
        private float[] m_timeoutTimes;

        private Vector3[] m_startLocations;
        private Vector3[] m_startRotations;
        private Vector3[] m_startScales;
        private Color[] m_startColors;

        private TransitionType[] m_transitionTypes;
        private ElementType[] m_elementTypes;
        private AnimationMode[] m_animationModes;

        private Vector3[] m_targetPoints;
        private Vector3[] m_targetScales;
        private Vector3[] m_targetRotations;
        private Color[] m_targetColors;

        void Start()
        {
            m_currentTasks = new string[maxConcurrentAnimations];

            m_targetTexts = new Text[maxConcurrentAnimations];
            m_targetButtons = new Button[maxConcurrentAnimations];
            m_targetImages = new Image[maxConcurrentAnimations];
            m_targetRawImages = new RawImage[maxConcurrentAnimations];
            m_targetTMPTexts = new TMP_Text[maxConcurrentAnimations];

            m_durations = new float[maxConcurrentAnimations];
            m_pixelOffsets = new int[maxConcurrentAnimations];
            m_startTimes = new float[maxConcurrentAnimations];
            m_timeoutTimes = new float[maxConcurrentAnimations];

            m_startLocations = new Vector3[maxConcurrentAnimations];
            m_startRotations = new Vector3[maxConcurrentAnimations];
            m_startScales = new Vector3[maxConcurrentAnimations];
            m_startColors = new Color[maxConcurrentAnimations];

            m_transitionTypes = new TransitionType[maxConcurrentAnimations];
            m_elementTypes = new ElementType[maxConcurrentAnimations];
            m_animationModes = new AnimationMode[maxConcurrentAnimations];

            m_targetPoints = new Vector3[maxConcurrentAnimations];
            m_targetScales = new Vector3[maxConcurrentAnimations];
            m_targetRotations = new Vector3[maxConcurrentAnimations];
            m_targetColors = new Color[maxConcurrentAnimations];

            ArrayUtils.InitializeValues(m_durations);
            ArrayUtils.InitializeValues(m_pixelOffsets);
            ArrayUtils.InitializeValues(m_startTimes);
            ArrayUtils.InitializeValues(m_timeoutTimes);

            ArrayUtils.InitializeValues(m_startLocations);
            ArrayUtils.InitializeValues(m_startRotations);
            ArrayUtils.InitializeValues(m_startScales);
            ArrayUtils.InitializeValues(m_startColors);
            
            ArrayUtils.InitializeValues(m_targetPoints);
            ArrayUtils.InitializeValues(m_targetScales);
            ArrayUtils.InitializeValues(m_targetRotations);
            ArrayUtils.InitializeValues(m_targetColors);
        }

        public CanvasAnimationSystem Show(Text element)
            => FadeInternal(element, ElementType.Text, 0f, 0f, FadeType.In, TransitionType.None);
        public CanvasAnimationSystem Show(Button element)
            => FadeInternal(element, ElementType.Button, 0f, 0f, FadeType.In, TransitionType.None);
        public CanvasAnimationSystem Show(Image element)
            => FadeInternal(element, ElementType.Image, 0f, 0f, FadeType.In, TransitionType.None);
        public CanvasAnimationSystem Show(RawImage element)
            => FadeInternal(element, ElementType.RawImage, 0f, 0f, FadeType.In, TransitionType.None);
        public CanvasAnimationSystem Show(TMP_Text element)
            => FadeInternal(element, ElementType.TMP_Text, 0f, 0f, FadeType.In, TransitionType.None);

        public CanvasAnimationSystem Hide(Text element)
            => FadeInternal(element, ElementType.Text, 0f, 0f, FadeType.Out, TransitionType.None);
        public CanvasAnimationSystem Hide(Button element)
            => FadeInternal(element, ElementType.Button, 0f, 0f, FadeType.Out, TransitionType.None);
        public CanvasAnimationSystem Hide(Image element)
            => FadeInternal(element, ElementType.Image, 0f, 0f, FadeType.Out, TransitionType.None);
        public CanvasAnimationSystem Hide(RawImage element)
            => FadeInternal(element, ElementType.RawImage, 0f, 0f, FadeType.Out, TransitionType.None);
        public CanvasAnimationSystem Hide(TMP_Text element)
            => FadeInternal(element, ElementType.TMP_Text, 0f, 0f, FadeType.Out, TransitionType.None);

        public CanvasAnimationSystem Fade(Text element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Text, duration, after, fadeType, transitionType);
        public CanvasAnimationSystem Fade(Button element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Button, duration, after, fadeType, transitionType);
        public CanvasAnimationSystem Fade(Image element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.Image, duration, after, fadeType, transitionType);
        public CanvasAnimationSystem Fade(RawImage element, float duration, float after, FadeType fadeType, TransitionType transitionType)
            => FadeInternal(element, ElementType.RawImage, duration, after, fadeType, transitionType);
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

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem Move(Text element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
            => MoveInternal(element, ElementType.Text, duration, after, pixelOffset, moveDirection, transitionType);
        public CanvasAnimationSystem Move(Button element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
            => MoveInternal(element, ElementType.Button, duration, after, pixelOffset, moveDirection, transitionType);
        public CanvasAnimationSystem Move(Image element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
            => MoveInternal(element, ElementType.Image, duration, after, pixelOffset, moveDirection, transitionType);
        public CanvasAnimationSystem Move(RawImage element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
            => MoveInternal(element, ElementType.RawImage, duration, after, pixelOffset, moveDirection, transitionType);
        public CanvasAnimationSystem Move(TMP_Text element, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
            => MoveInternal(element, ElementType.TMP_Text, duration, after, pixelOffset, moveDirection, transitionType);
        private CanvasAnimationSystem MoveInternal(Component element, ElementType elementType, float duration, float after, int pixelOffset, MoveDirection moveDirection, TransitionType transitionType)
        {
            AnimationMode animationMode;
            switch (moveDirection)
            {
                case MoveDirection.Up: animationMode = AnimationMode.MoveUp; break;
                case MoveDirection.Down: animationMode = AnimationMode.MoveDown; break;
                case MoveDirection.Left: animationMode = AnimationMode.MoveLeft; break;
                case MoveDirection.Right: animationMode = AnimationMode.MoveRight; break;
                default: animationMode = AnimationMode.MoveUp; break;
            }

            AddTask(element, duration, after, pixelOffset, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem MoveLocation(Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
            => MoveLocationInternal(element, ElementType.Text, duration, after, animationDirection, targetPoint, transitionType);
        public CanvasAnimationSystem MoveLocation(Button element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
            => MoveLocationInternal(element, ElementType.Button, duration, after, animationDirection, targetPoint, transitionType);
        public CanvasAnimationSystem MoveLocation(Image element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
            => MoveLocationInternal(element, ElementType.Image, duration, after, animationDirection, targetPoint, transitionType);
        public CanvasAnimationSystem MoveLocation(RawImage element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
            => MoveLocationInternal(element, ElementType.RawImage, duration, after, animationDirection, targetPoint, transitionType);
        public CanvasAnimationSystem MoveLocation(TMP_Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
            => MoveLocationInternal(element, ElementType.TMP_Text, duration, after, animationDirection, targetPoint, transitionType);
        private CanvasAnimationSystem MoveLocationInternal(Component element, ElementType elementType, float duration, float after, AnimationDirection animationDirection, Vector3 targetPoint, TransitionType transitionType)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.MoveTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.MoveFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, targetPoint, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem MoveFromTo(Text element, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
            => MoveFromToInternal(element, ElementType.Text, duration, after, startPoint, targetPoint, transitionType);
        public CanvasAnimationSystem MoveFromTo(Button element, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
            => MoveFromToInternal(element, ElementType.Button, duration, after, startPoint, targetPoint, transitionType);
        public CanvasAnimationSystem MoveFromTo(Image element, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
            => MoveFromToInternal(element, ElementType.Image, duration, after, startPoint, targetPoint, transitionType);
        public CanvasAnimationSystem MoveFromTo(RawImage element, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
            => MoveFromToInternal(element, ElementType.RawImage, duration, after, startPoint, targetPoint, transitionType);
        public CanvasAnimationSystem MoveFromTo(TMP_Text element, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
            => MoveFromToInternal(element, ElementType.TMP_Text, duration, after, startPoint, targetPoint, transitionType);
        private CanvasAnimationSystem MoveFromToInternal(Component element, ElementType elementType, float duration, float after, Vector3 startPoint, Vector3 targetPoint, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.MoveTo, elementType, targetPoint, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), startPoint, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem Rotate(Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
            => RotateInternal(element, ElementType.Text, duration, after, animationDirection, targetRotation, transitionType);
        public CanvasAnimationSystem Rotate(Button element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
            => RotateInternal(element, ElementType.Button, duration, after, animationDirection, targetRotation, transitionType);
        public CanvasAnimationSystem Rotate(Image element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
            => RotateInternal(element, ElementType.Image, duration, after, animationDirection, targetRotation, transitionType);
        public CanvasAnimationSystem Rotate(RawImage element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
            => RotateInternal(element, ElementType.RawImage, duration, after, animationDirection, targetRotation, transitionType);
        public CanvasAnimationSystem Rotate(TMP_Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
            => RotateInternal(element, ElementType.TMP_Text, duration, after, animationDirection, targetRotation, transitionType);
        private CanvasAnimationSystem RotateInternal(Component element, ElementType elementType, float duration, float after, AnimationDirection animationDirection, Vector3 targetRotation, TransitionType transitionType)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.RotateTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.RotateFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, targetRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem RotateFromTo(Text element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
            => RotateFromToInternal(element, ElementType.Text, duration, after, startRotation, targetRotation, transitionType);
        public CanvasAnimationSystem RotateFromTo(Button element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
            => RotateFromToInternal(element, ElementType.Button, duration, after, startRotation, targetRotation, transitionType);
        public CanvasAnimationSystem RotateFromTo(Image element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
            => RotateFromToInternal(element, ElementType.Image, duration, after, startRotation, targetRotation, transitionType);
        public CanvasAnimationSystem RotateFromTo(RawImage element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
            => RotateFromToInternal(element, ElementType.RawImage, duration, after, startRotation, targetRotation, transitionType);
        public CanvasAnimationSystem RotateFromTo(TMP_Text element, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
            => RotateFromToInternal(element, ElementType.TMP_Text, duration, after, startRotation, targetRotation, transitionType);
        private CanvasAnimationSystem RotateFromToInternal(Component element, ElementType elementType, float duration, float after, Vector3 startRotation, Vector3 targetRotation, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.RotateTo, elementType, Vector3.positiveInfinity, targetRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, startRotation, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem Scale(Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
            => ScaleInternal(element, ElementType.Text, duration, after, animationDirection, targetScale, transitionType);
        public CanvasAnimationSystem Scale(Button element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
            => ScaleInternal(element, ElementType.Button, duration, after, animationDirection, targetScale, transitionType);
        public CanvasAnimationSystem Scale(Image element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
            => ScaleInternal(element, ElementType.Image, duration, after, animationDirection, targetScale, transitionType);
        public CanvasAnimationSystem Scale(RawImage element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
            => ScaleInternal(element, ElementType.RawImage, duration, after, animationDirection, targetScale, transitionType);
        public CanvasAnimationSystem Scale(TMP_Text element, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
            => ScaleInternal(element, ElementType.TMP_Text, duration, after, animationDirection, targetScale, transitionType);
        private CanvasAnimationSystem ScaleInternal(Component element, ElementType elementType, float duration, float after, AnimationDirection animationDirection, Vector3 targetScale, TransitionType transitionType)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.ScaleTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.ScaleFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, targetScale, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem ScaleFromTo(Text element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
            => ScaleFromToInternal(element, ElementType.Text, duration, after, startScale, targetScale, transitionType);
        public CanvasAnimationSystem ScaleFromTo(Button element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
            => ScaleFromToInternal(element, ElementType.Button, duration, after, startScale, targetScale, transitionType);
        public CanvasAnimationSystem ScaleFromTo(Image element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
            => ScaleFromToInternal(element, ElementType.Image, duration, after, startScale, targetScale, transitionType);
        public CanvasAnimationSystem ScaleFromTo(RawImage element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
            => ScaleFromToInternal(element, ElementType.RawImage, duration, after, startScale, targetScale, transitionType);
        public CanvasAnimationSystem ScaleFromTo(TMP_Text element, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
            => ScaleFromToInternal(element, ElementType.TMP_Text, duration, after, startScale, targetScale, transitionType);
        private CanvasAnimationSystem ScaleFromToInternal(Component element, ElementType elementType, float duration, float after, Vector3 startScale, Vector3 targetScale, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.ScaleTo, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, targetScale, ColorUtils.GetInvalidColor(), Vector3.positiveInfinity, Vector3.positiveInfinity, startScale, ColorUtils.GetInvalidColor());
            return this;
        }
        
        public CanvasAnimationSystem Color(Text element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
            => ColorInternal(element, ElementType.Text, duration, after, animationDirection, targetColor, transitionType);
        public CanvasAnimationSystem Color(Button element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
            => ColorInternal(element, ElementType.Button, duration, after, animationDirection, targetColor, transitionType);
        public CanvasAnimationSystem Color(Image element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
            => ColorInternal(element, ElementType.Image, duration, after, animationDirection, targetColor, transitionType);
        public CanvasAnimationSystem Color(RawImage element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
            => ColorInternal(element, ElementType.RawImage, duration, after, animationDirection, targetColor, transitionType);
        public CanvasAnimationSystem Color(TMP_Text element, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
            => ColorInternal(element, ElementType.TMP_Text, duration, after, animationDirection, targetColor, transitionType);
        private CanvasAnimationSystem ColorInternal(Component element, ElementType elementType, float duration, float after, AnimationDirection animationDirection, Color targetColor, TransitionType transitionType)
        {
            AnimationMode animationMode = AnimationMode.None;
            switch (animationDirection)
            {
                case AnimationDirection.To: animationMode = AnimationMode.ColorTo; break;
                case AnimationDirection.From: animationMode = AnimationMode.ColorFrom; break;
            }

            AddTask(element, duration, after, -1, transitionType, animationMode, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, targetColor, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, ColorUtils.GetInvalidColor());
            return this;
        }

        public CanvasAnimationSystem ColorFromTo(Text element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Text, duration, after, startColor, targetColor, transitionType);
        public CanvasAnimationSystem ColorFromTo(Button element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Button, duration, after, startColor, targetColor, transitionType);
        public CanvasAnimationSystem ColorFromTo(Image element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.Image, duration, after, startColor, targetColor, transitionType);
        public CanvasAnimationSystem ColorFromTo(RawImage element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.RawImage, duration, after, startColor, targetColor, transitionType);
        public CanvasAnimationSystem ColorFromTo(TMP_Text element, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
            => ColorFromToInternal(element, ElementType.TMP_Text, duration, after, startColor, targetColor, transitionType);
        private CanvasAnimationSystem ColorFromToInternal(Component element, ElementType elementType, float duration, float after, Color startColor, Color targetColor, TransitionType transitionType)
        {
            AddTask(element, duration, after, -1, transitionType, AnimationMode.ColorTo, elementType, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, targetColor, Vector3.positiveInfinity, Vector3.positiveInfinity, Vector3.positiveInfinity, startColor);
            return this;
        }

        public CanvasAnimationSystem Cancel(Text element)
            => CancelInternal(element, ElementType.Text);
        public CanvasAnimationSystem Cancel(Button element)
            => CancelInternal(element, ElementType.Button);
        public CanvasAnimationSystem Cancel(Image element)
            => CancelInternal(element, ElementType.Image);
        public CanvasAnimationSystem Cancel(RawImage element)
            => CancelInternal(element, ElementType.RawImage);
        public CanvasAnimationSystem Cancel(TMP_Text element)
            => CancelInternal(element, ElementType.TMP_Text);
        private CanvasAnimationSystem CancelInternal(Component element, ElementType elementType)
        {
            for (int i = 0; i < m_currentTasks.Length; i++)
            {
                if (m_currentTasks[i] == null || m_currentTasks[i] == "") continue;

                string[] parsedStrData = UdonUtils.ParseDataString(m_currentTasks[i]);

                int objectIndex = int.Parse(parsedStrData[OBJECT_INDEX]);
                if (objectIndex == -1) continue;

                ElementType objectElementType = int.Parse(parsedStrData[ELEMENT_TYPE_INDEX]) == -1 ? ElementType.None : m_elementTypes[int.Parse(parsedStrData[ELEMENT_TYPE_INDEX])];
                if (elementType != objectElementType) continue;

                Component targetObj = null;
                switch (elementType)
                {
                    case ElementType.Text: targetObj = m_targetTexts[objectIndex]; break;
                    case ElementType.TMP_Text: targetObj = m_targetTMPTexts[objectIndex]; break;
                    case ElementType.Image: targetObj = m_targetImages[objectIndex]; break;
                    case ElementType.RawImage: targetObj = m_targetRawImages[objectIndex]; break;
                    case ElementType.Button: targetObj = m_targetButtons[objectIndex]; break;
                }

                if (targetObj == null || targetObj != element) continue;

                Debug.Log($"{string.Format(LogTag, ColoredTag)} Animation Cannceled - Object: {targetObj.name} - Task: {i}");
                RemoveTask(parsedStrData, i);
            }
            
            return this;
        }

        public CanvasAnimationSystem CancelAll()
            => CancelAllInternal();
        private CanvasAnimationSystem CancelAllInternal()
        {
            for (int i = 0; i < m_currentTasks.Length; i++)
            {
                if (m_currentTasks[i] == null || m_currentTasks[i] == "") continue;

                string[] parsedStrData = UdonUtils.ParseDataString(m_currentTasks[i]);

                Debug.Log($"{string.Format(LogTag, ColoredTag)} Animation Cannceled - Task: {i}");
                RemoveTask(parsedStrData, i);
            }

            return this;
        }

        public void Exit()
        {
            Debug.Log($"{string.Format(LogTag, ColoredTag)} Exit triggered - disabling component...");
            if (runningAnimations > 0) Debug.LogWarning($"{string.Format(LogTag, ColoredTag)} Some animations are still playing, but the component is being disabled.");

            Destroy(this);
        }

        private void AddTask(
            Component element,
            float time, float after,
            int pixelOffset,
            TransitionType transition,
            AnimationMode mode,
            ElementType elementType,
            Vector3 targetPoint, Vector3 targetRotation, Vector3 targetScale, Color targetColor,
            Vector3 startPoint, Vector3 startRotation, Vector3 startScale, Color startColor
        )
        {
            int objectIndex;

            switch (elementType)
            {
                case ElementType.Text:
                    {
                        Text text = (Text)element;
                        objectIndex = ArrayUtils.AssignArrayValue(m_targetTexts, text);
                        break;
                    }
                case ElementType.Button:
                    {
                        Button button = (Button)element;
                        objectIndex = ArrayUtils.AssignArrayValue(m_targetButtons, button);
                        break;
                    }
                case ElementType.Image:
                    {
                        Image image = (Image)element;
                        objectIndex = ArrayUtils.AssignArrayValue(m_targetImages, image);
                        break;
                    }
                case ElementType.RawImage:
                    {
                        RawImage rawImage = (RawImage)element;
                        objectIndex = ArrayUtils.AssignArrayValue(m_targetRawImages, rawImage);
                        break;
                    }
                case ElementType.TMP_Text:
                    {
                        TMP_Text tmpText = (TMP_Text)element;
                        objectIndex = ArrayUtils.AssignArrayValue(m_targetTMPTexts, tmpText);
                        break;
                    }
                default:
                    return;
            }

            string stats = AssignData(objectIndex, time, after, pixelOffset, transition, mode, elementType, targetPoint, targetRotation, targetScale, targetColor, startPoint, startRotation, startScale, startColor);

            int statsIndex = ArrayUtils.AssignArrayValue(m_currentTasks, stats);

            if (statsIndex == -1) Debug.LogError($"{string.Format(LogTag, ColoredTag)} Failed to create Animation Task");
            else Debug.Log($"{string.Format(LogTag, ColoredTag)} Animation Task Created - Object: {element.name} - Task: {statsIndex}");
        }

        private string AssignData(int objectIndex, float time, float after, int pixelOffset, TransitionType transitionType, AnimationMode mode, ElementType elementType, Vector3 targetPoint, Vector3 targetRotation, Vector3 targetScale, Color targetColor, Vector3 startPoint, Vector3 startRotation, Vector3 startScale, Color startColor)
        {
            int durationIndex = ArrayUtils.AssignArrayValue(m_durations, time);
            int startTimeIndex = ArrayUtils.AssignArrayValue(m_startTimes, Time.time);
            int pixelOffsetIndex = ArrayUtils.AssignArrayValue(m_pixelOffsets, pixelOffset);
            int timeoutTimesIndex = ArrayUtils.AssignArrayValue(m_timeoutTimes, after);
            int startLocationIndex = MathUtils.IsPositiveInfinity(startPoint) ? -1 : ArrayUtils.AssignArrayValue(m_startLocations, startPoint);
            int startRotationIndex = MathUtils.IsPositiveInfinity(startRotation) ? -1 : ArrayUtils.AssignArrayValue(m_startRotations, startRotation);
            int startScaleIndex = MathUtils.IsPositiveInfinity(startScale) ? -1 : ArrayUtils.AssignArrayValue(m_startScales, startScale);
            int startColorIndex = ColorUtils.IsInvalidColor(startColor) ? -1 : ArrayUtils.AssignArrayValue(m_startColors, startColor);
            int transitionTypeIndex = ArrayUtils.AssignArrayValue(m_transitionTypes, transitionType);
            int elementTypeIndex = ArrayUtils.AssignArrayValue(m_elementTypes, elementType);
            int modesIndex = ArrayUtils.AssignArrayValue(m_animationModes, mode);
            int targetPointIndex = ArrayUtils.AssignArrayValue(m_targetPoints, targetPoint);
            int targetRotationIndex = ArrayUtils.AssignArrayValue(m_targetRotations, targetRotation);
            int targetScaleIndex = ArrayUtils.AssignArrayValue(m_targetScales, targetScale);
            int targetColorIndex = ArrayUtils.AssignArrayValue(m_targetColors, targetColor);

            return $"{objectIndex},,,{durationIndex},,,{startTimeIndex},,,{pixelOffsetIndex},,,{timeoutTimesIndex},,,{startLocationIndex},,,{startRotationIndex},,,{startScaleIndex},,,{startColorIndex},,,{transitionTypeIndex},,,{modesIndex},,,{elementTypeIndex},,,{targetPointIndex},,,{targetRotationIndex},,,{targetScaleIndex},,,{targetColorIndex}";
        }

        private void Update()
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
                Vector3 startLocation = int.Parse(parsedStrData[START_LOCATION_INDEX]) == -1 ? Vector3.positiveInfinity : m_startLocations[int.Parse(parsedStrData[START_LOCATION_INDEX])];
                Vector3 startRotation = int.Parse(parsedStrData[START_ROTATION_INDEX]) == -1 ? Vector3.positiveInfinity : m_startRotations[int.Parse(parsedStrData[START_ROTATION_INDEX])];
                Vector3 startScale = int.Parse(parsedStrData[START_SCALE_INDEX]) == -1 ? Vector3.positiveInfinity : m_startScales[int.Parse(parsedStrData[START_SCALE_INDEX])];
                Color startColor = int.Parse(parsedStrData[START_COLOR_INDEX]) == -1 ? ColorUtils.GetInvalidColor() : m_startColors[int.Parse(parsedStrData[START_COLOR_INDEX])];
                TransitionType transitionType = int.Parse(parsedStrData[TRANSITION_TYPE_INDEX]) == -1 ? TransitionType.None : m_transitionTypes[int.Parse(parsedStrData[TRANSITION_TYPE_INDEX])];
                AnimationMode animationMode = int.Parse(parsedStrData[ANIMATION_MODE_INDEX]) == -1 ? AnimationMode.None : m_animationModes[int.Parse(parsedStrData[ANIMATION_MODE_INDEX])];
                ElementType elementType = int.Parse(parsedStrData[ELEMENT_TYPE_INDEX]) == -1 ? ElementType.None : m_elementTypes[int.Parse(parsedStrData[ELEMENT_TYPE_INDEX])];
                Vector3 targetPoint = int.Parse(parsedStrData[TARGET_POINT_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetPoints[int.Parse(parsedStrData[TARGET_POINT_INDEX])];
                Vector3 targetRotation = int.Parse(parsedStrData[TARGET_ROTATION_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetRotations[int.Parse(parsedStrData[TARGET_ROTATION_INDEX])];
                Vector3 targetScale = int.Parse(parsedStrData[TARGET_SCALE_INDEX]) == -1 ? Vector3.positiveInfinity : m_targetScales[int.Parse(parsedStrData[TARGET_SCALE_INDEX])];
                Color targetColor = int.Parse(parsedStrData[TARGET_COLOR_INDEX]) == -1 ? ColorUtils.GetInvalidColor() : m_targetColors[int.Parse(parsedStrData[TARGET_COLOR_INDEX])];


                if (objectIndex == -1) continue;

                Component targetObj = null;
                switch (elementType)
                {
                    case ElementType.Text: targetObj = m_targetTexts[objectIndex]; break;
                    case ElementType.TMP_Text: targetObj = m_targetTMPTexts[objectIndex]; break;
                    case ElementType.Image: targetObj = m_targetImages[objectIndex]; break;
                    case ElementType.RawImage: targetObj = m_targetRawImages[objectIndex]; break;
                    case ElementType.Button: targetObj = m_targetButtons[objectIndex]; break;
                }

                if (targetObj == null) continue;

                if ((Time.time - (startTime + timeoutTime)) <= 0f) continue;

                if (int.Parse(parsedStrData[START_LOCATION_INDEX]) == -1)
                {
                    Vector3 startLocationLocal = targetObj.GetComponent<RectTransform>().localPosition;
                    int startLocationIndex = ArrayUtils.AssignArrayValue(m_startLocations, startLocationLocal);
                    parsedStrData[START_LOCATION_INDEX] = startLocationIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startLocation = startLocationLocal;
                }

                if (int.Parse(parsedStrData[START_ROTATION_INDEX]) == -1)
                {
                    Vector3 startRotationLocal = targetObj.GetComponent<RectTransform>().localEulerAngles;
                    int startRotationIndex = ArrayUtils.AssignArrayValue(m_startRotations, startRotationLocal);
                    parsedStrData[START_ROTATION_INDEX] = startRotationIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startRotation = startRotationLocal;
                }

                if (int.Parse(parsedStrData[START_SCALE_INDEX]) == -1)
                {
                    Vector3 startScaleLocal = targetObj.GetComponent<RectTransform>().localScale;
                    int startScaleIndex = ArrayUtils.AssignArrayValue(m_startScales, startScaleLocal);
                    parsedStrData[START_SCALE_INDEX] = startScaleIndex.ToString();
                    m_currentTasks[i] = string.Join(",,,", parsedStrData);

                    startScale = startScaleLocal;
                }

                if (int.Parse(parsedStrData[START_COLOR_INDEX]) == -1)
                {
                    Color startColorLocal = ColorUtils.GetColor(targetObj, elementType);
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
                    case AnimationMode.MoveDown:
                        {
                            Vector3 newPos = startLocation;
                            newPos.y += pixelOffset * (1f - eased);
                            targetObj.GetComponent<RectTransform>().localPosition = newPos;
                            break;
                        }
                    case AnimationMode.MoveUp:
                        {
                            Vector3 newPos = startLocation;
                            newPos.y -= pixelOffset * (1f - eased);
                            targetObj.GetComponent<RectTransform>().localPosition = newPos;
                            break;
                        }
                    case AnimationMode.MoveLeft:
                        {
                            Vector3 newPos = startLocation;
                            newPos.x += pixelOffset * (1f - eased);
                            targetObj.GetComponent<RectTransform>().localPosition = newPos;
                            break;
                        }
                    case AnimationMode.MoveRight:
                        {
                            Vector3 newPos = startLocation;
                            newPos.x -= pixelOffset * (1f - eased);
                            targetObj.GetComponent<RectTransform>().localPosition = newPos;
                            break;
                        }
                    case AnimationMode.MoveTo:
                        {
                            Vector3 newPosition = new Vector3(
                                targetPoint.x == float.PositiveInfinity ? startLocation.x : startLocation.x + ((targetPoint.x - startLocation.x) * eased),
                                targetPoint.y == float.PositiveInfinity ? startLocation.y : startLocation.y + ((targetPoint.y - startLocation.y) * eased),
                                targetPoint.z == float.PositiveInfinity ? startLocation.z : startLocation.z + ((targetPoint.z - startLocation.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localPosition = newPosition;
                            break;
                        }
                    case AnimationMode.MoveFrom:
                        {
                            Vector3 newPosition = new Vector3(
                                targetPoint.x == float.PositiveInfinity ? startLocation.x : targetPoint.x + ((startLocation.x - targetPoint.x) * eased),
                                targetPoint.y == float.PositiveInfinity ? startLocation.y : targetPoint.y + ((startLocation.y - targetPoint.y) * eased),
                                targetPoint.z == float.PositiveInfinity ? startLocation.z : targetPoint.z + ((startLocation.z - targetPoint.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localPosition = newPosition;
                            break;
                        }
                    case AnimationMode.ScaleTo:
                        {
                            Vector3 newScale = new Vector3(
                                targetScale.x == float.PositiveInfinity ? startScale.x : startScale.x + ((targetScale.x - startScale.x) * eased),
                                targetScale.y == float.PositiveInfinity ? startScale.y : startScale.y + ((targetScale.y - startScale.y) * eased),
                                targetScale.z == float.PositiveInfinity ? startScale.z : startScale.z + ((targetScale.z - startScale.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localScale = newScale;
                            break;
                        }
                    case AnimationMode.ScaleFrom:
                        {
                            Vector3 newScale = new Vector3(
                                targetScale.x == float.PositiveInfinity ? startScale.x : targetScale.x + ((startScale.x - targetScale.x) * eased),
                                targetScale.y == float.PositiveInfinity ? startScale.y : targetScale.y + ((startScale.y - targetScale.y) * eased),
                                targetScale.z == float.PositiveInfinity ? startScale.z : targetScale.z + ((startScale.z - targetScale.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localScale = newScale;
                            break;
                        }
                    case AnimationMode.RotateTo:
                        {
                            Vector3 newEulerAngles = new Vector3(
                                targetRotation.x == float.PositiveInfinity ? startRotation.x : startRotation.x + ((targetRotation.x - startRotation.x) * eased),
                                targetRotation.y == float.PositiveInfinity ? startRotation.y : startRotation.y + ((targetRotation.y - startRotation.y) * eased),
                                targetRotation.z == float.PositiveInfinity ? startRotation.z : startRotation.z + ((targetRotation.z - startRotation.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localEulerAngles = newEulerAngles;
                            break;
                        }
                    case AnimationMode.RotateFrom:
                        {
                            Vector3 newEulerAngles = new Vector3(
                                targetRotation.x == float.PositiveInfinity ? startRotation.x : targetRotation.x + ((startRotation.x - targetRotation.x) * eased),
                                targetRotation.y == float.PositiveInfinity ? startRotation.y : targetRotation.y + ((startRotation.y - targetRotation.y) * eased),
                                targetRotation.z == float.PositiveInfinity ? startRotation.z : targetRotation.z + ((startRotation.z - targetRotation.z) * eased)
                            );

                            targetObj.GetComponent<RectTransform>().localEulerAngles = newEulerAngles;
                            break;
                        }
                    case AnimationMode.ColorTo:
                        {
                            Color newColor = new Color(
                                float.IsPositiveInfinity(targetColor.r) ? startColor.r : startColor.r + ((targetColor.r - startColor.r) * eased),
                                float.IsPositiveInfinity(targetColor.g) ? startColor.g : startColor.g + ((targetColor.g - startColor.g) * eased),
                                float.IsPositiveInfinity(targetColor.b) ? startColor.b : startColor.b + ((targetColor.b - startColor.b) * eased),
                                float.IsPositiveInfinity(targetColor.a) ? startColor.a : startColor.a + ((targetColor.a - startColor.a) * eased)
                            );

                            ColorUtils.SetColor(targetObj, elementType, newColor);
                            break;
                        }
                    case AnimationMode.ColorFrom:
                        {
                            Color newColor = new Color(
                                float.IsPositiveInfinity(targetColor.r) ? startColor.r : targetColor.r + ((startColor.r - targetColor.r) * eased),
                                float.IsPositiveInfinity(targetColor.g) ? startColor.g : targetColor.g + ((startColor.g - targetColor.g) * eased),
                                float.IsPositiveInfinity(targetColor.b) ? startColor.b : targetColor.b + ((startColor.b - targetColor.b) * eased),
                                float.IsPositiveInfinity(targetColor.a) ? startColor.a : targetColor.a + ((startColor.a - targetColor.a) * eased)
                            );

                            ColorUtils.SetColor(targetObj, elementType, newColor);
                            break;
                        }
                }

                if (t >= 1f)
                {
                    Debug.Log($"{string.Format(LogTag, ColoredTag)} Animation Finished - Object: {targetObj.name} - Task: {i}");
                    RemoveTask(parsedStrData, i);
                }
            }

            if (peakConcurrentAnimations < currentWorkingTasks) peakConcurrentAnimations = currentWorkingTasks;
            runningAnimations = currentWorkingTasks;
        }

        private void RemoveTask(string[] parsedTaskDataString, int taskIndex)
        {
            int objectIndex = int.Parse(parsedTaskDataString[OBJECT_INDEX]);
            if (objectIndex == -1) return;

            ElementType elementType = int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX]) == -1 ? ElementType.None : m_elementTypes[int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX])];

            switch (elementType)
            {
                case ElementType.Text: m_targetTexts[objectIndex] = null; break;
                case ElementType.TMP_Text: m_targetTMPTexts[objectIndex] = null; break;
                case ElementType.Image: m_targetImages[objectIndex] = null; break;
                case ElementType.RawImage: m_targetRawImages[objectIndex] = null; break;
                case ElementType.Button: m_targetButtons[objectIndex] = null; break;
            }

            if (int.Parse(parsedTaskDataString[DURATION_INDEX]) != -1) m_durations[int.Parse(parsedTaskDataString[DURATION_INDEX])] = -1;
            if (int.Parse(parsedTaskDataString[PIXEL_OFFSET_INDEX]) != -1) m_pixelOffsets[int.Parse(parsedTaskDataString[PIXEL_OFFSET_INDEX])] = -1;
            if (int.Parse(parsedTaskDataString[START_TIME_INDEX]) != -1) m_startTimes[int.Parse(parsedTaskDataString[START_TIME_INDEX])] = -1;
            if (int.Parse(parsedTaskDataString[TIME_OUT_INDEX]) != -1) m_timeoutTimes[int.Parse(parsedTaskDataString[TIME_OUT_INDEX])] = -1;
            if (int.Parse(parsedTaskDataString[START_LOCATION_INDEX]) != -1) m_startLocations[int.Parse(parsedTaskDataString[START_LOCATION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_ROTATION_INDEX]) != -1) m_startRotations[int.Parse(parsedTaskDataString[START_ROTATION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_SCALE_INDEX]) != -1) m_startScales[int.Parse(parsedTaskDataString[START_SCALE_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[START_COLOR_INDEX]) != -1) m_startColors[int.Parse(parsedTaskDataString[START_COLOR_INDEX])] = ColorUtils.GetInvalidColor();
            if (int.Parse(parsedTaskDataString[TRANSITION_TYPE_INDEX]) != -1) m_transitionTypes[int.Parse(parsedTaskDataString[TRANSITION_TYPE_INDEX])] = TransitionType.None;
            if (int.Parse(parsedTaskDataString[ANIMATION_MODE_INDEX]) != -1) m_animationModes[int.Parse(parsedTaskDataString[ANIMATION_MODE_INDEX])] = AnimationMode.None;
            if (int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX]) != -1) m_elementTypes[int.Parse(parsedTaskDataString[ELEMENT_TYPE_INDEX])] = ElementType.None;
            if (int.Parse(parsedTaskDataString[TARGET_POINT_INDEX]) != -1) m_targetPoints[int.Parse(parsedTaskDataString[TARGET_POINT_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_ROTATION_INDEX]) != -1) m_targetRotations[int.Parse(parsedTaskDataString[TARGET_ROTATION_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_SCALE_INDEX]) != -1) m_targetScales[int.Parse(parsedTaskDataString[TARGET_SCALE_INDEX])] = Vector3.positiveInfinity;
            if (int.Parse(parsedTaskDataString[TARGET_COLOR_INDEX]) != -1) m_targetColors[int.Parse(parsedTaskDataString[TARGET_COLOR_INDEX])] = ColorUtils.GetInvalidColor();

            m_currentTasks[taskIndex] = null;
        }
    }
}
