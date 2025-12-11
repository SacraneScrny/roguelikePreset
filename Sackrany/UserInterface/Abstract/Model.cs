using System.Collections.Generic;

using Sackrany.UserInterface.Components;
using Sackrany.UserInterface.Entities;

using UnityEngine;

namespace Sackrany.UserInterface.Abstract
{
    public class Model
    {
        public const float BaseScreenCanvasDistance = 2;
        public static Vector2 ReferenceResolution = new Vector2(1920, 1080);
        
        public List<Presenter> Presenters = new List<Presenter>();

        public UserInterfaceEvents Events;
        public Camera MainCamera;
        public Camera UICamera;
        public PrefabContainer Prefabs;


        public Model(
            Camera mainCamera,
            Camera UICamera,
            PrefabContainer prefabContainer
            )
        {
            MainCamera = mainCamera;
            this.UICamera = UICamera;
            Prefabs = prefabContainer;
            Events = new UserInterfaceEvents();
        }

        public void InstallPresenters(Transform parent, params Presenter[] value)
        {
            Presenters.AddRange(value);
            foreach (var presenter in Presenters)
                presenter.Initialize(this, parent);
            Debug.Log("UI presenter has been installed");
        }
    }
}