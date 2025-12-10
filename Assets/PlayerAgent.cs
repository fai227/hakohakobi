using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PlayerAgent : Agent
{
    [Header("References")]
    [Tooltip("固定のゴール（青）。Transform を割り当てる。IsTrigger=ON 推奨")]
    public Transform goal;

    [Header("Movement")]
    [Tooltip("等速移動の速度（水平速度）。")]
    public float moveSpeed = 5f;

    [Header("Respawn (Player Only)")]
    [Tooltip("初期位置を中心に x±spawnHalfRange.x, z±spawnHalfRange.y の範囲でリスポーン")]
    public Vector2 spawnHalfRange = new Vector2(2f, 2f);
    [Tooltip("接地高さ（箱の底面が地面に乗るよう調整）。例：キューブ(1m)なら 0.5")]
    public float spawnY = 0.5f;

    [Header("Rewards")]
    [Tooltip("距離が縮むごとの微小報酬係数")]
    public float distanceRewardScale = 0.01f;
    [Tooltip("1 ステップあたりの小ペナルティ（短時間で到達を促す）")]
    public float stepPenalty = 1f;

    private Rigidbody rb;
    private Vector3 startPosPlayer;
    private float lastDistanceToGoal;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        // 物理の安定化（箱が転がらないように）
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // シーン上で配置した位置を「初期位置」として記録
        startPosPlayer = transform.position;

        // ゴールは固定：位置は記録しても書き換えない
        if (goal == null)
        {
            Debug.LogError("[PlayerAgent] goal が未設定です。インスペクタで割り当ててください。");
        }
    }

    public override void OnEpisodeBegin()
    {
        // 速度リセット
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ── Player だけをリスポーン（goal は動かさない）──
        Vector3 p = new Vector3(
            startPosPlayer.x + Random.Range(-spawnHalfRange.x, spawnHalfRange.x),
            spawnY,
            startPosPlayer.z + Random.Range(-spawnHalfRange.y, spawnHalfRange.y)
        );
        transform.position = p;

        // 初期距離
        if (goal != null)
            lastDistanceToGoal = Vector3.Distance(transform.position, goal.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 目標までの相対位置（XZ）
        Vector3 rel = (goal.position - transform.position);
        sensor.AddObservation(rel.x / 10f); // スケールは適当に正規化（迷路10m縦なら 10 など）
        sensor.AddObservation(rel.z / 10f);

        // 現在の水平速度
        sensor.AddObservation(rb.velocity.x / 10f);
        sensor.AddObservation(rb.velocity.z / 10f);

        // 合計 4 次元（必要に応じて Ray センサーを別途アタッチ可）
    }


    private int stepCount = 0;
    private float timer = 0f;


    public override void OnActionReceived(ActionBuffers actions)
    {
        int a = actions.DiscreteActions[0];

        // 待機/前/後/左/右
        Vector3 dir = Vector3.zero;
        switch (a)
        {
            case 1: dir = Vector3.forward; break; // W
            case 2: dir = Vector3.back; break; // S
            case 3: dir = Vector3.left; break; // A
            case 4: dir = Vector3.right; break; // D
                                                // 0: 待機
        }

        // 等速：水平速度を直接セット（慣性なしの操作感）
        Vector3 v = dir * moveSpeed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);

        // 距離が縮んだら微小報酬
        float d = Vector3.Distance(transform.position, goal.position);
        float delta = lastDistanceToGoal - d;
        AddReward(delta * distanceRewardScale);
        lastDistanceToGoal = d;

        // ステップごとの微小ペナルティ
        if (MaxStep > 0)
            AddReward(-stepPenalty / MaxStep);




        stepCount++;
        timer += Time.deltaTime;

        // 1秒ごとにステップ数を出力
        if (timer >= 1f)
        {
            Debug.Log("Steps per second: " + stepCount);
            stepCount = 0;
            timer = 0f;
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;
        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        else if (Input.GetKey(KeyCode.S)) da[0] = 2;
        else if (Input.GetKey(KeyCode.A)) da[0] = 3;
        else if (Input.GetKey(KeyCode.D)) da[0] = 4;
    }

    private void OnTriggerEnter(Collider other)
    {
        // goal は固定。到達で +1 & エピソード終了 → OnEpisodeBegin() が呼ばれて player のみ再配置
        if (other.transform == goal || other.CompareTag("goal"))
        {
            AddReward(+10f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 壁に当たったら微ペナルティ（任意）
        if (collision.collider.CompareTag("wall"))
        {
            AddReward(-0.15f);
        }
    }
}
