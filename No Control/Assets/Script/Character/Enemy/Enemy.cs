using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
  public float damage;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<Character>().TakeDamage(damage);
            }
    }

}