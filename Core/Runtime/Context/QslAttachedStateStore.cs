using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QSL.Core.Runtime.Context
{
    /// <summary>
    /// Stores runtime state attached to exact consumer-owned
    /// object instances without requiring those objects to
    /// implement any QSL contract.
    ///
    /// Attached members must be explicitly received before
    /// they may be read or updated.
    /// </summary>
    public sealed class QslAttachedStateStore
    {
        private readonly ConditionalWeakTable<
            object,
            Dictionary<string, object?>>
            _state =
                new ConditionalWeakTable<
                    object,
                    Dictionary<string, object?>>();

        public bool Introduce(
            object target,
            string memberName,
            object? initialValue)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    memberName))
            {
                throw new ArgumentException(
                    "Attached-state member name cannot be empty.",
                    nameof(memberName)
                );
            }

            var members =
                _state.GetOrCreateValue(
                    target
                );

            lock (members)
            {
                if (members.ContainsKey(
                        memberName))
                {
                    return false;
                }

                members.Add(
                    memberName,
                    initialValue
                );

                return true;
            }
        }

        public bool Contains(
            object target,
            string memberName)
        {
            if (target == null ||
                string.IsNullOrWhiteSpace(
                    memberName))
            {
                return false;
            }

            if (!_state.TryGetValue(
                    target,
                    out var members))
            {
                return false;
            }

            lock (members)
            {
                return members.ContainsKey(
                    memberName
                );
            }
        }

        public bool TryGet(
            object target,
            string memberName,
            out object? value)
        {
            value =
                null;

            if (target == null ||
                string.IsNullOrWhiteSpace(
                    memberName))
            {
                return false;
            }

            if (!_state.TryGetValue(
                    target,
                    out var members))
            {
                return false;
            }

            lock (members)
            {
                return members.TryGetValue(
                    memberName,
                    out value
                );
            }
        }

        public bool TrySet(
            object target,
            string memberName,
            object? value)
        {
            if (target == null ||
                string.IsNullOrWhiteSpace(
                    memberName))
            {
                return false;
            }

            if (!_state.TryGetValue(
                    target,
                    out var members))
            {
                return false;
            }

            lock (members)
            {
                if (!members.ContainsKey(
                        memberName))
                {
                    return false;
                }

                members[memberName] =
                    value;

                return true;
            }
        }

        /// <summary>
        /// Removes one attached-state member from the exact
        /// consumer-owned runtime object.
        ///
        /// Returns false when the target has no attached state
        /// or the requested member does not exist.
        /// </summary>
        public bool Remove(
            object target,
            string memberName)
        {
            if (target == null ||
                string.IsNullOrWhiteSpace(
                    memberName))
            {
                return false;
            }

            if (!_state.TryGetValue(
                    target,
                    out var members))
            {
                return false;
            }

            lock (members)
            {
                if (!members.Remove(
                        memberName))
                {
                    return false;
                }

                if (members.Count == 0)
                {
                    _state.Remove(
                        target
                    );
                }

                return true;
            }
        }

        /// <summary>
        /// Removes all attached state associated with the exact
        /// consumer-owned runtime object.
        ///
        /// Returns false when the target has no attached-state
        /// container.
        /// </summary>
        public bool RemoveAll(
            object target)
        {
            if (target == null)
            {
                return false;
            }

            return _state.Remove(
                target
            );
        }
        
        /// <summary>
        /// Removes every attached-state association currently retained
        /// by this store.
        /// </summary>
        public void Clear()
        {
            _state.Clear();
        }
    }
}