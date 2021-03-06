﻿using UnityEngine;
using System.Collections;

public class Robot : MonoBehaviour {

	public float SPEED_LAUNCH = 5.0f;

	public float GRAVITY_SCALE = 1.0f;

	private Camera camera;

    public AudioClip launchSound;

    public GameObject laser;

	void Awake()
	{
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        if(PlayerPrefs.GetInt("startInCheckpoint") != 0)
            camera.transform.position = transform.position;

        GetComponent<SpriteRenderer>().enabled = false;

	}

	// Use this for initialization
	void Start () {
        GameObject g = Instantiate(Resources.Load("RobotInicial"), transform.position, transform.rotation) as GameObject;
        g.GetComponent<Animator>().SetInteger("Skin", gameObject.GetComponent<RobotSkins>().skinNo);
        g.transform.parent = this.transform.parent;

        StartCoroutine("TocarAnimacaoInicial", g);
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.parent == null)
        {
            transform.up = Vector3.Slerp(transform.up, GetComponent<Rigidbody2D>().velocity.normalized, Time.deltaTime);
        }
	}

    IEnumerator TocarAnimacaoInicial(GameObject g)
    {
        yield return new WaitForSeconds(0.857f);
        GameObject.Destroy(g);
        GetComponent<SpriteRenderer>().enabled = true;
    }

	public void launch () {

        if (GetComponent<SpriteRenderer>().enabled == false)
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }

        laser.SetActive(false);

        GetComponent<AudioSource>().PlayOneShot(launchSound);

        camera.GetComponent<CameraValuesScript>().podeMover = true;
		GetComponentInParent<Transform>().position = transform.position + transform.up * 2;

		activeFieldLaunchAnimation ();

		Rigidbody2D rb = GetComponent<Rigidbody2D> ();
		rb.gravityScale = GRAVITY_SCALE;
		rb.velocity = transform.up * SPEED_LAUNCH;

		transform.parent = null;
		GetComponent<BoxCollider2D> ().isTrigger = false;

	}

	void OnCollisionEnter2D(Collision2D coll){
        if (coll.gameObject.tag != "MovePlace" && coll.gameObject.tag == "Fields")
        {
            // Condicoes de ligar o laser - ter comprado
            // Liga o laser
            if (!coll.gameObject.name.StartsWith("A-STA") && 
                !coll.gameObject.name.StartsWith("FAL") && 
                !coll.gameObject.name.StartsWith("FIN") && 
                !coll.gameObject.name.StartsWith("K"))
            {
                if (PlayerPrefs.GetInt("Laser") == 1)
                {
                    laser.SetActive(true);
                }
            }

            // Marca
            if (PlayerPrefs.GetInt("Mark") == 1)
            {
                if (!coll.gameObject.name.StartsWith("FAL") &&
                    !coll.gameObject.name.StartsWith("FIN"))
                {
                    bool passouMark = false;
                    for (int i = 0; i < coll.transform.childCount; i++)
                    {
                        if (coll.transform.GetChild(i).name.StartsWith("Mark"))
                        {
                            passouMark = true;
                            break;
                        }
                    }

                    if (!passouMark)
                    {
                        GameObject g = Instantiate(Resources.Load("Mark"), coll.transform.position, coll.transform.rotation) as GameObject;
                        g.transform.SetParent(coll.transform);
                        g.transform.localScale = new Vector3(3, 3, 1);
                    }
                    
                }
            }

			this.transform.parent = coll.gameObject.transform;
			GetComponent<BoxCollider2D> ().isTrigger = true;
			this.transform.position = coll.gameObject.transform.position;
			this.transform.rotation = coll.gameObject.transform.rotation;
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (0.0f, 0.0f);
			GetComponent<Rigidbody2D> ().gravityScale = 0.0f;
		}
	}

	void goToField (GameObject field) {
		this.transform.position = field.transform.position;
		this.transform.rotation = field.transform.rotation;
		this.transform.parent = field.transform;
	}

	void activeFieldLaunchAnimation () {
		if (transform.parent.gameObject.GetComponent<Animator> ()) {
			transform.parent.gameObject.GetComponent<Animator> ().SetTrigger ("Launch");
		}
	}

    public void destruir() {
        Destroy(this.gameObject);
    }

    public void desligarAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }
}
