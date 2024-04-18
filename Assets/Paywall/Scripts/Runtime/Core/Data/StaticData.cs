namespace Paywall {

    public static class StaticData {
        /// Sometimes velocity is near-zero when it should be zero. Use this for conditionals that use <= >= 0 velocity
        public static float VelocityBuffer = 0.000001f;
    }
}
