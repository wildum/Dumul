using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

namespace menu
{
    public class NetworkPlayerMenu : MonoBehaviourPunCallbacks
    {
        public Transform head;
        public Transform rightHand;
        public Transform leftHand;

        public Animator leftHandAnimator;
        public Animator rightHandAnimator;

        private Transform headRig;
        private Transform leftHandRig;
        private Transform rightHandRig;

        private MenuHandPresence leftHandPresence;
        private MenuHandPresence rightHandPresence;

        private XRRig rig;

        private MenuStartPosition menuStartPosition;

        // Start is called before the first frame update
        void Start()
        {
            rig = FindObjectOfType<XRRig>();
            headRig = rig.transform.Find("Camera Offset/Main Camera");
            leftHandRig = rig.transform.Find("Camera Offset/LeftHand Controller");
            rightHandRig = rig.transform.Find("Camera Offset/RightHand Controller");

            leftHandPresence = GameObject.Find("Camera Offset/LeftHand Controller/Left Hand Presence").GetComponent<MenuHandPresence>();
            rightHandPresence = GameObject.Find("Camera Offset/RightHand Controller/Right Hand Presence").GetComponent<MenuHandPresence>();

            if (photonView != null)
            {
                if (photonView.IsMine)
                {
                    menuStartPosition = MenuSpawnPositionsManager.getMenuStartPosition();
                    rig.transform.position = menuStartPosition.position;
                    rig.transform.eulerAngles = menuStartPosition.rotation;
                    foreach (var item in GetComponentsInChildren<Renderer>())
                    {
                        item.enabled = false;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView != null && photonView.IsMine)
            {
                MapPosition(head, headRig);
                MapPosition(leftHand, leftHandRig);
                MapPosition(rightHand, rightHandRig);

                UpdateHandAnimation(leftHandAnimator, leftHandPresence);
                UpdateHandAnimation(rightHandAnimator, rightHandPresence);
            }
        }

        void UpdateHandAnimation(Animator handAnimator, MenuHandPresence handPresence)
        {
            handAnimator.SetFloat("Trigger", handPresence.getTriggerValue());
            handAnimator.SetFloat("Grip", handPresence.getGripValue());
        }

        void MapPosition(Transform target, Transform rigTransform)
        {
            target.position = rigTransform.position;
            target.rotation = rigTransform.rotation;
        }
        public Vector3 getHeadPosition()
        {
            return head.position;
        }
    }
}
