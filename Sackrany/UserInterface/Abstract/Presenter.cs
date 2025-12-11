using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Sackrany.UserInterface.Abstract
{
    public abstract class Presenter
    {        
        public List<IView> Views = new();

        public Canvas Canvas;
        public Transform Parent;

        private Vector2 _actualResolution;
        public Model model;

        private protected virtual void OnDestroy() { }
        
        public bool IsActive { get; private set; } = true;

        private float _worldAspectRatio;
        
        public Vector2 GetActualResolution => _actualResolution;
        public float GetActualAspectCoef => _actualResolution.x / _actualResolution.y;
        public float GetWorldAspect => _worldAspectRatio;
        
        public void Initialize(Model model, Transform parent)
        {
            this.model = model;
            Parent = ParentCreate(parent);
            model.Events.OnDestroyUI += OnDestroy;
            Awake();
        }
        
        private protected virtual Transform ParentCreate(Transform original_parent)
        {
            if (original_parent == null)
            {
                DeactivatePresenter("(from instantiate) Original Parent");
                return null;
            }

            var g = new GameObject(string.Format("{0}-{1} [{2}]",
                GetType().BaseType?.Name,
                GetType().Name,
                original_parent.childCount)
            );

            g.transform.SetParent(original_parent, false);
            g.transform.localPosition = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            return g.transform;
        }
        
        /// <summary>
        /// Only initialize shit
        /// </summary>
        private protected virtual void Awake()
        {
            Canvas = CreateScreenSpaceCanvas();
        }
        
        public Canvas CreateRawCanvas()
        {
            var obj = model.Prefabs.GetActualGameObject("Canvas", Parent);
            if (obj == null)
            {
                DeactivatePresenter("Canvas", obj);
                return null;
            }

            var canvas = obj.GetComponent<Canvas>();

            canvas.worldCamera = model.UICamera;
            updateActualResolution(canvas);
            return canvas;
        }

        public Canvas CreateScreenSpaceCanvas(float matchWidthOrHeight = 1)
        {
            var ret = CreateRawCanvas();
            if (ret == null)
                return null;

            ret.renderMode = RenderMode.ScreenSpaceCamera;
            ret.planeDistance = Model.BaseScreenCanvasDistance - model.Presenters
                .Where(x => x.Canvas != null)
                .Count(x => x.Canvas.renderMode == RenderMode.ScreenSpaceCamera) / 200f - 0.01f;
            ret.sortingOrder = model.Presenters
                .Where(x => x.Canvas != null)
                .Count(x => x.Canvas.renderMode == RenderMode.ScreenSpaceCamera);

            var scaler = ret.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Model.ReferenceResolution;
            if (matchWidthOrHeight >= 0)
                scaler.matchWidthOrHeight = matchWidthOrHeight;
            else
                scaler.matchWidthOrHeight = model.UICamera.pixelWidth > model.UICamera.pixelHeight ? 1 : 0;

            updateActualResolution(ret);
            return ret;
        }

        private void updateActualResolution(Canvas canvas)
        {
            _actualResolution = canvas.ActualSize(model.UICamera);
            _worldAspectRatio = _actualResolution.x / Screen.width;
        }

        public GameObject CreateCustomPrefab(
            string name,
            bool initialize_views = true,
            bool put_inside_canvas = true)
        {
            var obj = model.Prefabs.GetActualGameObject(name, Parent);
            if (obj == null)
            {
                DeactivatePresenter("null", obj);
                return null;
            }
            if (put_inside_canvas)
                obj.transform.SetParent(Canvas.transform, false);
            if (initialize_views)
                foreach (var a in GetAllViewsFromGameObject(obj))
                    a.Initialize(this);

            return obj;
        }

        private protected IView[] GetAllViewsFromGameObject(GameObject go)
        {
            return go.GetComponentsInChildren<IView>();
        }

        private protected void DeactivatePresenter(string cause = "nothing", object reason = null)
        {
            Debug.Log(this.GetType().Name + " is deactivated - " + cause + " - " + reason);
            IsActive = false;
        }
        
        public virtual void Close() { }
    }
}