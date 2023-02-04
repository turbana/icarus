using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using static Unity.Mathematics.math;
using Unity.Mathematics;

namespace Icarus.Mathematics
{
    /// <summary>
    /// A dquaternion type for representing rotations.
    /// </summary>
    // [Il2CppEagerStaticClassConstruction]
    [Serializable]
    public struct dquaternion : System.IEquatable<dquaternion>, IFormattable
    {
        /// <summary>
        /// The dquaternion component values.
        /// </summary>
        public double4 value;

        /// <summary>A dquaternion representing the identity transform.</summary>
        public static readonly dquaternion identity = new dquaternion(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>Constructs a dquaternion from four double values.</summary>
        /// <param name="x">The dquaternion x component.</param>
        /// <param name="y">The dquaternion y component.</param>
        /// <param name="z">The dquaternion z component.</param>
        /// <param name="w">The dquaternion w component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public dquaternion(double x, double y, double z, double w) { value.x = x; value.y = y; value.z = z; value.w = w; }

        /// <summary>Constructs a dquaternion from double4 vector.</summary>
        /// <param name="value">The dquaternion xyzw component values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public dquaternion(double4 value) { this.value = value; }

        /// <summary>Implicitly converts a double4 vector to a dquaternion.</summary>
        /// <param name="v">The dquaternion xyzw component values.</param>
        /// <returns>The dquaternion constructed from a double4 vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator dquaternion(double4 v) { return new dquaternion(v); }

        /// <summary>Implicitly converts a quaternion to a dquaternion.</summary>
        /// <param name="q">The quaternion.</param>
        /// <returns>The dquaternion constructed from a quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator dquaternion(quaternion q) { return new dquaternion((double4)q.value); }

        /// <summary>Implicitly converts a dquaternion to a quaternion.</summary>
        /// <param name="q">The dquaternion.</param>
        /// <returns>The quaternion constructed from a dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator quaternion(dquaternion q) { return new quaternion((float4)q.value); }

        /// <summary>Constructs a unit dquaternion from a double3x3 rotation matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The double3x3 orthonormal rotation matrix.</param>
        public dquaternion(double3x3 m)
        {
            throw new NotImplementedException("not implemented");
            // double3 u = m.c0;
            // double3 v = m.c1;
            // double3 w = m.c2;

            // uint u_sign = (asuint(u.x) & 0x80000000);
            // double t = v.y + asdouble(asuint(w.z) ^ u_sign);
            // uint4 u_mask = uint4((int)u_sign >> 31);
            // uint4 t_mask = uint4(asint(t) >> 31);

            // double tr = 1.0f + abs(u.x);

            // uint4 sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^ (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^ (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

            // value = double4(tr, u.y, w.x, v.z) + asdouble(asuint(double4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

            // value = asdouble((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
            // value = asdouble((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));
            // value = normalize(value);
        }

        /// <summary>Constructs a unit dquaternion from an orthonormal double4x4 matrix.</summary>
        /// <param name="m">The double4x4 orthonormal rotation matrix.</param>
        public dquaternion(double4x4 m)
        {
            throw new NotImplementedException("not implemented");
            // double4 u = m.c0;
            // double4 v = m.c1;
            // double4 w = m.c2;

            // uint u_sign = (asuint(u.x) & 0x80000000);
            // double t = v.y + asdouble(asuint(w.z) ^ u_sign);
            // uint4 u_mask = uint4((int)u_sign >> 31);
            // uint4 t_mask = uint4(asint(t) >> 31);

            // double tr = 1.0f + abs(u.x);

            // uint4 sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^ (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^ (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

            // value = double4(tr, u.y, w.x, v.z) + asdouble(asuint(double4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

            // value = asdouble((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
            // value = asdouble((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));

            // value = normalize(value);
        }

        /// <summary>
        /// Returns a dquaternion representing a rotation around a unit axis by an angle in radians.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in radians.</param>
        /// <returns>The dquaternion representing a rotation around an axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion AxisAngle(double3 axis, double angle)
        {
            double sina, cosa;
            math.sincos(0.5f * angle, out sina, out cosa);
            return dmath.dquaternion(double4(axis * sina, cosa));
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the x-axis, then the y-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in x-y-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerXYZ(double3 xyz)
        {
            // return mul(rotateZ(xyz.z), mul(rotateY(xyz.y), rotateX(xyz.x)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(-1.0f, 1.0f, -1.0f, 1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the x-axis, then the z-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in x-z-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerXZY(double3 xyz)
        {
            // return mul(rotateY(xyz.y), mul(rotateZ(xyz.z), rotateX(xyz.x)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.z * s.x
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(1.0f, 1.0f, -1.0f, -1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the y-axis, then the x-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in y-x-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerYXZ(double3 xyz)
        {
            // return mul(rotateZ(xyz.z), mul(rotateX(xyz.x), rotateY(xyz.y)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.z * s.x
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(-1.0f, 1.0f, 1.0f, -1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the y-axis, then the z-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in y-z-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerYZX(double3 xyz)
        {
            // return mul(rotateX(xyz.x), mul(rotateZ(xyz.z), rotateY(xyz.y)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(-1.0f, -1.0f, 1.0f, 1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the z-axis, then the x-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// This is the default order rotation order in Unity.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in z-x-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerZXY(double3 xyz)
        {
            // return mul(rotateY(xyz.y), mul(rotateX(xyz.x), rotateZ(xyz.z)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(1.0f, -1.0f, -1.0f, 1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the z-axis, then the y-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in z-y-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerZYX(double3 xyz)
        {
            // return mul(rotateX(xyz.x), mul(rotateY(xyz.y), rotateZ(xyz.z)));
            double3 s, c;
            sincos(0.5f * xyz, out s, out c);
            return dmath.dquaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.x * s.z
                double4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * double4(c.xyz, s.x) * double4(1.0f, -1.0f, 1.0f, -1.0f)
                );
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the x-axis, then the y-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in x-y-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerXYZ(double x, double y, double z) { return EulerXYZ(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the x-axis, then the z-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in x-z-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerXZY(double x, double y, double z) { return EulerXZY(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the y-axis, then the x-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in y-x-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerYXZ(double x, double y, double z) { return EulerYXZ(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the y-axis, then the z-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in y-z-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerYZX(double x, double y, double z) { return EulerYZX(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the z-axis, then the x-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// This is the default order rotation order in Unity.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in z-x-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerZXY(double x, double y, double z) { return EulerZXY(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing a rotation around the z-axis, then the y-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in z-y-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion EulerZYX(double x, double y, double z) { return EulerZYX(double3(x, y, z)); }

        /// <summary>
        /// Returns a dquaternion constructed by first performing 3 rotations around the principal axes in a given order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// When the rotation order is known at compile time, it is recommended for performance reasons to use specific
        /// Euler rotation constructors such as EulerZXY(...).
        /// </summary>
        /// <param name="xyz">A double3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <param name="order">The order in which the rotations are applied.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in the specified order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion Euler(double3 xyz, RotationOrder order = RotationOrder.ZXY)
        {
            switch (order)
            {
                case RotationOrder.XYZ:
                    return EulerXYZ(xyz);
                case RotationOrder.XZY:
                    return EulerXZY(xyz);
                case RotationOrder.YXZ:
                    return EulerYXZ(xyz);
                case RotationOrder.YZX:
                    return EulerYZX(xyz);
                case RotationOrder.ZXY:
                    return EulerZXY(xyz);
                case RotationOrder.ZYX:
                    return EulerZYX(xyz);
                default:
                    return dquaternion.identity;
            }
        }

        /// <summary>
        /// Returns a dquaternion constructed by first performing 3 rotations around the principal axes in a given order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// When the rotation order is known at compile time, it is recommended for performance reasons to use specific
        /// Euler rotation constructors such as EulerZXY(...).
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <param name="order">The order in which the rotations are applied.</param>
        /// <returns>The dquaternion representing the Euler angle rotation in the specified order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion Euler(double x, double y, double z, RotationOrder order = RotationOrder.Default)
        {
            return Euler(double3(x, y, z), order);
        }

        /// <summary>Returns a dquaternion that rotates around the x-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the x-axis towards the origin in radians.</param>
        /// <returns>The dquaternion representing a rotation around the x-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion RotateX(double angle)
        {
            double sina, cosa;
            math.sincos(0.5f * angle, out sina, out cosa);
            return dmath.dquaternion(sina, 0.0f, 0.0f, cosa);
        }

        /// <summary>Returns a dquaternion that rotates around the y-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the y-axis towards the origin in radians.</param>
        /// <returns>The dquaternion representing a rotation around the y-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion RotateY(double angle)
        {
            double sina, cosa;
            math.sincos(0.5f * angle, out sina, out cosa);
            return dmath.dquaternion(0.0f, sina, 0.0f, cosa);
        }

        /// <summary>Returns a dquaternion that rotates around the z-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the z-axis towards the origin in radians.</param>
        /// <returns>The dquaternion representing a rotation around the z-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion RotateZ(double angle)
        {
            double sina, cosa;
            math.sincos(0.5f * angle, out sina, out cosa);
            return dmath.dquaternion(0.0f, 0.0f, sina, cosa);
        }

        /// <summary>
        /// Returns a dquaternion view rotation given a unit length forward vector and a unit length up vector.
        /// The two input vectors are assumed to be unit length and not collinear.
        /// If these assumptions are not met use double3x3.LookRotationSafe instead.
        /// </summary>
        /// <param name="forward">The view forward direction.</param>
        /// <param name="up">The view up direction.</param>
        /// <returns>The dquaternion view rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion LookRotation(double3 forward, double3 up)
        {
            double3 t = normalize(cross(up, forward));
            return dmath.dquaternion(double3x3(t, cross(forward, t), forward));
        }

        /// <summary>
        /// Returns a dquaternion view rotation given a forward vector and an up vector.
        /// The two input vectors are not assumed to be unit length.
        /// If the magnitude of either of the vectors is so extreme that the calculation cannot be carried out reliably or the vectors are collinear,
        /// the identity will be returned instead.
        /// </summary>
        /// <param name="forward">The view forward direction.</param>
        /// <param name="up">The view up direction.</param>
        /// <returns>The dquaternion view rotation or the identity dquaternion.</returns>
        public static dquaternion LookRotationSafe(double3 forward, double3 up)
        {
            double forwardLengthSq = dot(forward, forward);
            double upLengthSq = dot(up, up);

            forward *= rsqrt(forwardLengthSq);
            up *= rsqrt(upLengthSq);

            double3 t = cross(up, forward);
            double tLengthSq = dot(t, t);
            t *= rsqrt(tLengthSq);

            double mn = min(min(forwardLengthSq, upLengthSq), tLengthSq);
            double mx = max(max(forwardLengthSq, upLengthSq), tLengthSq);

            bool accept = mn > 1e-35f && mx < 1e35f && isfinite(forwardLengthSq) && isfinite(upLengthSq) && isfinite(tLengthSq);
            return dmath.dquaternion(select(double4(0.0f, 0.0f, 0.0f, 1.0f), dmath.dquaternion(double3x3(t, cross(forward, t),forward)).value, accept));
        }

        /// <summary>Returns true if the dquaternion is equal to a given dquaternion, false otherwise.</summary>
        /// <param name="x">The dquaternion to compare with.</param>
        /// <returns>True if the dquaternion is equal to the input, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(dquaternion x) { return value.x == x.value.x && value.y == x.value.y && value.z == x.value.z && value.w == x.value.w; }

        /// <summary>Returns whether true if the dquaternion is equal to a given dquaternion, false otherwise.</summary>
        /// <param name="x">The object to compare with.</param>
        /// <returns>True if the dquaternion is equal to the input, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object x) { return x is dquaternion converted && Equals(converted); }

        /// <summary>Returns a hash code for the dquaternion.</summary>
        /// <returns>The hash code of the dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return (int)math.hash(this.value); }

        /// <summary>Returns a string representation of the dquaternion.</summary>
        /// <returns>The string representation of the dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("dquaternion({0}f, {1}f, {2}f, {3}f)", value.x, value.y, value.z, value.w);
        }

        /// <summary>Returns a string representation of the dquaternion using a specified format and culture-specific format information.</summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider to use during string formatting.</param>
        /// <returns>The formatted string representation of the dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("dquaternion({0}f, {1}f, {2}f, {3}f)", value.x.ToString(format, formatProvider), value.y.ToString(format, formatProvider), value.z.ToString(format, formatProvider), value.w.ToString(format, formatProvider));
        }
    }

    public static class dmath
    {
        /// <summary>Returns a dquaternion constructed from four double values.</summary>
        /// <param name="x">The x component of the dquaternion.</param>
        /// <param name="y">The y component of the dquaternion.</param>
        /// <param name="z">The z component of the dquaternion.</param>
        /// <param name="w">The w component of the dquaternion.</param>
        /// <returns>The dquaternion constructed from individual components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion dquaternion(double x, double y, double z, double w) { return new dquaternion(x, y, z, w); }

        /// <summary>Returns a dquaternion constructed from a double4 vector.</summary>
        /// <param name="value">The double4 containing the components of the dquaternion.</param>
        /// <returns>The dquaternion constructed from a double4.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion dquaternion(double4 value) { return new dquaternion(value); }

        /// <summary>Returns a unit dquaternion constructed from a double3x3 rotation matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The double3x3 rotation matrix.</param>
        /// <returns>The dquaternion constructed from a double3x3 matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion dquaternion(double3x3 m) { return new dquaternion(m); }

        /// <summary>Returns a unit dquaternion constructed from a double4x4 matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The double4x4 matrix (must be orthonormal).</param>
        /// <returns>The dquaternion constructed from a double4x4 matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion dquaternion(double4x4 m) { return new dquaternion(m); }

       /// <summary>Returns the conjugate of a dquaternion value.</summary>
       /// <param name="q">The dquaternion to conjugate.</param>
       /// <returns>The conjugate of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion conjugate(dquaternion q)
        {
            return dquaternion(q.value * double4(-1.0f, -1.0f, -1.0f, 1.0f));
        }

       /// <summary>Returns the inverse of a dquaternion value.</summary>
       /// <param name="q">The dquaternion to invert.</param>
       /// <returns>The dquaternion inverse of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion inverse(dquaternion q)
        {
            double4 x = q.value;
            return dquaternion(rcp(dot(x, x)) * x * double4(-1.0f, -1.0f, -1.0f, 1.0f));
        }

        /// <summary>Returns the dot product of two dquaternions.</summary>
        /// <param name="a">The first dquaternion.</param>
        /// <param name="b">The second dquaternion.</param>
        /// <returns>The dot product of two dquaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double dot(dquaternion a, dquaternion b)
        {
            return math.dot(a.value, b.value);
        }

        /// <summary>Returns the length of a dquaternion.</summary>
        /// <param name="q">The input dquaternion.</param>
        /// <returns>The length of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double length(dquaternion q)
        {
            return sqrt(dot(q.value, q.value));
        }

        /// <summary>Returns the squared length of a dquaternion.</summary>
        /// <param name="q">The input dquaternion.</param>
        /// <returns>The length squared of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double lengthsq(dquaternion q)
        {
            return dot(q.value, q.value);
        }

        /// <summary>Returns a normalized version of a dquaternion q by scaling it by 1 / length(q).</summary>
        /// <param name="q">The dquaternion to normalize.</param>
        /// <returns>The normalized dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion normalize(dquaternion q)
        {
            double4 x = q.value;
            return dquaternion(rsqrt(dot(x, x)) * x);
        }

        /// <summary>
        /// Returns a safe normalized version of the q by scaling it by 1 / length(q).
        /// Returns the identity when 1 / length(q) does not produce a finite number.
        /// </summary>
        /// <param name="q">The dquaternion to normalize.</param>
        /// <returns>The normalized dquaternion or the identity dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion normalizesafe(dquaternion q)
        {
            double4 x = q.value;
            double len = math.dot(x, x);
            double4 identity = Icarus.Mathematics.dquaternion.identity.value;
            return dquaternion(select(identity, x * math.rsqrt(len), len > FLT_MIN_NORMAL));
        }

        /// <summary>
        /// Returns a safe normalized version of the q by scaling it by 1 / length(q).
        /// Returns the given default value when 1 / length(q) does not produce a finite number.
        /// </summary>
        /// <param name="q">The dquaternion to normalize.</param>
        /// <param name="defaultvalue">The default value.</param>
        /// <returns>The normalized dquaternion or the default value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion normalizesafe(dquaternion q, dquaternion defaultvalue)
        {
            double4 x = q.value;
            double len = math.dot(x, x);
            return dmath.dquaternion(math.select(defaultvalue.value, x * math.rsqrt(len), len > FLT_MIN_NORMAL));
        }

        /// <summary>Returns the natural exponent of a dquaternion. Assumes w is zero.</summary>
        /// <param name="q">The dquaternion with w component equal to zero.</param>
        /// <returns>The natural exponent of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion unitexp(dquaternion q)
        {
            double v_rcp_len = rsqrt(math.dot(q.value.xyz, q.value.xyz));
            double v_len = rcp(v_rcp_len);
            double sin_v_len, cos_v_len;
            sincos(v_len, out sin_v_len, out cos_v_len);
            return dmath.dquaternion(double4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len));
        }

        /// <summary>Returns the natural exponent of a dquaternion.</summary>
        /// <param name="q">The dquaternion.</param>
        /// <returns>The natural exponent of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion exp(dquaternion q)
        {
            double v_rcp_len = rsqrt(math.dot(q.value.xyz, q.value.xyz));
            double v_len = rcp(v_rcp_len);
            double sin_v_len, cos_v_len;
            sincos(v_len, out sin_v_len, out cos_v_len);
            return dmath.dquaternion(double4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len) * math.exp(q.value.w));
        }

        /// <summary>Returns the natural logarithm of a unit length dquaternion.</summary>
        /// <param name="q">The unit length dquaternion.</param>
        /// <returns>The natural logarithm of the unit length dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion unitlog(dquaternion q)
        {
            double w = clamp(q.value.w, -1.0f, 1.0f);
            double s = acos(w) * rsqrt(1.0f - w*w);
            return dmath.dquaternion(double4(q.value.xyz * s, 0.0f));
        }

        /// <summary>Returns the natural logarithm of a dquaternion.</summary>
        /// <param name="q">The dquaternion.</param>
        /// <returns>The natural logarithm of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion log(dquaternion q)
        {
            double v_len_sq = math.dot(q.value.xyz, q.value.xyz);
            double q_len_sq = v_len_sq + q.value.w*q.value.w;

            double s = acos(clamp(q.value.w * rsqrt(q_len_sq), -1.0f, 1.0f)) * rsqrt(v_len_sq);
            return dmath.dquaternion(double4(q.value.xyz * s, 0.5f * math.log(q_len_sq)));
        }

        /// <summary>Returns the result of transforming the dquaternion b by the dquaternion a.</summary>
        /// <param name="a">The dquaternion on the left.</param>
        /// <param name="b">The dquaternion on the right.</param>
        /// <returns>The result of transforming dquaternion b by the dquaternion a.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion mul(dquaternion a, dquaternion b)
        {
            return dmath.dquaternion(a.value.wwww * b.value + (a.value.xyzx * b.value.wwwx + a.value.yzxy * b.value.zxyy) * double4(1.0f, 1.0f, 1.0f, -1.0f) - a.value.zxyz * b.value.yzxz);
        }

        /// <summary>Returns the result of transforming a vector by a dquaternion.</summary>
        /// <param name="q">The dquaternion transformation.</param>
        /// <param name="v">The vector to transform.</param>
        /// <returns>The transformation of vector v by dquaternion q.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double3 mul(dquaternion q, double3 v)
        {
            double3 t = 2 * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }

        /// <summary>Returns the result of rotating a vector by a unit dquaternion.</summary>
        /// <param name="q">The dquaternion rotation.</param>
        /// <param name="v">The vector to rotate.</param>
        /// <returns>The rotation of vector v by dquaternion q.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double3 rotate(dquaternion q, double3 v)
        {
            double3 t = 2 * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }

        /// <summary>Returns the result of a normalized linear interpolation between two dquaternions q1 and a2 using an interpolation parameter t.</summary>
        /// <remarks>
        /// Prefer to use this over slerp() when you know the distance between q1 and q2 is small. This can be much
        /// higher performance due to avoiding trigonometric function evaluations that occur in slerp().
        /// </remarks>
        /// <param name="q1">The first dquaternion.</param>
        /// <param name="q2">The second dquaternion.</param>
        /// <param name="t">The interpolation parameter.</param>
        /// <returns>The normalized linear interpolation of two dquaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion nlerp(dquaternion q1, dquaternion q2, double t)
        {
            double dt = dot(q1, q2);
            if(dt < 0.0f)
            {
                q2.value = -q2.value;
            }

            return normalize(dquaternion(lerp(q1.value, q2.value, t)));
        }

        /// <summary>Returns the result of a spherical interpolation between two dquaternions q1 and a2 using an interpolation parameter t.</summary>
        /// <param name="q1">The first dquaternion.</param>
        /// <param name="q2">The second dquaternion.</param>
        /// <param name="t">The interpolation parameter.</param>
        /// <returns>The spherical linear interpolation of two dquaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dquaternion slerp(dquaternion q1, dquaternion q2, double t)
        {
            double dt = dot(q1, q2);
            if (dt < 0.0f)
            {
                dt = -dt;
                q2.value = -q2.value;
            }

            if (dt < 0.9995f)
            {
                double angle = acos(dt);
                double s = rsqrt(1.0f - dt * dt);    // 1.0f / sin(angle)
                double w1 = sin(angle * (1.0f - t)) * s;
                double w2 = sin(angle * t) * s;
                return dmath.dquaternion(q1.value * w1 + q2.value * w2);
            }
            else
            {
                // if the angle is small, use linear interpolation
                return nlerp(q1, q2, t);
            }
        }

        /// <summary>Returns a uint hash code of a dquaternion.</summary>
        /// <param name="q">The dquaternion to hash.</param>
        /// <returns>The hash code for the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint hash(dquaternion q)
        {
            return hash(q.value);
        }

        /// <summary>
        /// Returns a uint4 vector hash code of a dquaternion.
        /// When multiple elements are to be hashes together, it can more efficient to calculate and combine wide hash
        /// that are only reduced to a narrow uint hash at the very end instead of at every step.
        /// </summary>
        /// <param name="q">The dquaternion to hash.</param>
        /// <returns>The uint4 vector hash code of the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 hashwide(dquaternion q)
        {
            return hashwide(q.value);
        }


        /// <summary>
        /// Transforms the forward vector by a dquaternion.
        /// </summary>
        /// <param name="q">The dquaternion transformation.</param>
        /// <returns>The forward vector transformed by the input dquaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double3 forward(dquaternion q) { return mul(q, double3(0, 0, 1)); }  // for compatibility
    }
}
