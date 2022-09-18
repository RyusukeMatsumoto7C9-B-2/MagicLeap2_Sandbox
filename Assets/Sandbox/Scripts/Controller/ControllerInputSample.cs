using UnityEngine;
using UnityEngine.InputSystem;


namespace Sandbox.Controller
{
    /// <summary>
    /// コントローラ入力サンプル.
    /// </summary>
    public class ControllerInputSample : MonoBehaviour
    {

        private MagicLeapInputs _mlInputs;
        private MagicLeapInputs.ControllerActions _controllerActions;
        
        
        private void Start()
        {
            // 新しいインスタンスを作成し、起動.
            _mlInputs = new MagicLeapInputs();
            _mlInputs.Enable();

            // 各入力のイベントハンドラを登録.
            _controllerActions = new MagicLeapInputs.ControllerActions(_mlInputs);
            _controllerActions.Bumper.started += HandleOnBumperStarted;
            _controllerActions.Bumper.performed += HandleOnBumperPerformed;
            _controllerActions.Bumper.canceled += HandleOnBumperCanceled;

            _controllerActions.Menu.started += HandleOnMenuStarted;
            _controllerActions.Menu.performed += HandleOnMenuPerformed;
            _controllerActions.Menu.canceled += HandleOnMenuCanceled;

            _controllerActions.Trigger.started += HandleOnTriggerStarted;
            _controllerActions.Trigger.performed += HandleOnTriggerPerformed;
            _controllerActions.Trigger.canceled += HandleOnTriggerCanceled;

            _controllerActions.TouchpadClick.started += HandleOnTouchpadClickStarted;
            _controllerActions.TouchpadClick.performed += HandleOnTouchpadClickPerformed;
            _controllerActions.TouchpadClick.canceled += HandleOnTouchpadClickCanceled;

            _controllerActions.TouchpadTouch.started += HandleOnTouchpadTouchStarted;
            _controllerActions.TouchpadTouch.performed += HandleOnTouchpadTouchPerformed;
            _controllerActions.TouchpadTouch.canceled += HandleOnTouchpadTouchCanceled;

            _controllerActions.TouchpadForce.started += HandleOnTouchpadForceStarted;
            _controllerActions.TouchpadForce.performed += HandleOnTouchpadForcePerformed;
            _controllerActions.TouchpadForce.canceled += HandleOnTouchpadForceCanceled;

            _controllerActions.TouchpadPosition.started += HandleOnTouchpadPositionStarted;
            _controllerActions.TouchpadPosition.performed += HandleOnTouchpadPositionPerformed;
            _controllerActions.TouchpadPosition.canceled += HandleOnTouchpadPositionCanceled;

            _controllerActions.IsTracked.started += HandleOnIsTrackedStarted;
            _controllerActions.IsTracked.performed += HandleOnIsTrackedPerformed;
            _controllerActions.IsTracked.canceled += HandleOnIsTrackedCanceled;
        }
    

        private void OnDestroy()
        {
            // 登録していたハンドラを削除.
            _controllerActions.Bumper.started -= HandleOnBumperStarted;
            _controllerActions.Bumper.performed -= HandleOnBumperPerformed;
            _controllerActions.Bumper.canceled -= HandleOnBumperCanceled;

            _controllerActions.Menu.started -= HandleOnMenuStarted;
            _controllerActions.Menu.performed -= HandleOnMenuPerformed;
            _controllerActions.Menu.canceled -= HandleOnMenuCanceled;

            _controllerActions.Trigger.started -= HandleOnTriggerStarted;
            _controllerActions.Trigger.performed -= HandleOnTriggerPerformed;
            _controllerActions.Trigger.canceled -= HandleOnTriggerCanceled;

            _controllerActions.TouchpadClick.started -= HandleOnTouchpadClickStarted;
            _controllerActions.TouchpadClick.performed -= HandleOnTouchpadClickPerformed;
            _controllerActions.TouchpadClick.canceled -= HandleOnTouchpadClickCanceled;

            _controllerActions.TouchpadTouch.started -= HandleOnTouchpadTouchStarted;
            _controllerActions.TouchpadTouch.performed -= HandleOnTouchpadTouchPerformed;
            _controllerActions.TouchpadTouch.canceled -= HandleOnTouchpadTouchCanceled;
            
            _controllerActions.TouchpadForce.started -= HandleOnTouchpadForceStarted;
            _controllerActions.TouchpadForce.performed -= HandleOnTouchpadForcePerformed;
            _controllerActions.TouchpadForce.canceled -= HandleOnTouchpadForceCanceled;
            
            _controllerActions.TouchpadPosition.started -= HandleOnTouchpadPositionStarted;
            _controllerActions.TouchpadPosition.performed -= HandleOnTouchpadPositionPerformed;
            _controllerActions.TouchpadPosition.canceled -= HandleOnTouchpadPositionCanceled;
            
            _controllerActions.IsTracked.started -= HandleOnIsTrackedStarted;
            _controllerActions.IsTracked.performed -= HandleOnIsTrackedPerformed;
            _controllerActions.IsTracked.canceled -= HandleOnIsTrackedCanceled;

            // 入力の購読を終了.
            _mlInputs.Dispose();
        }


