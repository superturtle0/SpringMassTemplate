All elements have been completed:

(1) Each cube has exactly eight particles that are correctly placed on each of their corners.
(2) There is exactly one spring for every pair of particles, with no duplicates, and their rest lengths match their starting lengths.
(3) The variables pertaining to the ground plane are initialized based off of the ground plane game object.
(4) Ground contact penetration penalty springs have the correct properties, attach at the nearest point on the ground plane.
(5) Ground contact penetration penalty springs are only active when the ground plane is penetrated.
(6) The vertices of the mesh are updated correctly after each simulation loop.
(7) Particle-particle forces are computed correctly and minimally (no redundancy).
(8) Mesh bounds and normals are correct after mesh updates.
(9) Symplectic Euler integration scheme used.
(10) Particles updated with correct update callback and time.
(11) Cube properties match test case.