namespace OuterBeyond {
    internal class patch_THGame : THGame {
        protected override void Initialize() {
            base.Initialize();
            Breach.Loader.PostLoad();
        }
    }
}