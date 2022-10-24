using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LambdaTwist : MonoBehaviour
{

    // Computes the eigen decomposition of a 3x3 matrix given that one eigenvalue is zero.
    void compute_eig3x3known0(float[,] M, out float[,] E, out float sig1, out float sig2) {

        E = new float[3, 3];
        // In the original paper there is a missing minus sign here (for M(0,0))
        float p1 = -M[0, 0] - M[1, 1] - M[2, 2];
        float p0 = -M[0, 1] * M[0, 1] - M[0, 2] * M[0, 2] - M[1, 2] * M[1, 2] + M[0, 0] * (M[1, 1] + M[2, 2]) + M[1, 1] * M[2, 2];


        float disc = Mathf.Sqrt(p1 * p1 / 4.0f - p0);
        float tmp = -p1 / 2.0f;
        sig1 = tmp + disc;
        sig2 = tmp - disc;

        if (Mathf.Abs(sig1) < Mathf.Abs(sig2)) {
            float temp = sig2;
            sig2 = sig1;
            sig1 = temp;
        }

        float c = sig1 * sig1 + M[0, 0] * M[1, 1] - sig1 * (M[0, 0] + M[1, 1]) - M[0, 1] * M[0, 1];
        float a1 = (sig1 * M[0, 2] + M[0, 1] * M[1, 2] - M[0, 2] * M[1, 1]) / c;
        float a2 = (sig1 * M[1, 2] + M[0, 1] * M[0, 2] - M[0, 0] * M[1, 2]) / c;
        float n = 1.0f / Mathf.Sqrt(1 + a1 * a1 + a2 * a2);
        E[0, 0] = a1 * n;
        E[1, 0] = a2 * n;
        E[2, 0] = n;

        c = sig2 * sig2 + M[0, 0] * M[1, 1] - sig2 * (M[0, 0] + M[1, 1]) - M[0, 1] * M[0, 1];
        a1 = (sig2 * M[0, 2] + M[0, 1] * M[1, 2] - M[0, 2] * M[1, 1]) / c;
        a2 = (sig2 * M[1, 2] + M[0, 1] * M[0, 2] - M[0, 0] * M[1, 2]) / c;
        n = 1.0f / Mathf.Sqrt(1 + a1 * a1 + a2 * a2);
        E[0, 1] = a1 * n;
        E[1, 1] = a2 * n;
        E[2, 1] = n;

        // This is never used so we don't compute it
        //E.col(2) = M.col(1).cross(M.col(2)).normalized();
        E[0, 2] = 0;
        E[1, 2] = 0;
        E[2, 2] = 0;
    }

    // Performs a few newton steps on the equations
    void refine_lambda(float lambda1, float lambda2, float lambda3,
                      float a12, float a13, float a23,
                       float b12, float b13, float b23,
                       out float newLambda1, out float newLambda2, out float newLambda3)
    {
        newLambda1 = lambda1;
        newLambda2 = lambda2;
        newLambda3 = lambda3;

        for (int iter = 0; iter < 5; ++iter)
        {
            float r1 = (newLambda1 * newLambda1 - 2.0f * newLambda1 * newLambda2 * b12 + newLambda2 * newLambda2 - a12);
            float r2 = (newLambda1 * newLambda1 - 2.0f * newLambda1 * newLambda3 * b13 + newLambda3 * newLambda3 - a13);
            float r3 = (newLambda2 * newLambda2 - 2.0f * newLambda2 * newLambda3 * b23 + newLambda3 * newLambda3 - a23);
            if (Mathf.Abs(r1) + Mathf.Abs(r2) + Mathf.Abs(r3) < 1e-10)
                return;
            float x11 = newLambda1 - newLambda2 * b12; float x12 = newLambda2 - newLambda1 * b12;
            float x21 = newLambda1 - newLambda3 * b13; float x23 = newLambda3 - newLambda1 * b13;
            float x32 = newLambda2 - newLambda3 * b23; float x33 = newLambda3 - newLambda2 * b23;
            float detJ = 0.5f / (x11 * x23 * x32 + x12 * x21 * x33); // half minus inverse determinant 
                                                                     // This uses the closed form of the inverse for the jacobean.
                                                                     // Due to the zero elements this actually becomes quite nice.
            newLambda1 += (-x23 * x32 * r1 - x12 * x33 * r2 + x12 * x23 * r3) * detJ;
            newLambda2 += (-x21 * x33 * r1 + x11 * x33 * r2 - x11 * x23 * r3) * detJ;
            newLambda3 += (x21 * x32 * r1 - x11 * x32 * r2 - x12 * x21 * r3) * detJ;
        }
    }


    // Solves for camera pose such that: lambda*x = R*X+t  with positive lambda.
    List<Matrix4x4> p3p(Vector3[] x, Vector3[] X)
    {
        List<Matrix4x4> output = new List<Matrix4x4>();

        Vector3 dX12 = X[0] - X[1];
        Vector3 dX13 = X[0] - X[2];
        Vector3 dX23 = X[1] - X[2];

        float a12 = dX12.sqrMagnitude;
        float b12 = Vector3.Dot(x[0], x[1]);

        float a13 = dX13.sqrMagnitude;
        float b13 = Vector3.Dot(x[0], x[2]);

        float a23 = dX23.sqrMagnitude;
        float b23 = Vector3.Dot(x[1], x[2]);

        float a23b12 = a23 * b12;
        float a12b23 = a12 * b23;
        float a23b13 = a23 * b13;
        float a13b23 = a13 * b23;

        float[,] D1 = new float[3, 3];
        float[,] D2 = new float[3, 3];

        D1[0, 0] = a23;
        D1[0, 1] = -a23b12;
        D1[0, 2] = 0;
        D1[1, 0] = -a23b12;
        D1[1, 1] = a23 - a12;
        D1[1, 2] = a12b23;
        D1[2, 0] = 0;
        D1[2, 1] = a12b23;
        D1[2, 2] = -a12;

        D2[0, 0] = a23;
        D2[0, 1] = 0;
        D2[0, 2] = a23b13;
        D2[1, 0] = 0;
        D2[1, 1] = -a13;
        D2[1, 2] = a13b23;
        D2[2, 0] = -a23b13;
        D2[2, 1] = a13b23;
        D2[2, 2] = a23 - a13;

        float[,] DX1 = new float[3, 3];
        float[,] DX2=new float[3,3];

        Vector3 D1Col0 = new Vector3(D1[0, 0], D1[1, 0], D1[2, 0]);
        Vector3 D1Col1 = new Vector3(D1[0, 1], D1[1, 1], D1[2, 1]);
        Vector3 D1Col2 = new Vector3(D1[0, 2], D1[1, 2], D1[2, 2]);

        Vector3 crossD1Col0D1Col2 = Vector3.Cross(D1Col1, D1Col2);
        DX1[0, 0] = crossD1Col0D1Col2[0];
        DX1[0, 1] = crossD1Col0D1Col2[1];
        DX1[0, 2] = crossD1Col0D1Col2[2];

        Vector3 crossD1Col2D1Col0 = Vector3.Cross(D1Col2, D1Col0);
        DX1[1, 0] = crossD1Col2D1Col0[0];
        DX1[1, 1] = crossD1Col2D1Col0[1];
        DX1[1, 2] = crossD1Col2D1Col0[2];

        Vector3 crossD1Col0D1Col1 = Vector3.Cross(D1Col0, D1Col1);
        DX1[2, 0] = crossD1Col0D1Col1[0];
        DX1[2, 1] = crossD1Col0D1Col1[1];
        DX1[2, 2] = crossD1Col0D1Col1[2];

        Vector3 D2Col0 = new Vector3(D2[0, 0], D2[1, 0], D2[2, 0]);
        Vector3 D2Col1 = new Vector3(D2[0, 1], D2[1, 1], D2[2, 1]);
        Vector3 D2Col2 = new Vector3(D2[0, 2], D2[1, 2], D2[2, 2]);

        Vector3 crossD2Col0D2Col2 = Vector3.Cross(D2Col1, D2Col2);
        DX2[0, 0] = crossD2Col0D2Col2[0];
        DX2[0, 1] = crossD2Col0D2Col2[1];
        DX2[0, 2] = crossD2Col0D2Col2[2];

        Vector3 crossD2Col2D2Col0 = Vector3.Cross(D2Col2, D2Col0);
        DX2[1, 0] = crossD2Col2D2Col0[0];
        DX2[1, 1] = crossD2Col2D2Col0[1];
        DX2[1, 2] = crossD2Col2D2Col0[2];

        Vector3 crossD2Col0D2Col1 = Vector3.Cross(D2Col0, D2Col1);
        DX2[2, 0] = crossD2Col0D2Col1[0];
        DX2[2, 1] = crossD2Col0D2Col1[1];
        DX2[2, 2] = crossD2Col0D2Col1[2];

        // Coefficients of p(gamma) = det(D1 + gamma*D2)
        // In the original paper c2 and c1 are switched.
        float c3 = D2[0, 0] * DX2[0, 0] + D2[1, 0] * DX2[1, 0] + D2[2, 0] * DX2[2, 0];

        float c2 = 0;
        float c1 = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j=0; j < 3; j++)
            {
                c2 += D1[i, j] * DX2[i, j];
                c1 += D2[i, j] * DX1[i, j];
            }
        }

        float c0 = Vector3.Dot(D1Col0, new Vector3(DX1[0, 0], DX1[1, 0], DX1[2, 0]));

        // closed root solver for cubic root
        float c3inv = 1.0f / c3;
        c2 *= c3inv; c1 *= c3inv; c0 *= c3inv;

        float a = c1 - c2 * c2 / 3.0f;
        float b = (2.0f * c2 * c2 * c2 - 9.0f * c2 * c1) / 27.0f + c0;
        float c = b * b / 4.0f + a * a * a / 27.0f;
        float gamma;
        if (c > 0)
        {
            c = Mathf.Sqrt(c);
            b *= -0.5f;
            gamma = CubeRoot(b + c) + CubeRoot(b - c) - c2 / 3.0f;
        }
        else
        {
            c = 3.0f * b / (2.0f * a) * Mathf.Sqrt(-3.0f / a);
            gamma = 2.0f * Mathf.Sqrt(-a / 3.0f) * Mathf.Cos(Mathf.Acos(c) / 3.0f) - c2 / 3.0f;
        }


        // We do a single newton step on the cubic equation
        float f = gamma * gamma * gamma + c2 * gamma * gamma + c1 * gamma + c0;
        float df = 3.0f * gamma * gamma + 2.0f * c2 * gamma + c1;
        gamma = gamma - f / df;


        float[,] D0 = new float[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                D0[i, j] = D1[i, j] + gamma * D2[i, j];
            }
        }

        compute_eig3x3known0(D0, out float[,] E, out float sig1, out float sig2);

        float s = Mathf.Sqrt(-sig2 / sig1);

        float lambda1, lambda2, lambda3;
        Matrix4x4 pose = new Matrix4x4();
        Matrix4x4 XX = Matrix4x4.zero;

        XX[0, 0] = dX12[0];
        XX[0, 1] = dX12[1];
        XX[0, 2] = dX12[2];
        XX[1, 0] = dX13[0];
        XX[1, 1] = dX13[1];
        XX[1, 2] = dX13[2];

        Vector3 cross = Vector3.Cross(dX12, dX13);

        XX[2, 0] = cross[0];
        XX[2, 1] = cross[1];
        XX[2, 2] = cross[2];

        XX[3, 3] = 1;

        XX = XX.inverse;

        Vector3 v1, v2;
        Matrix4x4 YY = Matrix4x4.zero;

        const double TOL_DOUBLE_ROOT = 1e-12;

        for (int s_flip = 0; s_flip < 2; ++s_flip, s = -s)
        {
            // [u1 u2 u3] * [lambda1; lambda2; lambda3] = 0
            float u1 = E[0, 0] - s * E[0, 1];
            float u2 = E[1, 0] - s * E[1, 1];
            float u3 = E[2, 0] - s * E[2, 1];

            // here we run into trouble if u1 is zero,
            // so depending on which is larger, we solve for either lambda1 or lambda2
            // The case u1 = u2 = 0 is degenerate and can be ignored
            bool switch_12 = Mathf.Abs(u1) < Mathf.Abs(u2);

            float w0;
            float w1;

            if (switch_12)
            {
                // solve for lambda2
                w0 = -u1 / u2;
                w1 = -u3 / u2;
                a = -a13 * w1 * w1 + 2 * a13b23 * w1 - a13 + a23;
                b = 2 * a13b23 * w0 - 2 * a23b13 - 2 * a13 * w0 * w1;
                c = -a13 * w0 * w0 + a23;

                float b2m4ac = b * b - 4 * a * c;

                // if b2m4ac is zero we have a double root
                // to handle this case we allow slightly negative discriminants here
                if (b2m4ac < -TOL_DOUBLE_ROOT)
                    continue;
                // clip to zero here in case we have double root
                float sq = Mathf.Sqrt(Mathf.Max(0, b2m4ac));

                // first root of tau
                float tau = (b > 0) ? (2.0f * c) / (-b - sq) : (2.0f * c) / (-b + sq);

                for (int tau_flip = 0; tau_flip < 2; ++tau_flip, tau = c / (a * tau))
                {
                    if (tau > 0)
                    {
                        lambda1 = Mathf.Sqrt(a13 / (tau * (tau - 2 * b13) + 1));
                        lambda3 = tau * lambda1;
                        lambda2 = w0 * lambda1 + w1 * lambda3;
                        // since tau > 0 and lambda1 > 0 we only need to check lambda2 here
                        if (lambda2 < 0)
                            continue;

                        refine_lambda(lambda1, lambda2, lambda3, a12, a13, a23, b12, b13, b23, out lambda1, out lambda2, out lambda3);
                        v1 = lambda1 * x[0] - lambda2 * x[1];
                        v2 = lambda1 * x[0] - lambda3 * x[2];
                        YY[0, 0] = v1[0];
                        YY[0, 1] = v1[1];
                        YY[0, 2] = v1[2];
                        YY[1, 0] = v2[0];
                        YY[1, 1] = v2[1];
                        YY[1, 2] = v2[2];

                        cross = Vector3.Cross(v1, v2);

                        YY[2, 0] = cross[0];
                        YY[2, 1] = cross[1];
                        YY[2, 2] = cross[2];

                        YY[3, 3] = 1;

                        pose = YY * XX;
                        Vector3 translation = lambda1 * x[0] - pose.MultiplyPoint3x4(X[0]);
                        pose[0, 3] = translation[0];
                        pose[1, 3] = translation[1];
                        pose[2, 3] = translation[2];
                        output.Add(pose);
                    }

                    if (b2m4ac < TOL_DOUBLE_ROOT)
                    {
                        // double root we can skip the second tau
                        break;
                    }
                }

            }
            else
            {
                // Same as except we solve for lambda1 as a combination of lambda2 and lambda3
                // (default case in the paper)            
                w0 = -u2 / u1;
                w1 = -u3 / u1;
                a = (a13 - a12) * w1 * w1 + 2.0f * a12 * b13 * w1 - a12;
                b = -2.0f * a13 * b12 * w1 + 2.0f * a12 * b13 * w0 - 2.0f * w0 * w1 * (a12 - a13);
                c = (a13 - a12) * w0 * w0 - 2.0f * a13 * b12 * w0 + a13;
                float b2m4ac = b * b - 4.0f * a * c;
                if (b2m4ac < -TOL_DOUBLE_ROOT)
                    continue;
                float sq = Mathf.Sqrt(Mathf.Max(0, b2m4ac));
                float tau = (b > 0) ? (2.0f * c) / (-b - sq) : (2.0f * c) / (-b + sq);
                for (int tau_flip = 0; tau_flip < 2; ++tau_flip, tau = c / (a * tau))
                {
                    if (tau > 0)
                    {
                        lambda2 = Mathf.Sqrt(a23 / (tau * (tau - 2.0f * b23) + 1.0f));
                        lambda3 = tau * lambda2;
                        lambda1 = w0 * lambda2 + w1 * lambda3;

                        if (lambda1 < 0)
                            continue;
                        refine_lambda(lambda1, lambda2, lambda3, a12, a13, a23, b12, b13, b23, out float newLambda1, out float newLambda2, out float newLambda3);
                        v1 = lambda1 * x[0] - lambda2 * x[1];
                        v2 = lambda1 * x[0] - lambda3 * x[2];
                        YY[0, 0] = v1[0];
                        YY[0, 1] = v1[1];
                        YY[0, 2] = v1[2];
                        YY[1, 0] = v2[0];
                        YY[1, 1] = v2[1];
                        YY[1, 2] = v2[2];

                        cross = Vector3.Cross(v1, v2);

                        YY[2, 0] = cross[0];
                        YY[2, 1] = cross[1];
                        YY[2, 2] = cross[2];

                        YY[3, 3] = 1;
                        pose = YY * XX;
                        Vector3 translation = lambda1 * x[0] - pose.MultiplyPoint3x4(X[0]);
                        pose[0, 3] = translation[0];
                        pose[1, 3] = translation[1];
                        pose[2, 3] = translation[2];
                        output.Add(pose);
                    }
                    if (b2m4ac < TOL_DOUBLE_ROOT)
                    {
                        break;
                    }
                }
            }
        }
        return output;
    }
    float CubeRoot(float d)
    {
        return Mathf.Pow(Mathf.Abs(d), 1f / 3f) * Mathf.Sign(d);
    }
}
