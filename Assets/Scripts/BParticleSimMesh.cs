using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Check this out we can require components be on a game object!
[RequireComponent(typeof(MeshFilter))]

public class BParticleSimMesh : MonoBehaviour
{
    public struct BSpring
    {
        public float kd;                        // damping coefficient
        public float ks;                        // spring coefficient
        public float restLength;                // rest length of this spring
        public int attachedParticle;            // index of the attached other particle (use me wisely to avoid doubling springs and sprign calculations)
    }

    public struct BContactSpring
    {
        public float kd;                        // damping coefficient
        public float ks;                        // spring coefficient
        public float restLength;                // rest length of this spring (think about this ... may not even be needed o_0
        public Vector3 attachPoint;             // the attached point on the contact surface
    }

    public struct BParticle
    {
        public Vector3 position;                // position information
        public Vector3 velocity;                // velocity information
        public float mass;                      // mass information
        public BContactSpring contactSpring;    // Special spring for contact forces
        public bool attachedToContact;          // is thi sparticle currently attached to a contact (ground plane contact)
        public List<BSpring> attachedSprings;   // all attached springs, as a list in case we want to modify later fast
        public Vector3 currentForces;           // accumulate forces here on each step        
    }

    public struct BPlane
    {
        public Vector3 position;                // plane position
        public Vector3 normal;                  // plane normal
    }

    public float contactSpringKS = 1000.0f;     // contact spring coefficient with default 1000
    public float contactSpringKD = 20.0f;       // contact spring daming coefficient with default 20

    public float defaultSpringKS = 100.0f;      // default spring coefficient with default 100
    public float defaultSpringKD = 1.0f;        // default spring daming coefficient with default 1

    public bool debugRender = false;            // To render or not to render


    /*** 
     * I've given you all of the above to get you started
     * Here you need to publicly provide the:
     * - the ground plane transform (Transform)
     * - handlePlaneCollisions flag (bool)
     * - particle mass (float)
     * - useGravity flag (bool)
     * - gravity value (Vector3)
     * Here you need to privately provide the:
     * - Mesh (Mesh)
     * - array of particles (BParticle[])
     * - the plane (BPlane)
     ***/

    public Transform groundPlane;
    public bool handlePlaneCollisions = true;
    public float particleMass = 1f;
    public bool useGravity = true;
    public Vector3 gravityValue = new Vector3(0f, -9.8f, 0f);

    private Mesh mesh;
    private BParticle[] particles;
    private BPlane plane;

