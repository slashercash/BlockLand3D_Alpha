using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    public bool verticalMove = true;
    public bool horizontalMove = true;
    public int verticalLenght = 1;
    public int horizontalLenght = 1;
    private float verticalRayLength;
    private float horizontalRayLength;
    private string tagFront, tagBack, tagLeft, tagRight;
    private bool detectFront, detectBack, detectLeft, detectRight;
    private bool grounded = true;
    private RaycastHit frontHit, backHit, leftHit, rightHit, downHit;
    private Vector3 boxPosition, targetPosition;
    private PlayerRoboMovement player;
    private float speed;
    private float detectCorrY = 0.5f;



    void Start()
    {
        player = GameObject.Find("PlayerRobo").GetComponent<PlayerRoboMovement>();
        targetPosition = transform.position;
        speed = player.pushSpeed;

        horizontalRayLength = 0.5f + (horizontalLenght * 0.5f);
        verticalRayLength = 0.5f + (verticalLenght * 0.5f);
    }



    void Update()
    {
        if (!player.waitForMove)
        {
            // bewegt die Box zur targetPosition
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
        }


        boxPosition = transform.position;

        // Visualisierung der Raycast Linien
        /*Debug.DrawLine(transform.position + new Vector3(0, detectCorrY, 0), transform.position + new Vector3(0, detectCorrY, 1), Color.green);
          Debug.DrawLine(transform.position + new Vector3(0, detectCorrY, 0), transform.position + new Vector3(0, detectCorrY, -1), Color.green);
          Debug.DrawLine(transform.position + new Vector3(0, detectCorrY, 0), transform.position + new Vector3(-1, detectCorrY, 0), Color.green);
          Debug.DrawLine(transform.position + new Vector3(0, detectCorrY, 0), transform.position + new Vector3(1, detectCorrY, 0), Color.green);*/
    }



    void LateUpdate()
    {
        // Gravitation hinzufügen
        applyGravity();
    }



    // Die Move Funktionen werden vom Player aufgerufen
    public void moveFront()
    {
        targetPosition += Vector3.forward;
    }
    public void moveBack()
    {
        targetPosition += Vector3.back;
    }
    public void moveLeft()
    {
        targetPosition += Vector3.left;
    }
    public void moveRight()
    {
        targetPosition += Vector3.right;
    }



    // detektiert die Umgebung und gibt dem Player zurück, ob die Box in die angefragte Richtung verschoben werden kann
    public bool askDir(int rotAngle)
    {
        if (!grounded)
        {
            return false;
        }

        // Speed wird zurückgesetzt
        speed = player.pushSpeed;

        // detektiert die Umgebung
        detectTags();

        // gibt dem Player zurück, ob die Box in die angefragte Richtung verschoben werden kann
        switch (rotAngle)
        {
    //            private Vector3 front = new Vector3(0, 0, 1);
    //private Vector3 right = new Vector3(1, 0, 0);
    //private Vector3 back = new Vector3(0, 0, -1);
    //private Vector3 left = new Vector3(-1, 0, 0);


            case 0:
                if (tagFront == "" && verticalMove == true)
                    return true;
                else
                    return false;

            case 180:
                if (tagBack == "" && verticalMove == true)
                    return true;
                else
                    return false;

            case 270:
                if (tagLeft == "" && horizontalMove == true)
                    return true;
                else
                    return false;

            case 90:
                if (tagRight == "" && horizontalMove == true)
                    return true;
                else
                    return false;

            default:
                return true;
        }
    }



    void detectTags()
    {
        // detektiert die Collider um die Box
        detectFront = Physics.Linecast(transform.position, transform.position + new Vector3(0, detectCorrY, verticalRayLength), out frontHit);
        detectBack = Physics.Linecast(transform.position, transform.position + new Vector3(0, detectCorrY, -verticalRayLength), out backHit);
        detectLeft = Physics.Linecast(transform.position, transform.position + new Vector3(-horizontalRayLength, detectCorrY, 0), out leftHit);
        detectRight = Physics.Linecast(transform.position, transform.position + new Vector3(horizontalRayLength, detectCorrY, 0), out rightHit);

        // liest die Tags aus, falls Colider detektiert wurden
        if (detectFront == false)
            tagFront = "";
        else
            tagFront = frontHit.collider.tag;
        if (detectBack == false)
            tagBack = "";
        else
            tagBack = backHit.collider.tag;
        if (detectLeft == false)
            tagLeft = "";
        else
            tagLeft = leftHit.collider.tag;
        if (detectRight == false)
            tagRight = "";
        else
            tagRight = rightHit.collider.tag;
    }



    void applyGravity()
    {
        // detektiert die Fläche unter dem Player und misst die Distanz zum nächsten Collider
        Physics.Linecast(transform.position, transform.position + new Vector3(0, -20, 0), out downHit);

        // Funktioniert nicht bei ProbuilderMeshecolider
        //Physics.BoxCast(transform.position, new Vector3(0.4f, 0, 0.4f), Vector3.down, out downHit, transform.rotation, 10);

        // Passt die neue TargetPosition an, mit Hilfe der vorherigen Distanzmessung
        targetPosition.y = boxPosition.y + 0.5f - downHit.distance;
       
        // Bei kleinen Ausgleichungen wird die Deltatime umgangen und die Position direkt gesetzt
        if (downHit.distance < 0.6f)
        {
            detectCorrY = 0;
            transform.position += new Vector3(0, 0.5f - downHit.distance, 0);
            grounded = true;
        }
        else
        {
            detectCorrY = 0.5f;
            detectTags();
            grounded = false;
            speed = player.fallSpeed;


            // liest die Tags aus, falls Colider detektiert werden
            if (detectFront)
            {
                if (frontHit.collider.tag == "Player")
                    player.waitOnDir("Back");
                else
                    player.getTags();
            }
            else if (detectBack)
            {
                if (backHit.collider.tag == "Player")
                    player.waitOnDir("Front");
                else
                    player.getTags();
            }
            else if (detectLeft)
            {
                if (leftHit.collider.tag == "Player")
                    player.waitOnDir("Right");
                else
                    player.getTags();
            }
            else if (detectRight)
            {
                if (rightHit.collider.tag == "Player")
                    player.waitOnDir("Left");
                else
                    player.getTags();
            }
        }       
    }
}
