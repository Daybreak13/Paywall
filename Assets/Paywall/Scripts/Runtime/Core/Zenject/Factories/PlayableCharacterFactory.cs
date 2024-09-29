using Paywall.Interfaces;
using Zenject;

namespace Paywall
{

    public class PlayableCharacterFactory : IPlayableCharacter.IFactory
    {
        private readonly PlayableCharacter.Factory _factory;

        [Inject]
        public PlayableCharacterFactory(PlayableCharacter.Factory factory)
        {
            _factory = factory;
        }

        IPlayableCharacter IPlayableCharacter.IFactory.Create()
        {
            return _factory.Create();
        }
    }
}
