using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    [Header("Config Player")]
    public int HP;
    public float movementSpeed = 3f;
    private Vector3 direction;
    private bool isWalk;

    //Input
    private float horizontal;
    private float vertical;

    [Header("Attack Config")]
    public ParticleSystem fxAttack;
    public Transform hitBox;
    [Range(0.2f, 1f)]
    public float hitRange = 0.5f;
    public LayerMask hitMask;
    private bool isAttack;
    private int numAttacks;
    public Collider[] hitInfo;
    public int amountDmg;




    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    } 

    // Update is called once per frame
    void Update()
    {
        Inputs();

        MoveCharacter();

        UpdateAnimator();


    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "TakeDamage")
        {
            GetHit(1);
        }
    }

    #region MEUS METODOS

    // MÉTODO RESPONSAVEL PELOS INPUTES FEITOS PELO USER
    void Inputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (numAttacks == 1)
        {
            animator.SetBool("Attack 0", false);
            numAttacks = 0;
        }

        if (Input.GetButtonDown("Fire1") && isAttack == false)
        {
            
            Attack();
            numAttacks = numAttacks + 1;
        }

    }

    void Attack()
    {
        isAttack = true;
        animator.SetBool("Attack 0", true);
        fxAttack.Emit(1);

        hitInfo = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);

        foreach(Collider c in hitInfo)
        {
            c.gameObject.SendMessage("GetHit", amountDmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    // MÉTODO RESPONSAVEL POR MOVER O PERSONAGEM
    void MoveCharacter()
    {
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }

        controller.Move(direction * movementSpeed * Time.deltaTime);
    }

    // MÉTODO RESPONSAVEL POR ATUALIZAR O ANIMATOR
    void UpdateAnimator()
    {
        animator.SetBool("isWalk", isWalk);
    }

    void AttackisDone()
    {
        isAttack = false;
    }

    void GetHit(int ammount)
    {
        HP -= ammount;
        if(HP > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            animator.SetTrigger("Die");
        }
    }

    #endregion



    private void OnDrawGizmosSelected()
    {
        if (hitBox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitBox.position, hitRange);
        }

    }

}
