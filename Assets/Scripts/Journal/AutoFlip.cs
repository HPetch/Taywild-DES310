using UnityEngine;

namespace Journal
{
    [RequireComponent(typeof(JournalController))]
    public class AutoFlip : MonoBehaviour
    {
        [SerializeField] private FlipMode flipMode;
        private JournalController journal;

        [SerializeField] private float siblgePageFlipTime = 0.4f;
        [SerializeField] private float multiPageFlipTime = 0.2f;

        private bool flippingStarted = false;
        private bool isPageFlipping = false;
        private float nextPageCountDown = 0;
        private int targetPaper = 0;

        // Use this for initialization
        void Awake()
        {
            journal = GetComponent<JournalController>();
        }

        public void FlipRightPage()
        {
            if (isPageFlipping) return;
            if (journal.CurrentPaper >= journal.papers.Length) return;
            isPageFlipping = true;
            PageFlipper.FlipPage(journal, siblgePageFlipTime, FlipMode.RightToLeft, () => { isPageFlipping = false; });
        }

        public void FlipLeftPage()
        {
            if (isPageFlipping) return;
            if (journal.CurrentPaper <= 0) return;
            isPageFlipping = true;
            PageFlipper.FlipPage(journal, siblgePageFlipTime, FlipMode.LeftToRight, () => { isPageFlipping = false; });
        }

        public void StartFlipping(int target)
        {
            flippingStarted = true;
            nextPageCountDown = 0;
            targetPaper = target;

            if (target > journal.CurrentPaper) flipMode = FlipMode.RightToLeft;
            else if (target < journal.currentPaper) flipMode = FlipMode.LeftToRight;
        }

        void Update()
        {
            if (flippingStarted)
            {
                if (nextPageCountDown < 0)
                {
                    if ((journal.CurrentPaper < targetPaper &&
                        flipMode == FlipMode.RightToLeft) ||
                        (journal.CurrentPaper > targetPaper &&
                        flipMode == FlipMode.LeftToRight))
                    {
                        isPageFlipping = true;
                        PageFlipper.FlipPage(journal, multiPageFlipTime, flipMode, () => { isPageFlipping = false; });
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
    }
}