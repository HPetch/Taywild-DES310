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
        private AutoFlip autoFlipper = null;


        [SerializeField] private RectTransform bookPanel;
        [SerializeField] private Image clippingPlane;

        public RectTransform leftPageTransform;
        public RectTransform rightPageTransform;

        [HideInInspector] public int currentPaper = 0;

        [HideInInspector] public Paper[] papers;

        [HideInInspector] public int StartFlippingPaper = 0;
        [HideInInspector] public int EndFlippingPaper = 1;
        
        public bool interactable = true;

        private Canvas canvas;

        private Image Left;
        private Image Right;

        //current flip mode
        private FlipMode mode;

        /// <summary>
        /// this value should e true while the user darg the page
        /// </summary>
        private bool pageDragging = false;

        /// <summary>
        /// should be true when the page tween forward or backward after release
        /// </summary>
        private bool tweening = false;

        /// <summary>
        /// OnFlip invocation list, called when any page flipped
        /// </summary>
        public UnityEvent OnFlip;

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

        public Vector3 EndBottomLeft
        {
            get { return ebl; }
        }

        public Vector3 EndBottomRight
        {
            get { return ebr; }
        }

        // Use this for initialization
        void Start()
        {
            autoFlipper = GetComponent<AutoFlip>();
            canvas = GetComponentInParent<Canvas>();

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
        public Vector3 transformPoint(Vector3 global)
        {
            Vector2 localPos = bookPanel.InverseTransformPoint(global);
            return localPos;
        }
        /// <summary>
        /// transform mouse position to local space
        /// </summary>
        /// <param name="mouseScreenPos"></param>
        /// <returns></returns>
        public Vector3 transformPointMousePosition(Vector3 mouseScreenPos)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 mouseWorldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, canvas.planeDistance));
                Vector2 localPos = bookPanel.InverseTransformPoint(mouseWorldPos);

                return localPos;
            }
            else if (canvas.renderMode == RenderMode.WorldSpace)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 globalEBR = transform.TransformPoint(ebr);
                Vector3 globalEBL = transform.TransformPoint(ebl);
                Vector3 globalSt = transform.TransformPoint(st);
                Plane p = new Plane(globalEBR, globalEBL, globalSt);
                float distance;
                p.Raycast(ray, out distance);
                Vector2 localPos = bookPanel.InverseTransformPoint(ray.GetPoint(distance));
                return localPos;
            }
            else
            {
                //Screen Space Overlay
                Vector2 localPos = bookPanel.InverseTransformPoint(mouseScreenPos);
                return localPos;
            }

        }

        /// <summary>
        /// Update page orders
        /// This function should be called whenever the current page changed, the dragging of the page started or the page has been flipped
        /// </summary>
        public void UpdatePages()
        {
            int previousPaper = pageDragging ? currentPaper - 2 : currentPaper - 1;

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


        //mouse interaction events call back
        public void OnMouseDragRightPage()
        {
            if (interactable && !tweening)
            {

                DragRightPageToPoint(transformPointMousePosition(Input.mousePosition));
            }

        }
        public void DragRightPageToPoint(Vector3 point)
        {
            if (currentPaper > EndFlippingPaper) return;
            pageDragging = true;
            mode = FlipMode.RightToLeft;
            f = point;

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

            UpdateBookRTLToPoint(f);
        }
        public void OnMouseDragLeftPage()
        {
            if (interactable && !tweening)
            {
                DragLeftPageToPoint(transformPointMousePosition(Input.mousePosition));

            }

        }
        public void DragLeftPageToPoint(Vector3 point)
        {
            if (currentPaper <= StartFlippingPaper) return;
            pageDragging = true;
            mode = FlipMode.LeftToRight;
            f = point;

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
            UpdateBookLTRToPoint(f);
        }
        public void OnMouseRelease()
        {
            if (interactable)
                ReleasePage();
        }
        public void ReleasePage()
        {
            if (pageDragging)
            {
                pageDragging = false;
                float distanceToLeft = Vector2.Distance(c, ebl);
                float distanceToRight = Vector2.Distance(c, ebr);
                if (distanceToRight < distanceToLeft && mode == FlipMode.RightToLeft)
                    TweenBack();
                else if (distanceToRight > distanceToLeft && mode == FlipMode.LeftToRight)
                    TweenBack();
                else
                    TweenForward();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) FlipToPage(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) FlipToPage(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) FlipToPage(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) FlipToPage(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) FlipToPage(5);

            if (pageDragging && interactable)
            {
                UpdateBook();
            }
        }
        public void UpdateBook()
        {
            f = Vector3.Lerp(f, transformPointMousePosition(Input.mousePosition), Time.deltaTime * 10);
            if (mode == FlipMode.RightToLeft)
                UpdateBookRTLToPoint(f);
            else
                UpdateBookLTRToPoint(f);
        }

        public void FlipToPage(int _page)
        {
            if (_page < 0) _page = 0;
            if (_page > papers.Length * 2) _page = papers.Length * 2 - 1;

            autoFlipper.StartFlipping((_page + 1) / 2);
        }

        /// <summary>
        /// This function called when the page dragging point reached its distenation after releasing the mouse
        /// This function will call the OnFlip invocation list
        /// if you need to call any fnction after the page flipped just add it to the OnFlip invocation list
        /// </summary>
        public void Flip()
        {
            pageDragging = false;

            if (mode == FlipMode.LeftToRight)
                currentPaper -= 1;
            //Debug.Log(currentPaper);
            Left.transform.SetParent(bookPanel.transform, true);
            Left.rectTransform.pivot = new Vector2(0, 0);
            Right.transform.SetParent(bookPanel.transform, true);
            UpdatePages();

            clippingPlane.gameObject.SetActive(false);
            if (OnFlip != null)
                OnFlip.Invoke();
        }

        public void TweenForward()
        {
            if (mode == FlipMode.RightToLeft)
            {
                tweening = true;
                BookTweener.ValueTo(gameObject, f, ebl * 0.98f, 0.3f, TweenUpdate, () =>
                {
                    Flip();
                    tweening = false;
                });
            }
            else
            {
                tweening = true;
                BookTweener.ValueTo(gameObject, f, ebr * 0.98f, 0.3f, TweenUpdate, () =>
                {
                    Flip();
                    tweening = false;
                });
            }
        }
        void TweenUpdate(Vector3 _follow)
        {
            if (mode == FlipMode.RightToLeft)
                UpdateBookRTLToPoint(_follow);
            else
                UpdateBookLTRToPoint(_follow);
        }

        public void TweenBack()
        {
            if (mode == FlipMode.RightToLeft)
            {
                tweening = true;
                BookTweener.ValueTo(gameObject, f, ebr * 0.98f, 0.3f, TweenUpdate, () =>
                {
                    currentPaper -= 1;
                    Right.transform.SetParent(bookPanel.transform);
                    Left.transform.SetParent(bookPanel.transform);
                tweening = false;
                    UpdatePages();
                });
            }
            else
            {
                tweening = true;
                BookTweener.ValueTo(gameObject, f, ebl * 0.98f, 0.3f, TweenUpdate, () =>
                {
                    Left.transform.SetParent(bookPanel.transform);
                    Right.transform.SetParent(bookPanel.transform);
                tweening = false;

                    UpdatePages();
                });
            }
        }

        #region Page Curl Internal Calculations
        //for more info about this part please check this link : http://rbarraza.com/html5-canvas-pageflip/

        float radius1, radius2;
        //Spine Bottom
        Vector3 sb;
        //Spine Top
        Vector3 st;
        //corner of the page
        Vector3 c;
        //Edge Bottom Right
        Vector3 ebr;
        //Edge Bottom Left
        Vector3 ebl;
        //follow point 
        Vector3 f;

        private void CalcCurlCriticalPoints()
        {
            sb = new Vector3(0, -bookPanel.rect.height / 2);
            ebr = new Vector3(bookPanel.rect.width / 2, -bookPanel.rect.height / 2);
            ebl = new Vector3(-bookPanel.rect.width / 2, -bookPanel.rect.height / 2);
            st = new Vector3(0, bookPanel.rect.height / 2);
            radius1 = Vector2.Distance(sb, ebr);
            float pageWidth = bookPanel.rect.width / 2.0f;
            float pageHeight = bookPanel.rect.height;
            radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
        }
        public void UpdateBookRTLToPoint(Vector3 followLocation)
        {
            mode = FlipMode.RightToLeft;
            f = followLocation;

            Right.transform.SetParent(clippingPlane.transform, true);

            Left.transform.SetParent(bookPanel.transform, true);
            c = Calc_C_Position(followLocation);
            Vector3 t1;
            float T0_T1_Angle = Calc_T0_T1_Angle(c, ebr, out t1);
            if (T0_T1_Angle >= -90) T0_T1_Angle -= 180;

            clippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
            clippingPlane.transform.localEulerAngles = new Vector3(0, 0, T0_T1_Angle + 90);
            clippingPlane.transform.position = bookPanel.TransformPoint(t1);

            //page position and angle
            Right.transform.position = bookPanel.TransformPoint(c);
            float C_T1_dy = t1.y - c.y;
            float C_T1_dx = t1.x - c.x;
            float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
            Right.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - (T0_T1_Angle + 90));

            Left.transform.SetParent(clippingPlane.transform, true);
            Left.transform.SetAsFirstSibling();
        }
        public void UpdateBookLTRToPoint(Vector3 followLocation)
        {
            mode = FlipMode.LeftToRight;
            f = followLocation;

            Left.transform.SetParent(clippingPlane.transform, true);
            Right.transform.SetParent(bookPanel.transform, true);

            c = Calc_C_Position(followLocation);
            Vector3 t1;
            float T0_T1_Angle = Calc_T0_T1_Angle(c, ebl, out t1);
            if (T0_T1_Angle < 0) T0_T1_Angle += 180;

            clippingPlane.transform.localEulerAngles = new Vector3(0, 0, T0_T1_Angle - 90);
            clippingPlane.transform.position = bookPanel.TransformPoint(t1);

            //page position and angle
            Left.transform.position = bookPanel.TransformPoint(c);
            float C_T1_dy = t1.y - c.y;
            float C_T1_dx = t1.x - c.x;
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
            T1_X = normalizeT1X(T1_X, bookCorner, sb);
            t1 = new Vector3(T1_X, sb.y, 0);
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
            f = followLocation;
            float F_SB_dy = f.y - sb.y;
            float F_SB_dx = f.x - sb.x;
            float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
            Vector3 r1 = new Vector3(radius1 * Mathf.Cos(F_SB_Angle), radius1 * Mathf.Sin(F_SB_Angle), 0) + sb;

            float F_SB_distance = Vector2.Distance(f, sb);
            if (F_SB_distance < radius1)
                c = f;
            else
                c = r1;
            float F_ST_dy = c.y - st.y;
            float F_ST_dx = c.x - st.x;
            float F_ST_Angle = Mathf.Atan2(F_ST_dy, F_ST_dx);
            Vector3 r2 = new Vector3(radius2 * Mathf.Cos(F_ST_Angle),
               radius2 * Mathf.Sin(F_ST_Angle), 0) + st;
            float C_ST_distance = Vector2.Distance(c, st);
            if (C_ST_distance > radius2)
                c = r2;
            return c;
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
            CanvasGroup cgf = _page.GetComponent<CanvasGroup>();
            cgf.alpha = 1;
            cgf.blocksRaycasts = true;
        }

        /// <summary>
        /// Call this function to hide any page
        /// </summary>
        /// <param name="_page">the page to be hidden</param>
        public static void HidePage(GameObject _page)
        {
            CanvasGroup cgf = _page.GetComponent<CanvasGroup>();
            cgf.alpha = 0;
            cgf.blocksRaycasts = false;
            _page.transform.SetAsFirstSibling();
        }

        public static void CopyTransform(Transform _from, Transform _to)
        {
            _to.SetPositionAndRotation(_from.position, _from.rotation);
            _to.localScale = _from.localScale;
        }
    }
}