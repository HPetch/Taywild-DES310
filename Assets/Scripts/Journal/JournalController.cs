using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace Journal
{
    public enum FlipMode
    {
        RightToLeft,
        LeftToRight
    }

    public class JournalController : MonoBehaviour
    {
        public static JournalController Instance { get; private set; }

        [SerializeField] private RectTransform bookPanel;
        [SerializeField] private Image clippingPlane;

        public RectTransform leftPageTransform;
        public RectTransform rightPageTransform;

        [HideInInspector] public int currentPaper = 0;

        [HideInInspector] public Paper[] papers;

        [HideInInspector] public int StartFlippingPaper = 0;
        [HideInInspector] public int EndFlippingPaper = 1;
        
        public bool interactable = true;

        [SerializeField] private float siblgePageFlipTime = 0.4f;
        [SerializeField] private float multiPageFlipTime = 0.2f;

        private bool flippingStarted = false;
        private bool isPageFlipping = false;
        private float nextPageCountDown = 0;
        private int targetPaper = 0;

        private Image Left;
        private Image Right;

        //current flip mode
        private FlipMode flipMode;

        /// <summary>
        /// OnFlip invocation list, called when any page flipped
        /// </summary>
        public UnityEvent OnFlip;

        void Awake()
        {
            // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
            if (Instance != null) Destroy(gameObject);
            else Instance = this;

            UpdatePages();

            CalcCurlCriticalPoints();

            float pageWidth = bookPanel.rect.width / 2.0f;
            float pageHeight = bookPanel.rect.height;

            clippingPlane.rectTransform.sizeDelta = new Vector2(pageWidth * 2 + pageHeight, pageHeight + pageHeight * 2);
        }

        /// <summary>
        /// transform point from global (world-space) to local space
        /// </summary>
        /// <param name="global">poit iin world space</param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 global)
        {
            Vector2 localPos = bookPanel.InverseTransformPoint(global);
            return localPos;
        }

        /// <summary>
        /// Update page orders
        /// This function should be called whenever the current page changed, or the page has been flipped
        /// </summary>
        public void UpdatePages()
        {
            int previousPaper = currentPaper - 1;

            //Hide all pages
            for (int i = 0; i < papers.Length; i++)
            {
                BookUtility.HidePage(papers[i].Front);
                papers[i].Front.transform.SetParent(bookPanel.transform);
                BookUtility.HidePage(papers[i].Back);
                papers[i].Back.transform.SetParent(bookPanel.transform);
            }

            //Show the back page of all previous papers
            for (int i = 0; i <= previousPaper; i++)
            {
                BookUtility.ShowPage(papers[i].Back);
                papers[i].Back.transform.SetParent(bookPanel.transform);
                papers[i].Back.transform.SetSiblingIndex(i);
                BookUtility.CopyTransform(leftPageTransform.transform, papers[i].Back.transform);
            }

            //Show the front page of all next papers
            for (int i = papers.Length - 1; i >= currentPaper; i--)
            {
                BookUtility.ShowPage(papers[i].Front);
                papers[i].Front.transform.SetSiblingIndex(papers.Length - i + previousPaper);
                BookUtility.CopyTransform(rightPageTransform.transform, papers[i].Front.transform);
            }
        }

        public void DragRightPageToPoint(Vector3 point)
        {
            if (currentPaper > EndFlippingPaper) return;
            flipMode = FlipMode.RightToLeft;
            followLocation = point;

            clippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
            currentPaper += 1;

            UpdatePages();

            Left = papers[currentPaper - 1].Front.GetComponent<Image>();
            BookUtility.ShowPage(Left.gameObject);
            Left.rectTransform.pivot = new Vector2(0, 0);
            Left.transform.position = rightPageTransform.transform.position;
            Left.transform.localEulerAngles = new Vector3(0, 0, 0);

            Right = papers[currentPaper - 1].Back.GetComponent<Image>();
            BookUtility.ShowPage(Right.gameObject);
            Right.transform.position = rightPageTransform.transform.position;
            Right.transform.localEulerAngles = new Vector3(0, 0, 0);

            clippingPlane.gameObject.SetActive(true);

            UpdateBookRTLToPoint(followLocation);
        }

        public void DragLeftPageToPoint(Vector3 point)
        {
            if (currentPaper <= StartFlippingPaper) return;
            flipMode = FlipMode.LeftToRight;
            followLocation = point;

            UpdatePages();

            clippingPlane.rectTransform.pivot = new Vector2(0, 0.35f);

            Right = papers[currentPaper - 1].Back.GetComponent<Image>();
            BookUtility.ShowPage(Right.gameObject);
            Right.transform.position = leftPageTransform.transform.position;
            Right.transform.localEulerAngles = new Vector3(0, 0, 0);
            Right.transform.SetAsFirstSibling();

            Left = papers[currentPaper - 1].Front.GetComponent<Image>();
            BookUtility.ShowPage(Left.gameObject);
            Left.gameObject.SetActive(true);
            Left.rectTransform.pivot = new Vector2(1, 0);
            Left.transform.position = leftPageTransform.transform.position;
            Left.transform.localEulerAngles = new Vector3(0, 0, 0);

            clippingPlane.gameObject.SetActive(true);
            UpdateBookLTRToPoint(followLocation);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) FlipToPage(0);
            if (Input.GetKeyDown(KeyCode.Alpha1)) FlipToPage(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) FlipToPage(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) FlipToPage(3);

            if (flippingStarted)
            {
                if (nextPageCountDown < 0)
                {
                    if ((CurrentPaper < targetPaper &&
                        flipMode == FlipMode.RightToLeft) ||
                        (CurrentPaper > targetPaper &&
                        flipMode == FlipMode.LeftToRight))
                    {
                        isPageFlipping = true;
                        PageFlipper.FlipPage(this, multiPageFlipTime, flipMode, () => { isPageFlipping = false; });
                    }
                    else
                    {
                        flippingStarted = false;
                    }

                    nextPageCountDown = multiPageFlipTime + Time.deltaTime;
                }
                nextPageCountDown -= Time.deltaTime;
            }
        }

        #region Page FLipping
        public void FlipToPage(int _page)
        {
            _page = Math.Clamp(_page, 0, papers.Length * 2 - 1);

            if (_page - currentPaper == 1)
            {
                FlipRightPage();
                return;
            }

            if (_page - currentPaper == -1)
            {
                FlipLeftPage();
                return;
            }

            // Multi flip
            StartFlipping(_page);
        }

        /// <summary>
        /// This function will call the OnFlip invocation list
        /// </summary>
        public void Flip()
        {
            if (flipMode == FlipMode.LeftToRight)
                currentPaper -= 1;

            Left.transform.SetParent(bookPanel.transform, true);
            Left.rectTransform.pivot = new Vector2(0, 0);
            Right.transform.SetParent(bookPanel.transform, true);
            UpdatePages();

            clippingPlane.gameObject.SetActive(false);
            OnFlip?.Invoke();
        }

        private void StartFlipping(int target)
        {
            flippingStarted = true;
            nextPageCountDown = 0;
            targetPaper = target;

            if (target > CurrentPaper) flipMode = FlipMode.RightToLeft;
            else if (target < currentPaper) flipMode = FlipMode.LeftToRight;
        }

        /// <summary>
        /// Called by UI, flips page right
        /// </summary>
        public void FlipRightPage()
        {
            if (isPageFlipping) return;
            if (CurrentPaper >= papers.Length) return;
            isPageFlipping = true;
            PageFlipper.FlipPage(this, siblgePageFlipTime, FlipMode.RightToLeft, () => { isPageFlipping = false; });
        }


        /// <summary>
        /// Called by UI, flips page left
        /// </summary>
        public void FlipLeftPage()
        {
            if (isPageFlipping) return;
            if (CurrentPaper <= 0) return;
            isPageFlipping = true;
            PageFlipper.FlipPage(this, siblgePageFlipTime, FlipMode.LeftToRight, () => { isPageFlipping = false; });
        }
        #endregion

        #region Page Curl Internal Calculations
        //for more info about this part please check this link : http://rbarraza.com/html5-canvas-pageflip/
        public Vector3 EdgeBottomRight { get; private set; }
        public Vector3 EdgeBottomLeft { get; private set; }

        private float radius1, radius2;
        private Vector3 spineBottom;
        private Vector3 spineTop;
        private Vector3 pageCorner;
        private Vector3 followLocation;

        private void CalcCurlCriticalPoints()
        {
            spineBottom = new Vector3(0, -bookPanel.rect.height / 2);
            EdgeBottomRight = new Vector3(bookPanel.rect.width / 2, -bookPanel.rect.height / 2);
            EdgeBottomLeft = new Vector3(-bookPanel.rect.width / 2, -bookPanel.rect.height / 2);
            spineTop = new Vector3(0, bookPanel.rect.height / 2);
            radius1 = Vector2.Distance(spineBottom, EdgeBottomRight);
            float pageWidth = bookPanel.rect.width / 2.0f;
            float pageHeight = bookPanel.rect.height;
            radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
        }

        public void UpdateBookRTLToPoint(Vector3 followLocation)
        {
            flipMode = FlipMode.RightToLeft;
            this.followLocation = followLocation;

            Right.transform.SetParent(clippingPlane.transform, true);

            Left.transform.SetParent(bookPanel.transform, true);
            pageCorner = Calc_C_Position(followLocation);
            Vector3 t1;
            float T0_T1_Angle = Calc_T0_T1_Angle(pageCorner, EdgeBottomRight, out t1);
            if (T0_T1_Angle >= -90) T0_T1_Angle -= 180;

            clippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
            clippingPlane.transform.localEulerAngles = new Vector3(0, 0, T0_T1_Angle + 90);
            clippingPlane.transform.position = bookPanel.TransformPoint(t1);

            //page position and angle
            Right.transform.position = bookPanel.TransformPoint(pageCorner);
            float C_T1_dy = t1.y - pageCorner.y;
            float C_T1_dx = t1.x - pageCorner.x;
            float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
            Right.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - (T0_T1_Angle + 90));

            Left.transform.SetParent(clippingPlane.transform, true);
            Left.transform.SetAsFirstSibling();
        }
        public void UpdateBookLTRToPoint(Vector3 followLocation)
        {
            flipMode = FlipMode.LeftToRight;
            this.followLocation = followLocation;

            Left.transform.SetParent(clippingPlane.transform, true);
            Right.transform.SetParent(bookPanel.transform, true);

            pageCorner = Calc_C_Position(followLocation);
            Vector3 t1;
            float T0_T1_Angle = Calc_T0_T1_Angle(pageCorner, EdgeBottomLeft, out t1);
            if (T0_T1_Angle < 0) T0_T1_Angle += 180;

            clippingPlane.transform.localEulerAngles = new Vector3(0, 0, T0_T1_Angle - 90);
            clippingPlane.transform.position = bookPanel.TransformPoint(t1);

            //page position and angle
            Left.transform.position = bookPanel.TransformPoint(pageCorner);
            float C_T1_dy = t1.y - pageCorner.y;
            float C_T1_dx = t1.x - pageCorner.x;
            float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
            Left.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - 180 - (T0_T1_Angle - 90));

            Right.transform.SetParent(clippingPlane.transform, true);
            Right.transform.SetAsFirstSibling();

        }
        private float Calc_T0_T1_Angle(Vector3 c, Vector3 bookCorner, out Vector3 t1)
        {
            Vector3 t0 = (c + bookCorner) / 2;
            float T0_CORNER_dy = bookCorner.y - t0.y;
            float T0_CORNER_dx = bookCorner.x - t0.x;
            float T0_CORNER_Angle = Mathf.Atan2(T0_CORNER_dy, T0_CORNER_dx);
            float T0_T1_Angle = 90 - T0_CORNER_Angle;

            float T1_X = t0.x - T0_CORNER_dy * Mathf.Tan(T0_CORNER_Angle);
            T1_X = normalizeT1X(T1_X, bookCorner, spineBottom);
            t1 = new Vector3(T1_X, spineBottom.y, 0);
            ////////////////////////////////////////////////
            //clipping plane angle=T0_T1_Angle
            float T0_T1_dy = t1.y - t0.y;
            float T0_T1_dx = t1.x - t0.x;
            T0_T1_Angle = Mathf.Atan2(T0_T1_dy, T0_T1_dx) * Mathf.Rad2Deg;
            return T0_T1_Angle;
        }
        private float normalizeT1X(float t1, Vector3 corner, Vector3 sb)
        {
            if (t1 > sb.x && sb.x > corner.x)
                return sb.x;
            if (t1 < sb.x && sb.x < corner.x)
                return sb.x;
            return t1;
        }
        private Vector3 Calc_C_Position(Vector3 followLocation)
        {
            Vector3 c;
            this.followLocation = followLocation;
            float F_SB_dy = this.followLocation.y - spineBottom.y;
            float F_SB_dx = this.followLocation.x - spineBottom.x;
            float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
            Vector3 r1 = new Vector3(radius1 * Mathf.Cos(F_SB_Angle), radius1 * Mathf.Sin(F_SB_Angle), 0) + spineBottom;

            float F_SB_distance = Vector2.Distance(this.followLocation, spineBottom);
            if (F_SB_distance < radius1)
                c = this.followLocation;
            else
                c = r1;
            float F_ST_dy = c.y - spineTop.y;
            float F_ST_dx = c.x - spineTop.x;
            float F_ST_Angle = Mathf.Atan2(F_ST_dy, F_ST_dx);
            Vector3 r2 = new Vector3(radius2 * Mathf.Cos(F_ST_Angle),
               radius2 * Mathf.Sin(F_ST_Angle), 0) + spineTop;
            float C_ST_distance = Vector2.Distance(c, spineTop);
            if (C_ST_distance > radius2)
                c = r2;
            return c;
        }
        #endregion

        #region Utility
        /// <summary>
        /// The Current Shown paper (the paper its front shown in right part)
        /// </summary>
        public int CurrentPaper
        {
            get { return currentPaper; }
            set
            {
                if (value != currentPaper)
                {
                    if (value < StartFlippingPaper)
                        currentPaper = StartFlippingPaper;
                    else if (value > EndFlippingPaper + 1)
                        currentPaper = EndFlippingPaper + 1;
                    else
                        currentPaper = value;
                    UpdatePages();
                }
            }
        }
        #endregion
    }

    [Serializable]
    public class Paper
    {
        public GameObject Front;
        public GameObject Back;
    }

    public static class BookUtility
    {
        /// <summary>
        /// Call this function to Show a Hidden Page
        /// </summary>
        /// <param name="_page">the page to be shown</param>
        public static void ShowPage(GameObject _page)
        {
            CanvasGroup canvasGroup = _page.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Call this function to hide any page
        /// </summary>
        /// <param name="_page">the page to be hidden</param>
        public static void HidePage(GameObject _page)
        {
            CanvasGroup canvasGroup = _page.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            _page.transform.SetAsFirstSibling();
        }

        public static void CopyTransform(Transform _from, Transform _to)
        {
            _to.SetPositionAndRotation(_from.position, _from.rotation);
            _to.localScale = _from.localScale;
        }
    }
}