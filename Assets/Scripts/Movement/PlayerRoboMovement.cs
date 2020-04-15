using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoboMovement : MonoBehaviour
{
    public string tagFront, tagBack, tagLeft, tagRight, tagFrontLeft, tagFrontRight, tagBackLeft, tagBackRight, ladderDir;
    public float normalSpeed = 5;
    public float pushSpeed = 2;
    public float climbSpeed = 2;
    public float fallSpeed = 10;
    public bool waitForMove = false; // muss public sein
    private float speed, rotSpeed;
    private int newPos, lastPos;
    private string waitForMoveTarget;
    private bool isFree;
    private bool isMoving = false;
    private bool ladderMode = false;
    private bool jumpMode = false;
    private bool idleTrigger;
    private bool detectFront, detectBack, detectLeft, detectRight, detectFrontLeft, detectFrontRight, detectBackLeft, detectBackRight, detectLadderEnd;
    private RaycastHit frontHit, backHit, leftHit, rightHit, downHit, frontLeftHit, frontRightHit, backLeftHit, backRightHit;
    private Vector3 playerPosition, targetPosition;
    private Vector2 animFloat = new Vector2(0, 0);
    private Vector2 targetAnimFloat = new Vector2(0, 0);
    private Quaternion targetRotation;
    private Animator anim;

    // Animations Vektoren
    private Vector2 idle = new Vector2(0, 0);
    private Vector2 running = new Vector2(0, 1);
    private Vector2 pushAction = new Vector2(1, 1);
    private Vector2 climbing = new Vector2(-1, 1);



    void Start ()
    {
        playerPosition = transform.position;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        lastPos = (int) transform.eulerAngles.y;
        speed = normalSpeed;
        getTags();
        anim = GetComponent<Animator>();       
    }



    void Update ()
    {
        // Die Animations Übergangszeit wird Smooth gemacht (Ändert sich langsam über Zeit).
        animFloat = Vector2.MoveTowards(animFloat, targetAnimFloat, normalSpeed * Time.deltaTime);
        anim.SetFloat("posX", animFloat.x);
        anim.SetFloat("posY", animFloat.y);

        // Gravitation hinzufügen
        applyGravity();


        if (isMoving)
        {
            if (!waitForMove)
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

            // rotiert den Player zur targetRotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * normalSpeed * Time.deltaTime);

            // aktualisiert die Position des Players
            playerPosition = transform.position;

            // setzt isMoving zu false, sobald der Player seine targetPosition erreicht hat.
            if (playerPosition == targetPosition)
            {
                isMoving = false;
                jumpMode = false;

                // liest die Tags der Objekte um den Player aus.
                getTags();

                if (ladderMode)
                {
                    // Detektiert ob die Leiter beim nächsten Schritt nach oben zu Ende ist.
                    switch (ladderDir)
                    {
                        case "Front":
                            detectLadderEnd = !Physics.Linecast(transform.position + new Vector3(0, 1, 0), transform.position + new Vector3(0, 1, 1));
                            break;

                        case "Right":
                            detectLadderEnd = !Physics.Linecast(transform.position + new Vector3(0, 1, 0), transform.position + new Vector3(1, 1, 0));
                            break;

                        case "Back":
                            detectLadderEnd = !Physics.Linecast(transform.position + new Vector3(0, 1, 0), transform.position + new Vector3(0, 1, -1));
                            break;

                        case "Left":
                            detectLadderEnd = !Physics.Linecast(transform.position + new Vector3(0, 1, 0), transform.position + new Vector3(-1, 1, 0));
                            break;
                    }

                    // Beendet den ladderMode sobald der Player Bodenkontakt hat.
                    if (downHit.distance < 0.6f)
                    {
                        ladderMode = false;
                        detectLadderEnd = false;

                        waitForAnim(idle, "idle");
                        //targetAnimFloat = idle;
                    }
                }
                else
                {
                    targetAnimFloat = idle;

                    // setzt den Speed auf normal
                    speed = normalSpeed;
                }
            }
        }

        // wenn der Player seine targetPosition erreicht hat, (isMoving = false) können UserInputs getätigt werden.
        if (!isMoving)
        {
            if (!ladderMode && !waitForMove)
            {
                // Diagonal Collision Handler
                if (Input.GetButton("Front") && Input.GetButton("Left") && tagFrontLeft != "" && tagFrontLeft != "Trigger")
                    diagonalCollision(tagFront, tagLeft, Vector3.forward, Vector3.left, 0, 270);

                else if (Input.GetButton("Front") && Input.GetButton("Right") && tagFrontRight != "" && tagFrontRight != "Trigger")
                    diagonalCollision(tagFront, tagRight, Vector3.forward, Vector3.right, 0, 90);

                else if (Input.GetButton("Back") && Input.GetButton("Left") && tagBackLeft != "" && tagBackLeft != "Trigger")
                    diagonalCollision(tagBack, tagLeft, Vector3.back, Vector3.left, 180, 270);

                else if (Input.GetButton("Back") && Input.GetButton("Right") && tagBackRight != "" && tagBackRight != "Trigger")
                    diagonalCollision(tagBack, tagRight, Vector3.back, Vector3.right, 180, 90);

                else if ((Input.GetButton("Front") && Input.GetButton("Back")) || (Input.GetButton("Left") && Input.GetButton("Right")))
                    return;

                // Move Inputs (W, A, S, D)
                else
                {
                    if (Input.GetButton("Front") && tagFront != "Collisionbox")
                    {
                        // Steht vor einer Leiter nach OBEN
                        if (tagFront == "Ladder")
                        {
                            ladderEntryBottom("Front", 0);
                        }
                        // Steht vor einer Leiter nach UNTEN.
                        else if (getTag(transform.position + new Vector3(0, -1, 1), transform.position + new Vector3(0, -1, 0)) == "Ladder" && !Input.GetButton("Left") && !Input.GetButton("Right"))
                        {
                            ladderEntryTop("Back", 180, Vector3.forward);
                        }
                        // Steht vor einem ABGRUND
                        else if (getTag(transform.position + new Vector3(0, 0, 1), transform.position + new Vector3(0, -1, 1)) == "" && !Input.GetButton("Left") && !Input.GetButton("Right"))
                        {
                            prepareJumpPlayer(new Vector3(0, 0, 2), new Vector3(0, -1, 2), new Vector3(0, 0, 3), new Vector3(0, -1, 3), "wantFront", Vector3.forward, 0);
                        }
                        // Normaler Move
                        else
                        {
                            movePlayer(tagFront, frontHit, "wantFront", Vector3.forward, 0);
                        }
                    }

                    if (Input.GetButton("Back") && tagBack != "Collisionbox")
                    {
                        if (tagBack == "Ladder")
                        {
                            ladderEntryBottom("Back", 180);
                        }
                        else if(getTag(transform.position + new Vector3(0, -1, -1), transform.position + new Vector3(0, -1, 0)) == "Ladder" && !Input.GetButton("Left") && !Input.GetButton("Right"))
                        {
                            ladderEntryTop("Front", 0, Vector3.back);
                        }
                        else if (getTag(transform.position + new Vector3(0, 0, -1), transform.position + new Vector3(0, -1, -1)) == "" && !Input.GetButton("Left") && !Input.GetButton("Right"))
                        {
                            prepareJumpPlayer(new Vector3(0, 0, -2), new Vector3(0, -1, -2), new Vector3(0, 0, -3), new Vector3(0, -1, -3), "wantBack", Vector3.back, 180);
                        }
                        else
                        {
                            movePlayer(tagBack, backHit, "wantBack", Vector3.back, 180);
                        }
                    }

                    if (Input.GetButton("Left") && tagLeft != "Collisionbox")
                    {
                        if (tagLeft == "Ladder")
                        {
                            ladderEntryBottom("Left", 270);
                        }
                        else if (getTag(transform.position + new Vector3(-1, -1, 0), transform.position + new Vector3(0, -1, 0)) == "Ladder" && !Input.GetButton("Front") && !Input.GetButton("Back"))
                        {
                            ladderEntryTop("Right", 90, Vector3.left);
                        }
                        else if (getTag(transform.position + new Vector3(-1, 0, 0), transform.position + new Vector3(-1, -1, 0)) == "" && !Input.GetButton("Front") && !Input.GetButton("Back"))
                        {
                            prepareJumpPlayer(new Vector3(-2, 0, 0), new Vector3(-2, -1, 0), new Vector3(-3, 0, 0), new Vector3(-3, -1, 0), "wantLeft", Vector3.left, 270);
                        }
                        else
                        {
                            movePlayer(tagLeft, leftHit, "wantLeft", Vector3.left, 270);
                        }
                    }

                    if (Input.GetButton("Right") && tagRight != "Collisionbox")
                    {
                        if (tagRight == "Ladder")
                        {
                            ladderEntryBottom("Right", 90);
                        }
                        else if (getTag(transform.position + new Vector3(1, -1, 0), transform.position + new Vector3(0, -1, 0)) == "Ladder" && !Input.GetButton("Front") && !Input.GetButton("Back"))
                        {
                            ladderEntryTop("Left", 270, Vector3.right);
                        }
                        else if (getTag(transform.position + new Vector3(1, 0, 0), transform.position + new Vector3(1, -1, 0)) == "" && !Input.GetButton("Front") && !Input.GetButton("Back"))
                        {
                            prepareJumpPlayer(new Vector3(2, 0, 0), new Vector3(2, -1, 0), new Vector3(3, 0, 0), new Vector3(3, -1, 0), "wantRight", Vector3.right, 90);
                        }
                        else
                        {
                            movePlayer(tagRight, rightHit, "wantRight", Vector3.right, 90);
                        }
                    }
                }
            }

            // regelt das auf und absteigen im ladderMode.
            if(ladderMode)
            {
                switch (ladderDir)
                {
                    case "Front":
                        if (Input.GetButton("Front"))
                        {
                            if(detectLadderEnd)
                            {
                                targetPosition.z++;
                                waitForAnimTransission("ladderTopOut", idle);
                            }
                           
                            isMoving = true;
                            targetPosition.y++;
                            
                        }
                        else if (Input.GetButton("Back"))
                        {
                            isMoving = true;
                            targetPosition.y--;
                        }
                        break;

                    case "Right":
                        if (Input.GetButton("Right"))
                        {
                            if (detectLadderEnd)
                            {
                                targetPosition.x++;
                                waitForAnimTransission("ladderTopOut", idle);
                            }

                            isMoving = true;
                            targetPosition.y++;

                        }
                        else if (Input.GetButton("Left"))
                        {
                            isMoving = true;
                            targetPosition.y--;
                        }
                        break;

                    case "Back":
                        if (Input.GetButton("Back"))
                        {
                            if (detectLadderEnd)
                            {
                                targetPosition.z--;
                                waitForAnimTransission("ladderTopOut", idle);
                            }

                            isMoving = true;
                            targetPosition.y++;

                        }
                        else if (Input.GetButton("Front"))
                        {
                            isMoving = true;
                            targetPosition.y--;
                        }
                        break;

                    case "Left":
                        if (Input.GetButton("Left"))
                        {
                            if (detectLadderEnd)
                            {
                                targetPosition.x--;

                                waitForAnimTransission("ladderTopOut", idle);
                            }

                            isMoving = true;
                            targetPosition.y++;

                        }
                        else if (Input.GetButton("Right"))
                        {
                            isMoving = true;
                            targetPosition.y--;
                        }
                        break;
                }
            }
        }

        if (waitForMove)
        {
            switch (waitForMoveTarget)
            {
                case "idle":
                    if (animFloat == idle)
                    {
                        waitForMove = false;
                        speed = normalSpeed;
                    }
                    break;

                case "pushAction":
                    if (animFloat == pushAction)
                    {
                        waitForMove = false;
                        speed = pushSpeed;
                    }
                    break;

                case "climbing":
                    if (animFloat == climbing)
                    {
                        waitForMove = false;
                        speed = climbSpeed;
                    }
                    break;
                case "ladderTopOut":
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("ladderTopOut"))
                    {
                        anim.SetBool("ladderTopOut", false);
                        waitForMove = false;
                    }
                    break;

                case "ladderTopIn":
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("ladderTopIn"))
                    {
                        anim.SetBool("ladderTopIn", false);
                        waitForMove = false;
                    }
                    break;
            }
        }

        // Visualisierung der Raycast Linien
        /*Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0, 1), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0, -1), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(-1, 0, 0), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(1, 0, 0), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(-1, 0, 1), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(1, 0, 1), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(-1, 0, -1), Color.green);
          Debug.DrawLine(transform.position, transform.position + new Vector3(1, 0, -1), Color.green);*/
    }



    private void waitForAnim(Vector2 anim, string target)
    {
        // Setzt die Animation und wartet mit der Aktualisierung der Position bis die Animation erreicht wurde.
        waitForMove = true;
        targetAnimFloat = anim;
        waitForMoveTarget = target;
    }



    private void waitForAnimTransission(String animBool, Vector2 endTransissionAnim)
    {
        // Setzt Spielt die Animation "animBool" einmal durch und hängt dann "endTransissionAnim" als Loop an
        anim.SetBool(animBool, true);
        waitForMoveTarget = animBool;
        targetAnimFloat = endTransissionAnim;
        waitForMove = true;
    }



    private void ladderEntryTop(string Dir, int rot, Vector3 pos)
    {
        waitForAnimTransission("ladderTopIn", climbing);

        speed = climbSpeed;
        ladderMode = true;
        ladderDir = Dir;
        targetPosition += pos;
        rotate(rot);
    }



    private void ladderEntryBottom(string Dir, int rot)
    {
        waitForAnim(climbing, "climbing");
        ladderMode = true;
        ladderDir = Dir;
        rotate(rot);
    }



    private void diagonalCollision(string tagDir1, string tagDir2, Vector3 altDir1, Vector3 altDir2, int altRot1, int altRot2)
    {
        // Bei einer Diagonalen Kollision läuft der Player wenn möglich der Wand entlang.
        if ((tagDir1 == "" && tagDir2 != "" && tagDir2 != "NoDiagonalWalking") || tagDir1 == "NoDiagonalWalking")
        {
            // Fängt einen Allfälligen Sprung auf, beim entlanglaufen einer Wand.
            if (getTag(transform.position + altDir1, transform.position + altDir1 + Vector3.down) == "")
            {
                preparePrepareJumpPlayer(altRot1);
            }
            else
            {
                isMoving = true;
                targetAnimFloat = running;

                targetPosition += altDir1;
                rotate(altRot1);
            }
        }
        else if ((tagDir1 != "" && tagDir2 == "") || tagDir2 == "NoDiagonalWalking")
        {
            if (getTag(transform.position + altDir2, transform.position + altDir2 + Vector3.down) == "")
            {
                preparePrepareJumpPlayer(altRot2);
            }
            else
            {
                isMoving = true;
                targetAnimFloat = running;

                targetPosition += altDir2;
                rotate(altRot2);
            }
        }
    }



    private void movePlayer(string tagDir, RaycastHit dirHit, string wantDir, Vector3 vectorDir, int rotAngle)
    {
        isMoving = true;

        if (tagDir == "Box")
        {
            // fragt bei der Box nach, ob sie in die jeweilige Richtung verschoben werden kann
            isFree = dirHit.collider.GetComponent<BoxMovement>().askDir(rotAngle);
            if (isFree)
            {
                waitForAnim(pushAction, "pushAction");

                // die Box wird in die gewünschte Richtung verschoben
                switch (wantDir)
                {
                    case "wantFront":
                        frontHit.collider.GetComponent<BoxMovement>().moveFront();
                        break;
                    case "wantLeft":
                        leftHit.collider.GetComponent<BoxMovement>().moveLeft();
                        break;
                    case "wantBack":
                        backHit.collider.GetComponent<BoxMovement>().moveBack();
                        break;
                    case "wantRight":
                        rightHit.collider.GetComponent<BoxMovement>().moveRight();
                        break;
                }

                // Der Player erhaltet seinen neuen Targetpoint und Targetrotation
                targetPosition += vectorDir;
                rotate(rotAngle);
            }
            else
            {
                targetAnimFloat = idle;
            }
        }
        else
        {
            targetAnimFloat = running;
            // Der Player erhaltet seinen neuen Targetpoint und Targetrotation
            targetPosition += vectorDir;
            setRotation(rotAngle);
        }
    }



    private void preparePrepareJumpPlayer(int direction)
    {
        switch (direction)
        {
            case 0:
                prepareJumpPlayer(new Vector3(0, 0, 2), new Vector3(0, -1, 2), new Vector3(0, 0, 3), new Vector3(0, -1, 3), "wantFront", Vector3.forward, 0);
                break;
            case 90:
                prepareJumpPlayer(new Vector3(2, 0, 0), new Vector3(2, -1, 0), new Vector3(3, 0, 0), new Vector3(3, -1, 0), "wantRight", Vector3.right, 90);
                break;
            case 180:
                prepareJumpPlayer(new Vector3(0, 0, -2), new Vector3(0, -1, -2), new Vector3(0, 0, -3), new Vector3(0, -1, -3), "wantBack", Vector3.back, 180);
                break;
            case 270:
                prepareJumpPlayer(new Vector3(-2, 0, 0), new Vector3(-2, -1, 0), new Vector3(-3, 0, 0), new Vector3(-3, -1, 0), "wantLeft", Vector3.left, 270);
                break;
        }
    }



    private void prepareJumpPlayer(Vector3 shortJumpLandingStart, Vector3 shortJumpLandingEnd, Vector3 longJumpLandingStart, Vector3 longJumpLandingEnd, String wantDir, Vector3 playerMoveDir, int rotAngle)
    {
        // Wenn der Player einen Abgrund Runterlaufen will, wird zuerst die Möglichkeit auf einen kurzen oder Langen Sprung überprüft und durchgeführt. Ansonsten läuft er einfach hinunter.
        if (getTag(transform.position + shortJumpLandingStart, transform.position + shortJumpLandingEnd) != "" && getTag(transform.position + playerMoveDir, transform.position + shortJumpLandingStart) == "")
        {
            jumpPlayer(shortJumpLandingStart, rotAngle);
        }
        else if (getTag(transform.position + longJumpLandingStart, transform.position + longJumpLandingEnd) != "" && getTag(transform.position + playerMoveDir, transform.position + longJumpLandingStart) == "")
        {
            jumpPlayer(longJumpLandingStart, rotAngle);
        }
        else
        {
            movePlayer(tagFront, frontHit, wantDir, playerMoveDir, rotAngle);
        }
    }


    private void jumpPlayer(Vector3 jumpDir, int rotAngle)
    {
        isMoving = true;
        jumpMode = true;
        speed = fallSpeed;
        targetAnimFloat = idle; // später Jump Animation
        
        // Der Player erhaltet seinen neuen Targetpoint und Targetrotation
        targetPosition += jumpDir;
        rotate(rotAngle);
    }



    private void setRotation(int mainRot)
    {
        // Hier wird die PlayerRotation noch angepasst falls noch andere Buttons gedrückt sind.
        if (mainRot == 0)
        {
        if (Input.GetButton("Left"))
            rotate(315);
        else if (Input.GetButton("Right"))
            rotate(45);
        else if (!Input.GetButton("Back"))
            rotate(mainRot);
        }
        else if (mainRot == 180)
        {
            if (Input.GetButton("Left"))
                rotate(225);
            else if (Input.GetButton("Right"))
                rotate(135);
            else if (!Input.GetButton("Front"))
                rotate(mainRot);
        }
        else if (mainRot == 270)
        {
            if (!Input.GetButton("Front") && !Input.GetButton("Back") && !Input.GetButton("Right"))
                rotate(mainRot);
        }
        else if (mainRot == 90)
        {
            if (!Input.GetButton("Front") && !Input.GetButton("Back") && !Input.GetButton("Left"))
                rotate(mainRot);
        }
    }



    void rotate(int newPos)
    {
        // Der Länge der Rotation wird in Grad ausgerechnet und dazu passend die Rotationsgeschwindigkeit angepasst.
        if (newPos < lastPos)
            rotSpeed = lastPos - newPos;
        else
            rotSpeed = newPos - lastPos;

        if (rotSpeed > 180)
            rotSpeed -= (2*(rotSpeed - 180));

        // die targetRotation des Players wird der Blickrichtung angepasst
        targetRotation *= Quaternion.AngleAxis(newPos - lastPos, Vector3.up);

        // die newPos wird für den nächsten Aufruf als lastPos gespeichert.
        lastPos = newPos;
    }



    public String getTag(Vector3 fromDir, Vector3 toDir)
    {
        RaycastHit hit;
        if (Physics.Linecast(fromDir, toDir, out hit))
            return hit.collider.tag;
        else
            return "";
    }


    public void getTags()
    {
        // detektiert die Collider um den Player
        detectFront = Physics.Linecast(transform.position, transform.position + new Vector3(0, 0, 1), out frontHit);
        detectBack = Physics.Linecast(transform.position, transform.position + new Vector3(0, 0, -1), out backHit);
        detectLeft = Physics.Linecast(transform.position, transform.position + new Vector3(-1, 0, 0), out leftHit);
        detectRight = Physics.Linecast(transform.position, transform.position + new Vector3(1, 0, 0), out rightHit);
        detectFrontLeft = Physics.Linecast(transform.position, transform.position + new Vector3(-1, 0, 1), out frontLeftHit);
        detectFrontRight = Physics.Linecast(transform.position, transform.position + new Vector3(1, 0, 1), out frontRightHit);
        detectBackLeft = Physics.Linecast(transform.position, transform.position + new Vector3(-1, 0, -1), out backLeftHit);
        detectBackRight = Physics.Linecast(transform.position, transform.position + new Vector3(1, 0, -1), out backRightHit);

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
        if (detectFrontLeft == false)
            tagFrontLeft = "";
        else
            tagFrontLeft = frontLeftHit.collider.tag;
        if (detectFrontRight == false)
            tagFrontRight = "";
        else
            tagFrontRight = frontRightHit.collider.tag;
        if (detectBackLeft == false)
            tagBackLeft = "";
        else
            tagBackLeft = backLeftHit.collider.tag;
        if (detectBackRight == false)
            tagBackRight = "";
        else
            tagBackRight = backRightHit.collider.tag;
    }



    void applyGravity()
    {
        // detektiert die Fläche unter dem Player und misst die Distanz zum nächsten Collider
        Physics.Linecast(transform.position, transform.position + new Vector3(0, -20, 0), out downHit);

        // Funktioniert nicht bei ProbuilderMeshecolider
        //Physics.BoxCast(transform.position, new Vector3(0.25f, 0, 0.25f), Vector3.down, out downHit, transform.rotation, 20);

        if(!ladderMode && !jumpMode)
        {
            // Passt die neue TargetPosition an, mit Hilfe der vorherigen Distanzmessung
            targetPosition.y = playerPosition.y + 0.5f - downHit.distance;

            // Bei kleinen Ausgleichungen wird die Deltatime umgangen und die Position direkt gesetzt
            if (downHit.distance < 0.6f)
            {
                transform.position += new Vector3(0, 0.5f - downHit.distance, 0);
            }
            // Ansonsten befindet sich der Player im freien Fall also wird die Geschwindigkeit angepasst.
            else
            {
                speed = fallSpeed;
            }
        }
    }



    public void waitOnDir(string dir)
    {
        switch (dir)
        {
            case "Front":
                tagFront = "Collisionbox";
                break;
            case "Back":
                tagBack = "Collisionbox";
                break;
            case "Left":
                tagLeft = "Collisionbox";
                break;
            case "Right":
                tagRight = "Collisionbox";
                break;
            case "Free":
                getTags();
                break;
        }
    }
}
