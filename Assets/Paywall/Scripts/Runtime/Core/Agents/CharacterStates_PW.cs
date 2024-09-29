namespace Paywall
{

    public class CharacterStates_PW
    {
        public enum ConditionStates
        {
            Normal,
            Dodging,
            Parrying,
            Teleporting
        }

        public enum MovementStates
        {
            Null,
            Running,    // Default state
            Jumping,    // Includes falling after a jump, in order to set animation parameters correctly
            Falling,    // Falling without having jumped first
            RailRiding
        }
    }
}