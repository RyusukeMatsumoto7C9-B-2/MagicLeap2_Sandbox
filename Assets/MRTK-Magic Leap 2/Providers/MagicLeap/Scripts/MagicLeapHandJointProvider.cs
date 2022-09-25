using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR;

#if UNITY_MAGICLEAP || UNITY_ANDROID
using Mono.Cecil.Cil;
using UnityEngine.XR.MagicLeap;
using HandGestures = UnityEngine.XR.MagicLeap.InputSubsystem.Extensions.DeviceFeatureUsages.HandGesture;
#endif

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    public class MagicLeapHandJointProvider 
    {
        public Dictionary<TrackedHandJoint, MixedRealityPose> JointPoses;

        //An array of bones that are used to map between Magic Leap and MRTK
        private readonly TrackedHandJoint[] _handJoints = new TrackedHandJoint[]
        {
            TrackedHandJoint.ThumbTip, TrackedHandJoint.ThumbDistalJoint, TrackedHandJoint.ThumbProximalJoint,TrackedHandJoint.ThumbMetacarpalJoint,
            TrackedHandJoint.None, //Thumb returns 5
            TrackedHandJoint.IndexTip, TrackedHandJoint.IndexDistalJoint, TrackedHandJoint.IndexMiddleJoint,TrackedHandJoint.IndexKnuckle,TrackedHandJoint.IndexMetacarpal,
            TrackedHandJoint.MiddleTip, TrackedHandJoint.MiddleDistalJoint, TrackedHandJoint.MiddleMiddleJoint,TrackedHandJoint.MiddleKnuckle, TrackedHandJoint.MiddleMetacarpal,
            TrackedHandJoint.RingTip, TrackedHandJoint.RingDistalJoint, TrackedHandJoint.RingMiddleJoint,TrackedHandJoint.RingKnuckle, TrackedHandJoint.RingMetacarpal,
            TrackedHandJoint.PinkyTip, TrackedHandJoint.PinkyDistalJoint, TrackedHandJoint.PinkyMiddleJoint,TrackedHandJoint.PinkyKnuckle, TrackedHandJoint.PinkyMetacarpal
        };

        //Used to see if the hand is currently being tracked.
        private List<Bone> _pinkyFingerBones = new List<Bone>();
        private List<Bone> _ringFingerBones = new List<Bone>();
        private List<Bone> _middleFingerBones = new List<Bone>();
        private List<Bone> _indexFingerBones = new List<Bone>();
        private List<Bone> _thumbBones = new List<Bone>();

        private Handedness _controllerHandedness;

        private MagicLeapJointSmoother _jointSmoother;
        public bool IsPositionAvailable
        {
            get;
            private set;
        }
        public bool IsRotationAvailable
        {
            get;
            private set;
        }
        public MagicLeapHandJointProvider(Handedness controllerHandedness)
        {
            _controllerHandedness = controllerHandedness;
            JointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
            _jointSmoother = new MagicLeapJointSmoother();
        }

        public void Reset()
        {
            IsRotationAvailable = false;
            IsPositionAvailable = false;
            _jointSmoother.Reset();
            JointPoses.Clear();
        }

        public void UpdateHandJoints(InputDevice device, InputDevice gestureDevice, MagicLeapHandTrackingInputProfile.SmoothingType smoothingType)
        {
            if (JointPoses == null)
            {
                JointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
            }

            device.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.Confidence, out float confidence);
            bool isTracking = confidence > 0;
         
#if UNITY_MAGICLEAP || UNITY_ANDROID
            InputSubsystem.Extensions.MLHandTracking.TryGetKeyPointsMask(device, out bool[] keyPointsMask);
            if (!isTracking || !device.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand hand))
            {
                IsPositionAvailable = IsRotationAvailable = false;
                return;
            }

            IsPositionAvailable = true;
            UpdateFingerBones(hand, HandFinger.Thumb, keyPointsMask, ref this._thumbBones);

            UpdateFingerBones(hand, HandFinger.Index, keyPointsMask, ref this._indexFingerBones);

            UpdateFingerBones(hand, HandFinger.Middle, keyPointsMask, ref this._middleFingerBones);

            UpdateFingerBones(hand, HandFinger.Ring, keyPointsMask, ref this._ringFingerBones);

            UpdateFingerBones(hand, HandFinger.Pinky, keyPointsMask, ref this._pinkyFingerBones);
          
            UpdateWristPosition(device);
            UpdatePalmPose(device,gestureDevice);

            // Set the wrist to be the rotation of the hand

            _jointSmoother.SmoothJoints(ref JointPoses, smoothingType);
            IsRotationAvailable = CheckRotationAvailability();
            if (!IsRotationAvailable)
                return;

            UpdatePalmRotation();
            UpdateWristRotation();

            //Update the rotation after updating the finger positions
            //There are 4 valid bones per finger.
            if (_thumbBones.Count > 0)
                UpdateKeypointRotations(HandFinger.Thumb, keyPointsMask, ref this._thumbBones);

            if (_indexFingerBones.Count > 0)
                UpdateKeypointRotations(HandFinger.Index, keyPointsMask, ref this._indexFingerBones);

            if (_middleFingerBones.Count > 0)
                UpdateKeypointRotations(HandFinger.Middle, keyPointsMask, ref this._middleFingerBones);

            if (_ringFingerBones.Count > 0)
                UpdateKeypointRotations(HandFinger.Ring, keyPointsMask, ref this._ringFingerBones);

            if (_pinkyFingerBones.Count > 0)
                UpdateKeypointRotations(HandFinger.Pinky, keyPointsMask, ref this._pinkyFingerBones);
