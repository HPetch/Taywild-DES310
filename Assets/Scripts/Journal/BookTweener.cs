using UnityEngine;
using System;

namespace Journal
{
    public class BookTweener : MonoBehaviour
    {
        private Vector3 from, to = Vector3.zero;
        private float duration = 0;
        private Action<Vector3> update;
        private Action finish;
        private float elapsedtime = 0;
        private bool isWorking = false;

        public static Vector3 ValueTo(GameObject _obj,
            Vector3 _from, Vector3 _to, float _duration,
            Action<Vector3> _update = null, Action _finish = null)
        {
            BookTweener tween = _obj.GetComponent<BookTweener>();
            if (!tween) tween = _obj.AddComponent<BookTweener>();

            tween.elapsedtime = 0;
            tween.isWorking = true;
            tween.enabled = true;
            tween.from = _from;
            tween.to = _to;
            tween.duration = _duration;
            tween.update = _update;
            tween.finish = _finish;
            return Vector3.zero;
        }
        static Vector3 QuadOut(Vector3 _start, Vector3 _end, float _duration, float _elapsedTime)
        {
            if (_elapsedTime >= _duration)
                return _end;
            else
            {
                return (_elapsedTime / _duration) * (_elapsedTime / _duration - 2) * -(_end - _start) + _start;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isWorking)
            {
                elapsedtime += Time.deltaTime;
                Vector3 value = QuadOut(from, to, duration, elapsedtime);
                update?.Invoke(value);
                if (elapsedtime >= duration)
                {
                    isWorking = false;
                    enabled = false;
                    finish?.Invoke();
                }
            }
        }
    }
}