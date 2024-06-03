using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlimeA : MonoBehaviour
{
    private GameManager _GameManager;
    private Animator anim;
    public ParticleSystem hitEffect;
    public int HP;

    private bool isDead;

    public enemyState state;
    private int rand;



    //IA
    private bool iswalk;
    private bool isalert;
    private bool isAttack;
    private bool isPlayerVisible;
    private NavMeshAgent agent;
    private int idwaypoint;
    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        _GameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ChangeState(state);
    }

    // Update is called once per frame
    void Update()
    {
        StateManager();

        if (agent.desiredVelocity.magnitude >= 0.1f)
        {
            iswalk = true;
        }
        else
        {
            iswalk = false;
        }

        anim.SetBool("isWalk", iswalk);
        anim.SetBool("isAlert", isalert);
    }



    #region MEUS MÉTODOS

    void GetHit(int amount)
    {

        if (isDead == true) { return; }

        HP -= amount;

        if (HP > 0)
        {
            ChangeState(enemyState.FURY);
            anim.SetTrigger("GetHit");
            hitEffect.Emit(25);
        }
        else
        {
            ChangeState(enemyState.DEAD);
            anim.SetTrigger("Die");
            StartCoroutine("Died");
        }


    }

    IEnumerator Died()
    {
        isDead = true;
        yield return new WaitForSeconds(2.5f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

        if(_GameManager.gameState != GameState.GAMEPLAY) { return; }

        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = true;
            if (state == enemyState.IDLE || state == enemyState.PATROL)
            {
                ChangeState(enemyState.ALERT);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = false;
        }
    }
    void StateManager()
    {

        if(_GameManager.gameState == GameState.DEAD && (state == enemyState.FOLLOW || state == enemyState.FURY || state == enemyState.ALERT))
        {
            ChangeState(enemyState.IDLE);
        }

        switch (state)
        {

            case enemyState.ALERT:
                LookAt();
                break;

            case enemyState.FOLLOW:
                LookAt();
                destination = _GameManager.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;

            case enemyState.FURY:
                LookAt();
                destination = _GameManager.player.position;
                agent.destination = destination;
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;

            case enemyState.PATROL: { break; }
        }
    }

    void ChangeState(enemyState newState)
    {
        StopAllCoroutines();  // Encerra todas as Coroutines
        isalert = false;


        switch (newState)
        {
            case enemyState.IDLE:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;

                StartCoroutine("IDLE");
                break;

            case enemyState.ALERT:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                isalert = true;
                StartCoroutine("ALERT");
                break;

            case enemyState.PATROL:
                agent.stoppingDistance = 0;
                idwaypoint = Random.Range(0, _GameManager.slimesWayPoints.Length);
                destination = _GameManager.slimesWayPoints[idwaypoint].position;
                agent.destination = destination;

                StartCoroutine("PATROL");

                break;

            case enemyState.FURY:
                destination = transform.position;
                agent.stoppingDistance = _GameManager.slimedistancetoattack;
                agent.destination = destination;

                break;

            case enemyState.FOLLOW:

                agent.stoppingDistance = _GameManager.slimedistancetoattack;

                break;

            case enemyState.DEAD:
                destination = transform.position;
                agent.destination = destination;

                break;
        }

        state = newState;
    }


    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_GameManager.slimeIdleWaitTime);

        StayStill(50); //50% chance de ficar entrar no IDLE ou PATROL


    }

    IEnumerator PATROL()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0);

        StayStill(30);
    }

    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_GameManager.slimeAlertTime);

        if (isPlayerVisible == true)
        {
            ChangeState(enemyState.FOLLOW);
        }
        else
        {
            StayStill(10);
        }


    }

    IEnumerator ATTACK()
    {
        yield return new WaitForSeconds(_GameManager.slimeAttackDelay);
        isAttack = false;
    }

    void StayStill(int yes)
    {
        if (Rand() <= yes)
        {
            
            ChangeState(enemyState.IDLE);
        }
        else
        {
            
            ChangeState(enemyState.PATROL);
        }
    }
    int Rand()
    {
        rand = Random.Range(0, 100);  // 0,1,2,....,99
        return rand;
    }

    void Attack()
    {
        if (isAttack == false && isPlayerVisible == true && agent.remainingDistance <= agent.stoppingDistance)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
        }
        StartCoroutine("ATTACK");


    }

    void AttackisDone()
    {
        StartCoroutine("ATTACK");
    }

    void LookAt()
    {


        Vector3 lookDirection = (_GameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _GameManager.slimeLookAtSpeed * Time.deltaTime);

    }


    #endregion
}