#endif
        }

#if UNITY_MAGICLEAP || UNITY_ANDROID

        private void UpdateWristPosition(InputDevice hand)
        {
            if (hand.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.WristCenter,
                    out Vector3 position))
            {
                var wristPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(position));

                if (!JointPoses.ContainsKey(TrackedHandJoint.Wrist))
                {
                    JointPoses.Add(TrackedHandJoint.Wrist, wristPose);
                }
                else
                {
                    JointPoses[TrackedHandJoint.Wrist] = wristPose;
                }
            }
        }

        private void UpdateWristRotation()
        {
            JointPoses[TrackedHandJoint.Wrist] = new MixedRealityPose(JointPoses[TrackedHandJoint.Wrist].Position, JointPoses[TrackedHandJoint.Palm].Rotation);
        }

        private void UpdatePalmPose(InputDevice hand, InputDevice gestureDevice)
        {
            var palmPose = new MixedRealityPose();
            // // Try to use the gesture device for palm rotation and position
            // if (gestureDevice.isValid && gestureDevice.TryGetFeatureValue(HandGestures.GestureTransformPosition, out Vector3 handPosition) &&
            //     gestureDevice.TryGetFeatureValue(HandGestures.GestureTransformRotation, out Quaternion handRotation))
            // {
            //     palmPose.Position = handPosition;
            //     palmPose.Rotation = handRotation;
            // }
            // else 
            // Use the keypoints to get the palm rotation of gesture device does not return the hand position
            if (hand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition))
            {
                palmPose.Position = MixedRealityPlayspace.TransformPoint(devicePosition);
            }

            if (!JointPoses.ContainsKey(TrackedHandJoint.Palm))
            {
                JointPoses.Add(TrackedHandJoint.Palm, palmPose);
            }
            else
            {
                JointPoses[TrackedHandJoint.Palm] = palmPose;
            }
        }

        private void UpdatePalmRotation()
        {
            Vector3 centerPosition = Vector3.Lerp(JointPoses[TrackedHandJoint.Wrist].Position,
                JointPoses[TrackedHandJoint.MiddleKnuckle].Position, .5f);
            var palmPose = new MixedRealityPose(centerPosition);
            palmPose.Position = centerPosition;
            // Get the palm rotation based off the palm position and other hand joints
            JointPoses[TrackedHandJoint.Palm] = GetPalmRotation(palmPose);
        }

        private void UpdateFingerBones(Hand hand, HandFinger finger, bool[] keyPointsMask, ref List<Bone> bones)
        {
            hand.TryGetFingerBones(finger, bones);

            // The index of the MRTK bones is different than the index of the Magic Leap bones.
            // Currently all fingers return 5 bones. The MRTK Index index skips over these bones that 
            int mapIndex = 0;
            int mrtkIndex = 0;
            switch (finger)
            {
                case HandFinger.Thumb:
                    mapIndex = 0;
                    mrtkIndex = 0;
                    break;
                case HandFinger.Index:
                    mapIndex = 4;
                    mrtkIndex = 5;
                    break;
                case HandFinger.Middle:
                    mapIndex = 8;
                    mrtkIndex = 10;
                    break;
                case HandFinger.Ring:
                    mapIndex = 12;
                    mrtkIndex = 15;
                    break;
                case HandFinger.Pinky:
                    mapIndex = 16;
                    mrtkIndex = 20;
                    break;
            }

            for (int i = 0; i < bones.Count; i++)
            {
                if (keyPointsMask[mapIndex + i])
                {
                    bones[i].TryGetPosition(out Vector3 position);
                    bones[i].TryGetRotation(out Quaternion rotation);

                    var fingerPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(position), MixedRealityPlayspace.Rotation * rotation);

                    if (!JointPoses.ContainsKey(_handJoints[mrtkIndex + i]))
                    {
                        JointPoses.Add(_handJoints[mrtkIndex + i], fingerPose);
                    }
                    else
                    {
                        JointPoses[_handJoints[mrtkIndex + i]] = fingerPose;
                    }
                }
            }

        }

        private float Percentage(float value, float minimum, float maximum)
        {
            value -= minimum;
            value = Mathf.Max(0, value);
            return Mathf.Clamp01(value / (maximum - minimum));
        }

        private bool CheckRotationAvailability()
        {

            return JointPoses.ContainsKey(TrackedHandJoint.Wrist)
                   && JointPoses.ContainsKey(TrackedHandJoint.MiddleKnuckle)
                   && JointPoses.ContainsKey(TrackedHandJoint.ThumbProximalJoint)
                   && JointPoses.ContainsKey(TrackedHandJoint.IndexKnuckle)
                   && JointPoses.ContainsKey(TrackedHandJoint.RingKnuckle);
        }

        private MixedRealityPose GetPalmRotation( MixedRealityPose palmPose)
        {

            Vector3 centerPosition = palmPose.Position;

            //correct distances:
            float thumbMcpToWristDistance = Vector3.Distance(JointPoses[TrackedHandJoint.ThumbProximalJoint].Position,
                JointPoses[TrackedHandJoint.Wrist].Position) * .5f;
            //fix the distance between the wrist and thumbMcp as it incorrectly expands as the hand gets further from the camera:
            float distancePercentage = Mathf.Clamp01(Vector3.Distance(Camera.main.transform.position,
                JointPoses[TrackedHandJoint.Wrist].Position) / .5f);
            distancePercentage = 1 - Percentage(distancePercentage, .90f, 1) * .4f;
            thumbMcpToWristDistance *= distancePercentage;
            Vector3 wristToPalmDirection =
                Vector3.Normalize(Vector3.Normalize(centerPosition - JointPoses[TrackedHandJoint.Wrist].Position));
            Vector3 center = JointPoses[TrackedHandJoint.Wrist].Position +
                             (wristToPalmDirection * thumbMcpToWristDistance);
            Vector3 camToWristDirection =
                Vector3.Normalize(JointPoses[TrackedHandJoint.Wrist].Position - Camera.main.transform.position);

            //rays needed for planarity discovery for in/out palm facing direction:
            Vector3 camToWrist = new Ray(JointPoses[TrackedHandJoint.Wrist].Position, camToWristDirection).GetPoint(1);
            Vector3 camToThumbMcp = new Ray(JointPoses[TrackedHandJoint.ThumbProximalJoint].Position,
                Vector3.Normalize(JointPoses[TrackedHandJoint.ThumbProximalJoint].Position -
                                  Camera.main.transform.position)).GetPoint(1);
            Vector3 camToPalm = new Ray(center, Vector3.Normalize(center - Camera.main.transform.position)).GetPoint(1);

            //discover palm facing direction to camera:
            Plane palmFacingPlane = new Plane(camToWrist, camToPalm, camToThumbMcp);
            if (_controllerHandedness == Handedness.Left)
            {
                palmFacingPlane.Flip();
            }

            float palmForwardFacing = Mathf.Sign(Vector3.Dot(palmFacingPlane.normal, Camera.main.transform.forward));

            //use thumb/palm/wrist alignment to determine amount of roll in the hand:
            Vector3 toThumbMcp = Vector3.Normalize(JointPoses[TrackedHandJoint.ThumbProximalJoint].Position - center);
            Vector3 toPalm = Vector3.Normalize(center - JointPoses[TrackedHandJoint.Wrist].Position);
            float handRollAmount = (1 - Vector3.Dot(toThumbMcp, toPalm)) * palmForwardFacing;

            //where between the wrist and thumbMcp should we slide inwards to get the palm in the center:
            Vector3 toPalmOrigin = Vector3.Lerp(JointPoses[TrackedHandJoint.Wrist].Position,
                JointPoses[TrackedHandJoint.ThumbProximalJoint].Position, .35f);

            //get a direction from the camera to toPalmOrigin as psuedo up for use in quaternion construction:
            Vector3 toCam = Vector3.Normalize(Camera.main.transform.position - toPalmOrigin);

            //construct a quaternion that helps get angles needed between the wrist and thumbMCP to point towards the palm center:
            Vector3 wristToThumbMcp = Vector3.Normalize(JointPoses[TrackedHandJoint.ThumbProximalJoint].Position -
                                                        JointPoses[TrackedHandJoint.Wrist].Position);
            Quaternion towardsCamUpReference = Quaternion.identity;
            if (wristToThumbMcp != Vector3.zero && toCam != Vector3.zero)
            {
                towardsCamUpReference = Quaternion.LookRotation(wristToThumbMcp, toCam);
            }

            //rotate the inwards vector depending on hand roll to know where to push the palm back:
            float inwardsVectorRotation = 90;
            if (_controllerHandedness == Handedness.Left)
            {
                inwardsVectorRotation = -90;
            }

            towardsCamUpReference =
                Quaternion.AngleAxis(handRollAmount * inwardsVectorRotation, towardsCamUpReference * Vector3.forward) *
                towardsCamUpReference;
            Vector3 inwardsVector = towardsCamUpReference * Vector3.up;

            //slide palm location along inwards vector to get it into proper physical location in the center of the hand:
            center = toPalmOrigin - inwardsVector * thumbMcpToWristDistance;
            Vector3 deadCenter = center;

            //as the hand flattens back out balance corrected location with originally provided location for better forward origin:
            center = Vector3.Lerp(center, centerPosition, Mathf.Abs(handRollAmount));

            //get a forward using the corrected palm location:
            Vector3 forward = Vector3.Normalize(center - JointPoses[TrackedHandJoint.Wrist].Position);

            //switch back to physical center of hand - this reduces surface-to-surface movement of the center between back and palm:
            center = deadCenter;

            //get an initial hand up:
            Plane handPlane = new Plane(JointPoses[TrackedHandJoint.Wrist].Position,
                JointPoses[TrackedHandJoint.ThumbProximalJoint].Position, center);
            if (_controllerHandedness == Handedness.Left)
            {
                handPlane.Flip();
            }

            Vector3 up = handPlane.normal;

            //find out how much the back of the hand is facing the camera so we have a safe set of features for a stronger forward: 
            Vector3 centerToCam =
                Vector3.Normalize(Camera.main.transform.position - JointPoses[TrackedHandJoint.Wrist].Position);
            float facingDot = Vector3.Dot(centerToCam, up);

            if (facingDot > .5f)
            {
                float handBackFacingCamAmount = Percentage(facingDot, .5f, 1);

                Vector3 toMiddleMcp = Vector3.Normalize(JointPoses[TrackedHandJoint.MiddleKnuckle].Position - center);
                forward = Vector3.Lerp(forward, toMiddleMcp, handBackFacingCamAmount);
            }

            //make sure palm distance from wrist is consistant while also leveraging steered forward:
            center = JointPoses[TrackedHandJoint.Wrist].Position + (forward * thumbMcpToWristDistance);

            //an initial rotation of the hand:
            Quaternion orientation = Quaternion.identity;
            if (forward != Vector3.zero && up != Vector3.zero)
            {
                orientation = Quaternion.LookRotation(forward, up);
            }

            Vector3 knucklesVector = Vector3.Normalize(JointPoses[TrackedHandJoint.MiddleKnuckle].Position -
                                                       JointPoses[TrackedHandJoint.IndexKnuckle].Position);
            float knucklesDot = Vector3.Dot(knucklesVector, Vector3.up);
            if (knucklesDot > .5f)
            {
                float counterClockwiseRoll = Percentage(Vector3.Dot(knucklesVector, Vector3.up), .35f, .7f);
                center = Vector3.Lerp(center, centerPosition, counterClockwiseRoll);
                forward = Vector3.Lerp(forward,
                    Vector3.Normalize(JointPoses[TrackedHandJoint.MiddleKnuckle].Position - centerPosition),
                    counterClockwiseRoll);
                Plane backHandPlane = new Plane(centerPosition, JointPoses[TrackedHandJoint.IndexKnuckle].Position,
                    JointPoses[TrackedHandJoint.MiddleKnuckle].Position);
                if (_controllerHandedness == Handedness.Left)
                {
                    backHandPlane.Flip();
                }

                up = Vector3.Lerp(up, backHandPlane.normal, counterClockwiseRoll);
                orientation = Quaternion.LookRotation(forward, up);
            }

            //as the wrist tilts away from the camera (with the thumb down) at extreme angles the hand center will move toward the thumb:
            float handTiltAwayAmount =
                1 - Percentage(Vector3.Distance(centerPosition, JointPoses[TrackedHandJoint.Wrist].Position), .025f,
                    .04f);
            Vector3 handTiltAwayCorrectionPoint = JointPoses[TrackedHandJoint.Wrist].Position +
                                                  camToWristDirection * thumbMcpToWristDistance;
            center = Vector3.Lerp(center, handTiltAwayCorrectionPoint, handTiltAwayAmount);
            forward = Vector3.Lerp(forward,
                Vector3.Normalize(handTiltAwayCorrectionPoint - JointPoses[TrackedHandJoint.Wrist].Position),
                handTiltAwayAmount);
            Plane wristPlane = new Plane(JointPoses[TrackedHandJoint.Wrist].Position,
                JointPoses[TrackedHandJoint.ThumbProximalJoint].Position, center);
            if (_controllerHandedness == Handedness.Left)
            {
                wristPlane.Flip();
            }

            up = Vector3.Lerp(up, wristPlane.normal, handTiltAwayAmount);
            if (forward != Vector3.zero && up != Vector3.zero)
            {
                orientation = Quaternion.LookRotation(forward, up);
            }

            //steering for if thumb/index are not available from self-occlusion to help rotate the hand better outwards better:
            float forwardUpAmount = Vector3.Dot(forward, Vector3.up);

            if (forwardUpAmount > .7f)
            {
                float angle = 0;
                if (_controllerHandedness == Handedness.Right)
                {

                    Vector3 knucklesVector1 = Vector3.Normalize(JointPoses[TrackedHandJoint.RingKnuckle].Position -
                                                                JointPoses[TrackedHandJoint.IndexKnuckle].Position);
                    angle = Vector3.Angle(knucklesVector1, orientation * Vector3.right);
                    angle *= -1;
                }
                else
                {
                    Vector3 knucklesVector2 = Vector3.Normalize(JointPoses[TrackedHandJoint.IndexKnuckle].Position -
                                                                JointPoses[TrackedHandJoint.RingKnuckle].Position);
                    angle = Vector3.Angle(knucklesVector2, orientation * Vector3.right);
                }

                Quaternion selfOcclusionSteering = Quaternion.AngleAxis(angle, forward);
                orientation = selfOcclusionSteering * orientation;
            }
            else
            {
                //when palm is facing down we need to rotate some to compensate for an offset:
                float rollCorrection = Mathf.Clamp01(Vector3.Dot(orientation * Vector3.up, Vector3.up));
                float rollCorrectionAmount = -30;
                if (_controllerHandedness == Handedness.Left)
                {
                    rollCorrectionAmount = 30;
                }

                orientation = Quaternion.AngleAxis(rollCorrectionAmount * rollCorrection, forward) * orientation;
            }

            //set pose:
            palmPose.Position = center;
            palmPose.Rotation = orientation;
            return palmPose;
        }

        void UpdateKeypointRotations(HandFinger finger, bool[] keyPointsMask, ref List<Bone> bones)
        {
            

            int mapIndex = 0;
            int mrtkIndex = 0;
            switch (finger)
            {
                case HandFinger.Thumb:
                    mapIndex = 0;
                    mrtkIndex = 0;
                    break;
                case HandFinger.Index:
                    mapIndex = 4;
                    mrtkIndex = 5;
                    break;
                case HandFinger.Middle:
                    mapIndex = 8;
                    mrtkIndex = 10;
                    break;
                case HandFinger.Ring:
                    mapIndex = 12;
                    mrtkIndex = 15;
                    break;
                case HandFinger.Pinky:
                    mapIndex = 16;
                    mrtkIndex = 20;
                    break;
            }

            //Flip the rotation so that forward is the back of the palm.
            int sign = -1;

            //Each bone returns 5 but only 4 our valid so we subtract 1 from the list
            // we also subtract the last bone so we can calculate the tip at the end.
            for (int i = 0; i < bones.Count- 2; i++)
            {
                   bool isRotationValid = keyPointsMask[mapIndex + i + 1] && keyPointsMask[mapIndex + i];
                   if (!isRotationValid)
                       return;

                Vector3 forward = JointPoses[_handJoints[mrtkIndex + i + 1]].Position - JointPoses[_handJoints[mrtkIndex + i]].Position;
                Vector3 up;

                if (finger == HandFinger.Thumb)
                {
                    up = JointPoses[TrackedHandJoint.Palm].Rotation * Vector3.right;
                }
                else
                {
                    up = sign * Vector3.Cross(forward, JointPoses[TrackedHandJoint.Palm].Rotation * Vector3.right);
                }

                if (forward != Vector3.zero)
                {
                    //The mixed reality pose is a struct
                    var pose = JointPoses[_handJoints[mrtkIndex + i]];
                    pose.Rotation = Quaternion.LookRotation(forward, up);
                    JointPoses[_handJoints[mrtkIndex + i]] = pose;
                }
            }

            int last = bones.Count - 2;
            var pose2 = JointPoses[_handJoints[mrtkIndex + last]];
            pose2.Rotation = JointPoses[_handJoints[mrtkIndex + last - 1]].Rotation;
            JointPoses[_handJoints[mrtkIndex + last]] = pose2;
        }
#endif
    }
}
