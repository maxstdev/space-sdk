//#define FAST_TIME_AT_KEY_FRAME
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaxstXR.Extension
{
    public class KeyFrameGenerator
    {
        public static void ConvertPathModel(
            List<PathModel> paths,
            SmoothCameraManager smoothCameraManager,
            List<PovKeyFrame> result,
            ISequenceProperty currentSequenceProperty = null
            )
        {
            PovController current = null;
            PovController next = null;
            result.Clear();

            int index = result.Count - 1;
            foreach (var path in paths)
            {
                var go = smoothCameraManager.KnnManager.FindNearest(path.position);

                if (go)
                {
                    var pc = go.GetComponent<PovController>();
                    if (pc)
                    {
                        current = next;
                        next = pc;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (current)
                {
                    result.Add(new PovKeyFrame(++index, current, next, result.Last().NextRotate));
                }
                else
                {
                    result.Add(new PovKeyFrame(++index, smoothCameraManager, next));
                }
#if FAST_TIME_AT_KEY_FRAME
                //TODO::Quick Action for Debugging
#else
                result.Last().CalulateDurationTime();
#endif
            }

            if (currentSequenceProperty != null && result.Count > 0)
            {
                var resultPosition = result.Last();

                if (!Vector3.Equals(resultPosition.CurrentPosition, currentSequenceProperty.FramePosition))
                {
                    var go = smoothCameraManager.KnnManager.FindNearestK(currentSequenceProperty.FramePosition, 1).First();
                    var pov = go.GetComponent<PovController>();

                    var sequenceCount = 0;
                    foreach (var quaternion in currentSequenceProperty.Quaternions)
                    {
                        if (sequenceCount == 0)
                        {
                            result.Add(new PovKeyFrame(result.Count, pov, pov, resultPosition.CurrentRotate.Value, quaternion));
                        }
                        else
                        {
                            var previousRotaion = currentSequenceProperty.Quaternions[sequenceCount - 1];
                            result.Add(new PovKeyFrame(result.Count, pov, pov, previousRotaion, quaternion));
                        }

                        sequenceCount++;

#if FAST_TIME_AT_KEY_FRAME
                //TODO::Quick Action for Debugging
#else
                        result.Last().CalulateDurationTime();
#endif
                    }
                }
            }

            if (result.Count > 0)
            {
                result.First().KeyFrameType = KeyFrameType.Continuous;
                result.Last().KeyFrameType = KeyFrameType.ContinuousWith8K;
                result.Last().IsLastKeyFrame = true;
            }
        }

        public static void ConvertSequenceProperty(ISequenceProperty sequence, SmoothCameraManager smoothCameraManager, List<PovKeyFrame> result)
        {
            var index = result.Count - 1;
            PovController nextPov;
            Quaternion? rotate;
            if (result.IsNotEmpty())
            {
                var frame = result.Last();
                frame.IsLastKeyFrame = false;
                nextPov = frame.NextPov;
                rotate = frame.NextRotate;
            }
            else
            {
                nextPov = smoothCameraManager.povController;
                rotate = smoothCameraManager.transform.rotation;
            }

            if (sequence.Quaternions.Count > 0)
            {
                var duration = sequence.RotationDuration > 0 ? sequence.RotationDuration : SmoothCameraManager.DefaultSecondAtRotate / sequence.Quaternions.Count;
                for (var i = 0; i < sequence.Quaternions.Count; ++i)
                {
                    Quaternion? nRotate = (sequence.Quaternions.Count > (i + 1)) ? sequence.Quaternions[i + 1] : null;
                    var rotateFrame = new PovKeyFrame(++index, nextPov, nextPov, sequence.Quaternions[i], nRotate);
                    rotateFrame.KeyFrameType = KeyFrameType.ContinuousWith8K;
                    rotateFrame.DurationTimeAtRotate = duration;
                    rotateFrame.DurationTimeAtPos = duration;
                    rotate = rotateFrame.NextRotate;
                    result.Add(rotateFrame);
                }
            }

            if (sequence.Delay > 0)
            {
                var waitFrame = new PovKeyFrame(++index, nextPov, nextPov, rotate, rotate);
                waitFrame.KeyFrameType = KeyFrameType.ContinuousWith8K;
                waitFrame.DurationTimeAtRotate = sequence.Delay;
                waitFrame.DurationTimeAtPos = sequence.Delay;
                result.Add(waitFrame);
            }

            if (result.IsNotEmpty()) result.Last().IsLastKeyFrame = true;
        }
    }
}
