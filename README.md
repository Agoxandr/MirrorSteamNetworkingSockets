# MirrorSteamNetworkingSockets
A Transport layer for Mirror based on Facepunch Steamworks SteamNetworkingSockets.

# You can't use this out the box. You need to modify the Channels and make sure to integrate this into your Project properly.
It also runs in Update. Make sure to check if it is running in the correct update loop. LateUpdate last recommended by Mirror but I don't care. You probably want to change it.

Channels example

    public static class Channels
    {
        public const int DefaultReliable = (int)SendType.Reliable;
        public const int DefaultUnreliable = (int)SendType.Unreliable;
    }
