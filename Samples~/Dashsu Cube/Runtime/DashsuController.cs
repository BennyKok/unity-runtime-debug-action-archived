using System;
using System.Collections.Generic;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Attributes;
using BennyKok.RuntimeDebug.DebugInput;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace BennyKok.RuntimeDebug.Demo.DashsuCube
{
    public class DashsuController : MonoBehaviour
    {
        [Title("Character", false, 4)]
        public Rigidbody character;
        public float dashSpeed = 2;
        // public bool enableVerticalMovement;

        [Title("Lane", 4)]
        public int maxLaneToRight = 1;
        public float laneSpacing = 1;

        [Title("Obstacles", 4)]
        [Comment("This is calcuelated as the relative distance from the character", order = 1)]
        public float startDistance = 10;
        public float endDistance = -2;
        public float spawnRate = 0.5f;

        [DebugAction(closePanelAfterTrigger = true)]
        public float obstacleVelocity = 2;
        public string obstacleTag = "Obstacle";
        public List<GameObject> obstaclePrefabs;

        [Title("UI", 4)]
        public RevealText gameOverText;

        #region Private Field

        [Title("Demo", 4)]
        [DebugAction]
        public bool dummyExposedBool;

        //Our cached camera reference
        private Camera mCamera;
        private Vector3 characterOriginalPosition;

        private Vector3 cubeDirection;
        private int laneIndex = 0;

        private float lastSpawnTime = -999;

        private Vector2 firstTouchPosition;

        private bool hasSwiped;

        private List<Rigidbody> obstacleInstances = new List<Rigidbody>();

        //1 = ended
        private int gameState = 0;

        #endregion

        public bool GameEnded => gameState == 1;

        private void Awake()
        {
            mCamera = Camera.main;
            characterOriginalPosition = character.transform.position;

            var detector = character.gameObject.AddComponent<CollisionDetector>();
            detector.OnCollisionEnterEvent += (Collision other) =>
            {
                //Game ended
                if (GameEnded) return;

                if (other.gameObject.CompareTag(obstacleTag))
                {
                    HandleGameOver();
                }
            };
        }

        private void HandleGameOver()
        {
            //Stop all obstacle movement
            obstacleInstances.ForEach(x =>
            {
                x.velocity = Vector3.zero;
                x.constraints = RigidbodyConstraints.None;
                x.AddExplosionForce(10f, character.transform.position, 10f, 10, ForceMode.Impulse);
            });

            Debug.Log("Game Over");
            gameState = 1;
            gameOverText.Reveal();
        }

        private BaseDebugAction[] actions;

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            EnhancedTouchSupport.Enable();
#endif

            Debug.Log("RegisterActions");

            // actions = new BaseDebugAction[]
            // {
            //     DebugActionBuilder.Input()
            //         .WithName("Set Obstacle Velocity")
            //         .WithInputQuery(InputQuery.Create().Query("New velocity", ParamType.Float, null, () => obstacleVelcity))
            //         .WithInputAction((response) =>
            //         {
            //             var newVel = response.GetParamFloat("New velocity");
            //             obstacleVelcity = newVel;
            //         })
            //         .WithClosePanelAfterTrigger(),

            //         DebugActionBuilder.Input()
            //         .WithName("Set Game Over Text")
            //         .WithInputQuery(InputQuery.Create().Query("New label", ParamType.String, null, () => gameOverText.text.text))
            //         .WithInputAction((response) =>
            //         {
            //             var newLabel = response.GetParamString("New label");
            //             gameOverText.text.SetText(newLabel);
            //         })
            //         .WithClosePanelAfterTrigger(),

            //         DebugActionBuilder.Button()
            //         .WithName("Spawn A Cube")
            //         .WithAction(() =>
            //         {
            //             var bdy = GameObject.CreatePrimitive(PrimitiveType.Cube)
            //                 .AddComponent<Rigidbody>();

            //             bdy.gameObject.AddComponent<BoxCollider>();
            //             bdy.transform.position = new Vector3(0, 3, UnityEngine.Random.Range(-2, 2));
            //             bdy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //             bdy.AddExplosionForce(2, Vector3.zero, 10);
            //         })
            //         .WithShortcutKey("q")
            //         .WithClosePanelAfterTrigger()
            // };

            // RuntimeDebugSystem.RegisterActions(actions);
            actions = RuntimeDebugSystem.RegisterActionsAuto(this);
        }

        #region  Debug Action
        [DebugAction(shortcutKey = "q", closePanelAfterTrigger = true)]
        public void SpawnACube()
        {
            var bdy = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<Rigidbody>();

            bdy.gameObject.AddComponent<BoxCollider>();
            bdy.transform.position = new Vector3(0, 3, UnityEngine.Random.Range(-2, 2));
            bdy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            bdy.AddExplosionForce(2, Vector3.zero, 10);
        }

        [DebugAction]
        public void LogSum(int a, int b = 2)
        {
            Debug.Log(a + b);
        }

        [DebugAction]
        public string GameOverText
        {
            get => gameOverText.text.text;
            set => gameOverText.text.SetText(value);
        }
        #endregion

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            EnhancedTouchSupport.Disable();
#endif

            Debug.Log("UnregisterActions");

            RuntimeDebugSystem.UnregisterActions(actions);
        }

        private void FixedUpdate()
        {
            HandleCharacterMovement();
        }

        private void Update()
        {
            //Our character probably got killed
            if (GameEnded) return;

            if (!RuntimeDebugSystem.IsVisible)
                HandleInputControl();

            HandleObstacleSpawn();
            HandleObstacleMovement();
        }

        private void HandleObstacleMovement()
        {
            for (int i = obstacleInstances.Count - 1; i >= 0; i--)
            {
                var obstacle = obstacleInstances[i];
                obstacle.velocity = Vector3.right * obstacleVelocity;

                //Removing it if the obstacle is too far behind the character
                if (obstacle.transform.position.x - character.transform.position.x > -endDistance)
                {
                    obstacleInstances.Remove(obstacle);
                    Destroy(obstacle.gameObject);
                }
            }
        }

        private void HandleObstacleSpawn()
        {
            if (Time.time - lastSpawnTime >= spawnRate)
            {
                var targetSpawnPrefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Count)];
                var newObstacle = Instantiate(targetSpawnPrefab);
                var targetPos = UnityEngine.Random.Range(-maxLaneToRight, maxLaneToRight + 1) * Vector3.forward * laneSpacing + -Vector3.right * startDistance;

                //Make them at same level
                targetPos.y = characterOriginalPosition.y;
                newObstacle.transform.position = targetPos;

                obstacleInstances.Add(newObstacle.GetComponent<Rigidbody>());
                lastSpawnTime = Time.time;
            }
        }

        private void HandleCharacterMovement()
        {
            character.MovePosition(Vector3.Lerp(character.transform.position, characterOriginalPosition + cubeDirection, Time.deltaTime * dashSpeed));
        }

        private void HandleInputControl()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                laneIndex--;
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                laneIndex++;

            if (Touch.activeTouches.Count > 0)
            {
                var t = Touch.activeTouches[0];

                if (t.phase == TouchPhase.Began)
                    firstTouchPosition = t.screenPosition;

                if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
                    hasSwiped = false;

                if (!hasSwiped && t.phase == TouchPhase.Moved)
                {
                    var diff = t.screenPosition.x - firstTouchPosition.x;
                    diff *= 1 / Screen.dpi;

                    // Debug.Log(diff);

                    if (Mathf.Abs(diff) > 0.2f)
                    {
                        if (diff > 0)
                            laneIndex++;
                        else
                            laneIndex--;

                        hasSwiped = true;
                    }
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                laneIndex--;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                laneIndex++;

            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);

                if (t.phase == UnityEngine.TouchPhase.Began)
                    firstTouchPosition = t.position;

                if (t.phase == UnityEngine.TouchPhase.Canceled || t.phase == UnityEngine.TouchPhase.Ended)
                    hasSwiped = false;

                if (!hasSwiped && t.phase == UnityEngine.TouchPhase.Moved)
                {
                    var diff = t.position.x - firstTouchPosition.x;
                    diff *= 1 / Screen.dpi;

                    // Debug.Log(diff);

                    if (Mathf.Abs(diff) > 0.2f)
                    {
                        if (diff > 0)
                            laneIndex++;
                        else
                            laneIndex--;

                        hasSwiped = true;
                    }
                }
            }
#endif

            laneIndex = Mathf.Clamp(laneIndex, -maxLaneToRight, maxLaneToRight);

            cubeDirection = laneIndex * Vector3.forward * laneSpacing;
        }
    }
}