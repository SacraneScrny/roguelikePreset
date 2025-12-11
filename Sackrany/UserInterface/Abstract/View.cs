using System.Collections;

using UnityEngine;

namespace Sackrany.UserInterface.Abstract
{
    public interface IView { public IView Initialize(Presenter presenter); }

    public abstract class View : View<Presenter> { };
    
    public abstract class View<T> : MonoBehaviour, IView where T : Presenter
    {
        private protected bool isInitialized;
        private protected bool isStarted;
        private protected T presenter;

        private IEnumerator Start()
        {
            while (presenter == null)
                yield return null;
            while (!presenter.IsActive)
                yield return null;
            while (!isInitialized)
                yield return null;
            StartView();
            isStarted = true;
        }

        private void Update()
        {
            if (presenter == null)
                return;
            if (!presenter.IsActive)
                return;
            if (!isInitialized)
                return;
            if (!isStarted)
                return;
            Tick();
        }
        private void FixedUpdate()
        {
            if (presenter == null)
                return;
            if (!presenter.IsActive)
                return;
            if (!isInitialized)
                return;
            if (!isStarted)
                return;
            FixedTick();
        }
        public void LateUpdate()
        {
            if (presenter == null)
                return;
            if (!presenter.IsActive)
                return;
            if (!isInitialized)
                return;
            if (!isStarted)
                return;
            LateTick();
        }

        public IView Initialize(Presenter presenter)
        {
            if (isInitialized)
                return this;

            this.presenter = (T)presenter;
            AwakeView();
            presenter.Views.Add(this);
            isInitialized = true;

            return this;
        }

        private protected virtual void AwakeView() { }
        private protected virtual void StartView() { }

        private void OnDestroy()
        {
            if (presenter != null)
                presenter.Views.Remove(this);
        }
        public void Active()
        {
            gameObject.SetActive(true);
        }
        public void Deactive()
        {
            gameObject.SetActive(false);
        }

        private protected virtual void Tick()
        {
        }
        private protected virtual void FixedTick()
        {
        }
        private protected virtual void LateTick()
        {
        }
    }
}