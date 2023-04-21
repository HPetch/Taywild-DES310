using UnityEngine;
using System;

namespace Journal
{
    public class PageFlipper : MonoBehaviour
    {
        public float duration = 0;
        public JournalController journal = null;

        private bool isFlipping = false;
        private Action finish;
        private float elapsedTime = 0;

        private float centerXPosition = 0;
        private float pageWidth, pageHeight = 0;

        private FlipMode flipMode;

        public static void FlipPage(JournalController _journal, float _duration, FlipMode _mode, Action _OnComplete)
        {
            PageFlipper flipper = _journal.GetComponent<PageFlipper>();

            if (!flipper)
                flipper = _journal.gameObject.AddComponent<PageFlipper>();

            flipper.enabled = true;
            flipper.journal = _journal;
            flipper.isFlipping = true;
            flipper.duration = _duration - Time.deltaTime;
            flipper.finish = _OnComplete;
            flipper.centerXPosition = (_journal.EdgeBottomLeft.x + _journal.EdgeBottomRight.x) / 2;
            flipper.pageWidth = (_journal.EdgeBottomRight.x - _journal.EdgeBottomLeft.x) / 2;
            flipper.pageHeight = Mathf.Abs(_journal.EdgeBottomRight.y);
            flipper.flipMode = _mode;
            flipper.elapsedTime = 0;
            float x;

            if (_mode == FlipMode.RightToLeft)
            {
                x = flipper.centerXPosition + (flipper.pageWidth * 0.99f);
                float y = (-flipper.pageHeight / (flipper.pageWidth * flipper.pageWidth)) * (x - flipper.centerXPosition) * (x - flipper.centerXPosition);
                _journal.DragRightPageToPoint(new Vector3(x, y, 0));
            }
            else
            {
                x = flipper.centerXPosition - (flipper.pageWidth * 0.99f);
                float y = (-flipper.pageHeight / (flipper.pageWidth * flipper.pageWidth)) * (x - flipper.centerXPosition) * (x - flipper.centerXPosition);
                _journal.DragLeftPageToPoint(new Vector3(x, y, 0));
            }            
        }

        // Update is called once per frame
        void Update()
        {
            if (isFlipping)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime < duration)
                {
                    if (flipMode == FlipMode.RightToLeft)
                    {
                        float x = centerXPosition + (0.5f - elapsedTime / duration) * 2 * (pageWidth);
                        float y = (-pageHeight / (pageWidth * pageWidth)) * (x - centerXPosition) * (x - centerXPosition);
                        journal.UpdateBookRTLToPoint(new Vector3(x, y, 0));
                    }
                    else
                    {
                        float x = centerXPosition - (0.5f - elapsedTime / duration) * 2 * (pageWidth);
                        float y = (-pageHeight / (pageWidth * pageWidth)) * (x - centerXPosition) * (x - centerXPosition);
                        journal.UpdateBookLTRToPoint(new Vector3(x, y, 0));
                    }

                }
                else
                {
                    journal.Flip();
                    isFlipping = false;
                    enabled = false;
                    finish?.Invoke();
                }
            }
        }
    }
}