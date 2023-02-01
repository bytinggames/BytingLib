
namespace BytingLib
{
    public class AnimationBlender 
    {
        public float CurrentSecond { get; private set; } = 0f;

        private LayeredTransitioner<AnimationInstance> transitioner;
        private readonly GameSpeed drawSpeed;
        private bool drawnUpdate = false;

        public float DefaultTransitionDurationInSeconds { get; set;  }

        public AnimationBlender(GameSpeed drawSpeed, AnimationGL startAnimation, float defaultTransitionDurationInSeconds)
        {
            transitioner = new(new AnimationInstance(startAnimation, CurrentSecond));
            this.drawSpeed = drawSpeed;
            DefaultTransitionDurationInSeconds = defaultTransitionDurationInSeconds;
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

        public void TransitTo(AnimationGL animation)
        {
            transitioner.TransitTo(new AnimationInstance(animation, CurrentSecond), DefaultTransitionDurationInSeconds);
        }

        public void TransitTo(AnimationGL animation, float transitionDurationInSeconds)
        {
            transitioner.TransitTo(new AnimationInstance(animation, CurrentSecond), transitionDurationInSeconds);
        }

        public void Update()
        {
            float deltaSeconds = drawSpeed.DeltaMS / 1000f;
            CurrentSecond += deltaSeconds;

            transitioner.Update(deltaSeconds);

            drawnUpdate = false;
        }

        public void Draw()
        {
            if (drawnUpdate)
                return;

            drawnUpdate = true;

            if (transitioner.TransitionCount == 0)
                transitioner.OldestValue.Animation.UpdateAnimationTime(CurrentSecond - transitioner.OldestValue.StartTimeStamp);
            else
                transitioner.ApplyBlend(BlendStart, BlendContinue);
        }

        private void BlendStart(AnimationInstance from)
        {
            from.Animation.BlendStart(CurrentSecond - from.StartTimeStamp);
        }

        private void BlendContinue(AnimationInstance to, float interpolationAmount)
        {
            to.Animation.BlendAdd(CurrentSecond - to.StartTimeStamp, interpolationAmount);
        }
    }
}
