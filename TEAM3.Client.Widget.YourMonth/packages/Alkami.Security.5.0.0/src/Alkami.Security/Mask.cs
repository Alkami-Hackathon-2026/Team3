using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Alkami.Security
{
    /// <summary>
    /// The <see cref="Mask"/> encapsulates functionality for managing bit masks.
    /// </summary>
    public class Mask
    {
        /// <summary>
        /// The internal <see cref="BitArray"/> of the <see cref="Mask"/>.
        /// </summary>
        protected internal BitArray _storage;

        /// <summary>
        /// Initiates a new instance of the <see cref="Mask"/> class.
        /// </summary>
        public Mask()
        {
            _storage = new BitArray(160);
        }

        /// <summary>
        /// Obsolete: Use the constructor that does not take any parameters.
        /// </summary>
        /// <param name="size">The size of the <see cref="Mask"/>.</param>
        [Obsolete("Use the constructor that does not take any parameters.")]
        public Mask(int size)
        {
            // TODO: In a future release, mark this signature as internal and remove the ObsoleteAttribute.

            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _storage = new BitArray(size);
        }

        /// <summary>
        /// Serializes the <see cref="Mask"/> into an array of <see cref="Int32"/>s.
        /// </summary>
        /// <returns>The array of <see cref="Int32"/>s.</returns>
        public int[] Serialize()
        {
            var length = (_storage.Count + 31) / 32;

            var copy = new int[length];

            _storage.CopyTo(copy, 0);

            return copy;
        }

        /// <summary>
        /// Serializes the <see cref="Mask"/> to a value of integers separated by commas.
        /// </summary>
        /// <returns>A comma separated list of integers representing the <see cref="Mask"/>.</returns>
        public string SerializeToString()
        {
            return string.Join(",", Serialize());
        }

        /// <summary>
        /// Serializes the <see cref="Mask"/> into an array of bytes (little endian).
        /// </summary>
        /// <returns>An array of bytes (little endian) representing the value of the <see cref="Mask"/>.</returns>
        public byte[] SerializeToBytes()
        {
            var array = new byte[(this._storage.Length + 7) / 8];

            this._storage.CopyTo(array, 0);

            return array;
        }

        /// <summary>
        /// Obsolete: Use the 'Grant' method instead.
        /// </summary>
        /// <param name="position">The position to grant.</param>
        [Obsolete("Use the 'Grant' method instead.")]
        public void SetOn(int position)
        {
            this.Grant(position);
        }

        /// <summary>
        /// Grants the bit at the specified position.
        /// </summary>
        /// <param name="position">The position to grant.</param>
        public void Grant(int position)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            ExpandStorageIfNeeded(position);

            _storage.Set(position, true);
        }

        /// <summary>
        /// Revokes the bit at the specified position.
        /// </summary>
        /// <param name="position">The position to revoke.</param>
        public void Revoke(int position)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (position < this._storage.Length)
                _storage.Set(position, false);
        }

        /// <summary>
        /// Converts the <see cref="Mask"/> to a series of 0's and 1's.
        /// </summary>
        /// <returns>A series of 0's and 1's to represent the <see cref="Mask"/>.</returns>
        public override string ToString()
        {
            return string.Join("", _storage.Cast<bool>().Select(x => x ? "1" : "0").Reverse());
        }

        /// <summary>
        /// Checks to see if the <see cref="Mask"/> has all the positions set.
        /// </summary>
        /// <param name="positions">The positions to check.</param>
        /// <returns>True - All of the provided <paramref name="positions"/> were set.  False - otherwise.</returns>
        public bool HasAll(int[] positions)
        {
            if ((positions == null) || (positions.Length < 1))
                return true;

            if (positions.Any(position => (position < 0)))
                throw new ArgumentOutOfRangeException(nameof(positions));

            var maxPosition = positions.Max();

            return ((maxPosition < this._storage.Length) && positions.All(x => _storage[x]));
        }

        /// <summary>
        /// Obsolete: Change to call 'RevokeAllBut'.
        /// </summary>
        /// <param name="positions">The positions that are available to be on.</param>
        [Obsolete("Change to call 'RevokeAllBut'.")]
        public void RemoveAllBut(int[] positions)
        {
            this.RevokeAllBut(positions);
        }

        /// <summary>
        /// Updates the Mask to only allow enabled bits at the provided positions.
        /// </summary>
        /// <param name="positions">The positions that are available to be on.</param>
        public void RevokeAllBut(int[] positions)
        {
            if (positions != null)
            {
                if (positions.Any(position => position < 0))
                    throw new ArgumentOutOfRangeException(nameof(positions));

                var bitArray = new BitArray(this._storage.Length);

                foreach (var position in positions)
                    if (position < this._storage.Length)
                        bitArray.Set(position, true);

                this._storage.And(bitArray);
            }
        }

        /// <summary>
        /// Obsolete: Change to call 'GrantAllFrom'.
        /// </summary>
        /// <param name="mask">The mask to set bits from.</param>
		[Obsolete("Change to call 'GrantAllFrom'.")]
        public void SetAllFrom(Mask mask)
        {
            this.GrantAllFrom(mask);
        }

        /// <summary>
        /// Grants all of the same bits as the provide <see cref="Mask"/>.
        /// </summary>
        /// <param name="mask">The mask to set bits from.</param>
        public void GrantAllFrom(Mask mask)
        {
            this.ExpandStorageIfNeeded(mask._storage.Length - 1);

            var bitArray = new BitArray(mask._storage);

            if (bitArray.Length < this._storage.Length)
                bitArray.Length = this._storage.Length;

            this._storage.Or(bitArray);
        }

        /// <summary>
        /// Expands the underlying <see cref="BitArray"/> based on the largest potentially requestable position.
        /// </summary>
        /// <param name="largestPosition">The largest position potentially requested by the caller.</param>
        protected void ExpandStorageIfNeeded(int largestPosition)
        {
            if (largestPosition >= this._storage.Length)
            {
                this._storage.Length = GetLengthBy32BitBoundary(largestPosition + 1);
            }
        }

        /// <summary>
        /// Generates a <see cref="Mask"/> from a comma separated list of integers.
        /// </summary>
        /// <param name="commaSeperatedInt">The comma separated list of integers.</param>
        /// <returns>The <see cref="Mask"/> generated from <paramref name="commaSeperatedInt"/>.</returns>
        public static Mask FromString(string commaSeperatedInt)
        {
            var array = commaSeperatedInt
                .Split(',')
                .Select(int.Parse)
                .ToArray();

#pragma warning disable CS0618 // Type or member is obsolete
            var mask = new Mask(0)
            {
                _storage = new BitArray(array)
            };
#pragma warning restore CS0618 // Type or member is obsolete

            return mask;
        }

        /// <summary>
        /// Generates a <see cref="Mask"/> from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <returns>The <see cref="Mask"/> generated from <paramref name="bytes"/>.</returns>
        public static Mask FromBytes(byte[] bytes)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var mask = new Mask(0)
            {
                _storage = new BitArray(bytes)
            };
