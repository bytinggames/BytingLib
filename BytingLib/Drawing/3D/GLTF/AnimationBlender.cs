
namespace BytingLib
{
    public class AnimationBlender 
    {
        private readonly GameSpeed drawSpeed;
        private readonly Ref<ModelGL> model;
        private LayeredTransitioner<AnimationInstance> transitioner;
        private bool drawnUpdate = false;

        public float CurrentSecond { get; private set; } = 0f;
        public float DefaultTransitionDurationInSeconds { get; set; }
        public float AnimationSpeedFactor { get; set; } = 1f;

        public AnimationBlender(GameSpeed drawSpeed, int startAnimation, float defaultTransitionDurationInSeconds, Ref<ModelGL> model)
        {
            transitioner = new(new AnimationInstance(startAnimation, CurrentSecond));
            transitioner.OnTransitionDone += Transitioner_OnTransitionDone;
            this.drawSpeed = drawSpeed;
            DefaultTransitionDurationInSeconds = defaultTransitionDurationInSeconds;
            this.model = model;
        }

        private void Transitioner_OnTransitionDone(AnimationInstance animationInstance)
        {
            // clean up (f.ex. for the case, when this animation scaled some nodes, and the next animation doesn't modify scale at all)
            GetAnimation(animationInstance.Animation)?.ApplyDefault();
        }

        public void TransitTo(AnimationInstance animationInstance)
            => TransitTo(animationInstance, DefaultTransitionDurationInSeconds);

        public void TransitTo(AnimationInstance animationInstance, float transitionDurationInSeconds)
        {
            var last = transitioner.GetAllValues().Last();
            if (last.Animation == animationInstance.Animation
                && last.StartTimeStamp == animationInstance.StartTimeStamp)
                return; // no need to add a duplicate
            transitioner.TransitTo(animationInstance, transitionDurationInSeconds);
        }

        public void TransitTo(int animation)
        {
            transitioner.TransitTo(new AnimationInstance(animation, CurrentSecond), DefaultTransitionDurationInSeconds);
        }

        public void TransitTo(int animation, float transitionDurationInSeconds)
        {
            transitioner.TransitTo(new AnimationInstance(animation, CurrentSecond), transitionDurationInSeconds);
        }

        public void Update()
        {
            float deltaSeconds = drawSpeed.DeltaMS / 1000f * AnimationSpeedFactor;
            CurrentSecond += deltaSeconds;

            transitioner.Update(deltaSeconds);

            drawnUpdate = false;
        }

        public void ApplyBlend()
        {
            if (drawnUpdate)
                return;

            drawnUpdate = true;

            if (transitioner.TransitionCount == 0)
                GetAnimation(transitioner.OldestValue.Animation)?.UpdateAnimationTime(CurrentSecond - transitioner.OldestValue.StartTimeStamp);
            else
                transitioner.ApplyBlend(BlendStart, BlendContinue);
        }

        private void BlendStart(AnimationInstance from)
        {
            GetAnimation(from.Animation)?.BlendStart(CurrentSecond - from.StartTimeStamp);
        }

        private void BlendContinue(AnimationInstance to, float interpolationAmount)
        {
            GetAnimation(to.Animation)?.BlendAdd(CurrentSecond - to.StartTimeStamp, interpolationAmount);
        }

        AnimationGL? GetAnimation(int index)
        {
            return model.Value.Animations?.Get(index);
        }
    }
}
