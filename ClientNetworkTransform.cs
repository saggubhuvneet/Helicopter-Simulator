using UnityEngine;
using Unity.Netcode.Components;

namespace Unity.Multiplayer.Sample.Utilities.CLientAuthority
{
    [DisallowMultipleComponent]

    public class ClientNetworkTransform : NetworkTransform
    {
        // override this method and return false switch to owner Authorive mode
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}