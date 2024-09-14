using UnityEngine;
using Zenject;

namespace Paywall.Installers {

    public class GameInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.Bind<LevelBoundsManager>().To<LevelBoundsManager>().AsSingle();
            Container.Bind<LevelSpeedManager>().To<LevelSpeedManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ISpeedManager>().To<LevelSpeedManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}