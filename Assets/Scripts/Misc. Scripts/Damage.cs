using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Damage : MonoBehaviour
{
    public enum Direction { Right, Up, Left, Down, UL, UR, DL, DR };
    public enum Type { Melee, Ranged };
    public enum Effect { None, Poison };
    public Entities attacker;

    public Direction direction;// = Direction.Down;
    public Type type;
    public Effect effect;

    public GameObject instanceCreated;
    public int instanceAmount;
    public string special;
    public bool needAttackerDirection;
    public float[] instancesXs;
    public float[] instancesYs;

    public int effectDamage;
    public float effectDuration;
    public float effectDamageCooldown;

    public int damage;// = 5;
    public float dCooldown;
    public float dCount;

    public float cooldown;// = 1f;
    public float count;// = 1f;

    public float damageBoost;
    public float critChance;
    public float critDamage;

    public float speed;// = .2f;
    public float xOffset;
    public float yOffset;

    // Start is called before the first frame update
    void Start()
    {
        //changeDirection(attacker.GetComponent<Entities>().direction.ToString());
        //Debug.Log(direction);
        //Debug.Log(attacker.GetComponent<Entities>().direction);
        findAttacker();

        damageBoost = attacker.damageBonus + 1;
        critChance = attacker.crit;
        damage = (int)(damage * damageBoost);
        Debug.Log(damage);
        if (critChance > 0)
        {
            critical();
            Debug.Log(damage+"\n"+critDamage);
        }

        if (needAttackerDirection)
        {
            List<float> xs = new List<float>();
            List<float> ys = new List<float>();

            for (int i = 1; i <= instanceAmount; i++)
            {
                int ind = i;

                while (ind > 4)
                {
                    ind -= 4;
                }

                int off = (ind % 2 == 1) ?
                    ind / 2 * -1 :
                    ind / 2;
                //Debug.Log(ind);
                int newDirection = (int)attacker.direction + off;

                while (newDirection > 3)
                {
                    newDirection -= 4;
                }
                while (newDirection < 0)
                {
                    newDirection += 4;
                }
                changeDirection(newDirection);
                //Debug.Log(direction);

                xs.Add(changeXOffset());
                ys.Add(changeYOffset());

                //Debug.Log("X: " + changeXOffset() + "\nY: " + changeYOffset());

                changeDirection((int)attacker.direction);
            }

            instancesXs = xs.ToArray();
            instancesYs = ys.ToArray();
        }

        for (int i = 0; i < instanceAmount; i++)
        {
            //checks if there are any specific coordinates to put things
            if (instancesXs.Length > 0)
            {
                int e = i;
                //checks if i is too big to be in the list of xs
                //if so, subtracts by length until it is within range and makes offset whatever that number is
                while (e >= instancesXs.Length)
                {
                    e -= instancesXs.Length;
                }
                
                xOffset = instancesXs[e];
            }

            if (instancesYs.Length > 0)
            {
                int e = i;
                //checks if i is too big to be in the list of ys
                //if so, subtracts by length until it is within range and makes offset whatever that number is
                while (e >= instancesYs.Length)
                {
                    e -= instancesYs.Length;
                }

                yOffset = instancesYs[e];
            }

            if (special == "Trigger")
            {
                xOffset = xOffset * -1;
                yOffset = yOffset * -1;
            }

            if (special == "Weapon")
            {
                instanceCreated = attacker.weapon;
            }

            Instantiate(instanceCreated, new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, -1), Quaternion.identity);
        }

        if (special == "Trigger")
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log("Scream");
        gameObject.transform.Translate(new Vector3(xOffset, yOffset, -1) * speed);
        //Debug.Log(xOffset + "\n" + yOffset);
        if (count <= 0)
        {
            Destroy(this.gameObject);
        }
        count -= Time.deltaTime;
        dCount -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision);
        //findAttacker();
        if (collision.GetComponent<Entities>() != null && collision.GetComponent<Entities>() != attacker && dCount <= 0)
        {
            Entities entity = collision.GetComponent<Entities>();
            //Debug.Log(effect);
            if (entity.isFlying)
            {
                //Debug.Log("Uh oh");
                if (type != Type.Melee)
                {
                    entity.Damage(damage);
                }
            }
            else
            {
                //Debug.Log("hmmm");
                entity.Damage(damage);
            }

            if (effect == Effect.Poison)
            {
                //Debug.Log("Poisn");
                entity.inflictEffect((int)effect, effectDamage, effectDuration, effectDamageCooldown);
            }
            dCount = dCooldown;
            //Debug.Log(entity.health);
            if (special != "Trigger")
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void findAttacker()
    {
        Entities attack = null;
        Entities[] entities = FindObjectsOfType<Entities>();
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Entities potentialAttacker in entities)
        {
            //Debug.Log(potentialAttacker);
            Transform potentialPosition = potentialAttacker.GetComponent<Transform>();
            Vector3 directionToTarget = potentialPosition.position - currentPosition;
            float dSqrToAttacker = directionToTarget.sqrMagnitude;
            if (potentialAttacker.isAttacking)
            {
                if (dSqrToAttacker < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToAttacker;
                    attack = potentialAttacker;
                    attacker = attack;
                    //Debug.Log(attack.GetComponent<Transform>());
                    changeDirection(attacker.direction.ToString());
                    //Debug.Log(attack);
                }
            }
        }
    }

    public void critical()
    {
        float critMod = 10;
        float crit = critChance;

        while (crit/2 > 10)
        {
            crit -= 10;
            critMod += 10;
        }

        float chance = Random.Range(0, critMod);

        if (chance < critChance / 2)
        {
            Debug.Log("Crit!");
            critDamage = (critChance + 1) * damage + 1;
            damage += (int)critDamage;
        }
    }

    public void changeDirection(int dir)
    {
        direction = (Direction)dir;
    }

    private void changeDirection(string dir)
    {
        if (dir.Equals("Up"))
        {
            direction = Direction.Up;
            yOffset = 1;
            xOffset = 0;
        }
        else if (dir.Equals("Down"))
        {
            direction = Direction.Down;
            yOffset = -1;
            xOffset = 0;
        }
        else if (dir.Equals("Right"))
        {
            direction = Direction.Right;
            yOffset = 0;
            xOffset = 1;
        }
        else if (dir.Equals("Left"))
        {
            direction = Direction.Left;
            yOffset = 0;
            xOffset = -1;
        }
        else if (dir.Equals("UL"))
        {
            direction = Direction.UL;
            yOffset = 1;
            xOffset = -1;
        }
        else if (dir.Equals("UR"))
        {
            direction = Direction.UR;
            yOffset = 1;
            xOffset = 1;
        }
        else if (dir.Equals("DL"))
        {
            direction = Direction.DL;
            yOffset = -1;
            xOffset = -1;
        }
        else if (dir.Equals("DR"))
        {
            direction = Direction.DR;
            yOffset = -1;
            xOffset = 1;
        }
    }

    public int changeXOffset()
    {
        int x = 0;

        if (direction == Direction.Left || direction == Direction.DL || direction == Direction.UL)
        {
            x = -1;
        }

        else if (direction == Direction.Right || direction == Direction.DR || direction == Direction.UR)
        {
            x = 1;
        }

        return x;
    }

    public int changeYOffset()
    {
        int y = 0;

        if (direction == Direction.Down || direction == Direction.DL || direction == Direction.DR)
        {
            y = -1;
        }

        else if (direction == Direction.Up || direction == Direction.UL || direction == Direction.UR)
        {
            y = 1;
        }

        return y;
    }
}