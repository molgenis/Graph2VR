using UnityEngine;

/// <summary>
/// Given four points in 3D space, solves for a sphere such that all four points
/// lie on the sphere's surface.
/// </summary>
/// <remarks>
/// Translated from Javascript on http://www.convertalot.com/sphere_solver.html, originally
/// linked to by http://stackoverflow.com/questions/13600739/calculate-centre-of-sphere-whose-surface-contains-4-points-c.
/// Changed to work with unity
/// </remarks>
/// 
public class CircumcentreSolver
{
    private const float ZERO = 0;
    private float m_X0, m_Y0, m_Z0;
    private float m_Radius;
    private float[,] P =
            {
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO }
            };

    /// <summary>
    /// The centre of the resulting sphere.
    /// </summary>
    public Vector3 Centre {
        get { return new Vector3(this.m_X0, this.m_Y0, this.m_Z0); }
    }

    /// <summary>
    /// The radius of the resulting sphere.
    /// </summary>
    public float Radius {
        get { return this.m_Radius; }
    }

    /// <summary>
    /// Whether the result was a valid sphere.
    /// </summary>
    public bool Valid {
        get { return this.m_Radius != 0; }
    }

    public CircumcentreSolver(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        this.Compute(
            new float[] { a.x, a.y, a.z },
            new float[] { b.x, b.y, b.z },
            new float[] { c.x, c.y, c.z },
            new float[] { d.x, d.y, d.z }
        );
    }

    /// <summary>
    /// Computes the centre of a sphere such that all four specified points in
    /// 3D space lie on the sphere's surface.
    /// </summary>
    /// <param name="a">The first point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="b">The second point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="c">The third point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="d">The fourth point (array of 3 doubles for X, Y, Z).</param>
    public CircumcentreSolver(float[] a, float[] b, float[] c, float[] d)
    {
        this.Compute(a, b, c, d);
    }

    /// <summary>
    /// Evaluate the determinant.
    /// </summary>
    private void Compute(float[] a, float[] b, float[] c, float[] d)
    {
        P[0, 0] = a[0];
        P[0, 1] = a[1];
        P[0, 2] = a[2];
        P[1, 0] = b[0];
        P[1, 1] = b[1];
        P[1, 2] = b[2];
        P[2, 0] = c[0];
        P[2, 1] = c[1];
        P[2, 2] = c[2];
        P[3, 0] = d[0];
        P[3, 1] = d[1];
        P[3, 2] = d[2];

        // Compute result sphere.
        this.Sphere();
    }

    private void Sphere()
    {
        float r, m11, m12, m13, m14, m15;
        float[,] a =
                {
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO }
                };

        // Find minor 1, 1.
        for (int i = 0; i < 4; i++) {
            a[i, 0] = P[i, 0];
            a[i, 1] = P[i, 1];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m11 = this.Determinant(a, 4);

        // Find minor 1, 2.
        for (int i = 0; i < 4; i++) {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 1];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m12 = this.Determinant(a, 4);

        // Find minor 1, 3.
        for (int i = 0; i < 4; i++) {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m13 = this.Determinant(a, 4);

        // Find minor 1, 4.
        for (int i = 0; i < 4; i++) {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 1];
            a[i, 3] = 1;
        }
        m14 = this.Determinant(a, 4);

        // Find minor 1, 5.
        for (int i = 0; i < 4; i++) {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 1];
            a[i, 3] = P[i, 2];
        }
        m15 = this.Determinant(a, 4);

        // Calculate result.
        if (m11 == 0) {
            this.m_X0 = 0;
            this.m_Y0 = 0;
            this.m_Z0 = 0;
            this.m_Radius = 0;
        } else {
            this.m_X0 = 0.5f * m12 / m11;
            this.m_Y0 = -0.5f * m13 / m11;
            this.m_Z0 = 0.5f * m14 / m11;
            this.m_Radius = (float)System.Math.Sqrt(this.m_X0 * this.m_X0 + this.m_Y0 * this.m_Y0 + this.m_Z0 * this.m_Z0 - m15 / m11);
        }
    }

    /// <summary>
    /// Recursive definition of determinate using expansion by minors.
    /// </summary>
    private float Determinant(float[,] a, int n)
    {
        int i, j, j1, j2;
        float d = 0;
        float[,] m =
                {
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO }
                };

        if (n == 2) {
            // Terminate recursion.
            d = a[0, 0] * a[1, 1] - a[1, 0] * a[0, 1];
        } else {
            d = 0;
            for (j1 = 0; j1 < n; j1++) // Do each column.
            {
                for (i = 1; i < n; i++) // Create minor.
                {
                    j2 = 0;
                    for (j = 0; j < n; j++) {
                        if (j == j1) continue;
                        m[i - 1, j2] = a[i, j];
                        j2++;
                    }
                }

                // Sum (+/-)cofactor * minor.
                d = d + (float)System.Math.Pow(-1.0f, j1) * a[0, j1] * this.Determinant(m, n - 1);
            }
        }

        return d;
    }
}