    /// <summary>
    /// Init everything
    /// HINT: in particular you should probbaly handle the mesh, init all the particles, and the ground plane
    /// HINT 2: I'd for organization sake put the init particles and plane stuff in respective functions
    /// HINT 3: Note that mesh vertices when accessed from the mesh filter are in local coordinates.
    ///         This script will be on the object with the mesh filter, so you can use the functions
    ///         transform.TransformPoint and transform.InverseTransformPoint accordingly 
    ///         (you need to operate on world coordinates, and render in local)
    /// HINT 4: the idea here is to make a mathematical particle object for each vertex in the mesh, then connect
    ///         each particle to every other particle. Be careful not to double your springs! There is a simple
    ///         inner loop approach you can do such that you attached exactly one spring to each particle pair
    ///         on initialization. Then when updating you need to remember a particular trick about the spring forces
    ///         generated between particles. 
    /// </summary>
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        InitParticles();
        InitPlane();
    }

    void InitPlane()
    {
        
    }

    void InitParticles()
    {
        particles = new BParticle[mesh.vertices.Length];
        for (int particleIndex = 0; particleIndex < particles.Length; particleIndex++)
        {
            BParticle part = new BParticle();
            part.position = transform.TransformPoint(mesh.vertices[particleIndex]);
            part.velocity = Vector3.zero;
            part.mass = particleMass;

            BContactSpring cspr = new BContactSpring();
            cspr.kd = contactSpringKD;
            cspr.ks = contactSpringKS;
            cspr.restLength = 1f; //change later, idk what is meant by "think about it"
            cspr.attachPoint = part.position;
            part.contactSpring = cspr;

            part.attachedToContact = false;
            part.attachedSprings = new List<BSpring>();

            for (int otherParticle = particleIndex + 1; otherParticle < particles.Length; otherParticle++)
            {
                BSpring spr = new BSpring();
                spr.kd = defaultSpringKD;
                spr.ks = defaultSpringKS;
                spr.restLength = 1f; //same thing, change later
                spr.attachedParticle = otherParticle;
                part.attachedSprings.Add(spr);
            }

            part.currentForces = Vector3.zero;

            particles[particleIndex] = part;
        }
    }



    /*** BIG HINT: My solution code has as least the following functions
     * InitParticles()
     * InitPlane()
     * UpdateMesh() (remember the hint above regarding global and local coords)
     * ResetParticleForces()
     * ...
     ***/

    private void ResetParticleForces()
    {
        for (int particleIndex = 0; particleIndex < mesh.vertices.Length; particleIndex++)
        {
            particles[particleIndex].currentForces = Vector3.zero;
        }
    }

    void UpdateMesh()
    {
        Vector3[] newVertices = new Vector3[mesh.vertices.Length];
        for(int particleIndex = 0; particleIndex < mesh.vertices.Length; particleIndex++)
        {
            foreach(BSpring spr in particles[particleIndex].attachedSprings)
            {
                Vector3 sprForce = ParticleParticleForce(particles[particleIndex], spr);
                particles[particleIndex].currentForces += sprForce;
                particles[spr.attachedParticle].currentForces -= sprForce;
                //if (spr.attachedParticle == mesh.vertices.Length - 1) connected++;
                //if (spr.attachedParticle == mesh.vertices.Length - 1) Debug.Log(particles[spr.attachedParticle].currentForces);
                //if (particleIndex == mesh.vertices.Length - 1) Debug.Log("PARTICLE" + particles[particleIndex].currentForces);
            }
            //Debug.Log("----");
            //Debug.Log(connected);
            particles[particleIndex].velocity += particles[particleIndex].currentForces / particles[particleIndex].mass;
            particles[particleIndex].position += particles[particleIndex].velocity * Time.deltaTime;
            newVertices[particleIndex] = transform.InverseTransformPoint(particles[particleIndex].position);
        }
        mesh.SetVertices(newVertices);
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    Vector3 ParticleParticleForce2(BParticle part, BSpring spr)
    {
        return new Vector3(0, 1, 0);
    }

    Vector3 ParticleParticleForce(BParticle part, BSpring spr)
    {
        BParticle otherPart = particles[spr.attachedParticle];
        Vector3 posDiff = part.position - otherPart.position;
        float diffMag = posDiff.magnitude;
        Vector3 diffNormalized = posDiff / diffMag;
        Vector3 springTerm = spr.ks * ((spr.restLength - diffMag) * diffNormalized);
        Vector3 dampTerm = spr.kd * Vector3.Dot((part.velocity - otherPart.velocity), diffNormalized) * diffNormalized;
        return springTerm - dampTerm;
    }


    /// <summary>
    /// Draw a frame with some helper debug render code
    /// </summary>
    public void Update()
    {
        ResetParticleForces();
        UpdateMesh();
        
        if (debugRender)
        {
            int particleCount = particles.Length;
            for (int i = 0; i < particleCount; i++)
            {
                Debug.DrawLine(particles[i].position, particles[i].position + particles[i].currentForces, Color.blue);

                int springCount = particles[i].attachedSprings.Count;
                for (int j = 0; j < springCount; j++)
                {
                    Debug.DrawLine(particles[i].position, particles[particles[i].attachedSprings[j].attachedParticle].position, Color.red);
                }
            }
        }
        
    }
}
