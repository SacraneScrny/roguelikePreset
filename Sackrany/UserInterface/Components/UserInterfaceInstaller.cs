using Sackrany.UserInterface.Abstract;
using Sackrany.UserInterface.Factory;
using Sackrany.Utils;

using UnityEngine;

namespace Sackrany.UserInterface.Components
{
    public class UserInterfaceInstaller : AManager<UserInterfaceInstaller>
    {
        public Transform Content;
        public PresenterType[] UiContent;
        public PrefabContainer Prefabs;

        public Model _model;
        public static Model Model => Instance._model;
        
        private void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            _model = new Model(
                Camera.main,
                Camera.main,//GameObject.FindWithTag("MainUICamera").GetComponent<Camera>(),
                Prefabs
            );

            _model.InstallPresenters(
                Content,
                PresenterFactory.GetPresenters(UiContent)
            );
            
            Debug.Log("Initialized UserInterface");
        }
    }
}