using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MotorPrototype
{
    /// <summary>Creates a complete arcade driving playground from Unity primitives on Play.</summary>
    public sealed class MotorGameBootstrap : MonoBehaviour
    {
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private readonly Vector3[] checkpoints =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(35f, 0f, 45f),
            new Vector3(70f, 0f, 0f),
            new Vector3(35f, 0f, -45f),
            new Vector3(0f, 0f, -45f)
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartAutomatically()
        {
            if (FindFirstObjectByType<MotorGameBootstrap>() == null)
                new GameObject("MOTOR - prototype").AddComponent<MotorGameBootstrap>();
        }

        private void Start()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.7f, 0.75f, 0.85f);
            BuildTrack();
            BuildScenery();
            ArcadeCarController car = BuildCar();
            SetupCamera(car.transform);
            SetupLight();

            GameObject raceObject = new GameObject("Lap timing and HUD");
            raceObject.transform.SetParent(transform);
            var race = raceObject.AddComponent<LapTimeTrial>();
            race.Initialize(car, checkpoints, BuildGates());
            car.SetRace(race);
        }

        private void BuildTrack()
        {
            Block("Grass field", new Vector3(35, -0.61f, 0), new Vector3(210, 1.2f, 210),
                new Color(0.22f, 0.46f, 0.23f));
            Color asphalt = new Color(0.14f, 0.16f, 0.18f);
            Block("West straight", new Vector3(0, 0.07f, 0), new Vector3(14, 0.16f, 104), asphalt);
            Block("East straight", new Vector3(70, 0.07f, 0), new Vector3(14, 0.16f, 104), asphalt);
            Block("North straight", new Vector3(35, 0.07f, 45), new Vector3(84, 0.16f, 14), asphalt);
            Block("South straight", new Vector3(35, 0.07f, -45), new Vector3(84, 0.16f, 14), asphalt);

            Color paint = new Color(0.95f, 0.89f, 0.61f);
            for (int i = -5; i <= 5; i++)
            {
                float p = i * 8f;
                Block("West dashed line", new Vector3(0, 0.157f, p), new Vector3(0.19f, 0.012f, 3.4f), paint, false);
                Block("East dashed line", new Vector3(70, 0.157f, p), new Vector3(0.19f, 0.012f, 3.4f), paint, false);
                Block("North dashed line", new Vector3(35 + p * 0.75f, 0.157f, 45), new Vector3(3.4f, 0.012f, 0.19f), paint, false);
                Block("South dashed line", new Vector3(35 + p * 0.75f, 0.157f, -45), new Vector3(3.4f, 0.012f, 0.19f), paint, false);
            }

            // Visual start/finish stripes; not collidable.
            for (int i = -3; i <= 3; i++)
                Block("Start grid", new Vector3(i * 2f, 0.16f, -45), new Vector3(1f, 0.02f, 1.9f),
                    Color.white, false);
        }

        private void BuildScenery()
        {
            var rng = new System.Random(2026);
            for (int i = 0; i < 50; i++)
            {
                float x = (float)(rng.NextDouble() * 195 - 62);
                float z = (float)(rng.NextDouble() * 170 - 85);
                if (Mathf.Abs(x) < 17 || Mathf.Abs(x - 70) < 17 ||
                    Mathf.Abs(z - 45) < 17 || Mathf.Abs(z + 45) < 17) continue;

                float height = 2.5f + (float)rng.NextDouble() * 2f;
                Block("Tree trunk", new Vector3(x, height * 0.35f, z),
                    new Vector3(0.5f, height * 0.7f, 0.5f), new Color(0.36f, 0.24f, 0.15f), false);
                Sphere("Tree crown", new Vector3(x, height, z),
                    new Vector3(2.3f, 2.6f, 2.3f), new Color(0.1f, 0.36f, 0.18f), false);
            }
        }

        private ArcadeCarController BuildCar()
        {
            var vehicle = new GameObject("Player car");
            vehicle.transform.SetParent(transform);
            vehicle.transform.SetPositionAndRotation(new Vector3(0, 0.85f, -45f), Quaternion.identity);
            var collider = vehicle.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.85f, 0.72f, 3.3f);

            var rb = vehicle.AddComponent<Rigidbody>();
            rb.mass = 950f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearDamping = 0.4f;

            ChildBlock(vehicle.transform, "Body", new Vector3(0, 0.06f, 0),
                new Vector3(1.7f, 0.56f, 3.05f), new Color(0.95f, 0.26f, 0.1f));
            ChildBlock(vehicle.transform, "Cabin", new Vector3(0, 0.47f, -0.38f),
                new Vector3(1.35f, 0.44f, 1.45f), new Color(0.12f, 0.19f, 0.27f));
            ChildBlock(vehicle.transform, "Windshield", new Vector3(0, 0.49f, 0.41f),
                new Vector3(1.12f, 0.27f, 0.05f), new Color(0.38f, 0.74f, 0.85f));
            for (int side = -1; side <= 1; side += 2)
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Wheel";
                    wheel.transform.SetParent(vehicle.transform, false);
                    wheel.transform.localPosition = new Vector3(0.9f * side, -0.04f, 1.04f * axle);
                    wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    wheel.transform.localScale = new Vector3(0.33f, 0.15f, 0.33f);
                    wheel.GetComponent<Collider>().enabled = false;
                    wheel.GetComponent<Renderer>().sharedMaterial = Material(new Color(0.08f, 0.08f, 0.09f));
                }

            return vehicle.AddComponent<ArcadeCarController>();
        }

        private List<Renderer[]> BuildGates()
        {
            var result = new List<Renderer[]>();
            // Gate orientations: north, east, south, west, north.
            int[] headings = { 0, 90, 180, 270, 0 };
            for (int i = 0; i < checkpoints.Length; i++)
            {
                Vector3 across = Quaternion.Euler(0, headings[i], 0) * Vector3.right;
                Vector3 c = checkpoints[i];
                var left = Block("Checkpoint " + (i + 1) + " left",
                    c - across * 5.6f + Vector3.up * 1.65f,
                    new Vector3(0.38f, 3.3f, 0.38f), Color.cyan, false);
                var right = Block("Checkpoint " + (i + 1) + " right",
                    c + across * 5.6f + Vector3.up * 1.65f,
                    new Vector3(0.38f, 3.3f, 0.38f), Color.cyan, false);
                result.Add(new[] { left.GetComponent<Renderer>(), right.GetComponent<Renderer>() });
            }
            return result;
        }

        private void SetupCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.fieldOfView = 68;
            cam.farClipPlane = 450f;
            cam.clearFlags = CameraClearFlags.Skybox;
            var follow = cam.GetComponent<ArcadeFollowCamera>();
            if (follow == null) follow = cam.gameObject.AddComponent<ArcadeFollowCamera>();
            follow.Target = target;
        }

        private void SetupLight()
        {
            if (FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Sun");
            go.transform.rotation = Quaternion.Euler(48, -32, 0);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
        }

        private GameObject ChildBlock(Transform parent, string name, Vector3 localPos, Vector3 size, Color color)
        {
            GameObject block = Block(name, Vector3.zero, size, color, false);
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPos;
            return block;
        }

        private GameObject Block(string name, Vector3 position, Vector3 scale, Color color, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Material(color);
            if (!solid) go.GetComponent<Collider>().enabled = false;
            return go;
        }

        private GameObject Sphere(string name, Vector3 position, Vector3 scale, Color color, bool solid)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Material(color);
            if (!solid) go.GetComponent<Collider>().enabled = false;
            return go;
        }

        private Material Material(Color color)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color);
            if (materials.TryGetValue(key, out Material existing)) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { color = color };
            materials.Add(key, material);
            return material;
        }

        private void OnDestroy()
        {
            foreach (var entry in materials) if (entry.Value != null) Destroy(entry.Value);
        }
    }
}
