using Paywall.Interfaces;
using UnityEngine;
using Zenject;

namespace Paywall.Installers
{

    public class GameInstaller : MonoInstaller
    {
        public GameObject PlayerCharacterPrefab;

        public override void InstallBindings()
        {
            Container.Bind<LevelBoundsManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LevelSpeedManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerSpawnManager>().FromComponentInHierarchy().AsSingle();

            //Container.BindIFactory<IPlayableCharacter, IPlayableCharacterFactory>().FromComponentInNewPrefab(PlayerCharacterPrefab).AsSingle();
            //Container.BindIFactory<IPlayableCharacter, IPlayableCharacterFactory>().FromFactory<PlayableCharacterFactory>().FromComponentInNewPrefab(PlayerCharacterPrefab).AsSingle();

            //Container.BindFactory<PlayableCharacter, PlayableCharacter.Factory>().FromComponentInNewPrefab(PlayerCharacterPrefab).AsSingle();

            Container.BindFactory<IPlayableCharacter, PlayableCharacter.Factory>()
                .FromComponentInNewPrefab(PlayerCharacterPrefab).AsSingle();

            // Bind the custom factory implementation
            Container.Bind<IPlayableCharacter.IFactory>()
                     .To<PlayableCharacterFactory>().AsSingle();
        }
    }
}