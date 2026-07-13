using UnityEngine;
using UnityEngine.UI;

namespace TweenKit
{
    /// <summary>DoTween 스타일의 DO* 확장 메서드 모음.</summary>
    public static class TweenExtensions
    {
        // ─────────────────────────────────────────────────────────
        //  Transform - 이동
        // ─────────────────────────────────────────────────────────
        public static Tweener<Vector3> DOMove(this Transform t, Vector3 endValue, float duration)
            => Tw.To(() => t.position, v => t.position = v, endValue, duration);

        public static Tweener<float> DOMoveX(this Transform t, float endX, float duration)
            => Tw.To(() => t.position.x, x => { var p = t.position; p.x = x; t.position = p; }, endX, duration);

        public static Tweener<float> DOMoveY(this Transform t, float endY, float duration)
            => Tw.To(() => t.position.y, y => { var p = t.position; p.y = y; t.position = p; }, endY, duration);

        public static Tweener<float> DOMoveZ(this Transform t, float endZ, float duration)
            => Tw.To(() => t.position.z, z => { var p = t.position; p.z = z; t.position = p; }, endZ, duration);

        public static Tweener<Vector3> DOLocalMove(this Transform t, Vector3 endValue, float duration)
            => Tw.To(() => t.localPosition, v => t.localPosition = v, endValue, duration);

        public static Tweener<float> DOLocalMoveX(this Transform t, float endX, float duration)
            => Tw.To(() => t.localPosition.x, x => { var p = t.localPosition; p.x = x; t.localPosition = p; }, endX, duration);

        public static Tweener<float> DOLocalMoveY(this Transform t, float endY, float duration)
            => Tw.To(() => t.localPosition.y, y => { var p = t.localPosition; p.y = y; t.localPosition = p; }, endY, duration);

        public static Tweener<float> DOLocalMoveZ(this Transform t, float endZ, float duration)
            => Tw.To(() => t.localPosition.z, z => { var p = t.localPosition; p.z = z; t.localPosition = p; }, endZ, duration);

        // ─────────────────────────────────────────────────────────
        //  Transform - 스케일
        // ─────────────────────────────────────────────────────────
        public static Tweener<Vector3> DOScale(this Transform t, Vector3 endValue, float duration)
            => Tw.To(() => t.localScale, v => t.localScale = v, endValue, duration);

        public static Tweener<Vector3> DOScale(this Transform t, float endValue, float duration)
            => Tw.To(() => t.localScale, v => t.localScale = v, Vector3.one * endValue, duration);

        public static Tweener<float> DOScaleX(this Transform t, float endX, float duration)
            => Tw.To(() => t.localScale.x, x => { var s = t.localScale; s.x = x; t.localScale = s; }, endX, duration);

        public static Tweener<float> DOScaleY(this Transform t, float endY, float duration)
            => Tw.To(() => t.localScale.y, y => { var s = t.localScale; s.y = y; t.localScale = s; }, endY, duration);

        public static Tweener<float> DOScaleZ(this Transform t, float endZ, float duration)
            => Tw.To(() => t.localScale.z, z => { var s = t.localScale; s.z = z; t.localScale = s; }, endZ, duration);

        // ─────────────────────────────────────────────────────────
        //  Transform - 회전 (오일러각 보간 → 여러 바퀴 회전 가능)
        // ─────────────────────────────────────────────────────────
        public static Tweener<Vector3> DORotate(this Transform t, Vector3 endEuler, float duration)
            => Tw.To(() => t.eulerAngles, v => t.eulerAngles = v, endEuler, duration);

        public static Tweener<Vector3> DOLocalRotate(this Transform t, Vector3 endEuler, float duration)
            => Tw.To(() => t.localEulerAngles, v => t.localEulerAngles = v, endEuler, duration);

        public static Tweener<Quaternion> DORotateQuaternion(this Transform t, Quaternion endValue, float duration)
            => Tw.To(() => t.rotation, v => t.rotation = v, endValue, duration);

        // ─────────────────────────────────────────────────────────
        //  RectTransform (UI)
        // ─────────────────────────────────────────────────────────
        public static Tweener<Vector2> DOAnchorPos(this RectTransform rt, Vector2 endValue, float duration)
            => Tw.To(() => rt.anchoredPosition, v => rt.anchoredPosition = v, endValue, duration);