        private void Update()
        {
            // Update で直接確認することも可能( Bumperボタン以外は省略 ).
            if (_controllerActions.Bumper.IsPressed())
            {
                Debug.Log("Update : IsBumperPressed");
            }

            // コントローラの座標と回転はワールド座標で取得される.
            //Debug.Log($"Controller Position {_controllerActions.Position.ReadValue<Vector3>()}");
            //Debug.Log($"Controller Rotation {_controllerActions.Rotation.ReadValue<Quaternion>().eulerAngles}");
        }


        #region --- Menu Button ---
        
        private void HandleOnMenuStarted(InputAction.CallbackContext obj)
        {
            Debug.Log("The Menu is started");
        }


        private void HandleOnMenuPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("The Menu is performed");
        }


        private void HandleOnMenuCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log("The Menu is canceled");
        }
        
        #endregion --- Menu Button ---


        #region --- Bumper Button ---
        
        private void HandleOnBumperStarted(InputAction.CallbackContext obj)
        {
            Debug.Log("The Bumper is started.");
        }
        

        private void HandleOnBumperPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Bumper is performed.");
        }


        private void HandleOnBumperCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Bumper is canceled.");
        }
        
        #endregion --- Bumper Button ---

        
        #region --- Trigger ---

        private void HandleOnTriggerStarted(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Trigger started value : {obj.ReadValue<float>()}");
        }


        private void HandleOnTriggerPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Trigger performed value : {obj.ReadValue<float>()}");
        }


        private void HandleOnTriggerCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Trigger canceled value : {obj.ReadValue<float>()}");
        }
        
        #endregion --- Trigger ---
        
        
        #region --- Touchpad Click ---

        private void HandleOnTouchpadClickStarted(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Click started {obj.ReadValueAsButton()}");
        }


        private void HandleOnTouchpadClickPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Click performed {obj.ReadValueAsButton()}");
        }


        private void HandleOnTouchpadClickCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Click canceled {obj.ReadValueAsButton()}");
        }

        #endregion --- Touchpad Click ---

        
        #region --- Touchpad Touch ---

        private void HandleOnTouchpadTouchStarted(InputAction.CallbackContext obj)
        {
            Debug.Log("The TouchPad Touch started.");
        }


        private void HandleOnTouchpadTouchPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("The TouchPad Touch performed.");
        }

        
        private void HandleOnTouchpadTouchCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log("The TouchPad Touch canceled.");
        }

        #endregion --- Touchpad Touch ---
        
        
        #region --- Touchpad Force ---

        private void HandleOnTouchpadForceStarted(InputAction.CallbackContext obj)
        {
            Debug.Log("The Touchpad Force started.");
        }

        
        private void HandleOnTouchpadForcePerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("The Touchpad Force performed.");
        }

        
        private void HandleOnTouchpadForceCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log("The Touchpad Force canceled.");
        }
        
        #endregion --- Touchpad Force ---
        
        
        #region --- Touchpad Position ---
        
        /// <summary>
        /// 中心座標が0のタッチパッド上の XY 座標値.
        /// </summary>
        private void HandleOnTouchpadPositionStarted(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Position started value {obj.ReadValue<Vector2>()}");
        }

        
        private void HandleOnTouchpadPositionPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Position performed value {obj.ReadValue<Vector2>()}");
        }

        
        private void HandleOnTouchpadPositionCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"The Touchpad Position canceled value {obj.ReadValue<Vector2>()}");
        }

        #endregion --- Touchpad Position ---
        
        
        #region --- IsTracked ---

        /// <summary>
        /// コントローラが接続しているか判定.
        /// なぜかはわからんが Started は呼ばれない.
        /// 接続したときに Performed が一度呼ばれる.
        /// 切断したときに Canceled が一度呼ばれる.
        /// 常に把握したい場合は Update() なりで常時監視したほうがいいかも?
        /// </summary>
        private void HandleOnIsTrackedStarted(InputAction.CallbackContext obj)
        {
            Debug.Log("The IsTracked started.");
        }
        

        private void HandleOnIsTrackedPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("The IsTracked performed.");
        }


        private void HandleOnIsTrackedCanceled(InputAction.CallbackContext obj)
        {
            Debug.Log("The IsTracked canceled.");
        }
        
        #endregion --- IsTracked ---
    }
}

