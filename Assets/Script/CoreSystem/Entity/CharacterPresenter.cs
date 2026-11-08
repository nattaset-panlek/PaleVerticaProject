using CoreSystem.UI;
using UnityEngine;

namespace CoreSystem.Entity
{
    [System.Serializable]
    public class CharacterPresenter
    {
        #region [Variables/Fields]
        public Animator Anim;
        public string MovingBoolParam = "IsMoving";
        public string DeadTriggerParam = "IsDead";
        public string AttackTriggerParam = "Attack";
        public string AttackSpeedFloatParam = "AttackSpeed";
        public Transform ModelTransform;
        public bool LockModelRotation = true;
        public float FlipCooldown = 0.1f;

        private Character _owner;
        private float _lastFlipTime = -1f;
        private FloatingStatusBar _worldStatusBar;
        private bool _hasAttackSpeedParam;
        #endregion

        #region [Properties]
        public bool HasWorldStatusBar => _worldStatusBar != null;
        #endregion

        #region [Public Methods]
        public void Initialize(Character owner)
        {
            _owner = owner;
            if (Anim == null) Anim = owner.GetComponentInChildren<Animator>(true);
            if (ModelTransform == null)
            {
                var model = owner.transform.Find("Model");
                ModelTransform = model != null ? model : (Anim != null ? Anim.transform : null);
            }
            CacheAnimatorParameters();
        }

        public void SetMoving(bool isMoving)
        {
            if (Anim != null)
            {
                if (!_hasAttackSpeedParam && isMoving && Anim.speed != 1f) Anim.speed = 1f;
                Anim.SetBool(MovingBoolParam, isMoving);
            }
        }

        public void PlayAttack(float attackSpeedMultiplier = 1f)
        {
            if (Anim != null)
            {
                float speed = Mathf.Max(0.5f, attackSpeedMultiplier);
                if (_hasAttackSpeedParam)
                {
                    Anim.SetFloat(AttackSpeedFloatParam, speed);
                }
                else
                {
                    Anim.speed = speed;
                }
                Anim.SetTrigger(AttackTriggerParam);
            }
        }

        public void PlayDeath()
        {
            if (Anim != null) Anim.SetTrigger(DeadTriggerParam);
        }

        public void ResetAnimator()
        {
            if (Anim == null) return;
            if (!_hasAttackSpeedParam) Anim.speed = 1f;
            Anim.Rebind();
            Anim.Update(0f);
        }
        #endregion

        #region [Private Methods]
        private void CacheAnimatorParameters()
        {
            _hasAttackSpeedParam = false;
            if (Anim == null) return;
            foreach (var param in Anim.parameters)
            {
                if (param.name == AttackSpeedFloatParam)
                {
                    _hasAttackSpeedParam = true;
                    break;
                }
            }
        }

        public void Face(Vector3 direction)
        {
            if (ModelTransform == null) return;
            if (direction.x > 0.1f) ModelTransform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < -0.1f) ModelTransform.localScale = new Vector3(1, 1, 1);
        }

        public void FaceWithCooldown(Vector3 direction)
        {
            if (ModelTransform == null) return;
            if (Time.time - _lastFlipTime < FlipCooldown) return;

            if (direction.x > 0.1f && ModelTransform.localScale.x > 0)
            {
                ModelTransform.localScale = new Vector3(-1, 1, 1);
                _lastFlipTime = Time.time;
            }
            else if (direction.x < -0.1f && ModelTransform.localScale.x < 0)
            {
                ModelTransform.localScale = new Vector3(1, 1, 1);
                _lastFlipTime = Time.time;
            }
        }

        public void LateTick()
        {
            if (LockModelRotation && ModelTransform != null) ModelTransform.rotation = Quaternion.identity;
        }

        public void ShowWorldStatusBar()
        {
            if (_worldStatusBar != null || _owner == null) return;

            GameObject prefab = Resources.Load<GameObject>("Prefab/HealthLocalStatus");
            if (prefab == null) return;

            Transform parent = null;
            GameObject worldSpace = GameObject.Find("WorldSpace");
            if (worldSpace != null && worldSpace.layer == LayerMask.NameToLayer("UI")) parent = worldSpace.transform;

            GameObject go = Object.Instantiate(prefab, parent);
            _worldStatusBar = go.GetComponent<FloatingStatusBar>();
            if (_worldStatusBar != null) _worldStatusBar.Initialize(_owner);

            var follow = go.GetComponent<UIFollowTarget>();
            if (follow != null) follow.Target = _owner.transform;
        }

        public void HideWorldStatusBar()
        {
            if (_worldStatusBar == null) return;
            Object.Destroy(_worldStatusBar.gameObject);
            _worldStatusBar = null;
        }
        #endregion
    }
}