#pragma warning restore CS0618 // Type or member is obsolete

            return mask;
        }

        /// <summary>
        /// Gets the length adjusted to the 32-bit boundary that encompasses the desired length.
        /// </summary>
        /// <param name="requestedLength">The requested length for the <see cref="BitArray"/>.</param>
        /// <returns>The length adjusted to a 32-bit boundary.</returns>
        protected internal static int GetLengthBy32BitBoundary(int requestedLength)
        {
            // Adjust the number so that it bumps up to the upperbound of the 32 bit boundary.
            return (((requestedLength + 31) / 32) * 32);
        }
    }

    /// <summary>
    /// The <see cref="Mask{T}"/> encapsulates functionality for managing bit masks for an enum of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the enum to manage the bit mask for.</typeparam>
    public class Mask<T> : Mask where T : struct
    {
        private static readonly int StorageSize;

        static Mask()
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidEnumArgumentException("You must use this class with enums only");

            var values = Enum.GetValues(type).Cast<int>().ToList();

            if (!values.Any())
                throw new InvalidEnumArgumentException("The enum supplied must have one or more values");

            if (values.Any(value => value < 0))
                throw new InvalidEnumArgumentException("Enums cannot be assigned a value less than zero when used with this class");

            StorageSize = values.Max() + 1;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Mask{T}"/>.
        /// </summary>
        public Mask()
#pragma warning disable CS0618 // Type or member is obsolete
            : base(0)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            _storage = new BitArray(StorageSize);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Mask{T}"/>.
        /// </summary>
        /// <param name="mask">The <see cref="Mask"/> to build the instance off of.</param>
        private Mask(Mask mask)
        {
            _storage = new BitArray(mask._storage);
        }

        /// <summary>
        /// Grants the provided <typeparamref name="T"/> to the mask.
        /// </summary>
        /// <param name="permissions">The array of <typeparamref name="T"/> to grant.</param>
        public void Grant(params T[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            foreach (var permission in permissions)
            {
                var position = Convert.ToInt32(permission);
                Grant(position);
            }
        }

        /// <summary>
        /// Revokes the provided <typeparamref name="T"/> from the mask.
        /// </summary>
        /// <param name="permissions">The array of <typeparamref name="T"/> to revoke.</param>
        public void Revoke(params T[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            foreach (var permission in permissions)
            {
                var position = Convert.ToInt32(permission);
                Revoke(position);
            }
        }

        /// <summary>
        /// Checks to see if the provided <typeparamref name="T"/> is set.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True - The permission is set; False - otherwise.</returns>
        public bool HasPermission(T permission)
        {
            var position = Convert.ToInt32(permission);

            return ((position < this._storage.Length) && this._storage[position]);
        }

        /// <summary>
        /// Checks to see if the provided <typeparamref name="T"/> have all been set.
        /// </summary>
        /// <param name="permissions">The permissions to check.</param>
        /// <returns>True - All of the permissions are set; False - otherwise.</returns>
        public bool HasAllPermission(params T[] permissions)
        {
            var positions = permissions?
                .Select(p => Convert.ToInt32(p))
                .ToArray();

            return this.HasAll(positions);
        }

        /// <summary>
        /// Obsolete: Change to call 'RevokeAllBut'.
        /// </summary>
        /// <param name="permissionMask">The mask containing the super-set of permissions that are allowed.</param>
        [Obsolete("Change to call 'RevokeAllBut'.")]
        public void RemoveAllBut(Mask<T> permissionMask)
        {
            this.RevokeAllBut(permissionMask);
        }

        /// <summary>
        /// Updates the <see cref="Mask{T}"/> to only include the provided permissions.
        /// </summary>
        /// <param name="permissionMask">The mask containing the super-set of permissions that are allowed.</param>
        public void RevokeAllBut(Mask<T> permissionMask)
        {
            if (permissionMask == null)
                throw new ArgumentNullException(nameof(permissionMask));

            var bitArray = new BitArray(permissionMask._storage);

            if (bitArray.Length != this._storage.Length)
                bitArray.Length = this._storage.Length;

            this._storage.And(bitArray);
        }

        /// <summary>
        /// Obsolete: Change to call 'RevokeAllBut'.
        /// </summary>
        /// <param name="permissions">The permissions that are available to be set.</param>
        [Obsolete("Change to call 'RevokeAllBut'.")]
        public void RemoveAllBut(params T[] permissions)
        {
            this.RevokeAllBut(permissions);
        }

        /// <summary>
        /// Updates the <see cref="Mask{T}"/> to only include the provided permissions.
        /// </summary>
        /// <param name="permissions">The permissions that are available to be set.</param>
        public void RevokeAllBut(params T[] permissions)
        {
            var positions = permissions?
                .Select(p => Convert.ToInt32(p))
                .ToArray();

            this.RevokeAllBut(positions);
        }

        /// <summary>
        /// Generates a <see cref="Mask{T}"/> from the provided string.
        /// </summary>
        /// <param name="input">The string containing the comma separated list of integers.</param>
        /// <returns>The <see cref="Mask{T}"/> generated from the provided string.</returns>
        public new static Mask<T> FromString(string input)
        {
            var mask = Mask.FromString(input);
            return new Mask<T>(mask);
        }

        /// <summary>
        /// Generates a <see cref="Mask{T}"/> from the provided array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to generate the <see cref="Mask{T}"/> from.</param>
        /// <returns>The <see cref="Mask{T}"/> generated from the provided array of bytes.</returns>
        public new static Mask<T> FromBytes(byte[] bytes)
        {
            var mask = Mask.FromBytes(bytes);
            return new Mask<T>(mask);
        }
    }
}