        public static Tweener<float> DOAnchorPosX(this RectTransform rt, float endX, float duration)
            => Tw.To(() => rt.anchoredPosition.x, x => { var p = rt.anchoredPosition; p.x = x; rt.anchoredPosition = p; }, endX, duration);

        public static Tweener<float> DOAnchorPosY(this RectTransform rt, float endY, float duration)
            => Tw.To(() => rt.anchoredPosition.y, y => { var p = rt.anchoredPosition; p.y = y; rt.anchoredPosition = p; }, endY, duration);

        public static Tweener<Vector2> DOSizeDelta(this RectTransform rt, Vector2 endValue, float duration)
            => Tw.To(() => rt.sizeDelta, v => rt.sizeDelta = v, endValue, duration);

        // ─────────────────────────────────────────────────────────
        //  CanvasGroup / SpriteRenderer / UI.Graphic - 색·페이드
        // ─────────────────────────────────────────────────────────
        public static Tweener<float> DOFade(this CanvasGroup cg, float endAlpha, float duration)
            => Tw.To(() => cg.alpha, a => cg.alpha = a, endAlpha, duration);

        public static Tweener<Color> DOColor(this SpriteRenderer sr, Color endValue, float duration)
            => Tw.To(() => sr.color, c => sr.color = c, endValue, duration);

        public static Tweener<float> DOFade(this SpriteRenderer sr, float endAlpha, float duration)
            => Tw.To(() => sr.color.a, a => { var c = sr.color; c.a = a; sr.color = c; }, endAlpha, duration);

        public static Tweener<Color> DOColor(this Graphic g, Color endValue, float duration)
            => Tw.To(() => g.color, c => g.color = c, endValue, duration);

        public static Tweener<float> DOFade(this Graphic g, float endAlpha, float duration)
            => Tw.To(() => g.color.a, a => { var c = g.color; c.a = a; g.color = c; }, endAlpha, duration);

        // ─────────────────────────────────────────────────────────
        //  Material
        // ─────────────────────────────────────────────────────────
        public static Tweener<Color> DOColor(this Material m, Color endValue, float duration)
            => Tw.To(() => m.color, c => m.color = c, endValue, duration);

        public static Tweener<Color> DOColor(this Material m, Color endValue, string property, float duration)
            => Tw.To(() => m.GetColor(property), c => m.SetColor(property, c), endValue, duration);

        public static Tweener<float> DOFloat(this Material m, float endValue, string property, float duration)
            => Tw.To(() => m.GetFloat(property), v => m.SetFloat(property, v), endValue, duration);

        // ─────────────────────────────────────────────────────────
        //  Camera
        // ─────────────────────────────────────────────────────────
        public static Tweener<float> DOFieldOfView(this Camera cam, float endValue, float duration)
            => Tw.To(() => cam.fieldOfView, v => cam.fieldOfView = v, endValue, duration);

        public static Tweener<float> DOOrthoSize(this Camera cam, float endValue, float duration)
            => Tw.To(() => cam.orthographicSize, v => cam.orthographicSize = v, endValue, duration);

        public static Tweener<Color> DOColor(this Camera cam, Color endValue, float duration)
            => Tw.To(() => cam.backgroundColor, c => cam.backgroundColor = c, endValue, duration);

        // ─────────────────────────────────────────────────────────
        //  AudioSource / Light
        // ─────────────────────────────────────────────────────────
        public static Tweener<float> DOFade(this AudioSource src, float endVolume, float duration)
            => Tw.To(() => src.volume, v => src.volume = v, endVolume, duration);

        public static Tweener<float> DOPitch(this AudioSource src, float endPitch, float duration)
            => Tw.To(() => src.pitch, v => src.pitch = v, endPitch, duration);

        public static Tweener<Color> DOColor(this Light light, Color endValue, float duration)
            => Tw.To(() => light.color, c => light.color = c, endValue, duration);

        public static Tweener<float> DOIntensity(this Light light, float endValue, float duration)
            => Tw.To(() => light.intensity, v => light.intensity = v, endValue, duration);
    }
}
